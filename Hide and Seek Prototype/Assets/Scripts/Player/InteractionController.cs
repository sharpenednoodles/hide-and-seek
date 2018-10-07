using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used to pass information about the players current facing direction
/// Triggers props
/// Health Pickups
/// Weapon Pickups
/// TBA - Ammo pickups
/// </summary>
public class InteractionController : MonoBehaviour {

    private Health health;
    private InventoryController inventoryController;

	// Use this for initialization
	void Start ()
    {
        health = GetComponent<Health>();
        inventoryController = GameObject.Find("InventoryHolder").GetComponent<InventoryController>();

	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
