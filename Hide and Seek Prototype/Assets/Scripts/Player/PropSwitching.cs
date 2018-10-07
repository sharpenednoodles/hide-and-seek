using UnityEngine; 
using System.Collections; 

/// <summary>
/// Handles Prop switching and network sync
/// To USE
/// Props to switch into must be tagged with a prop tag, and have a prop script attached, with the label matching the one found in resources 
/// </summary>

public class PropSwitching : MonoBehaviour
{
    private bool isProp, remoteIsProp;
    private GameObject playerModel;
    public GameObject prop;
    private CharacterController playerCollider;
    private MonoBehaviour firstPersonController, thirdPersonController, followCam;
    private PlayerNetwork playerNetwork;
    private HideSeek.WeaponController.WeaponController weaponController;
    private InteractionController interactionController;

    private GameObject aimedAt, newItem, weapons;
    private bool isLookingAtProp, debug = true;
	
	private float startTime = 0, holdTime = 0;
    public int timeHold = 1;
    private PhotonView photonView;
    

    CameraControl camControl;
    //PropInfo propInfo;
    //public string prefabName;

    private int playerID, newPropID;

    public void Start()
	{
        //Get game objects
        photonView = GetComponent<PhotonView>();
        isProp = false;
        //Only want this on our local player
        /*
        if (photonView.isMine)
		    InvokeRepeating ("Raycast", 0.1f, 0.1f);
        */
        playerModel = gameObject.transform.GetChild(1).gameObject;
        weapons = gameObject.transform.GetChild(3).gameObject;
        playerCollider = GetComponent<CharacterController>();
        firstPersonController = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        followCam = GetComponent<CameraControl>();
        camControl = GetComponent<CameraControl>();
        playerNetwork = GetComponent<PlayerNetwork>();
        interactionController = GetComponent<InteractionController>();
        weaponController = GetComponent<HideSeek.WeaponController.WeaponController>();
    }

    public void Update()
    {
        //Exit Prop Form
        if (Input.GetMouseButtonDown(1) && isProp)
        {
            playerID = photonView.viewID;
            RevertToPlayer();
        }
    }

  

    //Use this to call remote client
    [PunRPC]
    public void RemoteSwitch(int playerID, int remoteID, bool remoteIsProp)
    {
        if (remoteIsProp && !photonView.isMine)
        {
            if (debug)
            {
                Debug.Log("Called Remote Switch !isProp from client " + photonView.viewID);
                Debug.Log("Recieved Player ID = " + playerID + " remoteID = " + remoteID);
            }
            GameObject newRemoteItem = PhotonView.Find(remoteID).gameObject;
            GameObject remotePlayerModel = PhotonView.Find(playerID).gameObject;
            GameObject robotModel = remotePlayerModel.transform.GetChild(1).gameObject;
            GameObject weaponRemote = remotePlayerModel.transform.GetChild(3).gameObject;
            robotModel.SetActive(false);
            weaponRemote.SetActive(false);
            

            newRemoteItem.transform.parent = transform;
            //If no rigidbody, add one
            if (newRemoteItem.GetComponent<Rigidbody>() == null)
            {
                newRemoteItem.AddComponent<Rigidbody>();
            }

            Rigidbody rb = newRemoteItem.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            newRemoteItem.name = "PropModel";
            newRemoteItem.AddComponent<HealthPointer>();
        }

        if (!remoteIsProp && !photonView.isMine)
        {
            if (debug)
                Debug.Log("Called remote switch back to player from client " + photonView.viewID);
          
            GameObject newRemoteItem = PhotonView.Find(remoteID).gameObject;
            GameObject remotePlayerModel = PhotonView.Find(playerID).gameObject;
            GameObject robotModel = remotePlayerModel.transform.GetChild(1).gameObject;
            GameObject weaponRemote = remotePlayerModel.transform.GetChild(3).gameObject;
            robotModel.SetActive(true);
            weaponRemote.SetActive(true);
            //PhotonNetwork.Destroy(newRemoteItem);
        }
    }

    //Handle Player Prop Switching
    public void PropSwitch(int playerID, string prefabName, float aimedAtYRot)
    {
        //holdTime = 0;
 
   
        //Switch from Prop to Prop - CURRENTLY NOT IN USE
        if (isProp && photonView.isMine)
        {
            if (debug)
                Debug.Log("Local Player is transforming into another prop from " +photonView.viewID);

            PhotonNetwork.Destroy(prop);
            newItem = Instantiate(aimedAt, playerModel.transform.position, Quaternion.Euler(-90, aimedAtYRot, 0));

            newItem.transform.parent = transform;
            //If no rigidbody, add one
            if (newItem.GetComponent<Rigidbody>() == null)
            {
                newItem.AddComponent<Rigidbody>();
            }
            Rigidbody rb = newItem.GetComponent<Rigidbody>();
            
            rb.isKinematic = true;
            newItem.name = "PropModel";
            newItem.AddComponent<HealthPointer>();
            weaponController.PropMode(true);
            camControl.SwitchTarget();

            //Call RPC Function for remote players
            //KNOWN ISSUES
            //When a player joins a game in progress, and a player has turned into a prop, the prop will spawn on the new players machine where the player originally transformed
            photonView.RPC("RemoteSwitch", PhotonTargets.AllBuffered, playerID, newPropID, isProp);
        }

        //Switch from Robot to Prop
        if (!isProp && photonView.isMine)
        {
            if (debug)
                Debug.Log("Local is player turning into a prop from " +photonView.viewID);
            playerModel.SetActive(false);
            weapons.SetActive(false);
            
            //newItem = PhotonNetwork.Instantiate(aimedAt.name, playerModel.transform.position, aimedAt.transform.rotation, 0);
            //Todo - Calculate appropriate y height vaule here (or just read in from a script)
            newItem = PhotonNetwork.Instantiate(prefabName, playerModel.transform.position, Quaternion.Euler(-90, aimedAtYRot, 0), 0);
           
            newPropID = newItem.GetComponent<PhotonView>().viewID;
            
            newItem.transform.parent = transform;

            //if no rigidbody, add one
            if (newItem.GetComponent<Rigidbody>() == null)
            {
                newItem.AddComponent<Rigidbody>();
            }
            Rigidbody rb = newItem.GetComponent<Rigidbody>();
            rb.isKinematic = true;

            firstPersonController.enabled = false;
            thirdPersonController.enabled = true;

            prop = newItem;
            newItem.name = "PropModel";
            newItem.AddComponent<HealthPointer>();
            isProp = true;
            weaponController.PropMode(true);

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
            if (debug)
                Debug.Log("Local Player is turning back into a robot from " + photonView.viewID);
            playerModel.SetActive(true);
            weapons.SetActive(true);
            PhotonNetwork.Destroy(prop);

            followCam.enabled = false;

            thirdPersonController.enabled = false;
            firstPersonController.enabled = true;
            isProp = false;
            weaponController.PropMode(false);
            playerNetwork.HideHead();
  
            //Call RPC Function for remote players
            photonView.RPC("RemoteSwitch", PhotonTargets.AllBufferedViaServer, playerID, newPropID, isProp);
        }
    }


}