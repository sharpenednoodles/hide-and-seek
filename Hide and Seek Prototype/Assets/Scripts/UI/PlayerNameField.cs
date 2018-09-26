using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Basic script to handle player name saving and networking
/// Attach to player name field
/// Maybe integrated into another script at some point
/// </summary>

public class PlayerNameField : MonoBehaviour {

    private TMP_InputField playerName;

	void Start ()
    {
        playerName = GetComponent<TMP_InputField>();
        PhotonNetwork.playerName = PlayerPrefs.GetString("Username", "Default Name");
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
