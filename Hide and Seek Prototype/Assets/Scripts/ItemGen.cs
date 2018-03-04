/**************
 * ItemGen.cs
 * Version 1.0
 * Created by Dion Drake
 * Last Edited: 04/03/2018
*/

//Instructions: Must be as many GameObjects declared as there are intended pickups. In Inspector, set each GameObject to an intended item. Now add a case to the 
//nested if-else, e.g. if (rndNumGen == 3), if (rndNumGen == 4). The purpose of the second line in each statement is to immediately rename it upon instantiation, as 
//it will be given a name like "Cube(Clone)" whilst we just want it to be named "Cube".

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGen : MonoBehaviour {
	//Must be one "objToGen" GameObject for each item that is intended to spawn
	public GameObject chosenObjToGen, objToGen1, objToGen2;

	void Start () {
		//Playing with byte. Less mem usage and longer computation time (everything converted to int for calculations) anyway, hence typecast required.
		byte rndNumGen = (byte)Random.Range(1,3);
		if(rndNumGen==1){
			chosenObjToGen = Instantiate (objToGen1, transform.position + (transform.forward * 2), transform.rotation);
			chosenObjToGen.name = objToGen1.name;
		}else 
			if(rndNumGen==2){
			chosenObjToGen = Instantiate (objToGen2, transform.position + (transform.forward * 2), transform.rotation);
			chosenObjToGen.name = objToGen2.name;
		}
		//else if's continue here for more GameObjects
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
