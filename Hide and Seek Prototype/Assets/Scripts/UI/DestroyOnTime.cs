using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic script to destory a game object after a set time
/// </summary>
public class DestroyOnTime : MonoBehaviour {

    //Time in seconds until object is destroyed
    public float time;
	// Use this for initialization
	void Start ()
    {
        StartCoroutine(DestroyInTime(time));
	}
	
    private IEnumerator DestroyInTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
