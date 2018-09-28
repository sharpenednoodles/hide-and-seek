﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/////////////////////////////////////////////////////////////////////////////////////////////////
/// DEPRECATED
/// <summary>
/// This is for release 1
/// 
///Will just set the amount of alive players to the canvas to show how many players are "alive"
///
/// No one can currently die though lol
/// 
/// To do: remove this and move to new system
/// DONE
/// </summary>

public class AlivePlayers : MonoBehaviour {

    public TextMeshProUGUI textMeshPro;
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        updateAlivePlayers();
    }

    [PunRPC]
    public void updateAlivePlayers()
    {
        textMeshPro.SetText("{0}", PhotonNetwork.playerList.Length);
    }


    void OnPhotonPlayerDisconnected()
    {
        callRPC();
    }

    public void callRPC()
    {
        photonView.RPC("updateAlivePlayers", PhotonTargets.All);
    }


}
