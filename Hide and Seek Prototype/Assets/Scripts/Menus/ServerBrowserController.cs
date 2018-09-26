using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the server browsing functionality for the menus
/// </summary>

public class ServerBrowserController : Photon.MonoBehaviour {

    [SerializeField] [Tooltip("Prefab for menu items")] private GameObject listMenuItem;
    [SerializeField] [Tooltip("Text item for loading screen")] private TMP_Text statusText;
    [SerializeField] [Tooltip("Gameobject containing the content feed")] private GameObject contentFeed;

    private RoomInfo[] roomList;
    public GameObject[] gameMenuItems;
    private bool debug = true;
    

	// Use this for initialization
	void Start ()
    {
        statusText.text = ("Now Loading...");
        PhotonNetwork.ConnectUsingSettings(PhotonNetworkManager.gameVersion);
        //RefreshRoomList();
	}

    //add function when player is offline in appropriate callback

    public virtual void OnConnectedToMaster()
    {
        Debug.Log("CONNECTED TO MASTER");
        //Join lobby to discover networked peers
        PhotonNetwork.JoinLobby();
    }

    public virtual void OnJoinedLobby()
    {
        Debug.Log("CONNECTED TO LOBBY");
        RefreshRoomList();
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void RefreshRoomList()
    {
        statusText.text = ("Refreshing Room List");
        if (debug)
            Debug.Log("Refreshing room list");

        if (contentFeed.transform.childCount != 0)
            ClearContentFeed();

        //Get room list
        roomList = PhotonNetwork.GetRoomList();
        
        //Give user feed back on connection/room status
        if (roomList.Length == 0)
            statusText.text = "No Servers Found";
        else
        {
            statusText.text = "";
            //Parse the room info array
            foreach (RoomInfo room in roomList)
            {
                Debug.Log(room.Name + " server discovered");
                GameObject listItem = Instantiate(listMenuItem, contentFeed.transform);
                
                //Set a reference to the server join button
                listItem.GetComponent<ServerBrowserConnect>().serverName = room.Name;
                //TODO: Update string to reflect maximum player count set in server
                listItem.GetComponentInChildren<Text>().text = room.Name +" (" +room.PlayerCount +"/20)";
           
            }
        }
    }

    void ClearContentFeed()
    {
        //clear previously found list entries
        foreach (Transform child in contentFeed.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
