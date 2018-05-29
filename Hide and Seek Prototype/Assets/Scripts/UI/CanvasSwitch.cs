using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// This script is used to trigger switching between different canvases.
/// It also runs the methods for the 
/// </summary>

public class CanvasSwitch : MonoBehaviour {

    GameMenuController gameMenuController;
    public GameObject priorCanvas;
    public GameObject canvasToSwich;
    public GameMenuController gameMenu;

	public AudioMixer audioMixer;

	void Start()
    {
        //Debug.Log ("This code is running");
        /*GameObject HUDCanvas = this.transform.parent.gameObject;
        gameMenuController = HUDCanvas.GetComponent<GameMenuController>();*/
	}


    public void ButtonPress()
    {
		Debug.Log ("Call From Button Press");
        priorCanvas.SetActive(false);
        canvasToSwich.SetActive(true);
	}


	public void InGameQuit()
	{
		Debug.Log ("In Game Quite");
		Application.Quit ();
	}


	public void SetVolume(float volume)
    {
		Debug.Log (volume);
		audioMixer.SetFloat ("InGameVolume", volume);
	}



	public void BackPress()
	{
		Debug.Log ("Call From Back");
		priorCanvas.SetActive(true);
		canvasToSwich.SetActive(false);
	}


    public void ContinuePress()
    {

        Debug.Log("Call From Continue");
        gameMenu.HideMenu();
      
    }
}
