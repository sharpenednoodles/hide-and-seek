using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Used to handle create game button objects
/// TO DO: Replace build index with direct scene name references
/// </summary>
public class CreateGameMenu : MonoBehaviour {


	public void StartGame()
	{
        //TO DO: reference scene name by string title
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex + 1);
		Debug.Log ("start game click ");
        //Disconnect from server for legacy reasons
        PhotonNetwork.Disconnect();
	}

	public void BackToMain()
	{
        //Currently deprecated
        //TO DO: remove function
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex - 1);
		Debug.Log ("back game click");
	}

}
