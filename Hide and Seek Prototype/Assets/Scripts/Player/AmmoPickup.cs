using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the ammo pickup
/// </summary>

[RequireComponent(typeof(PhotonView))]
public class AmmoPickup : Photon.MonoBehaviour
{

    [Header("Properties")]
    //Type of ammo to pickup
    public HideSeek.WeaponController.Weapon.ID weaponID;
    public int amount;

    //Destory the object on network
    public void DestroyObject()
    {
        photonView.RPC("DisableAmmo", PhotonTargets.AllBuffered, photonView.viewID);
    }

    [PunRPC]
    private void DisableAmmo(int viewID)
    {
        GameObject disable = PhotonView.Find(viewID).gameObject;
        gameObject.SetActive(false);
    }
}
