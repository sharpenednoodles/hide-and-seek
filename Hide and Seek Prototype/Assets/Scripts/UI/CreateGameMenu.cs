using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateGameMenu : MonoBehaviour {


	public void StartGame()
	{

		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex + 1);
		Debug.Log ("start game click ");
	}

	public void BackToMain()
	{
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex - 1);
		Debug.Log ("back game click");
	}

}
