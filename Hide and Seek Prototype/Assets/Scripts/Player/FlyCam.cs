/**************
 * FlyCam.cs
 * Version 1.0
 * Based on code written by Unity users Windex, Ellandar, LookForward and AngryBoy
 * Source: http://forum.unity3d.com/threads/fly-cam-simple-cam-script.67042/
 * Last Edited: 18/04/2018
*/

using System.Collections;
using UnityEngine;

public class FlyCam : MonoBehaviour {
	public float mainSpeed = 5.0f, shiftAdd = 25.0f, maxShift = 100.0f, camSens = 0.25f, totalRun= 1.0f, speedMultiplier, rotationY = 0.0f, mouseSensitivity = 3.0f; 
	//private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)

	private bool isRotating = true, isColliding = false; // Angryboy: Can be called by other things (e.g. UI) to see if camera is rotating

	//micah_3d: added so camera will be able to collide with world objects if users chooses
	//physic material added to keep camera from spinning out of control if it hits a corner or multiple colliders at the same time.  
	PhysicMaterial myMaterial;

    void Start()
    {
        if (isColliding)
        {
            myMaterial = new PhysicMaterial("ZeroFriction");
            myMaterial.dynamicFriction = myMaterial.staticFriction = myMaterial.bounciness = 0f;
            myMaterial.frictionCombine = PhysicMaterialCombine.Multiply;
            myMaterial.bounceCombine = PhysicMaterialCombine.Average;
            gameObject.AddComponent<CapsuleCollider>();
            gameObject.GetComponent<CapsuleCollider>().radius = 1f;
            gameObject.GetComponent<CapsuleCollider>().height = 1.68f;
            gameObject.GetComponent<CapsuleCollider>().material = myMaterial;

            gameObject.AddComponent<Rigidbody>();
            gameObject.GetComponent<Rigidbody>().useGravity = false;
        }
    }
	void Update () {

		
        //Disabled Nouse Hold
        /*
		if (Input.GetMouseButtonDown (1)) {
			isRotating = true;
		}
		if (Input.GetMouseButtonUp (1)) {
			isRotating = false;
		}
        */
		if (isRotating) {
			// Made by LookForward
			// Angryboy: Replaced min/max Y with numbers, not sure why we had variables in the first place
			float rotationX = transform.localEulerAngles.y + Input.GetAxis ("Mouse X") * mouseSensitivity;
			rotationY += Input.GetAxis ("Mouse Y") * mouseSensitivity;
			rotationY = Mathf.Clamp (rotationY, -90, 90);
			transform.localEulerAngles = new Vector3 (-rotationY, rotationX, 0.0f);
		}

		//Keyboard commands
		//float f = 0.0f;
		Vector3 p = GetBaseInput();
		if (Input.GetKey (KeyCode.LeftShift)){
			totalRun += Time.deltaTime;
			p  = p * totalRun * shiftAdd;
			p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
			p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
			p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
			// Angryboy: Use these to ensure that Y-plane is affected by the shift key as well
			speedMultiplier = totalRun * shiftAdd * Time.deltaTime;
			speedMultiplier = Mathf.Clamp(speedMultiplier, -maxShift, maxShift);
		}
		else{
			totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
			p = p * mainSpeed;
			speedMultiplier = mainSpeed * Time.deltaTime; // Angryboy: More "correct" speed
		}

		p = p * Time.deltaTime;

		// Angryboy: Removed key-press requirement, now perma-locked to the Y plane
		Vector3 newPosition = transform.position;//If player wants to move on X and Z axis only
		transform.Translate(p);
		newPosition.x = transform.position.x;
		newPosition.z = transform.position.z;

		// Angryboy: Manipulate Y plane by using Q/E keys
		if (Input.GetKey (KeyCode.Q)){
			newPosition.y += -speedMultiplier;
		}
		if (Input.GetKey (KeyCode.E)){
			newPosition.y += speedMultiplier;
		}

		transform.position = newPosition;
	}

	// Angryboy: Can be called by other code to see if camera is rotating
	// Might be useful in UI to stop accidental clicks while turning?
	public bool amIRotating(){
		return isRotating;
	}

	private Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
		Vector3 p_Velocity = new Vector3();
		if (Input.GetKey (KeyCode.W)){
			p_Velocity += new Vector3(0, 0 , 1);
		}
		if (Input.GetKey (KeyCode.S)){
			p_Velocity += new Vector3(0, 0, -1);
		}
		if (Input.GetKey (KeyCode.A)){
			p_Velocity += new Vector3(-1, 0, 0);
		}
		if (Input.GetKey (KeyCode.D)){
			p_Velocity += new Vector3(1, 0, 0);
		}
		return p_Velocity;
	}
}