using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Connect to specified server
/// Attached to server join list items
/// </summary>

public class ServerBrowserConnect : MonoBehaviour {

    //Server name to join
    public string serverName;

    
    public void ServerJoin()
    {
        PlayerPrefs.SetString("ServerToConnect", serverName);
        //TO DO: reference scene name by string title
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        PhotonNetwork.Disconnect();
    }

}
