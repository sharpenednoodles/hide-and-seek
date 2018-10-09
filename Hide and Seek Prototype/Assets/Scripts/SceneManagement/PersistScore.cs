using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Script to store player data between scene loads
/// Keeps the player Class populated with data using the save and load functions
/// Used in combination with DontDestroyOnLoad
/// 
/// </summary>
public class PersistScore : MonoBehaviour {

    private static bool LOADED = false;
    [System.Serializable]
    public class Players
    {
        public int actorID;
        public string name;
        public bool isAlive;
        public int score;
    }
    public List<Players> savedPlayers;
    //Default game stats
    public bool previouslyJoined = false;
    public int roundNumber = 1;

    //We want to keep this object between scene loads
    private void Awake()
    {
        if (!LOADED)
        {
            DontDestroyOnLoad(this.gameObject);
            LOADED = true;
            Debug.Log("Persist Score not being destroyed");
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            //StartCoroutine(Respawn());
            Debug.Log("Persist score already present");
        }
    }

    // Use this for initialization
    void Start ()
    {
        savedPlayers = new List<Players>();
	}

    //Use to reset score data
    public void ResetScore()
    {
        roundNumber = 1;
        previouslyJoined = false;
        ClearList();
    }

    public void SaveToList(List<PhotonNetworkManager.Players> list)
    {
        foreach (PhotonNetworkManager.Players player in list)
        {
            Players newPlayer = new Players
            {
                actorID = player.actorID,
                isAlive = true,
                name = player.name,
                score = player.score,
            };
            savedPlayers.Add(newPlayer);
        }
    }

    //For debugging saved list contents
    public void VerifySavedList()
    {
        Debug.Log("<color=yellow>Verify Saved list has " + savedPlayers.Count + " elements</color>");
        foreach (Players player in savedPlayers)
        {
            Debug.Log("<color=yellow>Name " + player.name+"</color>");
            Debug.Log("<color=yellow>ID " + player.actorID+"</color>");
            Debug.Log("<color=yellow>Score " + player.score + "</color>");
        }
    }

    public List<PhotonNetworkManager.Players> LoadFromList()
    {
        List<PhotonNetworkManager.Players> list = new List<PhotonNetworkManager.Players>();
        foreach (Players player in savedPlayers)
        {
            PhotonNetworkManager.Players newPlayer = new PhotonNetworkManager.Players
            {
                actorID = player.actorID,
                isAlive = true,
                name = player.name,
                score = player.score,
            };
            list.Add(newPlayer);
        }
        return list;
    }

    public void ClearList()
    {
        savedPlayers.Clear();
    }
	
}
