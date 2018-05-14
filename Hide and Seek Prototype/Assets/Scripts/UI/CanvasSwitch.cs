using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSwitch : MonoBehaviour {

    public GameObject priorCanvas;
    public GameObject canvasToSwich;

    public void ButtonPress()
    {
        priorCanvas.SetActive(false);
        canvasToSwich.SetActive(true);
	}

    public void ContinuePress()
    {
        //Call to unpause
    }
}
