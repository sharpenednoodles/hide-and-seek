using UnityEngine; 
using System.Collections; 

/// <summary>
/// Handles Prop switching and network sync
/// To USE
/// Props to switch into must be tagged with a prop tag, and have a corresponding named prefab. 
/// Obviosuly this causes an issue where players that are disguised can't be used as a prop to turn into
/// Will change later to read from a script located upon object so that scene objects can be arbitrarily named
/// </summary>

public class PropSwitching : MonoBehaviour
{
    private bool isProp, remoteIsProp;
    private GameObject playerModel;
    public GameObject prop;
    private CharacterController playerCollider;
    private MonoBehaviour firstPersonController, thirdPersonController, followCam;

    private GameObject aimedAt, newItem;
    private bool isLookingAtProp;
	
	private float startTime = 0, holdTime = 0;
    public int timeHold = 1;
    private PhotonView photonView;

    CameraControl camControl;
    PropInfo propInfo;
    public string prefabName;

    private int propID, playerID, newPropID;

    public void Start()
	{
        photonView = GetComponent<PhotonView>();
        isProp = false;
		InvokeRepeating ("Raycast", 0.1f, 0.1f);
        playerModel = this.gameObject.transform.GetChild(1).gameObject;
        playerCollider = this.GetComponent<CharacterController>();
        firstPersonController = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        thirdPersonController = this.GetComponent<ThirdPersonController>();
        followCam = this.GetComponent<CameraControl>();
        camControl = GetComponent<CameraControl>();
        
    }

	public void Update()
	{

        if (Input.GetMouseButtonDown(1) && isProp)
        {
            RevertToPlayer();
        }

        if (isLookingAtProp) {
            if (Input.GetMouseButton(1))
            {
                holdTime += Time.deltaTime;
                playerID = photonView.viewID;
                propID = aimedAt.GetPhotonView().viewID;

                //Turning this off for release 1
                propInfo = aimedAt.GetComponent<PropInfo>();
                prefabName = propInfo.prefabName;
            }
            else holdTime = 0;

            if (holdTime >= timeHold)
            {
                //Call propswitch after timeHold has passed
                PropSwitch(playerID, propID);
			}
		}
    }

    //Use this to call remote client
    [PunRPC]
    public void RemoteSwitch(int playerID, int remoteID, bool remoteIsProp)
    {
        if (remoteIsProp && !photonView.isMine)
        {
            Debug.Log("Called Remote Switch !isProp from client " +photonView.viewID);
            Debug.Log("Recieved Player ID = " + playerID + " remoteID = " + remoteID);

            GameObject newRemoteItem = PhotonView.Find(remoteID).gameObject;
            GameObject remotePlayerModel = PhotonView.Find(playerID).gameObject;
            GameObject robotModel = remotePlayerModel.transform.GetChild(1).gameObject;
            robotModel.SetActive(false);

            newRemoteItem.transform.parent = transform;

            Rigidbody rb = newRemoteItem.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            newRemoteItem.name = "PropModel";
        }

        if (!remoteIsProp && !photonView.isMine)
        {
            Debug.Log("Called remote switch back to player from client " + photonView.viewID);
          
            GameObject newRemoteItem = PhotonView.Find(remoteID).gameObject;
            GameObject remotePlayerModel = PhotonView.Find(playerID).gameObject;
            GameObject robotModel = remotePlayerModel.transform.GetChild(1).gameObject;
            robotModel.SetActive(true);
            //PhotonNetwork.Destroy(newRemoteItem);
        }
    }

    //Handle Player Prop Switching
    public void PropSwitch(int playerID, int propID)
    {
        holdTime = 0;
   
        //Switch from Prop to Prop - currently not implemented
        if (isProp && photonView.isMine)
        {
            Debug.Log("Local Player is transforming into another prop from " +photonView.viewID);

            PhotonNetwork.Destroy(prop);
            newItem = Instantiate(aimedAt, playerModel.transform.position, aimedAt.transform.rotation);

            newItem.transform.parent = transform;
            Rigidbody rb = newItem.GetComponent<Rigidbody>();
            
            rb.isKinematic = true;
            newItem.name = "PropModel";
            camControl.SwitchTarget();

            //Call RPC Function for remote players
            photonView.RPC("RemoteSwitch", PhotonTargets.AllBufferedViaServer, playerID, newPropID, isProp);
        }

        //Switch from Robot to Prop
        if (!isProp && photonView.isMine)
        {
            Debug.Log("Local is player turning into a prop from " +photonView.viewID);
            playerModel.SetActive(false);

            /*
            if (prefabName == null)
            {
                Debug.Log("Unable to Parse Prop info script");
                return;
            }*/
            
            //newItem = PhotonNetwork.Instantiate(aimedAt.name, playerModel.transform.position, aimedAt.transform.rotation, 0);
            //Calculate appropriate y height vaule here
            newItem = PhotonNetwork.Instantiate(prefabName, playerModel.transform.position, aimedAt.transform.rotation, 0);
            newPropID = newItem.GetComponent<PhotonView>().viewID;
            
            newItem.transform.parent = transform;

            Rigidbody rb = newItem.GetComponent<Rigidbody>();
            rb.isKinematic = true;

            firstPersonController.enabled = false;
            thirdPersonController.enabled = true;

            prop = newItem;
            newItem.name = "PropModel";
            isProp = true;

            followCam.enabled = true;
            camControl.SwitchTarget();

            //Call RPC Function for remote players
            photonView.RPC("RemoteSwitch", PhotonTargets.AllBufferedViaServer, playerID, newPropID, isProp);
        }
    }

    private void RevertToPlayer()
    {
        if (photonView.isMine)
        {
            Debug.Log("Local Player is turning back into a robot from " + photonView.viewID);
            playerModel.SetActive(true);
            PhotonNetwork.Destroy(prop);

            followCam.enabled = false;

            thirdPersonController.enabled = false;
            firstPersonController.enabled = true;
            isProp = false;

            //Call RPC Function for remote players
            photonView.RPC("RemoteSwitch", PhotonTargets.AllBufferedViaServer, playerID, newPropID, isProp);
        }
    }

    public void Raycast()
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast(ray, out hit)){
			aimedAt = hit.collider.gameObject;
			if (hit.collider.tag == "Prop" && hit.distance <= 2)
				isLookingAtProp = true;
			else
				isLookingAtProp = false;
		}
	}

	public void OnGUI()
	{
        if (isLookingAtProp) GUI.Box(new Rect(140, Screen.height - 50, Screen.width - 300, 120), "Hold Right Mouse Button to transform into this prop");
	}
}