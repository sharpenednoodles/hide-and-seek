using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic Script to represent health pickups
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class HealthPickup : MonoBehaviour {

    [Header("Properties")]
    //Default amount of health to give player on pickup
    public int healthAmount = 20;

}
