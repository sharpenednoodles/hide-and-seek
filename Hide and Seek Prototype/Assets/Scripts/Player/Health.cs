using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script to handle player and object health
/// This script can be put onto game objects to make them player destructible
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class Health : MonoBehaviour {

    [SerializeField]
    [Tooltip("Set the default health of this object")]
    private int defaultHealth = 100;
    [SerializeField]
    [Tooltip("Enables debug messages from this script")]
    private bool debug = true;
    //Temp GUI image, will replace with something less shit - Lyhuy do the thigns with the makin of the good graphics
    private GameObject healthObject;
    private PhotonNetworkManager master;
    private Image healthBar;
    private float currentHealth;
    private int playerID;
    private PhotonView photonView;
    private bool deathCalled;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        currentHealth = defaultHealth;
        if (gameObject.tag == "Player")
        {
            healthObject = GameObject.Find("Health Bar");
            master = GameObject.Find("Game Controller").GetComponent<PhotonNetworkManager>();
            healthBar = healthObject.GetComponent<Image>();
            playerID = photonView.viewID;
            deathCalled = false;
        } 
    }

    //Scales players health with a given scale upon transformation into a prop, and returns scale after reverting
    //Currently unimplemented
    public void ScaleHealth(bool hidden, float scale)
    {
        if (hidden)
            currentHealth *= scale;
        else
            currentHealth *= 1 / scale;
    }

    //Send target taking damage across clients
    public void SendDamage(int damage)
    {
        photonView.RPC("TakeDamage", PhotonTargets.All, damage);
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Target hit: current health = " + currentHealth +" Target Name" + gameObject.name);
        
        if (gameObject.tag == "Player" &&photonView.isMine)
        {
            Debug.Log("Health Shrink reached with fillamount "+healthBar.fillAmount);
            healthBar.fillAmount -= ((float)damage / defaultHealth);
        }

        if (currentHealth <= 0 && !deathCalled)
            Death();
    }

    void Death()
    {
        deathCalled = true;
        //Convert to ragdoll
        //Send death RPC to master client
        //Convert to flycam
        Debug.Log(transform.name +" destroyed");
        photonView.RPC("DeathEvent", PhotonTargets.All, playerID);
        
    }

    //Todo keep reference of flycam to destroy later
    [PunRPC]
    public void DeathEvent(int remoteID)
    {
        master.PlayerDeath(remoteID);
        if (photonView.isMine)
        {
            Transform pos = gameObject.transform;
            //Cancel invoke calls
            gameObject.GetComponent<PropSwitching>().Death();
            Destroy(gameObject);
            //Not working for whatever reason
            //GameObject flycam = (GameObject)Instantiate(Resources.Load("Flycam"), pos);
            GameObject flycam = (GameObject)Instantiate(Resources.Load("Flycam"));

        }
        else
        {
            Destroy(PhotonView.Find(remoteID).gameObject);
        }
    }
}
