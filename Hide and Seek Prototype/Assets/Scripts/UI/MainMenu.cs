using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	public void CreateGame()
	{
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex + 1);
		Debug.Log("Create Game Click");

	}

	public void ServerBroswer()
	{

		Debug.Log("Server Browser Click");
	}

	public void QuitGame()
	{
		Debug.Log ("Quit Game Click");
		Application.Quit ();
	}


}
