using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Photon Netwrok manager handles all instances of rounds
/// </summary>
/// 


public class PhotonNetworkManager : Photon.MonoBehaviour
{
    private PlayerNetwork local;
    //experimental
    [System.Serializable]
    public class PlayerData
    {
        public string tag;
        public int playerID;
        public GameObject playerModel;
        public GameObject weapons;
        public GameObject propModel;
        public GameObject unarmed, pistol, lightningGun, minigun;
        public bool isProp, isAlive;
    }

    public Dictionary<int, PlayerData> playerData;

    [SerializeField] Text connectText;
    [SerializeField] GameObject player;
    //An array of spawn points for players to randomly spawn at
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] GameObject joinCam;
    PlayerData playa;

    //Time Declerations
    float gameTimer, roundTimer;

    [Header("Coordinate Range")]
    [SerializeField] int minX = -25;
    [SerializeField] int maxX = 25;
    [SerializeField] int minZ = -25;
    [SerializeField] int maxZ = 25;
    //Enter Game version here, this is to prevent different versions from connecting to the same servers
    static public string gameVersion = "Sprint 2 Week 3";
    
    public bool offlineMode = false, debug = true, useSpawnPoint = false;
    private Text debugFeed;
    [Header("Game Stats")]
    public int[] alivePlayersList, connectedPlayersList, deadPlayersList;
    public int alivePlayers, connectedPlayers, deadPlayers;
    public enum gameState
    {
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
    void Start()
    {
        PhotonNetwork.offlineMode = offlineMode;
        debugFeed = GameObject.Find("Debug Feed").GetComponent<Text>();
        playerData = new Dictionary<int, PlayerData>();

        if (PhotonNetwork.offlineMode == true)
        {
            Debug.Log("Offline Mode");
            //OnJoinedRoom();
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
        if (debug)
            Debug.Log("Connected to master server");
        PhotonNetwork.JoinOrCreateRoom("test", null, null);
    }

    public virtual void OnJoinedRoom()
    {

        SpawnPlayer();
        
    }


    // Update is called once per frame
    void Update()
    {
        //Debug output
        connectText.text = PhotonNetwork.connectionStateDetailed.ToString();
        //Remove this before final build
        gameTimer += Time.deltaTime;

    }

    //Announce player death
    public void PlayerDeath(int playerID)
    {
        if (PhotonNetwork.isMasterClient)
        {
            //Store death
        }
       debugFeed.text = ("You Died");
       StartCoroutine(RespawnTest());
    }

    //temporary, pls ignore
    private IEnumerator RespawnTest()
    {
        yield return new WaitForSeconds(5);
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        //Old spawn method, to be migrated to spawn points
        if (debug)
            Debug.Log("Connected to room");
        int randX = Random.Range(minX, maxX);
        int randZ = Random.Range(minZ, maxZ);
        int randYRot = Random.Range(0, 360);

        //Store a reference to our player
        GameObject playerRef;
        playerRef = PhotonNetwork.Instantiate(player.name, new Vector3(randX, 2, randZ), Quaternion.Euler(0, randYRot, 0), 0);
        //Switch from Server Cam to Player Cam
        joinCam.SetActive(false);
        if (debug)
            Debug.Log("Spawned at " + randX + ",5," + randZ + " with rotation " + randYRot);

        local = playerRef.GetComponent<PlayerNetwork>();
        RecieveData();
    }

    //Announce player variables to the master client
    //Consider renaming to make more sense
    public void RecieveData()
    {
        playa = local.SendPlayerData();
        playerData.Add(playa.playerID, playa);
        DebugData(playa.playerID);
    }

    void DebugData(int key)
    {
        Debug.Log("Object name to debug is " + playerData[key].playerModel.transform.name);
        Debug.Log("Pistol name to debug is " + playerData[key].pistol.transform.name);
    }
}

