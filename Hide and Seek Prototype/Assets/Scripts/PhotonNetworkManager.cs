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

    //TO COMPLETE
    //Recreation of player data on new game
    //Not start game until 2 or more players enter
    //retart of timer on game restart
    //death when in shut off location
    //kill scoring

public class PhotonNetworkManager : Photon.MonoBehaviour
{
    //Enter Game version here, this is to prevent different versions from connecting to the same servers
    static public string gameVersion = "Week 10 Testing build";

    private bool isSpawnable = true;
    const int ZONE_COUNT = 5;
    private PlayerNetwork local;
    private PersistScore persistScore;

    [SerializeField] private Text connectText;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject joinCam;

    [Header("Event Feed")]
    [SerializeField] [Tooltip("The game object containing the content feed from the Event feed")] private GameObject eventContentFeed;
    [SerializeField] [Tooltip("Event List Prefab")] private GameObject eventListPrefab;
    [SerializeField] [Tooltip("Event List Prefab, with timer of 1 second")] private GameObject eventListPrefabShort;

    //SpawnPoints
    [Header("Spawn Points")]
    [SerializeField] private bool useSpawnPoints = true;
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

    //For randomised spawn - don't use once spawnPoints have been set
    [Header("Coordinate Range")]
    [SerializeField] int minX = -25;
    [SerializeField] int maxX = 25;
    [SerializeField] int minZ = -25;
    [SerializeField] int maxZ = 25;

    //might remove later
    private int remainingZones;

    //Time Decelerations
    float gameTimer, roundTimer;

    [Header("Debug Properties")]

    [SerializeField] private bool debug = true;
    [SerializeField] private bool eventToDebug = true;
    public bool offlineMode = false;
    private bool previouslyJoined = false;
    private string roomName;
   
    private static bool LOADED = false;

    [Header("Game Stats")]
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
    private CFX_SpawnSystem FX;

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

    #region State Definitions
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
        /// This occurs when a zone is shut off. This is really a substate of roundStart
        /// </summary>
        waitForPlayer,
        /// <summary>
        /// This is the game state that occurs if there is still more than one player remaining on the map with only 1 zone remaining.
        /// </summary>
        roundEnd,
        /// <summary>
        /// Current score displayed, temporary state until the next round begins
        /// Will trigger a timer across clients to reload the world
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
    #endregion

    //Default state - will update as the game progresses
    public GameState globalState = GameState.warmUp;

    //Keep scoreData struct available between scene loads
    private void Awake()
    {
        if (!LOADED)
        {
            LOADED = true;
            Debug.Log("Loaded Scene into memory");
        }
        else
        {
            StartCoroutine(Respawn());
            Debug.Log("Starting Respawn Coroutine");
        }
    }
    
    //Use this for initialization
    void Start()
    {
        PhotonNetwork.offlineMode = offlineMode;
        //debugFeed = GameObject.Find("Debug Feed").GetComponent<Text>();
        //playerData = new Dictionary<int, PlayerData>();
        spawnList = new List<Vector2>();
        players = new List<Players>();
        remainingZones = ZONE_COUNT;
        zoneController = GetComponent<ZoneController>();
        FX = gameObject.transform.GetChild(2).gameObject.GetComponent<CFX_SpawnSystem>();
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

    #region PhotonCallbacks
    //Hook into the connection to master method
    public virtual void OnConnectedToMaster()
    {
        if (debug)
            Debug.Log("OnConnectedToMaster Hook");
        connectText.text = "Connected to Master Server";
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
        connectText.text = "Joined Room " +roomName;
        GetGameState();
    
        //THIS WILL NOT WORK AS WE DO NOT KNOW ANYTHING ABOUT THE SERVER YET
        if (globalState == GameState.warmUp)
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
            photonView.RPC("SetGameState", PhotonTargets.AllViaServer, (byte)globalState, (byte)EventType.playerDisconnect, currentID);
        }
    }

    //Called if client is unexpectedly disconnected from the server
    public virtual void OnConnectionFail()
    {
        connectText.text = ("Connection Failed");
        Debug.LogError("Disconnected from server!");
        UpdateEventFeed("You have been Disconnected!");
        StartCoroutine(LoadMainMenu(5));
    }

