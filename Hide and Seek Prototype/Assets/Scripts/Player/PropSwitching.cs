﻿using UnityEngine; 
using System.Collections; 

/// <summary>
/// Handles Prop switching and network sync
/// </summary>

public class PropSwitching : MonoBehaviour
{
    private bool isProp;
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

    public int propID, playerID;


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

    public void CallRemoteMethod()
    {
        photonView.RPC("PropSwitch", PhotonTargets.AllBufferedViaServer, playerID, propID);
        photonView.RPC("RevertToPlayer", PhotonTargets.AllBufferedViaServer, playerID);
        photonView.RPC("RemoteSwitch", PhotonTargets.AllBufferedViaServer, playerID, propID);
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

    //DELET THIS
    [PunRPC]
    public void RemoteSwitch(int playerID, int remoteID, bool isProp)
    {
        if (!isProp)
        {
            Debug.Log("Called Remote Switch !isProp");

            GameObject newItem = PhotonView.Find(remoteID).gameObject;
            GameObject remotePlayerModel = PhotonView.Find(playerID).gameObject;

            GameObject robotModel = remotePlayerModel.transform.GetChild(1).gameObject;
            robotModel.SetActive(false);
            //playerCollider.enabled = false;

            newItem.transform.parent = transform;

            Rigidbody rb = newItem.GetComponent<Rigidbody>();
            //rb.detectCollisions = true;
            rb.isKinematic = true;
            newItem.name = "PropModel";
        }

        if (isProp && !photonView.isMine)
        {

        }
    }

    //Handle Player Prop Switching
    [PunRPC]
    public void PropSwitch(int playerID, int remoteID)
    {
        //reset time
        holdTime = 0;
        aimedObj_name = aimedAt.name;

        if (photonView.isMine)
        {
            if (isProp)
            {
                Debug.Log("is prop called while as prop");

                Destroy(prop);
                newItem = Instantiate(aimedAt, playerModel.transform.position, aimedAt.transform.rotation);
                
                newItem.transform.parent = transform;
                Rigidbody rb = newItem.GetComponent<Rigidbody>();
                //rb.detectCollisions = true;
                rb.isKinematic = true;
                newItem.name = "PropModel";
                camControl.SwitchTarget();
                RemoteSwitch(playerID, remoteID, true);
            }
            

        }
        else
        {
            //code for remote player
        }

        if (photonView.isMine)
        {
            if (!isProp)
            {
                Debug.Log("local Callback called");
                playerModel.SetActive(false);
                //playerCollider.enabled = false;


                //newItem = Instantiate(aimedAt, playerModel.transform.position, aimedAt.transform.rotation);
                newItem = PhotonNetwork.Instantiate(aimedAt.name, playerModel.transform.position, aimedAt.transform.rotation, 0);
                
                //disableView.enabled = true;
                newItem.transform.parent = transform;

                Rigidbody rb = newItem.GetComponent<Rigidbody>();
                //rb.detectCollisions = true;
                rb.isKinematic = true;

                firstPersonController.enabled = false;
                thirdPersonController.enabled = true;

                prop = newItem;
                newItem.name = "PropModel";
                isProp = true;

                followCam.enabled = true;
                camControl.SwitchTarget();
                RemoteSwitch(playerID, remoteID, true);
            }
        }
        else
        {
            //code for remote player
            Debug.Log("Called Remote RPC");

            GameObject newItem = PhotonView.Find(remoteID).gameObject;
            GameObject remotePlayerModel = PhotonView.Find(playerID).gameObject;

            GameObject robotModel = remotePlayerModel.transform.GetChild(1).gameObject;
            robotModel.SetActive(false);
            //playerCollider.enabled = false;

            newItem.transform.parent = transform;

            Rigidbody rb = newItem.GetComponent<Rigidbody>();
            //rb.detectCollisions = true;
            rb.isKinematic = true;
            newItem.name = "PropModel";
        }
    }

    [PunRPC]
    private void RevertToPlayer()
    {
        if (photonView.isMine)
        {
            Debug.Log("RevertPlayerCalled");
            playerModel.SetActive(true);
            Destroy(prop);
            
            followCam.enabled = false;
            
            thirdPersonController.enabled = false;
            //playerCollider.enabled = true;
            firstPersonController.enabled = true;

            isProp = false;
        }
        else
        {
            //code for remote player
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