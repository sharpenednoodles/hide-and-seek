using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the weapon pickup
/// </summary>

[RequireComponent(typeof(PhotonView))]
public class WeaponPickup : MonoBehaviour {

    [Header("Properties")]
    //Type of Weapon to pickup
    public HideSeek.WeaponController.Weapon.ID weaponID; 
	
}
