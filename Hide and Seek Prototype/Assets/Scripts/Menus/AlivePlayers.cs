﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// This is for release 1
/// Doesn't do much, actually it's a fucking joke
/// 
///Will just set the amount of alive players to the canvas to show how many players are "alive"
///
/// No one can currently die though lol
/// </summary>

public class AlivePlayers : MonoBehaviour {

    public TextMeshProUGUI textMeshPro;
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        updateAlivePlayers();
    }

    
    public void updateAlivePlayers()
    {
        textMeshPro.SetText("{0}", PhotonNetwork.playerList.Length);
    }


}