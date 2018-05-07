/**************
 * PropSwitching.cs
 * Version 1.0
 * Created by Dion Drake
 * Last Edited: 07/05/2018
*/

//To use - place "Prop" tag/add collider on obj intended to pickup. obj should have collider/rigidbody. Place on MainCamera object.

//Importing namespaces - collections of items
using UnityEngine; 
using System.Collections; 

public class PropSwitching : MonoBehaviour
{
	public KeyCode pickUpKey;
	private GameObject aimedAt, newItem;
	private bool isLookingAtObj, hasAcquiredObj;
	private string aimedObj_name, acquiredObj_name;
	private float startTime = 0, holdTime = 0;
	//Runs on script load
	public void Start()
	{
		InvokeRepeating ("Raycast", 0.1f, 0.1f);
	}

	public void Update()
	{
		if (isLookingAtObj) {
			if (Input.GetMouseButton (1)) {
				holdTime += Time.deltaTime;
			}
			if (holdTime > 5) {
				holdTime = 0;
				aimedObj_name = aimedAt.name;
				newItem = Instantiate (aimedAt, transform.position + (transform.forward * 2), transform.rotation); 
				newItem.name = aimedObj_name; //To remove "(Clone)" from the end of new obj
			}
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