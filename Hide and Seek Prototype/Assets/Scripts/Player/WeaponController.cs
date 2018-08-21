using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Script to handle all aspects of combat, ranged and close combat
/// 
/// Todo
/// Fix animations
/// Add additional warm up and cool down conditions
/// Sync weaponFX across clients (bullet holes, animations and SFX)
/// </summary>

namespace HideSeek.WeaponController
{
    public class WeaponController : Photon.MonoBehaviour
    {
        private DecalManager decal;
        [Header("Weapon Parameters")]
        [SerializeField] private Unarmed unarmed;
        [SerializeField] private Pistol pistol;
        [SerializeField] private LightningGun lightningGun;
        [SerializeField] private Minigun minigun;

        private Animator weaponAnim;
        private Crosshairs crosshairs;

        //Audio
        private AudioSource gunSound;
        const int WEAPON_COUNT = 4;

        //move this stuff to beffer location
        private float cooldown = 0;
        private bool fireHeld = false;
        private bool soundPlay = false;

        [Header("DEBUG")]
        [SerializeField] private Weapon.ID currentID = Weapon.ID.unarmed;
        [SerializeField] private Animator playerAnim;
        public Weapon currWeapon;
        public bool drawRay;

        private int playerID;
        private LineRenderer laserDebug;
        private WaitForSeconds shotDuration = new WaitForSeconds(.07f);

        //networkTempVars
        //Thinking of having a lookup on the master client with all these precached so every player can access at will, but we'll see

        private GameObject remotePlayer, remoteWeapon, remoteUnArmed, remotePistol, remoteLightningGun, remoteMinigun;

        void Start()
        {
            decal = GameObject.Find("Game Controller").GetComponent<DecalManager>();
            Debug.Log("<color=red>All Weapons enabled</color>");
            if (drawRay)
                Debug.Log("<color=green>Weapon Raycast Debug Enabled</color>");
            //GET VARS
            weaponAnim = gameObject.transform.GetChild(3).GetComponent<Animator>();
            gunSound = gameObject.transform.GetChild(3).GetComponent<AudioSource>();
            laserDebug = GetComponent<LineRenderer>();

            //DISABLE ALL WEAPONS INITIALLY
            minigun.model.SetActive(false);
            lightningGun.model.SetActive(false);
            pistol.model.SetActive(false);

            pistol.remainingClip = pistol.clipSize;
            lightningGun.remainingClip = lightningGun.clipSize;
            minigun.remainingClip = minigun.clipSize;

            //link crosshair component
            GameObject reticle = GameObject.FindGameObjectWithTag("Reticle");
            crosshairs = reticle.GetComponent<Crosshairs>();

            playerID = photonView.viewID;
        }


        void Update()
        {
            cooldown -= Time.deltaTime;
            //Switching Weapons, need to validate if a player has weapon

            if (photonView.isMine)
            {
                if (Input.GetKeyUp("0"))
                {
                    currentID = Weapon.ID.unarmed;
                    SwitchWeapon(currentID);
                }
                if (Input.GetKeyUp("1"))
                {
                    currentID = Weapon.ID.pistol;
                    SwitchWeapon(currentID);
                }
                if (Input.GetKeyUp("2"))
                {
                    currentID = Weapon.ID.lightningGun;
                    SwitchWeapon(currentID);
                }
                if (Input.GetKeyUp("3"))
                {
                    currentID = Weapon.ID.minigun;
                    SwitchWeapon(currentID);
                }
                //Scroll wheel
                if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    currentID += 1;
                    if ((int)currentID > WEAPON_COUNT - 1)
                        currentID = 0;
                    SwitchWeapon(currentID);

                }
                if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    currentID -= (1);
                    if ((int)currentID < 0)
                        currentID = (Weapon.ID)WEAPON_COUNT - 1;
                    SwitchWeapon(currentID);
                }

                //Get fire input
                if (Input.GetButton("Fire1"))
                {
                    Shoot();
                    fireHeld = true;
                    weaponAnim.SetBool("attack", true);
                    playerAnim.SetBool("attack", true);
                }
                //Player releasing trigger
                if (Input.GetButtonUp("Fire1"))
                {
                    weaponAnim.SetBool("attack", true);
                    playerAnim.SetBool("attack", false);
                    fireHeld = false;
                }

