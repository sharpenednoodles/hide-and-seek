/**************
 * PropSwitching.cs
 * Version 1.0
 * Created by Dion Drake
 * Last Edited: 26/05/2018
 * 
*/

using UnityEngine; 
using System.Collections; 

public class PropSwitching : MonoBehaviour
{
    //CameraControl cameraHook;
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



    public void Start()
	{
        photonView = GetComponent<PhotonView>();
        isProp = false;
		InvokeRepeating ("Raycast", 0.1f, 0.1f);
        playerModel = this.gameObject.transform.GetChild(1).gameObject;
        playerCollider = this.GetComponent<CharacterController>();
        firstPersonController = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        thirdPersonController = this.GetComponent<PropPlayerController>();
        followCam = this.GetComponent<CameraControl>();
        camControl = GetComponent<CameraControl>();
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
                PropSwitch();
			}
			/*if (holdTime > 5)
            {
                PropSwitch();
			}*/
		}

       
	}

    //Handle Player Prop Switching
    public void PropSwitch()
    {
        //reset time
        holdTime = 0;
        aimedObj_name = aimedAt.name;

        if (isProp)
        {
            if (photonView.isMine)
            {
                Debug.Log("is prop called while as prop");

                Destroy(prop);
                newItem = Instantiate(aimedAt, playerModel.transform.position, aimedAt.transform.rotation);
                newItem.transform.parent = transform;
                Rigidbody rb = newItem.GetComponent<Rigidbody>();
                rb.detectCollisions = true;
                newItem.name = "PropModel";
                camControl.SwitchTarget();
            }
            else
            {
                //code for remoate player
            }

        }

        if (!isProp)
        {
            if (photonView.isMine)
            {
                playerModel.SetActive(false);
                playerCollider.enabled = false;

                newItem = Instantiate(aimedAt, playerModel.transform.position, aimedAt.transform.rotation);
                newItem.transform.parent = transform;

                Rigidbody rb = newItem.GetComponent<Rigidbody>();
                rb.detectCollisions = true;

                firstPersonController.enabled = false;
                thirdPersonController.enabled = true;

                prop = newItem;
                newItem.name = "PropModel";
                isProp = true;

                followCam.enabled = true;
                camControl.SwitchTarget();
            }
            else
            {
                //code for remoate player
            }

        }

        
        }


    private void RevertToPlayer()
    {
        if (photonView.isMine)
        {
            Debug.Log("RevertPlayerCalled");
            playerModel.SetActive(true);
            Destroy(prop);
            
            followCam.enabled = false;
            
            thirdPersonController.enabled = false;
            playerCollider.enabled = true;
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
			if (hit.collider.tag == "Prop" && hit.distance <= 2 && aimedAt.name != "Terrain")
				isLookingAtObj = true;
			else
				isLookingAtObj = false;
		}
	}

	public void OnGUI()
	{
		if (isLookingAtObj) GUI.Box(new Rect (140, Screen.height - 50, Screen.width - 300, 120), "Hold Right Mouse Button to switch to " + aimedAt.name);
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