using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handle room browsing
/// </summary>

public class RoomBrowser : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        //Scan for new rooms every 5 seconds
        InvokeRepeating("ScanRoom", 0, 5);
	}
	
    private void ScanRoom()
    {
        PhotonNetwork.GetRoomList();
    }
}
