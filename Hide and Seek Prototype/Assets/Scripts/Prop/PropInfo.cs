using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is used to pass parameters about the current prop
/// such as prop resource name and health multiplier
/// Everything is public, will use properties and accessors later when I can be bothered
/// </summary>
public class PropInfo : MonoBehaviour {

    //We will encapsulte these fields later, this is just a temp implmentation
    [Header("Prop Properties")]
    public string prefabName;
    public float healthMult;
    
}
