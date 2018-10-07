using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Class used to pass information about the players current facing direction
/// Triggers props
/// Health Pickups
/// Weapon Pickups
/// TBA - Ammo pickups
/// </summary>
public class InteractionController : Photon.MonoBehaviour {

    [SerializeField] bool debug = true;
    [Header("Prop Properties")]
    [SerializeField] private int propTimeToHold = 1;
    [Header("Health Pickup Properties")]
    [SerializeField] private int healthTimeToHold = 2;
    private float startTime = 0, holdTime = 0;
    private int propPlayerID;

    private Health health;
    private InventoryController inventoryController;
    private PropSwitching propController;
    //GUI Elements
    private GameObject interactCanvasParent;
    private GameObject weaponCanvas;
    private GameObject propCanvas;
    private GameObject healthCanvas;
    private Image propTimer;
    private Image healthTimer;

    private GameObject aimedAt;
    private float yRot;
    private GameObject colouredObject;
    private bool clearedColour = true;

    //Pickup references
    PropInfo propInfo;
    WeaponPickup weaponPickup;
    HealthPickup healthPickup;

    //Bool variables where we have a bool for each tpye, as well as na interation in general
    public bool HealthPickup, PropSwitch, WeaponPickup, Interaction, LookingAt;


	// Use this for initialization
	void Start ()
    {
        health = GetComponent<Health>();
        propController = GetComponent<PropSwitching>();
        inventoryController = GameObject.Find("InventoryHolder").GetComponent<InventoryController>();
        //Fetch GUI
        interactCanvasParent = GameObject.Find("InteractionCanvas");
        propCanvas = interactCanvasParent.transform.GetChild(0).gameObject;
        weaponCanvas = interactCanvasParent.transform.GetChild(1).gameObject;
        healthCanvas = interactCanvasParent.transform.GetChild(2).gameObject;

        propTimer = propCanvas.transform.GetChild(2).gameObject.GetComponent<Image>();
        healthTimer = healthCanvas.transform.GetChild(2).gameObject.GetComponent<Image>();

        if (photonView.isMine)
            InvokeRepeating("FindInteractions", 0.1f, 0.1f);
    }

    // Update is called once per frame
    void Update ()
    {
        //Reset GUI after no longer looking at object
		if (Interaction && !LookingAt)
        {
            Interaction = false;
            ResetInterationState();
        }

        //Propswitching
        if (PropSwitch)
        {
            propTimer.fillAmount = holdTime / propTimeToHold;
            if (Input.GetMouseButton(1))
            {
                holdTime += Time.deltaTime;
                propPlayerID = photonView.viewID;
            }
            else holdTime = 0;

            if (holdTime >= propTimeToHold)
            {
                propController.PropSwitch(propPlayerID, propInfo.prefabName, yRot);
                ResetInterationState();
                holdTime = 0;
            }
        }

        //Weapon Pickup
        if (WeaponPickup)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                inventoryController.AddItem(weaponPickup.weaponID, true);
                ResetInterationState();
                weaponPickup.DestroyObject();
            }
        }

        //Health Pickup
        if (HealthPickup)
        {
            healthTimer.fillAmount = holdTime / healthTimeToHold;
            if (Input.GetKey(KeyCode.E))
            {
                holdTime += Time.deltaTime;
            }
            else holdTime = 0;

            if (holdTime > healthTimeToHold)
            {
                health.AddHealth(healthPickup.healthAmount);
                healthPickup.DestroyObject();
                ResetInterationState();
                holdTime = 0;
            }
        }
    }

    //Disable UI states
    private void ResetInterationState()
    {
        PropSwitch = false;
        propCanvas.SetActive(false);
        WeaponPickup = false;
        weaponCanvas.SetActive(false);
        HealthPickup = false;
        healthCanvas.SetActive(false);
        ClearColor();
        clearedColour = true;
    }

    private void ClearColor()
    {
        if (colouredObject != null)
            colouredObject.GetComponent<Renderer>().material.color = Color.white;
    }

    public void FindInteractions()
    {
        RaycastHit hit;
        Transform cam = Camera.main.transform;
        Ray ray = new Ray(cam.position, cam.forward);
        //Todo set layer mask to reduce execution time

        if (Physics.Raycast(ray, out hit, 3.0f))
        {
            Interaction = true;
            LookingAt = true;
            aimedAt = hit.collider.gameObject;
            if (hit.collider.tag == "Prop")
                PropInteraction(aimedAt);
            if (hit.collider.tag == "WeaponPickup")
                WeaponInteraction(aimedAt);
            if (hit.collider.tag == "HealthPickup")
                HealthInteraction(aimedAt);
        }
        else
            LookingAt = false;
    }

    //Colour our highlighted objects
    private void SetHighLightColour(GameObject interact)
    {
        if (clearedColour)
        {
            colouredObject = interact;
            interact.GetComponent<Renderer>().material.color = Color.red;
            clearedColour = false;
        }
    }

    //Do the interactions
    private void PropInteraction(GameObject interact)
    {
        if (debug)
            Debug.Log("Looking at Prop");
        propInfo = interact.GetComponent<PropInfo>();
        //Check if prop
        if (propInfo != null)
        {
            //Do the GUI things
            propCanvas.SetActive(true);

            SetHighLightColour(interact);
            
            yRot = interact.transform.rotation.y;
            PropSwitch = true;
        }
    }

    //Weapon Pickup
    private void WeaponInteraction(GameObject interact)
    {
        if (debug)
            Debug.Log("Looking at Weapon Pickup");
        weaponPickup = interact.GetComponent<WeaponPickup>();
        //Check if Weapon Pickup
        if (weaponPickup != null)
        {
            WeaponPickup = true;
            //Do GUI things
            weaponCanvas.SetActive(true);

            SetHighLightColour(interact);
            WeaponPickup = true;
        }
    }

    //Health Pickup
    private void HealthInteraction(GameObject interact)
    {
        if (debug)
            Debug.Log("Looking at Health pickup");
        healthPickup = interact.GetComponent<HealthPickup>();
        //Check if health pickup
        if (healthPickup != null)
        {
            HealthPickup = true;
            //Do GUI things
            healthCanvas.SetActive(true);
            SetHighLightColour(interact);
            HealthPickup = true;
        }
    }

    //Call to cancel invoke
    //UNUSED
    public void Death()
    {
        CancelInvoke();
    }
}
