using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Photon Netwrok manager handles all instances of rounds
/// </summary>
public class PhotonNetworkManager : Photon.MonoBehaviour
{
    [SerializeField] Text connectText;
    [SerializeField] GameObject player;
    //An array of spawn points for players to randomly spawn at
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] GameObject joinCam;

    //Time Declerations
    float gameTimer, roundTimer;

    [Header("Coordinate Range")]
    [SerializeField] int minX = -25;
    [SerializeField] int maxX = 25;
    [SerializeField] int minZ = -25;
    [SerializeField] int maxZ = 25;
    //Enter Game version here, this is to prevent different versions from connecting to the same servers
    static public string gameVersion = "Sprint 2 Week 2";

    public bool offlineMode = false, debug = true;
    private Text debugFeed;
    [Header("Game Stats")]
    public int[] alivePlayersList, connectedPlayersList, deadPlayersList;
    public int alivePlayers, connectedPlayers, deadPlayers;
    public enum gameState { 
        warmUp,
        ///<Sumary>
        ///Game state as players intially join the game. Players will be able to join the room during this state, but won't be able to score 
        ///</Sumary> 
        roundStart,
        /// <summary>
        /// This is the default game state, while in this state the round is active, players can score. 
        /// Players will be able to join, but as a flycam. Spawn occurs next round
        /// </summary>
        roundEnd,
        /// <summary>
        /// Current score displayed, temporary state until the next round begins
        /// </summary>
        matchEnd
        /// <summary>
        /// Final scoreboard, game ends after this state - players booted back to main menu
        /// </summary>
    }

    // Use this for initialization
    void Start ()
    {
        debugFeed = GameObject.Find("Debug Feed").GetComponent<Text>();

        if (PhotonNetwork.offlineMode == true)
        {
            Debug.Log("Offline Mode");
            OnJoinedRoom();
        }
        else
        {
            //Specify game build verison
            PhotonNetwork.ConnectUsingSettings(gameVersion);
        }
        
	}

	
    //To do, migrate this to another scene before hand
    public virtual void OnConnectedToMaster()
    {
        Debug.Log("Connected to master server");
        PhotonNetwork.JoinOrCreateRoom("test", null, null);
    }

    public virtual void OnJoinedRoom()
    {
        //Old spawn method, to be migrated to spawn points
        Debug.Log("Connected to room");
        int randX = Random.Range(minX, maxX);
        int randZ = Random.Range(minZ, maxZ);
        int randYRot = Random.Range(0, 360);
   
        PhotonNetwork.Instantiate(player.name, new Vector3(randX, 2, randZ), Quaternion.Euler(0, randYRot, 0), 0);
        //Switch from Server Cam to Player Cam
        joinCam.SetActive(false);
        Debug.Log("Spawned at "+ randX + ",5," + randZ +" with rotation " +randYRot);
    }


    // Update is called once per frame
    void Update ()
    {
        //Debug output
        connectText.text = PhotonNetwork.connectionStateDetailed.ToString();
        //Remove this before final build
        gameTimer += Time.deltaTime;
	}

    public void PlayerDeath(int playerID)
    {
        if (PhotonNetwork.isMasterClient)
        {
            //Store death
        }
    }
}
