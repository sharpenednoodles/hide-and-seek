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
    public int viewID;
    private int actorID;

    private PhotonNetworkManager master;
    private Health health;

    public ZoneController.Zone currentLocation = ZoneController.Zone.error;
    
    private void Start()
    {
        //photonView = GetComponent<PhotonView>();
        master = FindObjectOfType<PhotonNetworkManager>();
        health = GetComponent<Health>();
        actorID = photonView.ownerId;
        Initialise();
    }

    //handle local and global objects/scripts
    private void Initialise()
    {
        if (photonView.isMine)
        {
            //Functionality for player
            this.name = "Local Player";
            PhotonNetwork.playerName = PlayerPrefs.GetString("Username", "Default Name");
            viewID = photonView.viewID;
            //Shrink our head bone so we don't see it
            //TODO - Switch to selective renderer
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

    //DEPRECATE THIS SHIT
    public void HideHead()
    {
        if (debug)
            Debug.Log("Hide Head Called");
        //Fix head scale, needs deprecating
        headBoneRoot.transform.localScale = new Vector3(0, 0, 0);
    }

    private void OnTriggerExit(Collider triggerZoneCheck)
    {
        Debug.LogWarning("Exiting the trigger area");
        UpdateZoneLocation();
    }

    public void UpdateZoneLocation()
    {
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
    }

    //I don't have an interface to transfer across so doing it here - doesn't make sense, but no time for that
    public void TransferDamage(int senderID, int recieverID, int damage)
    {
        Debug.Log("Local.Transfer senderID:" + senderID + " recieverID: " + recieverID);
        photonView.RPC("SendDamage", PhotonTargets.AllBuffered, senderID, recieverID, damage);
    }

    [PunRPC]
    public void SendDamage(int senderID, int recieverID, int damage)
    {
        if (photonView.isMine)
        {
            Debug.LogWarning("MY ID IS " + photonView.ownerId);
        }
        
        Debug.Log("Local.Send senderID:" + senderID + " recieverID: " + recieverID +" actorID on player: " +actorID +" photonview ID" + photonView.ownerId);

        SendDamageLocal(senderID, recieverID, damage);


    }

    private void SendDamageLocal(int senderID, int recieverID, int damage)
    {
        Debug.Log("Local.SendLocal senderID:" + senderID + " recieverID: " + recieverID + " actorID on player: " + actorID + " photonview ID" + photonView.ownerId +" masterRef" + master.currentID);

        if (senderID == recieverID)
        {
            Debug.Log("Player " + senderID + " hit Player " + recieverID + " for " + damage + " damage");
            Debug.Log("Player Hitting self");
            return;
        }


        //Do the damage
        if (master.currentID == recieverID)
        {
            if (debug)
                Debug.Log("Player " + senderID + " hit Player " + recieverID + " for " + damage + " damage");
            //health.TakeDamage(damage, senderID);
        }

    }
}


