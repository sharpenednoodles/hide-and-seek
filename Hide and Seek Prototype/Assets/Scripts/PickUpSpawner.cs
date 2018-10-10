using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used to generate pickups in the scene
/// </summary>
public class PickUpSpawner : Photon.MonoBehaviour {

    [SerializeField] private bool debug = true;
    [System.Serializable]
    public class Weapons
    {
        public string name;
        public GameObject model;
    }

    [System.Serializable]
    public class AmmoBoxes
    {
        public string name;
        public GameObject model;
    }

    [System.Serializable]
    public class SpawnPoints
    {
        public string name;
        public Transform[] spawnPoints;
    }

    [Header("Spawn Points")]
    public List<SpawnPoints> spawnPoints;

    [Header("Weapons")]
    public List<Weapons> weapons;
    [Header("Ammo Boxes")]
    public List<AmmoBoxes> ammoBoxes;
    [Header("Health Crates")]
    [SerializeField] private GameObject healthCrate;


    private int numberOfWeapons;
    private int numberOfAmmo;
    private int numberOfHealth;
    private int totalSpawnPoints;
    int[] positions;

    List<Vector2> generated;
    List<Vector2> previous;

    //Spawn all the things
    private void SpawnFromGeneration()
    {
        //Spawn weapon pickups
        for (int i = 0; i < numberOfWeapons; i ++)
        {
            //Debug.Log("Weapon " +i);
            PhotonNetwork.Instantiate(weapons[Random.Range(0, weapons.Count)].model.name, GetValue(generated[i]).position, GetValue(generated[i]).rotation, 0);
        }
        //spawn ammo pickups
        for (int i = numberOfWeapons; i < numberOfWeapons+numberOfAmmo; i++)
        {
            //Debug.Log("Ammo " + i);
            PhotonNetwork.Instantiate(ammoBoxes[Random.Range(0, ammoBoxes.Count)].model.name, GetValue(generated[i]).position, GetValue(generated[i]).rotation, 0);
        }
        //spawnPoints health pickps
        for (int i = numberOfWeapons+numberOfAmmo; i < numberOfWeapons+numberOfAmmo+numberOfHealth; i++)
        {
            //Debug.Log("Health Box " + i);
            PhotonNetwork.Instantiate(healthCrate.name, GetValue(generated[i]).position, GetValue(generated[i]).rotation, 0);
        }
    }
	
    //Read transform from vector2
    private Transform GetValue(Vector2 pos)
    {
        return spawnPoints[(int)pos.x].spawnPoints[(int)pos.y];
    }

    //Spawn the pickups
    public void SpawnPickups(int playerCount)
    {
        CalculateQuantities(playerCount);
    }

    //Tweak spawn properties here
    private void CalculateQuantities(int playerCount)
    {
        numberOfWeapons = (Random.Range(2, playerCount * (3/2))) * 2;
        numberOfAmmo = (Random.Range(2, playerCount * 2)) * 2;
        numberOfHealth = Random.Range(2, playerCount * 3);

        if (debug)
        {
            Debug.Log("Spawning " + numberOfWeapons +" number of weapons");
            Debug.Log("Spawning " + numberOfAmmo + " number of ammo pickups");
            Debug.Log("Spawning " + numberOfHealth + " number of health pickups");
        }

        GenerateSpawnLocationList(numberOfWeapons + numberOfAmmo + numberOfHealth);
        SpawnFromGeneration();
    }

    private List<Vector2> GenerateSpawnLocationList(int totalPositions)
    {
        generated = new List<Vector2>();
        previous = new List<Vector2>();
        Vector2 newEntry = new Vector2(0, 0);
        
        for (int i = 0; i < totalPositions; i++)
        {

            newEntry.x = Random.Range(0, spawnPoints.Count);
            newEntry.y = Random.Range(0, spawnPoints[(int)newEntry.x].spawnPoints.Length);

            while (previous.Contains(newEntry))
            {
                newEntry.x = Random.Range(0, spawnPoints.Count);
                newEntry.y = Random.Range(0, spawnPoints[(int)newEntry.x].spawnPoints.Length);
            }
            previous.Add(newEntry);
            generated.Add(newEntry);
        }
        return generated;
    }
}


