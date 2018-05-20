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


    //Enter Game version here, this is to prevent different versions from connecting to the same servers
    static public string gameVersion = "Network Test 0.1.1";



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
        int randX = Random.Range(-25, 25);
        int randZ = Random.Range(-25, 25);
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
