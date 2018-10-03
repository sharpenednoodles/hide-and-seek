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
    [SerializeField] private bool debug = true;
    private PhotonNetworkManager master;

    private void Start()
    {
        master = FindObjectOfType<PhotonNetworkManager>();
        actorID = photonView.ownerId;
        //playerHealth = GameObject.Find("Local Player").GetComponent<Health>();
        //FindHealthObjects();
        GetHealthComponent();
    }


    public void RecieveHit(int senderID, int recieverID, int damage)
    {
        Debug.Log("HP.RecieveHit senderID:" + senderID + " recieverID: " + recieverID);
        master.TransferDamage(senderID, recieverID, damage);
    }

    private void GetHealthComponent()
    {
        //Recursively search in parent for health object on player - in theory
        playerHealth = gameObject.GetComponentInParent(typeof(Health)) as Health;
        if (playerHealth == null)
        {
            GetHealthComponent();
            Debug.LogWarning("Get Health Failed");
        }
            
    }

    private void FindHealthObjects()
    {
        //Want to find the health script on our own player
        Health[] playerHealths = FindObjectsOfType<Health>();

        if (playerHealths.Length > 0 && debug)
            Debug.Log(playerHealths.Length +" Health Object(s) found");
        else
        {
            Debug.LogWarning("No Health Objects found!");
        }

        foreach (Health health in playerHealths)
        {
            if (debug)
            {
                Debug.Log("Player Health Actor ID" +health.GetComponent<PhotonView>().ownerId);
            }

            if (health.transform.IsChildOf(gameObject.transform))
            {
                if (debug)
                    Debug.Log("Found our Health object with ID" + health.GetComponent<PhotonView>().ownerId);
                playerHealth = health;
            }
        }
    }

    //DEPRECATED
    //Check to see whether the health object found is a parent
    private bool isParentHealth(Transform t)
    {
        while (t.parent!= null)
        {
            if (t.transform.parent)
                return true;
            t = t.transform.parent;
        }
        return false;
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
