using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameVersionDisplay : MonoBehaviour {

    [SerializeField] private Text GameVersion;

    private void Start()
    {
 
        Debug.Log("Game verion:" +PhotonNetworkManager.gameVersion);
        GameVersion.text = PhotonNetworkManager.gameVersion;

    }
}
