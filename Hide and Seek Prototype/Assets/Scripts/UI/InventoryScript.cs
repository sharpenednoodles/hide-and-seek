using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventoryScript : MonoBehaviour {




	public Inventory Inventory;
	// Use this for initialization
	void Start () {

		Inventory.ItemAdded += InventoryScript_ItemAdded;
		Debug.Log ("Call from Inventory Script Start");
	}


	// Inventory system related code
	private void InventoryScript_ItemAdded(object sender, InventoryEventArgs e)

	{

		Debug.Log ("Call from Inventory Script Item added");
		Transform InventoryPanel = transform.Find("InventoryPanel");
		foreach (Transform slot in InventoryPanel) 
		{
			Image image = slot.GetChild (0).GetChild (0).GetComponent<Image> ();


			if (!image.enabled) {
				image.enabled = true;
				image.sprite = e.Item.Image;

				//to do store a reference to the item

				break;
			}
		}
	}




}
