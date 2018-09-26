using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic Class to toggle between 2 UI canvases
/// </summary>
/// 
public class UISwitch : MonoBehaviour {

    [SerializeField] [Tooltip("Canvas/object(s) to disable on press")]  private GameObject[] priorCanvas;
    [SerializeField] [Tooltip("Canvas/object(s) to switch to")] private GameObject[] canvasToSwitch;
    [SerializeField] private bool debug = true;

    //Switch game canvas to new canvas
    public void ButtonPress()
    {
        if (debug)
            Debug.Log("Switch to new canvas");
        foreach (GameObject prior in priorCanvas)
        {
            prior.SetActive(false);
        }
        foreach (GameObject newCanvas in canvasToSwitch)
        {
            newCanvas.SetActive(true);
        }
    }
    
    //API to quit application
    public void InGameQuit()
    {
        if (debug)
            Debug.Log("In Game Quit");
        Application.Quit();
    }

    //Restore previous canvas
    public void BackPress()
    {
        if (debug)
            Debug.Log("Return to previous canvas");
        foreach (GameObject prior in priorCanvas)
        {
            prior.SetActive(true);
        }
        foreach (GameObject newCanvas in canvasToSwitch)
        {
            newCanvas.SetActive(false);
        }
    }
}