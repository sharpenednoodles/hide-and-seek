/**************
 * MapBoundary.cs
 * Version 1.0
 * Created by Dion Drake
 * Created: 20/08/18
*/
/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneLimitation : MonoBehaviour {
public bool playerInBoundary, z1shut, z2shut, z3shut, z4shut;
public string seconds = "";
	public Animator z1;
public GameObject boundarySphere;
	float TimeLeft = 5.0f, TimeLimit = 0.0f, roundTime = 300.0f, z1seal, z2seal, z3seal, z4seal;

	public void Start()
	{
		z1seal = roundTime - roundTime / 5;
		z2seal = roundTime - (roundTime / 5) * 2; //Seal zone 2 at 180 seconds
		z3seal = roundTime - (roundTime/5) * 3; //Seal zone 3 at 120 seconds
		z4seal = roundTime - (roundTime/5) * 4; //Seal zone 4 at 60 seconds
		//z1.enabled = false;
	}

	// Update is called once per frame
	public void Update () 
	{
        /*
        if (!playerInBoundary) 
			if (TimeLimit < TimeLeft) 
				TimeLeft -= Time.deltaTime;
		if (TimeLeft < 0) {
			//Cursor.visible = true;
			//Cursor.lockState = CursorLockMode.None;
			//Time.timeScale = 0;
		}
		roundTime -= Time.deltaTime;
		//z1.Play ("Doorbone|Doorbone Action");
        
	}

	public void FixedUpdate()
	{
        /*
		if (roundTime > 10) {
			boundarySphere.transform.localScale -= new Vector3 (0.1f, 0.1f, 0.1f);
		}

		if (roundTime <= z4seal) { //If zero zones open
			if (roundTime <= z3seal) { //If one zone open
				if (roundTime <= z3seal) { //If two zones open
					if (roundTime <= z1seal) { //If three zones open
						shutZone();
					}
				}
			}
		}
        
	}

	public void shutZone()
	{
		
		//z1 = GetComponent<Animator>();
		//z1.Stop();

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
        /*
		if (!playerInBoundary) {
			string seconds = Mathf.Floor(TimeLeft % 60).ToString("00");
			GUI.Box (new Rect (140, Screen.height - 50, Screen.width - 300, 120), "Return to game area in " + seconds + " seconds");
		}
        
    }

}
*/