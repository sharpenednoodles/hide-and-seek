/**************
 * ItemGen.cs
 * Version 1.0
 * Created by Dion Drake
 * Last Edited: 03/03/2018
*/

//Instructions: Must be as many GameObjects declared as there are intended props. In Inspector, set each GameObject to an intended item. Now add a case to the 
//nested if-else, e.g. if (rndNumGen == 3), if (rndNumGen == 4). The purpose of the second line in each statement is to immediately rename it upon instantiation, as 
//it will be given a name like "Cube(Clone)" whilst we just want it to be named "Cube".

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemGen : MonoBehaviour {
	public GameObject chosenObjToGen;
	public GameObject objToGen1;
	public GameObject objToGen2;

	void Start () {
		Random rnd = new Random();
		int rndNumGen = Random.Range(1,3);
		if (rndNumGen == 1) {
			chosenObjToGen = Instantiate (objToGen1, transform.position + (transform.forward * 2), transform.rotation);
			chosenObjToGen.name = objToGen1.name;
		} else 
			if (rndNumGen == 2) {
			chosenObjToGen = Instantiate (objToGen2, transform.position + (transform.forward * 2), transform.rotation);
			chosenObjToGen.name = objToGen2.name;
		}
		//else if continue here for more GameObjects
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