                //Reload the shit
                if (Input.GetButtonUp("Reload"))
                {
                    TriggerReload(currWeapon);
                }
            }
        }

        void LateUpdate()
        {
            //Apply secondary hit calculations here
            //Currently removed again
        }

        //Handles Player Shooting
        void Shoot()
        {
            if (photonView.isMine)
            {
                //Stop us from spamming weapons
                if (cooldown > 0)
                {
                    return;
                }

                //Prevent us from firing if our current clip is empty
                if (!SubtractAmmo(currWeapon))
                {
                    if (!soundPlay)
                        StartCoroutine(PlayEmptySound(currWeapon));
                    return;
                }

                Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
                Vector3 rayDirection = Camera.main.transform.forward;
                RaycastHit hit;

                //Get laser origin
                if (drawRay)
                    laserDebug.SetPosition(0, currWeapon.fireOrigin.transform.position);

                //Fire ray
                if (Physics.Raycast(rayOrigin, rayDirection, out hit, currWeapon.fireRange))
                {
                    //Draw laser
                    if (drawRay)
                        laserDebug.SetPosition(1, hit.point);

                    //Shot info
                    Debug.Log("Player " + photonView.viewID + " hit" + hit.transform.name);
                    Debug.DrawRay(rayOrigin, rayDirection * currWeapon.fireRange, Color.green);

                    Health hitHealth = hit.collider.GetComponent<Health>();

                    //Apply damage over network
                    if (hitHealth != null)
                        hitHealth.SendDamage(currWeapon.damage);

                    //Add weapon force
                    if (hit.rigidbody != null)
                    {
                        Vector3 direction = (-hit.normal).normalized;
                        PhotonView targetView = hit.rigidbody.gameObject.transform.GetComponent<PhotonView>();
                        if (targetView != null)
                        {
                            int targetID = targetView.viewID;
                            photonView.RPC("PhysicsForce", PhotonTargets.AllBuffered, targetID, currWeapon.impactForce, direction);
                        }
                        else
                        {
                            Debug.Log("No Photon View on target!");
                            hit.rigidbody.AddForce(-hit.normal * currWeapon.impactForce);
                        }
                    }

                }
                //If we didn't hit any colliders
                else
                {
                    if (drawRay)
                        laserDebug.SetPosition(1, rayOrigin + (rayDirection * currWeapon.fireRange));
                    Debug.DrawRay(rayOrigin, rayDirection * currWeapon.fireRange, Color.red);
                }
               
                StartCoroutine(ShotEffect());
                //Set cooldown timer
                cooldown = currWeapon.fireRate;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                
                //Call correct decal function
                if (hit.rigidbody != null)
                    decal.SpawnFromPool(currWeapon.damageDecals, hit.point, rotation, hit.rigidbody.gameObject);
                else
                    decal.SpawnFromPool(currWeapon.damageDecals, hit.point, rotation);

                photonView.RPC("SyncShot", PhotonTargets.Others, rayOrigin, rayDirection, currWeapon.fireRange, currWeapon.damageDecals);
            }
        }

        [PunRPC]
        void PhysicsForce(int targetID, int impactForce, Vector3 direction)
        {
            GameObject target = PhotonView.Find(targetID).gameObject;
            target.GetComponent<Rigidbody>().AddForce(direction * impactForce);
        }

        //Draw Remote player shots in editor
        //Hit detection is currently broken, no idea why
        [PunRPC]
        void SyncShot(Vector3 start, Vector3 dir, int fireRange, string decalString)
        {
            RaycastHit hit;
            if (Physics.Raycast(start, dir, out hit, currWeapon.fireRange))
            {
                Debug.Log("Remote DID HIT");
                //decal.SpawnDecal(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), hit.rigidbody.gameObject, currentID);
                Debug.DrawRay(start, dir * fireRange, Color.blue);

                if (hit.rigidbody != null)
                    decal.SpawnFromPool(decalString, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal), hit.rigidbody.gameObject);
                else
                    decal.SpawnFromPool(decalString, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            }
            else
            {
                Debug.Log("REMOTE DIDN'T HIT");
                Debug.DrawRay(start, dir * fireRange, Color.yellow);
            }
        }

        private IEnumerator ShotEffect()
        {
            PlayFireSound(currWeapon);
            //need to play animation instantly
            playerAnim.SetTrigger("recoil");
            if (drawRay)
                laserDebug.enabled = true;
            yield return shotDuration;
            laserDebug.enabled = false;
            playerAnim.ResetTrigger("recoil");
        }

        void PlayFireSound(Weapon c)
        {
            if (c.fireFX.Length != 0)
            {
                int n = Random.Range(0, c.fireFX.Length);
                gunSound.clip = c.fireFX[n];
            }
            else gunSound.clip = null;
            gunSound.Play();
        }

        private IEnumerator PlayEmptySound(Weapon c)
        {
            soundPlay = true;
            if (c.emptyFX.Length != 0)
            {
                int n = Random.Range(0, c.emptyFX.Length);
                gunSound.clip = c.emptyFX[n];
            }
            else gunSound.clip = null;
            gunSound.Play();
            if (gunSound.clip != null)
                yield return new WaitForSeconds(gunSound.clip.length);
            soundPlay = false;
        }

        void PlayReloadSound(Weapon c)
        {
            if (c.reloadFX.Length != 0)
            {
                int n = Random.Range(0, c.reloadFX.Length);
                gunSound.clip = c.reloadFX[n];
            }
            else gunSound.clip = null;
            gunSound.Play();
        }

        void TriggerReload(Weapon c)
        {
            Debug.Log("Trigger Reload Function");
            PlayReloadSound(currWeapon);
            StartCoroutine(Reload(currWeapon));
        }

        private IEnumerator Reload(Weapon c)
        {
            Debug.Log("Reload Triggered");

            yield return new WaitForSeconds(c.reloadTime);

            if (c.remainingClip != 0)
            {
                c.ammo += c.remainingClip;
                c.remainingClip = 0;
            }

            if (c.ammo <= c.clipSize)
            {
                c.remainingClip = c.ammo;
                c.ammo = 0;
            }
            else
            {
                c.remainingClip = c.clipSize;
                c.ammo -= c.clipSize;
            }
            c.canFire = true;
        }

        bool SubtractAmmo(Weapon c)
        {
            if (c.remainingClip <= 0)
            {
                c.canFire = false;
                return false;
            }
            c.remainingClip -= 1;
            return true;
        }

        void SwitchWeapon(Weapon.ID weapon)
        {
            //animation conditions
            weaponAnim.SetInteger("weaponType", (int)currentID);
            playerAnim.SetInteger("weaponType", (int)currentID);

            //unarmed.model.SetActive(false);
            minigun.model.SetActive(false);
            lightningGun.model.SetActive(false);
            pistol.model.SetActive(false);
            cooldown = 0;

            switch (currentID)
            {
                case Weapon.ID.unarmed:
                    //unarmed.model.SetActive(true);
                    currWeapon = unarmed;
                    break;
                case Weapon.ID.pistol:
                    pistol.model.SetActive(true);
                    currWeapon = pistol;
                    break;
                case Weapon.ID.minigun:
                    minigun.model.SetActive(true);
                    currWeapon = minigun;
                    break;
                case Weapon.ID.lightningGun:
                    lightningGun.model.SetActive(true);
                    currWeapon = lightningGun;
                    break;
            }
           
            //Call crosshair
            if (currWeapon.crosshair)
                crosshairs.ToggleCrossHairs(true);
            else
                crosshairs.ToggleCrossHairs(false);

            photonView.RPC("SyncWeapon", PhotonTargets.Others, playerID, (int)currentID);

        }

        //Note Enums cannot be serialized over network
        [PunRPC]
        void SyncWeapon(int playerID, int weapon)
        {
            //Debug.Log("Remote Switch Recieved from " + photonView.viewID + " for int cast " + weapon);
            //Cast back to enum
            currentID = (Weapon.ID)weapon;
            //Want to switch this to a cached global system to save on overhead
            remotePlayer = PhotonView.Find(playerID).gameObject;
            remoteWeapon = remotePlayer.transform.GetChild(3).gameObject;

            //remoteUnArmed = remoteWeapon.transform.GetChild(0).gameObject;
            remotePistol = remoteWeapon.transform.GetChild(0).gameObject;
            remoteLightningGun = remoteWeapon.transform.GetChild(1).gameObject;
            remoteMinigun = remoteWeapon.transform.GetChild(2).gameObject;

            //remoteUnArmed.SetActive(false);
            remotePistol.SetActive(false);
            remoteLightningGun.SetActive(false);
            remoteMinigun.SetActive(false);

            switch (currentID)
            {
                case Weapon.ID.unarmed:
                    //remoteUnArmed.SetActive(true);
                    break;
                case Weapon.ID.pistol:
                    remotePistol.SetActive(true);
                    break;
                case Weapon.ID.minigun:
                    remoteMinigun.SetActive(true);
                    break;
                case Weapon.ID.lightningGun:
                    remoteLightningGun.SetActive(true);
                    break;
            }
        }
    }
}
