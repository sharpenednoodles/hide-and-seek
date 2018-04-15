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



    // Use this for initialization
    void Start ()
    {
        //Specify game build verison
        PhotonNetwork.ConnectUsingSettings("NetworkTest 0.1");
	}
	
    public virtual void OnConnectedToMaster()
    {
        Debug.Log("Connected to master server");
        PhotonNetwork.JoinOrCreateRoom("test", null, null);
    }

    public virtual void OnJoinedRoom()
    {
        Debug.Log("Connected to room");
        PhotonNetwork.Instantiate(player.name, spawnPoint.position, spawnPoint.rotation, 0);
        //Switch from Server Cam to Player Cam
        joinCam.SetActive(false);
    }


    // Update is called once per frame
    void Update ()
    {
        //Debug output
        connectText.text = PhotonNetwork.connectionStateDetailed.ToString();
	}
}
