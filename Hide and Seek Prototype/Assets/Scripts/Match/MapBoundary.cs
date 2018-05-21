/**************
 * MapBoundary.cs
 * Version 1.0
 * Created by Dion Drake
 * Created: 21/05/18
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBoundary : MonoBehaviour {
public bool playerInBoundary;
public string seconds = "";
public GameObject boundarySphere;
float TimeLeft = 5.0f, TimeLimit = 0.0f, roundTime = 20;

	// Update is called once per frame
	public void Update () 
	{
		if (!playerInBoundary) 
			if (TimeLimit < TimeLeft) 
				TimeLeft -= Time.deltaTime;
		if (TimeLeft < 0) {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			Time.timeScale = 0;
		}
		roundTime -= Time.deltaTime;
	}

	public void FixedUpdate()
	{
		if (roundTime > 10)
		boundarySphere.transform.localScale -= new Vector3 (0.1f, 0.1f, 0.1f);
	}

	public void OnTriggerEnter (Collider other)
	{
		if (other.gameObject.tag == "Boundary") 
		{
			playerInBoundary = true;
			TimeLeft = 5.0f;
		} 
	}
		
	public void OnTriggerExit (Collider other)
	{
		playerInBoundary = false;
	}

	public void OnGUI() 
	{
		if (!playerInBoundary) {
			string seconds = Mathf.Floor(TimeLeft % 60).ToString("00");
			GUI.Box (new Rect (140, Screen.height - 50, Screen.width - 300, 120), "Return to game area in " + seconds + " seconds");
		}
	}
}
