using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script to handle cross hair animation
/// </summary>

public class Crosshairs : MonoBehaviour {

    //Used for setting colour of reticle, not implemented yet
    [SerializeField] private Color colorCross;
    public GameObject[] reticle;
   
	
	void Start () {

        ToggleCrossHairs(false);
	}

    //Toggles the cross hair visibility, on if enable is true
    public void ToggleCrossHairs(bool enable)
    {
        foreach (GameObject g in reticle)
        {
            if (enable)
            {
                g.SetActive(true);
                Debug.Log("Toggle Cross Hairs ON");
            }   
            else
            {
                g.SetActive(false);
                Debug.Log("Toggle Cross Hairs OFF");
            }
                
        }
    }
	
	
}
