using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles switching between canvases and also pause menu buttons 
/// (becasue I'm lazy and didn't put them in a seperate script)
/// NEEDS REWRITING
/// </summary>

public class GameMenuController : MonoBehaviour {

	public GameObject menuCanvas;
    [SerializeField] private bool debug = true;
    //we make this static so we can access from controller(s)
	public static bool MenuState = false;
   

	// Use this for initialization
	void Start ()
    {
		//Disable Menu on Game start
		menuCanvas.SetActive(false);
	}

	public void ShowMenu()
	{
		menuCanvas.SetActive(true);
        if(debug)
            Debug.Log ("Set Active");
		MenuState = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


	public void HideMenu()
	{
		menuCanvas.gameObject.SetActive (false);
        MenuState = false;
        if (debug)
		    Debug.Log ("Set Disable");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //menuGroup.blocksRaycasts = true;
    }


	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
			if (MenuState)
			{
				HideMenu();
                if (debug)
				    Debug.Log ("Close Pause Menu");
			} 
			else 
			{ 
				ShowMenu ();
                if (debug)
				    Debug.Log ("Open Pause Menu");
			}
		}
	}
