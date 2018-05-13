using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour {

	public CanvasGroup menuGroup;
	public Transform Menu;
	public bool MenuState = false;

	//public GameObject Menu;

	// Use this for initialization
	void Start () {
		menuGroup.alpha = 1f;
		menuGroup.blocksRaycasts = true;
		//Menu.gameObject.SetActive (false);
		//Menu.SetActive(false);
		MenuState = false;
	}


//	void updateMenu(){
//
//		if (Input.GetKeyDown(KeyCode.M)) {
//
//
//						if (Menu.gameObject.activeSelf == false) {
//
//			me
//			Menu.gameObject.SetActive (true);
//			Debug.Log ("Set Active");
//						} 
//						else {
//			
//							Menu.gameObject.SetActive (false);
//							Debug.Log ("Disable");
//						}
//		}
//
//	}

	void showMenu()
	{
		//Menu.SetActive(true);
		menuGroup.alpha = 1f;
		menuGroup.blocksRaycasts = true;
		Debug.Log ("Set Active");
		MenuState = true;
	}

	void hideMenu()
	{
		//Menu.gameObject.SetActive (false);
		//Menu.SetActive(false);
		menuGroup.alpha = 0f;
		menuGroup.blocksRaycasts = true;
		MenuState = false;
		Debug.Log ("Set Disable");
	}

	// Update is called once per frame
	void Update () {
		Debug.Log ("refresh key");
		if (Input.GetKeyDown(KeyCode.Escape))
			if (MenuState)
			{
				hideMenu();
				Debug.Log ("Call From hide");
			} 
			else 
			{ 
				showMenu ();
				Debug.Log ("Call From show");
			}
			
		}
	}