    //Called if no server connection could be established
    public virtual void OnFailedToConnectToPhoton()
    {
        connectText.text = ("Connection Failed");
        Debug.LogError("Unable to connect to Master Server!");
        UpdateEventFeed("Please check your internet connection");
        UpdateEventFeed("Unable to connect to Master Server!");
        StartCoroutine(LoadMainMenu(5));
    }

    public IEnumerator LoadMainMenu(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        SceneManager.LoadScene(0);
    }

    #endregion
    #region EventFeed

    //TODO - Add additional formatting and iconography to distiguish event types
    //Populate the event feed with messages
    private void UpdateEventFeed(string eventMessage)
    {
        if (eventToDebug)
            Debug.Log("<color=blue> Feed: "+eventMessage +"</color>");
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

    #endregion
    #region GameStates

    //Run on each client to get the current state of the game from the master
    //Currently only excecuted on the master client
    private void GetGameState()
    {
        //Pares connected player list
       // connectedPlayersList = (string)PhotonNetwork.playerList;
        if (PhotonNetwork.isMasterClient)
        {
            StartCoroutine(EventTimer(warmUpTime, globalState));
            //debugFeed.color = Color.red;
            if (debug)
                Debug.Log("Sending current gamestate as " + globalState);
            if (photonView == null)
                Debug.LogError("Photon View missing on master object");
            else
            photonView.RPC("SetGameState", PhotonTargets.AllBufferedViaServer, (byte)globalState, (byte)EventType.timer, currentID);
            //StartCoroutine(EventTimer(warmUpTime, currentState));
        }
        else
        {
            Debug.Log("Recieving Gamestate From Master Client");
        }
    }

    //Sets the game state when recieved from master
    [PunRPC]
    private void SetGameState(byte gameState, byte eventType, int ID)
    {
        switch((EventType)eventType)
        {
            //Only the master will ever send these event types
            case EventType.timer:
                if (debug)
                    Debug.Log("Set Game State: Timer");
                SetGameStateEvent((GameState)gameState, ID);
                break;

            case EventType.playerDeath:
                if (debug)
                    Debug.Log("Set Game State: Player Death");

                //If warmup, we don't care
                if (globalState == GameState.warmUp)
                {
                    Debug.Log("Player " + ID +" killed during warmup");
                    UpdateEventFeed(PhotonPlayer.Find(ID).NickName + " died during warmup");
                    if (currentID == ID)
                    {
                        GameObject temp = PhotonView.Find(local.viewID).gameObject;
                        PhotonNetwork.Destroy(temp);
                        SpawnPlayer();
                    }
                    return;
                }
            
                UpdateEventFeed(PhotonPlayer.Find(ID).NickName +" has been Eliminated");
                int index = GetIndexUsingActorID(ID);
                
                //Set player to dead in score
                players[index].isAlive = false;
                alivePlayers -= 1;
                deadPlayers = +1;
                UpdateGUI();

                //Destroy Player
                //Only perform on the dead player
                if (currentID == ID)
                {
                    if (debug)
                        Debug.Log("Destroying Self from " + currentID);
                    
                    GameObject player = PhotonView.Find(local.viewID).gameObject;
                    PhotonNetwork.Destroy(player);
                    DisplayCanvas(currentID, "defeat");
                    SpawnFlyCam(ID);
                }
                else
                {
                    if (debug)
                    {
                        Debug.Log("Recived player death but not directly contributing");
                    }
                }
                
                //Logic for ending the game
                if (PhotonNetwork.isMasterClient)
                {
                    if (globalState != GameState.warmUp)
                    {
                        if (debug)
                            Debug.Log("<color=red>Scanning to see if < 1 player remaining</color>");
                        if (alivePlayers <= 1)
                        {
                            if (debug)
                                Debug.Log("<color=red>Preparing to end the game</color>");
                            //StartCoroutine(EndGame());
                            photonView.RPC("SetGameState", PhotonTargets.AllBuffered, (byte)GameState.matchEnd, (byte)EventType.timer, currentID);
                        }
                    }
                    
                }
                break;

            case EventType.playerJoin:
                if (debug)
                {
                    Debug.Log("Set Game State: Player Join");
                }

                alivePlayers += 1;
                connectedPlayers += 1;
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
                UpdateEventFeed(PhotonPlayer.Find(ID).NickName +" has left the game");

                //Find player by ID
                index = GetIndexUsingActorID(ID);
                players.RemoveAt(index);
                deadPlayers -= 1;
                connectedPlayers -= 1;
                UpdateGUI();
                break;
        }

        Debug.Log("Recieving Gamestate from master as " + (GameState)gameState);
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
    private void SetGameStateEvent(GameState State, int ID)
    {
        //Update our reference to current state
        globalState = State;
        if (debug)
            Debug.Log("Set Game State()");
        switch (State)
        {
            case GameState.zoneShutoff:
                //Maybe remove this once zone handling is complete
                UpdateEventFeed("Zone has been shutoff");
                break;
            case GameState.warmUp:
                UpdateEventFeed("Now Warming Up");
                connectText.text = "Warm Up Round";
                break;
            case GameState.roundStart:
                //Find and destroy player
                //Respawn player afterwards
                if (debug)
                {
                    Debug.Log("Round Start Respawn: destory self reference "+local.viewID +" belonging to player " +currentID);
                }
                GameObject player = PhotonView.Find(local.viewID).gameObject;
                PhotonNetwork.Destroy(player);
                SpawnPlayer();

                //Reset the FX pool
                CFX_SpawnSystem.UnloadObjects(FX.GetComponent<CFX_PrefabPool>().muzzleFX);
                FX.Start();

                UpdateEventFeed("The Game has started");
                connectText.text = "";

                break;
            case GameState.matchEnd:
                //debugFeed.text = ("The round has now ended");
                UpdateEventFeed("The round has now ended");
                StartCoroutine(EndGame());
                break;
            case GameState.waitForPlayer:
                UpdateEventFeed("Eliminate the remaining players!");
                connectText.text = "Survive.";
                break;
        }
    }

    //RPC Callback for remote scripts attached to master
    public void SetGameStateRemote(byte gameState, byte eventType)
    {
        //Used to call from other scripts
        photonView.RPC("SetGameState", PhotonTargets.AllBufferedViaServer, gameState, eventType, currentID);
    }

    //RPC Callback sepcifically for death events
    public void SendGameKill(byte gameState, byte eventType, int targetID, int senderID)
    {
        photonView.RPC("SetGameState", PhotonTargets.AllBufferedViaServer, gameState, eventType, targetID);
        //Do stuff to give players more feedback
    }

    private void SpawnFlyCam(int actorID)
    {
        if (currentID == actorID)
        {
            Debug.Log("Spawning Flycam");
            UpdateEventFeed("Spawning Spectator Cam");
            connectText.text = "Spaectator Mode";
            GameObject flycam = (GameObject)Instantiate(Resources.Load("Flycam"));
        }
        else
            Debug.Log("Remote Flycam Spawn Ignored");
    }

    //Restart the match
    private IEnumerator EndGame()
    {
        if (debug)
            Debug.Log("Endgame Called");
        //Check to see which players are alive
        foreach (Players player in players)
        {
            if (player.isAlive == true)
            {
                //Check if we are the correct player
                if (player.actorID == currentID)
                    DisplayCanvas(currentID, "victory");
                player.score += 1;
            } 
        }
        persistScore.SaveToList(players);
        yield return new WaitForSeconds(5);
        //PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Local function to give player feedback
    //V basic, needs fixing and prettifying
    private void DisplayCanvas(int ID, string canvasName)
    {
        if (debug)
            Debug.Log("String recieved " + canvasName);
        if (canvasName == "victory")
        {
            if (debug)
                Debug.Log("<color=yellow>Creating Victory Canvas</color>");
            victoryCanvas.SetActive(true);
            return;
        }
        if (canvasName == "defeat")
        {
            if (debug)
                Debug.Log("<color=yellow>Creating defeat canvas</color>");
            defeatCanvas.SetActive(true);
            return;
        }
        Debug.LogWarning("Specified Canvas " + canvasName + " does not exist!");
    }

    //Respawn Coroutine
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
                if (zoneToShutdownNext != ZoneController.Zone.error)
                {
                    photonView.RPC("AnnounceNextZone", PhotonTargets.AllBuffered, (byte)zoneToShutdownNext);
                    timeToWait = roundTime / ZONE_COUNT;
                    StartCoroutine(EventTimer(timeToWait, GameState.roundStart));
                }
                    
                remainingZones -= 1;
                
                if (remainingZones < 0)
                {
                    Debug.Log("Remaining Zones < 0 call");
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
                UpdateEventFeed("Maintenance Zone shutting down soon");
                connectText.text = ("Prepare to evacuate the Maintenance Tunnel");
                EventCountDownTimerDelayNetwork(timerWarning, "Maintenance Zone closing in", timeToWait);
                break;

            case ZoneController.Zone.park:
                timerWarning = 10;
                timeToWait = ((roundTime / ZONE_COUNT) * 60 - timerWarning);
                UpdateEventFeed("Park Zone shutting down soon");
                connectText.text = ("Prepare to evacuate the Park");
                EventCountDownTimerDelayNetwork(timerWarning, "Park Zone closing in", timeToWait);
                break;

            case ZoneController.Zone.residential:
                timerWarning = 10;
                timeToWait = ((roundTime / ZONE_COUNT) * 60 - timerWarning);
                UpdateEventFeed("Residential Zone shutting down soon");
                connectText.text = ("Prepare to evacuate Residential");
                EventCountDownTimerDelayNetwork(timerWarning, "Residential Zone closing in", timeToWait);
                break;

            case ZoneController.Zone.retail:
                timerWarning = 10;
                timeToWait = ((roundTime / ZONE_COUNT) * 60 - timerWarning);
                UpdateEventFeed("Retail Zone shutting down soon");
                connectText.text = ("Prepare to evacuate Retail");
                EventCountDownTimerDelayNetwork(timerWarning, "Retail Zone closing in", timeToWait);
                break;

            case ZoneController.Zone.warehouse:
                timerWarning = 10;
                timeToWait = ((roundTime / ZONE_COUNT) * 60 - timerWarning);
                UpdateEventFeed("Warehouse Zone shutting down soon");
                connectText.text = ("Prepare to evacuate the Warehouse");
                EventCountDownTimerDelayNetwork(timerWarning, "Warehouse Zone closing in", timeToWait);
                break;
        }
    }

    #endregion
    #region GUI Functions

    //Call this to update GUI indicators
    private void UpdateGUI()
    {
        aliveDisplay.SetText("{0}", alivePlayers);
        deadDisplay.SetText("{0}", deadPlayers);
    }

    public void UpdateStatusBoard()
    {
        //Clear Old Entries
        if (statusContentFeed.transform.childCount != 0)
        {
            foreach (Transform child in statusContentFeed.transform)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (Players player in players)
        {
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
    #endregion
    #region PlayerSpawn Code
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
            photonView.RPC("RequestSpawn", PhotonTargets.MasterClient, currentID, previouslyJoined);
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
    public void RequestSpawn(int remoteID, bool remoteSpawnState)
    {
        if (PhotonNetwork.isMasterClient)
        {
            //Disable Spawn if the server isn't in warmup, and the peer is new
            if (globalState != GameState.warmUp && !remoteSpawnState)
            {
                if (debug)
                    Debug.Log("The Server is no longer warming up");
                photonView.RPC("RejectedSpawn", PhotonTargets.All, remoteID);
                return;
            }
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
                RequestSpawn(remoteID, remoteSpawnState);
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
            local.UpdateZoneLocation();
           
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

    [PunRPC]
    public void RejectedSpawn(int ID)
    {
        if (currentID == ID)
        {
            if (debug)
                Debug.Log("Spawn Rejected as server is no longer warming up");
            UpdateEventFeed("The Game has already begun");
            UpdateEventFeed("Joining Server as Spectator");
            SpawnFlyCam(ID);
        }
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
    #endregion
}