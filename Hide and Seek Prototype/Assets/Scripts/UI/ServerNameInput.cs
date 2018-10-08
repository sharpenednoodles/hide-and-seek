using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles server name creation/saving
/// </summary>
public class ServerNameInput : MonoBehaviour {

    private TMP_InputField serverNameInput;
    private string serverName;
    // Use this for initialization
    void Start()
    {
        //Used for testing, be very careful with PlayerPrefs.DeleteAll();
        //PlayerPrefs.DeleteAll();
        serverNameInput = GetComponent<TMP_InputField>();

        string playerName = PlayerPrefs.GetString("Username", "Default Name");
        serverNameInput.text = PlayerPrefs.GetString("Servername", playerName +"'s Server");
        serverNameInput.characterLimit = 16;
    }

    private void OnGUI()
    {
        serverName = serverNameInput.text;
        PlayerPrefs.GetString("Servername", serverNameInput.text);
    }

    //Save our username to disk
    private void OnDestroy()
    {
        PlayerPrefs.SetString("Servername", serverName);
        PlayerPrefs.SetString("ServerToConnect", serverName);
    }
}
