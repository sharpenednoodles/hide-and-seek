using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetwork : MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject mapCamera;
    //scripts we want to be disabled for other player
    [SerializeField] private MonoBehaviour[] playerControlScripts;

    private PhotonView photonView;
    int rand;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();

        rand = Random.Range(0, 100);

        Initialise();
        RPCTest1(rand);
        RPCTest2(rand);
        
    }

    //handle local and global objects/scripts
    private void Initialise()
    {
        if (photonView.isMine)
        {
            //Functionality for player
            this.name = "Local Player";
            Debug.Log("Local Hello World from " +photonView.viewID);
        }

        //Handle functionality for other players
        else
        {
            //Use Photon IDs later
            this.name = "Remote Player";
            Debug.Log("Remote Hello World from " +photonView.viewID);
            playerCamera.gameObject.SetActive(false);
            mapCamera.gameObject.SetActive(false);
            //like a for loop but I'm lazy - disable control scripts
            foreach (MonoBehaviour m in playerControlScripts)
            {
                m.enabled = false;
            }
        }
    }

    public void CallRemoteMethod()
    {
        photonView.RPC("RPCTest1", PhotonTargets.AllBufferedViaServer, rand);
        photonView.RPC("RPCTest2", PhotonTargets.OthersBuffered, rand);
    }

    public void Update()
    {
        if (Input.GetKeyDown("k"))
        {
            RPCTest1(rand);
            RPCTest2(rand);
        }
    }

    [PunRPC]
    public void RPCTest1(int rand)
    {
        if (photonView.isMine)
        {
            Debug.Log("RPC Test1 Local: Random = " + rand + " from " + photonView.viewID);
        }

        if (!photonView.isMine)
        {
            Debug.Log("RPC Test1 Networked: Random = " + rand + " from " + photonView.viewID);
        }
    }

    [PunRPC]
    public void RPCTest2(int rand)
    {
        if (photonView.isMine)
        {
            Debug.Log("RPC Test2 Local: Random =" + rand + " from " + photonView.viewID);
        }

        if (!photonView.isMine)
        {
            Debug.Log("RPC Test2 Networked: Random =" + rand + " from " + photonView.viewID);
        }
    }

}
