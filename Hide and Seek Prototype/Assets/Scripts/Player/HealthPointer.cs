using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple class to redirect hit targets from hitboxes to the players health component
/// Well it was, today we're redesigning the health system
/// </summary>
public class HealthPointer : Photon.MonoBehaviour {

    //A link to the players Health component
    public Health playerHealth;
    public int actorID;
    private PhotonNetworkManager master;

    private void Start()
    {
        master = FindObjectOfType<PhotonNetworkManager>();
        actorID = master.currentID;
        //playerHealth = GameObject.Find("Local Player").GetComponent<Health>();
       
    }


    public void RecieveHit(int senderID, int recieverID, int damage)
    {
        Debug.Log("HP.RecieveHit senderID:" + senderID + " recieverID: " + recieverID);
        master.TransferDamage(senderID, recieverID, damage);
    }


    /*
    private void OnEnable()
    {
        
        //Want to find the health script on our own player
        Health[] playerHealths = FindObjectsOfType<Health>();
        foreach (Health health in playerHealths)
        {
            playerHealth = health;
            if (health.transform.parent)
            {
                playerHealth = health;
            }
        }
    }
    */
}
