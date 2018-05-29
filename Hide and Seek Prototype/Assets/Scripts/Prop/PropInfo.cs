using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is used to pass parameters about the current prop
/// such as prop resource name and health multiplier
/// </summary>
public class PropInfo : MonoBehaviour {

    //We will encapsulte these fields later, this is just a temp implmentation
    [Header("Prop Properties")]
    public string prefabName;
    public float healthMult;
    
}
