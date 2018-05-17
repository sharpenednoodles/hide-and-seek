using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CanvasSwitch : MonoBehaviour {

    public GameObject priorCanvas;
    public GameObject canvasToSwich;
//	public static bool MenuState = false;
	// GameObject menuCanvas;



    public void ButtonPress()
    {
        priorCanvas.SetActive(false);
        canvasToSwich.SetActive(true);
	}

//	public void BackPress()
//	{
//		priorCanvas.SetActive(true);
//		canvasToSwich.SetActive(false);
//	}
//

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
