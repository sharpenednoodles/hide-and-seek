using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameMenuController : MonoBehaviour {

	public GameObject menuCanvas;

	public Inventory Inventory;
	//public Transform Menu;
    //we make this static so we can access from controller(s)
	public static bool MenuState = false;
    //public CanvasGroup menuGroup;

	//public GameObject Menu;

	// Use this for initialization
	void Start () {
		//menuGroup.alpha = 1f;
		//menuGroup.blocksRaycasts = true;
		//Menu.gameObject.SetActive (false);
		menuCanvas.SetActive(false);
		Inventory.ItemAdded += InventoryScript_ItemAdded;
	}

	// Inventory system related code
	private void InventoryScript_ItemAdded(object sender, InventoryEventArgs e)

	{
		Transform InventoryPanel = transform.Find("InventoryPanel");
		foreach (Transform slot in InventoryPanel) {
			Image image = slot.GetChild (0).GetChild (0).GetComponent<Image> ();
		

			if (!image.enabled) {
				image.enabled = true;
				image.sprite = e.Item.Image;

				//to do store a reference to the item

				break;
			}
		}
	}



	public void ShowMenu()
	{
		menuCanvas.SetActive(true);
        //menuGroup.alpha = 1f;
        //menuGroup.blocksRaycasts = true;
        Debug.Log ("Set Active");
		MenuState = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }



	public void HideMenu()
	{
		menuCanvas.gameObject.SetActive (false);
        //Menu.SetActive(false);
        //menuGroup.alpha = 0f;
        //menuGroup.blocksRaycasts = true;
        MenuState = false;
		Debug.Log ("Set Disable");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

	void Update () {
		//Debug.Log ("refresh key");
		if (Input.GetKeyDown(KeyCode.Escape))
			if (MenuState)
			{
				HideMenu();
				Debug.Log ("Call From hide");
			} 
			else 
			{ 
				ShowMenu ();
				Debug.Log ("Call From show");
			}
		}
	}
