using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Photon Netwrok manager handles all instances of rounds
/// 
/// Handles game events and communication between networked clients
/// TODO - possibly shift events as a callback in a seperate file
/// 
/// TODO - Shift player data to photon hashtables to optimise netcode and local clientside excecution times
/// </summary>

public class PhotonNetworkManager : Photon.MonoBehaviour
{
    //Enter Game version here, this is to prevent different versions from connecting to the same servers
    static public string gameVersion = "Week 10 Testing build";

    private bool isSpawnable = true;
    const int ZONE_COUNT = 5;
    private PlayerNetwork local;
    private PersistScore persistScore;

    //experimental - NOW DEPRECATED
    //TODO - remove this
    //[System.Serializable]
    /*
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
    */

    //DEPRECATED
    //public Dictionary<int, PlayerData> playerData;
    //DEPRECATED
    //PlayerData playa;

    [SerializeField] private Text connectText;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject joinCam;

    [Header("Event Feed")]
    [SerializeField] [Tooltip("The game object containing the content feed from the Event feed")] private GameObject eventContentFeed;
    [SerializeField] [Tooltip("Event List Prefab")] private GameObject eventListPrefab;
    [SerializeField] [Tooltip("Event List Prefab, with timer of 1 second")] private GameObject eventListPrefabShort;

    [Header("Spawn Points")]
    [SerializeField] private bool useSpawnPoints = false;
    //SpawnPoints
    /// <summary>
    /// /To use:
    /// Create a new entry in the inspector for your required zone
    /// Create some empty game objects you wish to use as spawn locations
    /// Insert/attach them into the SpawnPoint array
    /// 
    /// Spawning is handled on the master client, so players will never share spawn locations
    /// </summary>
    
    [System.Serializable]
    public class SpawnPoint
    {
        [Tooltip("Zone Tag")]
        public string tag;
        [Tooltip("Array of Spawn Points for selected zone")]
        public GameObject[] SpawnPointArray;
    }
    [SerializeField] private List<SpawnPoint> spawnPoints;
    private List<Vector2> spawnList;

    [Header("Coordinate Range")]
    [SerializeField] int minX = -25;
    [SerializeField] int maxX = 25;
    [SerializeField] int minZ = -25;
    [SerializeField] int maxZ = 25;

    //might remove later
    private int remainingZones;

    //Time Decelerations
    float gameTimer, roundTimer;

    public bool offlineMode = false, debug = true;
    private bool previouslyJoined = false;
    private string roomName;
    private Text debugFeed;

    private static bool LOADED = false;

    [Header("Game Stats")]
    //Unused
    //public string[] alivePlayersList, connectedPlayersList, deadPlayersList;
    public int alivePlayers, connectedPlayers, deadPlayers;

    //The PhotonView ID associated with the local client
    public int currentID;

    //Player Data
    public class Players
    {
        public int actorID;
        public string name;
        public bool isAlive;
        public int score;
    }
    private List<Players> players;

    [Header("GUI Elements")]
    [SerializeField] private GameObject statusBoard;
    [SerializeField] private GameObject statusContentFeed;
    [SerializeField] private GameObject statusListItem;
    [SerializeField] private TextMeshProUGUI aliveDisplay, deadDisplay;
    [SerializeField] private GameObject victoryCanvas;
    [SerializeField] private GameObject defeatCanvas;


    [Header("Timer Settings")]
    [Tooltip("Specify the length of events in minutes")]
    [SerializeField] private float warmUpTime;
    [SerializeField] private float roundTime;



    //ZoneController
    private ZoneController zoneController;

    public enum GameState
    {
        warmUp,
        ///<summary>
        ///Game state as players initially join the game. Players will be able to join the room during this state, but won't be able to score 
        ///</summary> 
        roundStart,
        /// <summary>
        /// This is the default game state, while in this state the round is active, players can score. 
        /// Players will be able to join, but as a flycam. Spawn occurs next round
        /// </summary>
        zoneShutoff,
        /// <summary>
        /// This occurs when a zone is shut off
        /// </summary>
        waitForPlayer,
        /// <summary>
        /// This is the game state that occurs if there is still more than one player remaining on the map with only 1 zone remaining.
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

