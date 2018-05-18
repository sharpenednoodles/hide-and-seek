using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class CanvasSwitch : MonoBehaviour {

    public GameObject priorCanvas;
    public GameObject canvasToSwich;

	public AudioMixer audioMixer;
//	public static bool MenuState = false;
	// GameObject menuCanvas;




	void Start(){

		Debug.Log ("This code is running");

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


	public void SetVolume(float volume){

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

		Debug.Log ("Call From Continue");
		//menuC.HideMenu();








//		menuCanvas.SetActive(true);
//		Debug.Log ("Set Disable");
//		Cursor.lockState = CursorLockMode.Locked;
//		Cursor.visible = false;
        //Call to unpause
    }
}
