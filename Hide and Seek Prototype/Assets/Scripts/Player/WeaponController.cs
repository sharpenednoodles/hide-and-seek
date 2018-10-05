using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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
        [SerializeField] private bool allEnabled = true;

        private Animator weaponAnim;
        private Crosshairs crosshairs;
        private InventoryController inventoryController;

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
        public bool isProp = false;

        private int playerID, actorID;
        private LineRenderer laserDebug;
        private WaitForSeconds shotDuration = new WaitForSeconds(.07f);

        //TempDisplayStuff
        GameObject ammoCanvas;
        private Text clipDisplay;
        private Text ammoDisplay;

        private GameObject GameManager, FX;

        //networkTempVars
        //Thinking of having a lookup on the master client with all these precached so every player can access at will, but we'll see

        private GameObject remotePlayer, remoteWeapon, remoteUnArmed, remotePistol, remoteLightningGun, remoteMinigun;

        
        void Start()
        {
            decal = GameObject.Find("Game Controller").GetComponent<DecalManager>();        
            if (drawRay)
                Debug.Log("<color=green>Weapon Raycast Debug Enabled</color>");
            //GET VARS
            weaponAnim = gameObject.transform.GetChild(3).GetComponent<Animator>();
            gunSound = gameObject.transform.GetChild(3).GetComponent<AudioSource>();
            laserDebug = GetComponent<LineRenderer>();

            //Ammo display stuff
            GameObject weaponDisplay = GameObject.Find("WeaponDisplay");
            ammoCanvas = weaponDisplay.transform.GetChild(0).gameObject;
            clipDisplay = ammoCanvas.transform.GetChild(0).gameObject.GetComponent<Text>();
            ammoDisplay = ammoCanvas.transform.GetChild(1).gameObject.GetComponent<Text>();

            //DISABLE ALL WEAPONS INITIALLY
            unarmed.model.SetActive(false);
            minigun.model.SetActive(false);
            lightningGun.model.SetActive(false);
            pistol.model.SetActive(false);

            unarmed.remainingClip = unarmed.clipSize;
            pistol.remainingClip = pistol.clipSize;
            lightningGun.remainingClip = lightningGun.clipSize;
            minigun.remainingClip = minigun.clipSize;

            //link crosshair component
            GameObject reticle = GameObject.FindGameObjectWithTag("Reticle");
            crosshairs = reticle.GetComponent<Crosshairs>();

            //Get controllers
            inventoryController = GameObject.Find("InventoryHolder").GetComponent<InventoryController>();
            GameManager = GameObject.Find("Game Controller");
            FX = GameManager.transform.GetChild(2).gameObject;
            inventoryController.ClearPlayerInventory();
            playerID = photonView.viewID;
            actorID = photonView.ownerId;
            //Switch to unarmed weapon as default

            if (allEnabled)
            {
                Debug.Log("<color=red>All Weapons enabled</color>");
                inventoryController.FillInventory();
            }
            else
            {
                inventoryController.GiveUnarmed();
            }
            SwitchWeapon(Weapon.ID.unarmed);

        }

        #region Player Input
        void Update()
        {
            cooldown -= Time.deltaTime;
            //Switching Weapons, need to validate if a player has weapon

            if (photonView.isMine && !GameMenuController.MenuState && !isProp)
            {
                if (Input.GetKeyUp("1"))
                {
                    inventoryController.currentSlot = 0;
                    inventoryController.RefreshUI();
                    currentID = inventoryController.GetIDFromSlot();
                    SwitchWeapon(currentID);
                }
                if (Input.GetKeyUp("2"))
                {
                    inventoryController.currentSlot = 1;
                    inventoryController.RefreshUI();
                    currentID = inventoryController.GetIDFromSlot();
                    SwitchWeapon(currentID);
                }
                if (Input.GetKeyUp("3"))
                {
                    inventoryController.currentSlot = 2;
                    inventoryController.RefreshUI();
                    currentID = inventoryController.GetIDFromSlot();
                    SwitchWeapon(currentID);
                }
                if (Input.GetKeyUp("4"))
                {
                    inventoryController.currentSlot = 3;
                    inventoryController.RefreshUI();
                    currentID = inventoryController.GetIDFromSlot();
                    SwitchWeapon(currentID);
                }
                //Scroll wheel
                if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    inventoryController.ScrollUpInput();
                    currentID = inventoryController.GetIDFromSlot();
                    SwitchWeapon(currentID);
                }
                if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    inventoryController.ScrollDownInput();
                    currentID = inventoryController.GetIDFromSlot();
                    SwitchWeapon(currentID);
                }

                //Get fire input
                if (Input.GetButton("Fire1"))
                {
                    if (!isProp)
                    {
                        fireHeld = true;
                        if (currWeapon.needWarmUp)
                        {
                            WarmUp();
                        }
                        else
                            Shoot();
                        
                        weaponAnim.SetBool("attack", true);
                        playerAnim.SetBool("attack", true);
                    }
                }
                //Player releasing trigger
                if (Input.GetButtonUp("Fire1"))
                {
                    weaponAnim.SetBool("attack", true);
                    playerAnim.SetBool("attack", false);
                    fireHeld = false;
                    if (currWeapon.needWarmUp)
                    {

                    }
                }

                //Reload the shit
                if (Input.GetButtonUp("Reload"))
                {
                    TriggerReload(currWeapon);
                }
            }
        }
        #endregion

        void LateUpdate()
        {
            //Apply secondary hit calculations here
            //Currently removed again
        }

        #region Shooting Logic

        void WarmUp()
        {

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
                if (!SubtractAmmo(currWeapon) && !currWeapon.melee)
                {
                    if (!soundPlay)
                        StartCoroutine(PlayEmptySound(currWeapon));
                    return;
                }

                //To do, vary origin based upon movement speed
                Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
                Vector3 rayDirection = Camera.main.transform.forward;
                RaycastHit hit;

                //Issues with this on respawn
                if (!currWeapon.melee)
                {
                    GameObject instance = CFX_SpawnSystem.GetNextObject(FX.GetComponent<CFX_PrefabPool>().muzzleFX);
                    instance.transform.position = currWeapon.fireOrigin.transform.position;
                    instance.transform.SetParent(currWeapon.fireOrigin.transform);

                    //Muzzle flash sync
                    photonView.RPC("WeaponFX", PhotonTargets.Others, currWeapon.fireOrigin.transform.position, currWeapon.fireOrigin.transform.rotation);
                }
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
                    HealthPointer HP = hit.collider.GetComponent<HealthPointer>();
                    PhotonView target = hit.collider.GetComponent<PhotonView>();

                    if (HP != null)
                    {
                        Debug.Log("Found a Health Pointer!");
                        hitHealth = HP.playerHealth;
                    }
        

                    if (hitHealth != null)
                    {
                        Debug.Log("Sending Damage");
                        hitHealth.SendDamage(currWeapon.damage, target.ownerId, actorID);
                        //hitHealth.SendDamage(currWeapon.damage);
                    }
                    
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
                //Have swapped to the code below as it can be networked better
                //May reintroduce later as parenting decals to rigidbody items looks great
                /*
                if (hit.rigidbody != null)
                {
                    decal.SpawnFromPool(currWeapon.damageDecals, hit.point, rotation, hit.rigidbody.gameObject);
                }
                else
                {
                    decal.SpawnFromPool(currWeapon.damageDecals, hit.point, rotation);
                }
                */

                //New less nice looking implementation

                //Rewrite later so that network aware first up

                if (hit.rigidbody == null)
                {
                    decal.SpawnFromPool(currWeapon.damageDecals, hit.point, rotation);
                    photonView.RPC("SyncShot", PhotonTargets.Others, currWeapon.damageDecals, hit.point, rotation);
                    Debug.LogWarning("Sending: "+currWeapon.damageDecals);
                }
                //photonView.RPC("SyncShotRayCast", PhotonTargets.Others, rayOrigin, rayDirection, currWeapon.fireRange, currWeapon.damageDecals);
                photonView.RPC("WeaponSparks", PhotonTargets.All, hit.point, rotation);
            }
        }

        [PunRPC]
        void PhysicsForce(int targetID, int impactForce, Vector3 direction)
        {
            GameObject target = PhotonView.Find(targetID).gameObject;
            target.GetComponent<Rigidbody>().AddForce(direction * impactForce);
        }

        [PunRPC]
        void WeaponFX (Vector3 position, Quaternion rotation)
        {
            GameObject instance = CFX_SpawnSystem.GetNextObject(FX.GetComponent<CFX_PrefabPool>().muzzleFX);
            instance.transform.position = position;
            //instance.transform.SetParent(currWeapon.fireOrigin.transform);
        }

        [PunRPC]
        void WeaponSparks (Vector3 position, Quaternion rotation)
        {
            GameObject instance = CFX_SpawnSystem.GetNextObject(FX.GetComponent<CFX_PrefabPool>().metalSparks);
            instance.transform.position = position;
        }
        //Draw Remote player shots in editor
        //Hit detection is currently broken, no idea why

        //DEPRECATING AS IT DOESN'T FUNCTION
        [PunRPC]
        void SyncShotRayCast(Vector3 start, Vector3 dir, int fireRange, string decalString)
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

        [PunRPC]
        void SyncShot(string decalString, Vector3 point, Quaternion rotation)
        {
            Debug.LogWarning("Recieving: " + decalString);
            decal.SpawnFromPool(currWeapon.damageDecals, point, rotation);
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

        #endregion
        #region Weapon SFX
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
        #endregion

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
            clipDisplay.text = c.remainingClip.ToString();
            ammoDisplay.text = c.ammo.ToString();
        }

        bool SubtractAmmo(Weapon c)
        {
            if (c.remainingClip <= 0)
            {
                c.canFire = false;
                return false;
            }
            c.remainingClip -= 1;
            clipDisplay.text = c.remainingClip.ToString();
            return true;
        }

        void SwitchWeapon(Weapon.ID weapon)
        {
            //animation conditions
            weaponAnim.SetInteger("weaponType", (int)currentID);
            playerAnim.SetInteger("weaponType", (int)currentID);

            unarmed.model.SetActive(false);
            minigun.model.SetActive(false);
            lightningGun.model.SetActive(false);
            pistol.model.SetActive(false);
            cooldown = 0;

            switch (currentID)
            {
                case Weapon.ID.unarmed:
                    unarmed.model.SetActive(true);
                    currWeapon = unarmed;
                    ammoCanvas.SetActive(false);
                    //UpdateAmmoCount();
                    break;
                case Weapon.ID.pistol:
                    pistol.model.SetActive(true);
                    currWeapon = pistol;
                    ammoCanvas.SetActive(true);
                    UpdateAmmoCount();
                    break;
                case Weapon.ID.minigun:
                    minigun.model.SetActive(true);
                    currWeapon = minigun;
                    ammoCanvas.SetActive(true);
                    UpdateAmmoCount();
                    break;
                case Weapon.ID.lightningGun:
                    lightningGun.model.SetActive(true);
                    currWeapon = lightningGun;
                    ammoCanvas.SetActive(true);
                    UpdateAmmoCount();
                    break;
            }
           
            //Call crosshair
            if (currWeapon.crosshair)
                crosshairs.ToggleCrossHairs(true);
            else
                crosshairs.ToggleCrossHairs(false);

            photonView.RPC("SyncWeapon", PhotonTargets.Others, playerID, (int)currentID);

        }

        private void UpdateAmmoCount()
        {
            clipDisplay.text = currWeapon.remainingClip.ToString();
            ammoDisplay.text = currWeapon.ammo.ToString();
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

            remoteUnArmed = remoteWeapon.transform.GetChild(0).gameObject;
            remotePistol = remoteWeapon.transform.GetChild(1).gameObject;
            remoteLightningGun = remoteWeapon.transform.GetChild(2).gameObject;
            remoteMinigun = remoteWeapon.transform.GetChild(3).gameObject;

            remoteUnArmed.SetActive(false);
            remotePistol.SetActive(false);
            remoteLightningGun.SetActive(false);
            remoteMinigun.SetActive(false);

            switch (currentID)
            {
                case Weapon.ID.unarmed:
                    remoteUnArmed.SetActive(true);
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

        //Used for switching player into prop
        public void PropMode(bool isAProp)
        {
            
            if (!isAProp)
            {
                Debug.LogWarning("PropMode Disabled");
                isProp = false;
                crosshairs.ToggleCrossHairs(true);
            }
            if(isAProp)
            {
                Debug.LogWarning("PropMode Enabled");
                isProp = true;
                crosshairs.ToggleCrossHairs(false);
            }
        }
    }
}
