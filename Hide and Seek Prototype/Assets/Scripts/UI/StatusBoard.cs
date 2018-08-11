using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StatusBoard : MonoBehaviour {

	public GameObject statusBoard;

	public static bool statusBoardState = false;
	// Use this for initialization
	void Start () 
	{
		statusBoard.SetActive (false);
	}

	public void HideStatus()
	{
		statusBoard.SetActive (false);
		Debug.Log ("Call From status hide");

	}
	public void ShowStatus()
	{
		statusBoard.SetActive (true);
		Debug.Log ("Call From status show");
	}

	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown(KeyCode.Tab)) {
			if (statusBoardState) 
			{
				HideStatus ();
				Debug.Log ("Call From if state hide");
			} 
			else 
			{
				ShowStatus ();
				Debug.Log ("Call From else state show");
			}
		}
		
	}
}
