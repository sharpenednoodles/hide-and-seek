using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotonNetworkManager : Photon.MonoBehaviour
{
    [SerializeField] Text connectText;
    [SerializeField] GameObject player;
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject joinCam;

    [Header("Coordinate Range")]
    [SerializeField] int minX = -25;
    [SerializeField] int maxX = 25;
    [SerializeField] int minZ = -25;
    [SerializeField] int maxZ = 25;
    //Enter Game version here, this is to prevent different versions from connecting to the same servers
    static public string gameVersion = "Release 1 Warehouse BlockIn";
  
    // Use this for initialization
    void Start ()
    {
        //Specify game build verison
        PhotonNetwork.ConnectUsingSettings(gameVersion);
	}
	
    public virtual void OnConnectedToMaster()
    {
        Debug.Log("Connected to master server");
        PhotonNetwork.JoinOrCreateRoom("test", null, null);
    }

    public virtual void OnJoinedRoom()
    {
        Debug.Log("Connected to room");
        int randX = Random.Range(minX, maxX);
        int randZ = Random.Range(minZ, maxZ);
        int randYRot = Random.Range(0, 360);
   
        PhotonNetwork.Instantiate(player.name, new Vector3(randX, 2, randZ), Quaternion.Euler(0, randYRot, 0), 0);
        //Switch from Server Cam to Player Cam
        joinCam.SetActive(false);
        Debug.Log("Spawned at "+ randX + ",5," + randZ +" with rotation " +randYRot);
    }


    // Update is called once per frame
    void Update ()
    {
        //Debug output
        connectText.text = PhotonNetwork.connectionStateDetailed.ToString();
	}
}
