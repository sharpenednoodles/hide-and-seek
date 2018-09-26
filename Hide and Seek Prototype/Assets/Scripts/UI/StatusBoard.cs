using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manage the players scoreboard
/// </summary>
public class StatusBoard : MonoBehaviour {

    private bool statusBoardState = false;
    [SerializeField] private GameObject statusBoard;
    [SerializeField] private GameObject contentFeed;
    [SerializeField] private GameObject scoreListItem;



    private PhotonNetworkManager master;

	// Use this for initialization
	void Start () 
	{
        master = FindObjectOfType<PhotonNetworkManager>();
		statusBoard.SetActive (false);
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (statusBoardState)
            {
                HideStatus();
                Debug.Log("Call From if state hide");
            }
            else
            {
                ShowStatus();
                Debug.Log("Call From else state show");
            }
        }
    }

    public void HideStatus()
	{
		statusBoard.SetActive (false);
        statusBoardState = false;
		Debug.Log ("Call From status hide");

	}
	public void ShowStatus()
	{
		statusBoard.SetActive (true);
        master.UpdateStatusBoard();
		Debug.Log ("Call From status show");
        statusBoardState = true;
	}
   
}
