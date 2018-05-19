using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningGun : MonoBehaviour, InventoryItem 
{

	public string Name
	{

		get { 

			return "Lightning Gun";
		}
	}
		


	public Sprite image = null;


	public Sprite Image
	{
		get { 

			return image;
		}
	}


	public void OnPickup()
	{
		// deactive the on screen object
		//gameObject.SetActive (false);
	}
}