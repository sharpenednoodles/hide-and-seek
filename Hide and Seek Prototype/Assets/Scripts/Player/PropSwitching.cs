using UnityEngine; 
using System.Collections; 

/// <summary>
/// Handles Prop switching and network sync
/// </summary>

public class PropSwitching : MonoBehaviour
{
    private bool isProp, remoteIsProp;
    private GameObject playerModel;
    public GameObject prop;
    private CharacterController playerCollider;
    private MonoBehaviour firstPersonController, thirdPersonController, followCam;

    private GameObject aimedAt, newItem;
    private bool isLookingAtObj, hasAcquiredObj;
	private string aimedObj_name, acquiredObj_name;
	private float startTime = 0, holdTime = 0;
    private PhotonView photonView;

    CameraControl camControl;

    public int propID, playerID, newPropID;


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

    //probably unneeded, this is for continuous streams
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(propID);
            stream.SendNext(playerID);
            stream.SendNext(newPropID);
        }
        else
        {
            propID = (int)stream.ReceiveNext();
            playerID = (int)stream.ReceiveNext();
            newPropID = (int)stream.ReceiveNext();
        }
    }
    public void CallRemoteMethod()
    {
        //photonView.RPC("PropSwitch", PhotonTargets.AllBufferedViaServer, playerID, propID);
        photonView.RPC("RevertToPlayer", PhotonTargets.AllBufferedViaServer, playerID);
        photonView.RPC("RemoteSwitch", PhotonTargets.AllBufferedViaServer, playerID, newPropID, remoteIsProp);
    }

	public void Update()
	{

        if (Input.GetMouseButtonDown(1) && isProp)
        {
            RevertToPlayer();
        }

        if (isLookingAtObj) {
			if (Input.GetMouseButtonDown (1))
            {
                //holdTime += Time.deltaTime;
                playerID = photonView.viewID;
                propID = aimedAt.GetPhotonView().viewID;
                PropSwitch(playerID, propID);

			}
			/*if (holdTime > 5)
            {
                PropSwitch();
			}*/
		}
	}

    //Use this to call remote client
    [PunRPC]
    public void RemoteSwitch(int playerID, int remoteID, bool remoteIsProp)
    {
        if (!remoteIsProp && !photonView.isMine)
        {
            Debug.Log("Called Remote Switch !isProp from client " +photonView.viewID);
            Debug.Log("Recieved Player ID = " + playerID + " remoteID = " + remoteID);

            GameObject newRemoteItem = PhotonView.Find(remoteID).gameObject;
            GameObject remotePlayerModel = PhotonView.Find(playerID).gameObject;

            GameObject robotModel = remotePlayerModel.transform.GetChild(1).gameObject;
            robotModel.SetActive(false);
            //playerCollider.enabled = false;

            newRemoteItem.transform.parent = transform;

            Rigidbody rb = newRemoteItem.GetComponent<Rigidbody>();
            //rb.detectCollisions = true;
            rb.isKinematic = true;
            newRemoteItem.name = "PropModel";

        }

        if (remoteIsProp)
        {
            Debug.Log("Called remote switch back to player from client " + photonView.viewID);
            //Implemmentation here
            GameObject newRemoteItem = PhotonView.Find(remoteID).gameObject;
            GameObject remotePlayerModel = PhotonView.Find(playerID).gameObject;
            GameObject robotModel = remotePlayerModel.transform.GetChild(1).gameObject;
            robotModel.SetActive(true);

            PhotonNetwork.Destroy(newRemoteItem);

        }
    }

    //Handle Player Prop Switching

    public void PropSwitch(int playerID, int propID)
    {
        //reset time
        holdTime = 0;
        aimedObj_name = aimedAt.name;


        //Still needs rewriting, mark when done
        if (isProp && photonView.isMine)
        {
            Debug.Log("Local Player is transforming into another prop from " +photonView.viewID);

            PhotonNetwork.Destroy(prop);
            newItem = Instantiate(aimedAt, playerModel.transform.position, aimedAt.transform.rotation);

            newItem.transform.parent = transform;
            Rigidbody rb = newItem.GetComponent<Rigidbody>();
            //rb.detectCollisions = true;
            rb.isKinematic = true;
            newItem.name = "PropModel";
            camControl.SwitchTarget();
            //RemoteSwitch(playerID, propID, true);
        }

        //Done
        if (!isProp && photonView.isMine)
        {
            Debug.Log("Local is player turning into a prop from " +photonView.viewID);
            playerModel.SetActive(false);
            //playerCollider.enabled = false;


            //newItem = Instantiate(aimedAt, playerModel.transform.position, aimedAt.transform.rotation);
            newItem = PhotonNetwork.Instantiate(aimedAt.name, playerModel.transform.position, aimedAt.transform.rotation, 0);
            newPropID = newItem.GetComponent<PhotonView>().viewID;
            //disableView.enabled = true;
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
            remoteIsProp = false;

            //Call RPC Function for remote players
            photonView.RPC("RemoteSwitch", PhotonTargets.AllBufferedViaServer, playerID, newPropID, remoteIsProp);
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
            //playerCollider.enabled = true;
            firstPersonController.enabled = true;

            isProp = false;
        }
    }

    public void Raycast()
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast(ray, out hit)){
			aimedAt = hit.collider.gameObject;
			if (hit.collider.tag == "Prop" && hit.distance <= 2)
				isLookingAtObj = true;
			else
				isLookingAtObj = false;
		}
	}

	public void OnGUI()
	{
		if (isLookingAtObj) GUI.Box(new Rect (140, Screen.height - 50, Screen.width - 300, 120), "Hold Right Mouse Button to transform into this prop");
		if (hasAcquiredObj) {
			GUI.Box(new Rect (140, Screen.height - 50, Screen.width - 300, 120), "Acquired " + acquiredObj_name);
			if (startTime < 5) startTime += Time.deltaTime;
			else {
				hasAcquiredObj = false;
				startTime = 0;
			}
		}
	}
}