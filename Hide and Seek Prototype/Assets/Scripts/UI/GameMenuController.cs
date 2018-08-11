using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles switching between canvases and also pause menu buttons 
/// (becasue I'm lazy and didn't put them in a seperate script)
/// </summary>

public class GameMenuController : MonoBehaviour {

	public GameObject menuCanvas;



	//public Transform Menu;
    //we make this static so we can access from controller(s)
	public static bool MenuState = false;
    //public CanvasGroup menuGroup;

	//public GameObject Menu;

	// Use this for initialization
	void Start ()
    {
		//menuGroup.alpha = 1f;
		//menuGroup.blocksRaycasts = true;
		//Menu.gameObject.SetActive (false);
		menuCanvas.SetActive(false);

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
