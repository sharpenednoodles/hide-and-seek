using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script to handle player and object health
/// This script can be put onto game objects to make them player destructible
/// 
/// //Todo - replace health bar with something better looking if we get the time
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class Health : Photon.MonoBehaviour
{

    [SerializeField]
    [Tooltip("Set the default health of this object")]
    private int defaultHealth = 100;
    [SerializeField]
    [Tooltip("Enables debug messages from this script")]
    private bool debug = true;
    [SerializeField]
    [Tooltip("Array of player box colliders")]
    GameObject playerHitbox;
    private GameObject propHitbox;
    private PhotonNetworkManager master;
    private Image healthBar;
    private float currentHealth;
    private int playerID = 69;
    private bool deathCalled;

    private void Start()
    {
        currentHealth = defaultHealth;
        if (gameObject.tag == "Player" &&photonView.isMine)
        {
            //Find the players health item
            master = FindObjectOfType<PhotonNetworkManager>();
            //healthBar = GameObject.Find("Health Bar").GetComponent<Image>();
            healthBar = GameObject.Find("Health Bar").GetComponent<Image>();
            if (photonView.isMine)
                playerID = photonView.ownerId;
            deathCalled = false;
            healthBar.fillAmount = 100;
        }
    }

    //Scales players health with a given scale upon transformation into a prop, and returns scale after reverting
    //Currently unused
    //Need to employ a growth/shrink animation to represent scale change
    public void ScaleHealth(bool hidden, float scale)
    {
        if (hidden)
            currentHealth *= scale;
        else
            currentHealth *= 1 / scale;
    }

    //Send target taking damage across clients
    public void SendDamage(int damage, int targetID, int senderID)
    {
        photonView.RPC("TakeDamageOld", PhotonTargets.All, damage, (byte)senderID, (byte)targetID);
    }

    //Call to refresh players GUI health on respawn
    //Only call on player objects!
    public void RefreshHealth()
    {
        healthBar.fillAmount = 100;
    }

    public void AddHealth(int healthToAdd)
    {
        currentHealth += healthToAdd;
        healthBar.fillAmount += ((float)healthToAdd / defaultHealth);
        //Prevent Player from excedding maximum health
        if (currentHealth > defaultHealth)
            currentHealth = defaultHealth;
    }

    //Called to damage target over network
    //We assume target is a player to save resources

    //TO DO - CLEAN THIS UP
    [PunRPC]
    public void TakeDamageOld(int damage, byte senderID, byte targetID)
    {
        if (debug)
            Debug.Log("dmg: " + damage + " sender ID: " + senderID + " targetID: " + targetID + "Cur Player ID: " + playerID);
        if (senderID == playerID)
        {
            if (debug)
                Debug.Log("Player shooting self");
            return;
        }

        if (targetID == playerID)
        {
            currentHealth -= damage;
            healthBar.fillAmount -= ((float)damage / defaultHealth);
        }

        if (debug)
        {
            Debug.Log("Target hit: current health = " + currentHealth + " Target Name " + gameObject.name);
            if (photonView.isMine)
                Debug.Log("Health bar fillamount = " + healthBar.fillAmount);
        }

        if (currentHealth <= 0 && !deathCalled)
        {
            deathCalled = true;
            if (gameObject.tag == "Player")
                Death(playerID, senderID);
            else
            {
                Destroy(gameObject);
                if (debug)
                    Debug.Log(transform.name + " destroyed");
            }

        }
    }

    void Death(int targetID, int senderID)
    {
        //Instantiate Call to create a ragdoll
        if (playerID == targetID)
        {
            //Send player death to network from player who died
            master.SendGameKill(0, (byte)PhotonNetworkManager.EventType.playerDeath, targetID, senderID);
            //Do photon instantiate of ragdoll he
        }
    }
}
