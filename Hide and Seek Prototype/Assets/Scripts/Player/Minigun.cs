using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HideSeek.WeaponController
{
    [System.Serializable]
    public class Minigun : Weapon
    {
        /*
        public GameObject model;
        public float fireRate = 0.1f;
        public int fireRange = 20;
        public int damage = 5;
        public int clipSize = 50;
        public int defaultAmmo = 200;
        public float reloadTime = 10;
        public AudioClip[] soundEffects;
        */
        public AudioClip[] warmUpFX;
        public AudioClip[] coolDownFX;

    }
}
