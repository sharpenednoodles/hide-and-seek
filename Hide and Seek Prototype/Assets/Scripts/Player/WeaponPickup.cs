using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the weapon pickup
/// </summary>

[RequireComponent(typeof(PhotonView))]
public class WeaponPickup : Photon.MonoBehaviour {

    [Header("Properties")]
    //Type of Weapon to pickup
    public HideSeek.WeaponController.Weapon.ID weaponID;

    //Destory the object on network
    public void DestroyObject()
    {
        photonView.RPC("DisableWeapon", PhotonTargets.AllBuffered, photonView.viewID);
    }

    [PunRPC]
    private void DisableWeapon(int viewID)
    {
        GameObject disable = PhotonView.Find(viewID).gameObject;
        gameObject.SetActive(false);
    }
}