    //Specify the event code being received
    public enum EventType
    {
        playerJoin,
        /// <summary>
        /// Occurs when a new player connects to the server during the warmup phase
        /// </summary>
        playerDisconnect,
        /// <summary>
        /// Occurs when a player disconnects from the current room
        /// </summary>
        playerDeath,
        /// <summary>
        /// Occurs when a player is killed by another player
        /// Todo: store the player who got the kill
        /// </summary>
        timer
        /// <summary>
        /// A timer event, such as a round or a zone shutoff
        /// </summary>
    }

    //Default state
    private GameState currentState = GameState.warmUp;

    
    //Trigger Player Spawn after game end
    private void Awake()
    {
        if (!LOADED)
        {
            LOADED = true;
            Debug.Log("Saved Network Manager on Load");
        }
        else
        {
            StartCoroutine(Respawn());
            Debug.Log("PhotonNetworkManager already loaded!");
        }
    }
    

    // Use this for initialization
    void Start()
    {
        PhotonNetwork.offlineMode = offlineMode;
        //debugFeed = GameObject.Find("Debug Feed").GetComponent<Text>();
        //playerData = new Dictionary<int, PlayerData>();
        spawnList = new List<Vector2>();
        players = new List<Players>();
        remainingZones = ZONE_COUNT;
        zoneController = GetComponent<ZoneController>();
        //Synchronise Scene Loading
        PhotonNetwork.automaticallySyncScene = true;
        persistScore = FindObjectOfType<PersistScore>();

        if (persistScore == null)
        {
            gameObject.AddComponent<PersistScore>();
            persistScore = FindObjectOfType<PersistScore>();
        }

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

    //Hook into the connection to master method
    public virtual void OnConnectedToMaster()
    {
        if (debug)
            Debug.Log("OnConnectedToMaster Hook");
        //Connect to saved room name from player prefs
        //There IS  a better native way to do this
        //Stay connected between scene loads
        //Currently NOT doing this

        //Get string name
        roomName = PlayerPrefs.GetString("ServerToConnect", "test");
        //Connect to room 
        PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
        Debug.Log("Connecting to " + roomName +" room");
    }

    
    public virtual void OnConnectedToLobby()
    {
        if (debug)
            Debug.Log("OnConnectedToLobby Hook");
        //Todo, join using selected room
        roomName = PlayerPrefs.GetString("ServerToConnect", "test");
        PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
    }

    //Run on connection to room
    public virtual void OnJoinedRoom()
    {
        if (debug)
            Debug.Log("OnJoinedRoom Hook");
        PhotonNetwork.playerName = PlayerPrefs.GetString("Username", "Default Name");
        GetGameState();
    
        if (currentState == GameState.warmUp)
            SpawnPlayer();
        else
        {
            //debugFeed.text = ("Spectating mode");
            UpdateEventFeed("Match Already in progress");
            UpdateEventFeed("Entering Spectate mode until next round");
            //Spawn flycam
        }
    }

    //Call disconnect stuff
    public virtual void OnDisconnect()
    {
        //Send disconnect message
        //if host disconnects, kick everyone back to the main menu
        if (PhotonNetwork.isMasterClient)
        {
            photonView.RPC("SetGameState", PhotonTargets.AllViaServer, (byte)currentState, (byte)EventType.playerDisconnect, currentID);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug output
        connectText.text = PhotonNetwork.connectionStateDetailed.ToString();
        //Remove this before final build
        gameTimer += Time.deltaTime;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //Event Feed

    //TODO - Add additional formatting and iconography to distiguish event types
    //Populate the event feed with messages
    private void UpdateEventFeed(string eventMessage)
    {
        GameObject listItem = Instantiate(eventListPrefab, eventContentFeed.transform);
        listItem.transform.SetAsFirstSibling();
        listItem.GetComponentInChildren<Text>().text = eventMessage;
    }
    //Same as above, but shorter display time
    private void UpdateEventFeedShort(string eventMessage)
    {
        GameObject listItem = Instantiate(eventListPrefabShort, eventContentFeed.transform);
        listItem.transform.SetAsFirstSibling();
        listItem.GetComponentInChildren<Text>().text = eventMessage;
    }

    //Call this function to activate event driven messages
    private void EventCountDown(string eventMessage, float time)
    {
        for (int i = 0; i < time; i++)
        {
            StartCoroutine(EventCountDownTimer(time, i, eventMessage));
        }
    }

    //Timer count down messages DO NOT CALL DIRECTLY
    private IEnumerator EventCountDownTimer(float totalTime, float currTime, string eventMessage)
    {
        float timeToPrint = totalTime - currTime;
        yield return new WaitForSeconds((float)(currTime));
        UpdateEventFeedShort(eventMessage + ": " + timeToPrint);
    }

    //PUN Callback for timer with delay
    private void EventCountDownTimerDelayNetwork(float time, string eventMessage, float delayTime)
    {
        StartCoroutine(EventCountDownTimerDelay(time, eventMessage, delayTime));
    }

    //Callback for above method with send delay DO NOT CALL DIRECTLY
    private IEnumerator EventCountDownTimerDelay(float time, string eventMessage, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        EventCountDown(eventMessage, time);
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //Game States

    //Run on each client to get the current state of the game from the master
    //Currently only excecuted on the master client
    private void GetGameState()
    {
        //Pares connected player list
       // connectedPlayersList = (string)PhotonNetwork.playerList;
        if (PhotonNetwork.isMasterClient)
        {
            StartCoroutine(EventTimer(warmUpTime, currentState));
            //debugFeed.color = Color.red;
            Debug.Log("Sending Gamestate as " + currentState);
            if (photonView == null)
                Debug.LogError("Photon View missing on master object");
            else
            photonView.RPC("SetGameState", PhotonTargets.AllBufferedViaServer, (byte)currentState, (byte)EventType.timer, currentID);
            //StartCoroutine(EventTimer(warmUpTime, currentState));
        }
        else
        {
            Debug.Log("Recieving Gamestate from master");
        }
    }

    [PunRPC]
    private void SetGameState(byte gameState, byte eventType, int ID)
    {
        //Cast from byte to enum
        currentState = (GameState)gameState;

        switch((EventType)eventType)
        {
            //Only the master will ever send these event types
            case EventType.timer:
                if (debug)
                    Debug.Log("Set Game State: Timer");
                SetGameStateEvent(currentState);
                break;

            case EventType.playerDeath:
                if (debug)
                    Debug.Log("Set Game State: Player Death");

                //If warmup, we don't care
                if (currentState == GameState.warmUp)
                {
                    UpdateEventFeed(PhotonPlayer.Find(ID).NickName + " did a bad job");
                    GameObject temp = PhotonView.Find(local.viewID).gameObject;
                    PhotonNetwork.Destroy(temp);
                    SpawnPlayer();
                    return;
                }
            
                UpdateEventFeed(PhotonPlayer.Find(ID).NickName +" has been Eliminated");
                int index = GetIndexUsingActorID(ID);
                
                //Set player to dead in score
                players[index].isAlive = false;
                alivePlayers -= 1;
                deadPlayers = +1;
                UpdateGUI();
                SpawnFlyCam(ID);

                //Destroy Player
                GameObject player = PhotonView.Find(local.viewID).gameObject;
                PhotonNetwork.Destroy(player);

                if (currentState != GameState.warmUp)
                {
                    if (alivePlayers <= 1)
                    {
                        EndGame();
                    }
                }
                break;

            case EventType.playerJoin:
                if (debug)
                {
                    Debug.Log("Set Game State: Player Join");
                }

                alivePlayers = +1;
                UpdateGUI();
                //player join code
                //new player data
                Players newPlayer = new Players
                {
                    actorID = ID,
                    name = PhotonPlayer.Find(ID).NickName,
                    isAlive = true,
                    score = 0,
                };

                UpdateEventFeed(newPlayer.name+" has joined the game");
                //add to list item
                players.Add(newPlayer);
                //VerifySavedList();
                break;

            case EventType.playerDisconnect:
                if (debug)
                    Debug.Log("Set Game State: Player Disconnect");
                //player disconnect code
                UpdateEventFeed("Player has disconnected");
                deadPlayers -= 1;
                UpdateGUI();
                break;
        }

        Debug.Log("Recieving Gamestate from master as " + currentState);
        //thing to call appropriate function for game states
    }

    //For debugging list contents
    public void VerifySavedList()
    {
        Debug.Log("Verify has " + players.Count + " elements");
        foreach (Players player in players)
        {
            Debug.Log("Name " + player.name);
            Debug.Log("ID " + player.actorID);
        }
    }

    //Set game state from SetGameState Function
    private void SetGameStateEvent(GameState State)
    {
        //Update our reference to current state
        currentState = State;
        if (debug)
            Debug.Log("Set Game State()");
        switch (State)
        {
            case GameState.zoneShutoff:
                //Maybe remove this once zone handling is complete
                UpdateEventFeed("Zone has been shutoff");
                break;
            case GameState.warmUp:
                //debugFeed.text = ("Now Warming Up");
                UpdateEventFeed("Now Warming Up");
                break;
            case GameState.roundStart:
                //debugFeed.text = ("The game has started");
                
                //Respawn player
                GameObject player = PhotonView.Find(local.viewID).gameObject;
                PhotonNetwork.Destroy(player);
                SpawnPlayer();

                UpdateEventFeed("The Game has started");
                break;
            case GameState.matchEnd:
                //debugFeed.text = ("The round has now ended");
                UpdateEventFeed("The round has now ended");
                break;
            case GameState.waitForPlayer:
                UpdateEventFeed("Eliminate the remaining players!");
                break;
        }
    }

    //RPC Callback for remote scripts attached to master
    public void SetGameStateRemote(byte gameState, byte eventType)
    {
        photonView.RPC("SetGameState", PhotonTargets.AllBufferedViaServer, gameState, eventType, currentID);
    }

    private void SpawnFlyCam(int actorID)
    {
        if (currentID == actorID)
        {
            Debug.Log("Spawning Flycam");
            GameObject flycam = (GameObject)Instantiate(Resources.Load("Flycam"));
        }
        else
            Debug.Log("Remote Flycam Spawn Ignored");
    }

    //DEPRECATED
    //Announce player death
    //TODO - replace with generic event type
    public void PlayerDeath(int playerID)
    {
        if (PhotonNetwork.isMasterClient)
        {
            
        }
        //debugFeed.text = ("You Died");
        UpdateEventFeed(PhotonPlayer.Find(playerID).NickName + " died!");
        StartCoroutine(RespawnTest());
    }

    //Restart the match
    private IEnumerator EndGame()
    {
        foreach (Players player in players)
        {
            if (player.isAlive == true)
            {
                DisplayCanvas(currentID, "victory");
                player.score += 1;
            } 
        }
        persistScore.SaveToList(players);
        yield return new WaitForSeconds(5);
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);

    }

    //Local function to give player feedback
    //V basic, needs fixing and prettifying
    private void DisplayCanvas(int ID, string canvasName)
    {
        if (canvasName == "victory")
        {
            victoryCanvas.SetActive(true);
        }

        if (canvasName == "defeat")
        {
            defeatCanvas.SetActive(true);
        }
    }

    //EXPERIMENTAL
    private IEnumerator RespawnTest()
    {
        //Add some test data
        Players newPlayer = new Players
        {
            actorID = 69,
            name = "TERRIBLE NAME",
            isAlive = true,
            score = 100,
        };
        players.Add(newPlayer);
        persistScore.SaveToList(players);
        yield return new WaitForSeconds(5);
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
    }

    private IEnumerator Respawn()
    {
        Debug.Log("Respawn Pinged");
        yield return new WaitForSeconds(3);
        ClearSpawnLists();
        SpawnPlayer();

        players = persistScore.LoadFromList();
        persistScore.VerifySavedList();
    }

    //Events timers
    private IEnumerator EventTimer(float time, GameState game)
    {
        //convert to seconds
        time *= 60;

        yield return new WaitForSeconds(time);
        //call helper function with parameters
        switch (game)
        {
            case GameState.warmUp:
                Debug.Log("Event Timer: warm up complete");
                //Want to call the next method to shut down the zone etc etc
                float timeToWait = roundTime / ZONE_COUNT;

                photonView.RPC("SetGameState", PhotonTargets.All, (byte)GameState.roundStart, (byte)EventType.timer, currentID);
                StartCoroutine(EventTimer(timeToWait, GameState.roundStart));
                Debug.Log("Event Timer: Round start");
                break;

            case GameState.roundStart:
                Debug.Log("Event Timer: Zone Shutoff");

                ZoneController.Zone zoneToShutdownNext;
                zoneToShutdownNext = zoneController.ZoneShutDown();
                photonView.RPC("AnnounceNextZone", PhotonTargets.AllBuffered, (byte)zoneToShutdownNext);
                remainingZones -= 1;
                timeToWait = roundTime / ZONE_COUNT;

                StartCoroutine(EventTimer(timeToWait, GameState.roundStart));
                if (remainingZones == 1)
                {
                    photonView.RPC("SetGameState", PhotonTargets.All, (byte)GameState.waitForPlayer, (byte)EventType.timer, currentID);
                    if (debug)
                        Debug.Log("All zones shut off, waiting for players");
                }
                break;
         
                //Unused cases - probably remove them
            case GameState.roundEnd:
                Debug.Log("Event Timer: round end");
                break;
            case GameState.matchEnd:
                Debug.Log("Event Timer: match end");
                break;
        }
    }

    //Announces zone shutdown to all players
    [PunRPC]
    private void AnnounceNextZone(byte zoneToShutdownNext)
    {
        switch ((ZoneController.Zone)zoneToShutdownNext)
        {
            case ZoneController.Zone.maintenance:
                int timerWarning = 10;
                float timeToWait = ((roundTime / ZONE_COUNT) * 60 - timerWarning);
                UpdateEventFeed("Maintenance Zone shutting down");
                EventCountDownTimerDelayNetwork(timerWarning, "Maintenance Zone Closing in", timeToWait);
                break;

            case ZoneController.Zone.park:
                timerWarning = 10;
                timeToWait = ((roundTime / ZONE_COUNT) * 60 - timerWarning);
                UpdateEventFeed("Park Zone shutting down");
                EventCountDownTimerDelayNetwork(timerWarning, "Park Zone Closing in", timeToWait);
                break;

            case ZoneController.Zone.residential:
                timerWarning = 10;
                timeToWait = ((roundTime / ZONE_COUNT) * 60 - timerWarning);
                UpdateEventFeed("Residential Zone shutting down");
                EventCountDownTimerDelayNetwork(timerWarning, "Residential Zone Closing in", timeToWait);
                break;

            case ZoneController.Zone.retail:
                timerWarning = 10;
                timeToWait = ((roundTime / ZONE_COUNT) * 60 - timerWarning);
                UpdateEventFeed("Retail Zone shutting down");
                EventCountDownTimerDelayNetwork(timerWarning, "Retail Zone Closing in", timeToWait);
                break;

            case ZoneController.Zone.warehouse:
                timerWarning = 10;
                timeToWait = ((roundTime / ZONE_COUNT) * 60 - timerWarning);
                UpdateEventFeed("Warehouse Zone shutting down");
                EventCountDownTimerDelayNetwork(timerWarning, "Warehouse Zone Closing in", timeToWait);
                break;
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //GUI Functions

    //Call this to update GUI indicators
    private void UpdateGUI()
    {
        aliveDisplay.SetText("{0}", alivePlayers);
        deadDisplay.SetText("{0}", deadPlayers);
    }

    public void UpdateStatusBoard()
    {
        foreach (Players player in players)
        {
            //Clear Old Entries
            if (statusContentFeed.transform.childCount != 0)
            {
                foreach (Transform child in statusContentFeed.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            //Create new entry
            GameObject listItem = Instantiate(statusListItem, statusContentFeed.transform);
            listItem.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().SetText(player.name);
            listItem.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().SetText("{0}", player.score);
            if (player.isAlive)
            {
                //If alive, we enable the alive text item
                listItem.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                //if not alive, we're dead, so enable the dead text item
                listItem.transform.GetChild(2).transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //Player Spawn Code
    public void SpawnPlayer()
    {
        currentID = PhotonNetwork.player.ID;
        if (debug)
            Debug.Log("Preparing to spawn Player with ID " +currentID);
        

        //Store a reference to our player
        GameObject playerReference;
        if (useSpawnPoints)
        {
            //Code to handle player spawning
            //TODO, add IEnumerator to check wheter spawn is handled correctly
            Debug.Log("Requesting Spawn from master");
            Debug.Log("Player View ID requesting spawn = " + currentID);
            photonView.RPC("RequestSpawn", PhotonTargets.MasterClient, currentID);
        }
        else
        {
            int randX = Random.Range(minX, maxX);
            int randZ = Random.Range(minZ, maxZ);
            int randYRot = Random.Range(0, 360);
            playerReference = PhotonNetwork.Instantiate(player.name, new Vector3(randX, 2, randZ), Quaternion.Euler(0, randYRot, 0), 0);
            //Switch from Server Cam to Player Cam
            joinCam.SetActive(false);
            if (debug)
                Debug.Log("Spawned at " + randX + ",5," + randZ + " with rotation " + randYRot);

            local = playerReference.GetComponent<PlayerNetwork>();
        }
    }

    //Call to request a player spawn
    [PunRPC]
    public void RequestSpawn(int remoteID)
    {
        if (PhotonNetwork.isMasterClient)
        {
            //Handle undefined inspector properties
            if (spawnPoints.Count == 0)
            {
                Debug.LogError("The Spawn Point class is empty, please populate the class or disable spawn points");
                return;
            }
            int spawnZone = Random.Range(0, spawnPoints.Count);

            if (spawnPoints[spawnZone].SpawnPointArray.Length == 0)
            {
                Debug.LogError("The Spawn Point class entry " + spawnPoints[spawnZone].tag + " has no spawn points defined. Please populate the array or disable spawn points");
                return;
            }
            int spawnArea = Random.Range(0, spawnPoints[spawnZone].SpawnPointArray.Length);

            //validate whether our generated spawn is unique
            if (ValidateSpawn(spawnZone, spawnArea))
            {
                
                photonView.RPC("PerformSpawn", PhotonTargets.All, spawnZone, spawnArea, remoteID);
            }
            else
            {
                //Recursively call until we generate a valid spawn area
                if (debug)
                    Debug.Log("Coordinates in use, requesting another spawn");
                RequestSpawn(remoteID);
            }
        }
        else
            Debug.LogError("You should never see this, only the master client may run RequestSpawn()");
    }

    //Only ever run by master client
    private bool ValidateSpawn(int spawnZone, int spawnArea)
    {
        Vector2 spawnVec = new Vector2(spawnZone, spawnArea);
        
        if (spawnList.Contains(spawnVec))
        {
            if (debug)
                Debug.Log("Player has already spawned here");
            return false;
        }
        else
        {
            spawnList.Add(spawnVec);
            if (debug)
                Debug.Log("Succesful verification: Adding to spawn list");
            return true;
        }
    }

    //Only excecutes on the player who has been authorised to spawn by the master
    [PunRPC]
    public void PerformSpawn(int spawnZone, int spawnArea, int ID)
    {
        if (debug)
        {
            Debug.Log("<color=green>Perform Spawn Recieved</color>");
            Debug.Log("Current ID = " + currentID + " Remote ID = " + ID);
        }
            
        if (currentID == ID)
        {
            if (debug)
            {
                Debug.Log("Spawning Player at " + spawnPoints[spawnZone].tag + " in postion " + spawnArea +" with ID " +ID);
            }
            joinCam.SetActive(false);
            Transform locationToSpawn = spawnPoints[spawnZone].SpawnPointArray[spawnArea].transform;
            GameObject playerReference = PhotonNetwork.Instantiate(player.name, locationToSpawn.position, Quaternion.Euler(0, locationToSpawn.transform.rotation.y, 0), 0);
            local = playerReference.GetComponent<PlayerNetwork>();
           
            //Debug stuff
            //UpdateEventFeed("My Photon View ID = " +currentID);
            //Method to send player data
            if (!previouslyJoined)
            {
                Debug.Log("Welcome Message");
                previouslyJoined = true;
                photonView.RPC("SetGameState", PhotonTargets.AllBufferedViaServer, (byte)GameState.warmUp, (byte)EventType.playerJoin, currentID);

                UpdateEventFeed("Welcome to " + roomName + " " + PhotonNetwork.player.NickName);
                //UpdateEventFeed("Welcome to " + roomName);

                if (PhotonNetwork.isMasterClient)
                    UpdateEventFeed("You are the Master Player");
            }
        }
        else
            Debug.Log("<color=green>Perform Spawn Ignored (not target player)</color>");
    }

    //Clears the temporary lists
    private void ClearSpawnLists()
    {
        spawnList.Clear();
    }

    //Probably unneeded now
    public List<Players> GetPlayerList()
    {
        return players;
    }

    //Gives us full reference of the player Class based on photonViewID
    //Slow, do not use this anymore
    private int GetIndexUsingActorID (int actorID)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].actorID == actorID)
                return i;
        }
        return 0;
    }
}

