using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HideSeek.WeaponController
{
    [System.Serializable]
    public class Weapon 
    {
        public GameObject model;
        public GameObject fireOrigin;
        //public GameObject weaponFX;
        public float fireRate;
        public int fireRange;
        public int damage;
        public int impactForce;
        public int clipSize;
        public int remainingClip;
        public int ammo;
        public float reloadTime;
        public bool canFire = true, isEquipped = false, crosshair = true, needWarmUp = false, melee = false;
        //Audio arrays containing our related audio files
        public AudioClip[] emptyFX;
        public AudioClip[] reloadFX;
        public AudioClip[] fireFX;
        //Array of various decal damages
        public string damageDecals;

        
        public enum ID {
            unarmed,
            /// <summary>
            /// Unarmed player state. Will be able to perform melee attacks at some stage
            /// </summary>
            pistol,
            /// <summary>
            /// Player armed with a pistol
            /// </summary>
            lightningGun,
            /// <summary>
            /// Player armed with a lightning gun
            /// </summary>
            minigun
            /// <summary>
            /// Player armed with the minigun
            /// </summary>
        }

    }


}
