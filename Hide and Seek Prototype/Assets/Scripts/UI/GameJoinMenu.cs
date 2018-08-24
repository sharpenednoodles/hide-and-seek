using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Basic script to handle player joining
/// Will handle server browsing, player names etc
/// TODO - Entire server browser
/// </summary>

public class GameJoinMenu : MonoBehaviour {

    public TMP_InputField playerName;
	// Use this for initialization
	void Start () {
        PhotonNetwork.playerName = PlayerPrefs.GetString("Username", "Default Name");
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(PlayerPrefs.GetString("Username"));
	}

    private void OnGUI()
    {
        PhotonNetwork.playerName = playerName.text;
        playerName.text = PhotonNetwork.playerName;
        PlayerPrefs.GetString("Username", playerName.text);
    }

    //Save our username to disk
    private void OnDestroy()
    {
        PlayerPrefs.SetString("Username", PhotonNetwork.playerName);
    }
}
