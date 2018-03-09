/**************
 * PickupController.cs
 * Version 1.1
 * Created by Dion Drake
 * Last Edited: 08/03/2018
*/

//To use - place "Pickup" tag/add collider on obj intended to pickup. obj should have collider/rigidbody. Place on MainCamera object.

//Importing namespaces - collections of items
using UnityEngine; 
using System.Collections; 

public class PickupController : MonoBehaviour
{
	private GameObject aimedAt;
	private bool isLookingAtObj, hasAcquiredObj;
	private string aimedObj_name, acquiredObj_name;
	private float startTime = 0;
	//Runs on script load
	public void Start()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.None;
		InvokeRepeating ("Raycast", 0.1f, 0.1f);
	}

	public void Update()
	{
		if(isLookingAtObj) {
			if (Input.GetKeyDown ("e")) {
				acquiredObj_name = aimedAt.name;
				Destroy (aimedAt);
				hasAcquiredObj = true;
				isLookingAtObj = false;
			}
		}
	}

	public void Raycast()
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast(ray, out hit)){
			aimedAt = hit.collider.gameObject;
			if (hit.collider.tag == "Pickup" && hit.distance <= 2 && aimedAt.name != "Terrain") {
					isLookingAtObj = true;
			}
			else
			{
				isLookingAtObj = false;
			}
		}
	}

	public void OnGUI()
	{
		if (isLookingAtObj) GUI.Box(new Rect (140, Screen.height - 50, Screen.width - 300, 120), "Press E to acquire " + aimedAt.name);
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