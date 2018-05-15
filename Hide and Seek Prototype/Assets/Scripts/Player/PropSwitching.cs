/**************
 * PropSwitching.cs
 * Version 1.0
 * Created by Dion Drake
 * Last Edited: 13/05/2018
 * 
*/

//To use - place "Prop" tag/add collider on obj intended to pickup. obj should have collider/rigidbody. Place on MainCamera object.


using UnityEngine; 
using System.Collections; 

public class PropSwitching : MonoBehaviour
{
    public bool isProp;
	public KeyCode pickUpKey;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private CharacterController playerCollider;
    [SerializeField] private MonoBehaviour firstPersonController, thirdPersonController, followCam;

    private GameObject aimedAt, newItem, self;
    private bool isLookingAtObj, hasAcquiredObj;
	private string aimedObj_name, acquiredObj_name;
	private float startTime = 0, holdTime = 0;
	
    
    //public Camera propCam;
	//Runs on script load
	public void Start()
	{
        isProp = false;
		InvokeRepeating ("Raycast", 0.1f, 0.1f);
		self = GameObject.Find ("CapsulePlayer");

	}

	public void Update()
	{
		if (isLookingAtObj) {
			if (Input.GetMouseButtonDown (1)) {
                //holdTime += Time.deltaTime;
                PropSwitch();
			}
			/*if (holdTime > 5)
            {
                PropSwitch();
			}*/
		}
	}

    public void PropSwitch()
    {
        //reset time
        holdTime = 0;
        aimedObj_name = aimedAt.name;
  
        //new object
        //playerCollider.enabled = false;
        //firstPersonController.enabled = false;
        //thirdPersonController.enabled = true;
        if (!isProp)
        {
            playerModel.SetActive(false);
            playerCollider.enabled = false;
            newItem = Instantiate(aimedAt, playerModel.transform.position, aimedAt.transform.rotation);
            newItem.transform.parent = transform;
            followCam.enabled = true;
            playerModel.SetActive(false);
            Rigidbody rb = newItem.GetComponent<Rigidbody>();
            rb.detectCollisions = true;
            rb.isKinematic = true;

            firstPersonController.enabled = false;
            thirdPersonController.enabled = true;

            //To remove "(Clone)" from the end of new obj name
            newItem.name = "PropModel";
            isProp = true;
            
        }

        if (isProp)
        {
            //stuff for when already a prop
        }

            

            //Destroy(aimedAt); //Destroys original prop so I don't need to deal with the problem of the old and new objects colliding
            //newItem.GetComponent<Camera>().enabled = true; //GetComponent is an intensive method that iterates through all gameObjects. This version should just iterate through children.
            //newItem.GetComponentInChildren<ThirdPersonController>().enabled = true;
            //self.GetComponentInChildren<FirstPersonController> ().enabled = false;
            //transform.parent = newItem.transform;

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