using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///This script handles player network spawns locally for the local and remotely connected players.
///ON connection, any control scripts from remote players (stored in playerControlScripts[]) will be disabed
///Same for cameras
///This is to prevent Unity from becoming confused
///TODO - Add offline mode
/// </summary>



public class PlayerNetwork : MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject mapCamera;
    //scripts we want to be disabled for other player
    [SerializeField] private MonoBehaviour[] playerControlScripts;
    //Head bone is connected to then, neck bone
    [SerializeField] private GameObject headBoneRoot;

    private PhotonNetworkManager master;
    private PhotonView photonView;
    public AlivePlayers alivePlayers;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        GameObject alive = GameObject.Find("Alive");
        PhotonNetworkManager master = GameObject.Find("Game Controller").GetComponent<PhotonNetworkManager>();
        //Communicate to server when we are connected and whether we are alive or dead
        //alivePlayers = alive.GetComponent<AlivePlayers>();
        Initialise();
    }

    //handle local and global objects/scripts
    private void Initialise()
    {
        if (photonView.isMine)
        {
            //Functionality for player
            this.name = "Local Player";
            PhotonNetwork.playerName = PlayerPrefs.GetString("Username", "Default Name");
            //uncomment this after
            //alivePlayers.callRPC();

            //Shrink our head bone so we don't see it
            HideHead();
            //Debug.Log("Local Hello World from " +photonView.viewID);

        }

        //Handle functionality for other players
        else
        {
            //Use Photon IDs later
            this.name = "Remote Player";
            //Debug.Log("Remote Hello World from " +photonView.viewID);

            playerCamera.gameObject.SetActive(false);
            mapCamera.gameObject.SetActive(false);
            //like a for loop but I'm lazy - disable control scripts
            foreach (MonoBehaviour m in playerControlScripts)
            {
                m.enabled = false;
            }
        }
    }

    public void HideHead()
    {
        Debug.Log("Hide Head Called");
        headBoneRoot.transform.localScale = new Vector3(0, 0, 0);
    }

    public PhotonNetworkManager.PlayerData SendPlayerData()
    {
        PhotonNetworkManager.PlayerData playerData = new PhotonNetworkManager.PlayerData
        {
            //Will use players name as tag eventually
            //tag = photonView.viewID.ToString(),
            playerID = photonView.viewID,
            playerModel = gameObject.transform.GetChild(1).gameObject,
            weapons = gameObject.transform.GetChild(3).gameObject,
            pistol = gameObject.transform.GetChild(3).gameObject.transform.GetChild(0).gameObject,
            lightningGun = gameObject.transform.GetChild(3).gameObject.transform.GetChild(1).gameObject,
            minigun = gameObject.transform.GetChild(3).gameObject.transform.GetChild(2).gameObject,
            isAlive = true
        };
        return playerData;
    }
}


