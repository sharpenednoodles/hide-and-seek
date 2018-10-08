﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///This script handles player network spawns locally for the local and remotely connected players.
///ON connection, any control scripts from remote players (stored in playerControlScripts[]) will be disabed
///Same for cameras
///This is to prevent Unity from becoming confused
///TODO - Add offline mode
/// </summary>

public class PlayerNetwork : Photon.MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject mapCamera;
    //scripts we want to be disabled for other player
    [SerializeField] private MonoBehaviour[] playerControlScripts;
    //Head bone is connected to then, neck bone
    [SerializeField] private GameObject headBoneRoot;
    [SerializeField] private bool debug = true;
    private bool searchStarted = false;
    public int viewID;
    private int actorID;

    private PhotonNetworkManager master;
    //private Health health;

    public ZoneController.Zone currentLocation = ZoneController.Zone.error;
    
    private void Start()
    {
        //photonView = GetComponent<PhotonView>();
        master = FindObjectOfType<PhotonNetworkManager>();
        //health = GetComponent<Health>();
        actorID = photonView.ownerId;
        Initialise();
    }

    //handle local and global objects/scripts
    private void Initialise()
    {
        if (photonView.isMine)
        {
            Debug.Log("View is mine");
            //Functionality for player
            this.name = "Local Player";
            PhotonNetwork.playerName = PlayerPrefs.GetString("Username", "Default Name");
            viewID = photonView.viewID;
            //Shrink our head bone so we don't see it
            //TODO - Switch to selective renderer

            //Set local player to ignore raycasts
            SetLayerRecursively(gameObject, 2);
            HideHead();
            //if (debug)
                    //Debug.Log("Local Hello World from " +photonView.viewID);

        }

        //Handle functionality for other players
        else
        {
            //Use Photon IDs later
            this.name = "Remote Player";
            //if (debug)
                //Debug.Log("Remote Hello World from " +photonView.viewID);

            playerCamera.gameObject.SetActive(false);
            mapCamera.gameObject.SetActive(false);

            //like a for loop but I'm lazy - disable control scripts
            foreach (MonoBehaviour m in playerControlScripts)
            {
                m.enabled = false;
            }
        }
    }

    //DEPRECATE THIS when you have time
    public void HideHead()
    {
        if (debug)
            Debug.Log("Hide Head Called");
        //Fix head scale, needs deprecating
        headBoneRoot.transform.localScale = new Vector3(0, 0, 0);
    }

    private void OnTriggerExit(Collider triggerZoneCheck)
    {
        if (debug)
            Debug.Log("<color = yellow>>Exiting the trigger area</color>");
        UpdateZoneLocation();
    }

    public void UpdateZoneLocation()
    {
        if (debug)
            Debug.Log("Updating Zone Location");
        Vector3 direction = new Vector3(0, -1, 0);
        float distance = 5f;
        RaycastHit groundRay;
        if (Physics.Raycast(transform.position, direction, out groundRay, distance))
        {
            if (groundRay.collider.GetComponent<ZoneID>() != null)
                currentLocation = groundRay.collider.GetComponent<ZoneID>().zoneID;
        }
        else
        {
            Debug.LogError("No Location Found");
        }
        Debug.Log("Player located in " + currentLocation);

        if (currentLocation == ZoneController.Zone.error && !searchStarted)
        {
            searchStarted = true;
            Debug.LogWarning("Player Location Unknown");
            InvokeRepeating("SearchForPlayer", 0.5f, 0.5f);
        }
    }

    private void SearchForPlayer()
    {
        UpdateZoneLocation();
        if (currentLocation != ZoneController.Zone.error)
        {
            CancelInvoke();
            searchStarted = false;
        }
            
    }

    private void SetLayerRecursively(GameObject g, int layerNumber)
    {
        if (g == null) return;
        foreach (Transform t in g.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = layerNumber;
        }
    }
}


