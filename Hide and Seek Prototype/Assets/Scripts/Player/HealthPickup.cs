using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic Script to represent health pickups
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class HealthPickup : Photon.MonoBehaviour {

    [Header("Properties")]
    //Default amount of health to give player on pickup
    public int healthAmount = 20;

    //Destory the object on network
    public void DestroyObject()
    {
        photonView.RPC("DisableHealth", PhotonTargets.AllBuffered, photonView.viewID);
    }

    [PunRPC]
    private void DisableHealth(int viewID)
    {
        GameObject disable = PhotonView.Find(viewID).gameObject;
        gameObject.SetActive(false);
    }

}
