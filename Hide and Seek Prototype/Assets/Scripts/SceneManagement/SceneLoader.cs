using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Basic Script for managing loading the spaceship
/// 
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private int scene;
    [SerializeField] private Text loadingText;

    private void Start()
    {
        loadingText.text = "Now Loading...";
        StartCoroutine(LoadNewScene());
    }
    //Updates once per frame
    void Update()
    {
        //Fancy loading effect, change transparency over time
        loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, Mathf.PingPong(Time.time, 1));
    }

    //The coroutine runs on its own at the same time as Update()
    IEnumerator LoadNewScene()
    {
        // Start an asynchronous operation to load the scene
        AsyncOperation async = SceneManager.LoadSceneAsync(scene);

        //Continue waiting until it's done.
        while (!async.isDone)
        {
            yield return null;
        }
    }
}