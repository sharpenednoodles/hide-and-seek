using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interacts with the master game object
/// </summary>
public class ZoneController : Photon.MonoBehaviour {

    public bool debug = true;
    //animators, self explanatory
    //TODO: Wrap within 2D array
    [Header("Animator Groups")]
    [SerializeField] private Animator[] maintenanceZone;
    [SerializeField] private Animator[] retailZone;
    [SerializeField] private Animator[] warehouseZone;
    [SerializeField] private Animator[] residentialZone;
    [SerializeField] private Animator[] parkZone;

    [SerializeField] private GameObject[] maintenanceCollider;
    [SerializeField] private GameObject[] retailCollider;
    [SerializeField] private GameObject[] warehouseCollider;
    [SerializeField] private GameObject[] residentialCollider;
    [SerializeField] private GameObject[] parkCollider;

    List<int[]> orderLists;

    private int[] shutdownList;
    private bool ShutDownTriggered;
    private const int TOTAL_ZONES = 5;
    private int zonesShut = 0;

    //Should this even be public
    public enum Zone
    {
        residential,
        /// <summary>
        /// The residential zone area
        /// </summary>
        park,
        /// <summary>
        /// The park zone area
        /// </summary>
        maintenance,
        /// <summary>
        /// The maintenace zone area
        /// </summary>
        retail,
        /// <summary>
        /// The retail zone area
        /// </summary>
        warehouse,
        /// <summary>
        /// The warehouse zone area
        /// </summary>
        error,
        /// <summary>
        /// Not a zone type, just for error handling
        /// </summary>
    }

    private PhotonNetworkManager master;

    void Start ()
    {
        master = FindObjectOfType<PhotonNetworkManager>();
        orderLists = new List<int[]>();
        BuildOrderLists();
	}
	
    private void BuildOrderLists()
    {
        //Build order lists from Graph code
        //
        //

        //For now we will just use predetermined
        int[] array0 = new int[5] { 4, 3, 2, 1, 0 };
        int[] array1 = new int[5] { 4, 3, 2, 0, 1 };
        int[] array2 = new int[5] { 3, 4, 2, 0, 1 };
        int[] array3 = new int[5] { 0, 1, 3, 2, 4 };
        int[] array4 = new int[5] { 1, 4, 0, 3, 2 };
        int[] array5 = new int[5] { 0, 4, 1, 2, 3 };
        int[] array6 = new int[5] { 1, 0, 3, 4, 2 };
        int[] array7 = new int[5] { 0, 1, 2, 3, 4 };
        int[] array8 = new int[5] { 1, 0, 2, 4, 3 };
        int[] array9 = new int[5] { 1, 4, 3, 2, 0 };

        //add to list for generation order
        orderLists.Add(array0); orderLists.Add(array1);
        orderLists.Add(array2); orderLists.Add(array3);
        orderLists.Add(array4); orderLists.Add(array5);
        orderLists.Add(array6); orderLists.Add(array7);
        orderLists.Add(array8); orderLists.Add(array9);
    }
	
    //Randomly select from the order lists
    private int[] SelectOrder()
    {
        int selection = Random.Range(0, orderLists.Count);
        if (debug)
        {
            Debug.Log("Building Zone Shutdown Order");
            Debug.Log((Zone)orderLists[selection][0]+", "+ (Zone)orderLists[selection][1] + ", " + (Zone)orderLists[selection][2] + ", " + (Zone)orderLists[selection][3] + ", " + (Zone)orderLists[selection][4]);
        }
            
        return orderLists[selection];
    }

    public Zone ZoneShutDown()
    {
        Zone ZoneToClose;
        if (!ShutDownTriggered)
        {
            ShutDownTriggered = true;
            shutdownList = SelectOrder();
        }

        //Array out of index error here after all zones have been shut
        if (zonesShut > shutdownList.Length - 1)
        {
            //Don't want to read beyond array bounds
            ZoneToClose = Zone.error;
        }
        else
        {
            ZoneToClose = (Zone)shutdownList[zonesShut];
        }
            
        if (debug)
        {
            Debug.Log("Shutting Down " + ZoneToClose +" next");
        }

        //We want this to be 1 behind so we have time to warn the players
        if (zonesShut >= 1)
        {
            if (debug)
                Debug.Log("Shutting down previous zone " +(Zone)shutdownList[zonesShut-1]);
            //ZoneToClose = (Zone)shutdownList[zonesShut-1];
            photonView.RPC("ToggleZone", PhotonTargets.AllBufferedViaServer, true, (byte)shutdownList[zonesShut-1]);
            //ToggleZone(false, (Zone)shutdownList[zonesShut]);
        }
        zonesShut += 1;
        return ZoneToClose;
    }

    //Called from PhotonNetworkManager (master)
    //Default state of doors being open in scene!
    [PunRPC]
    public void ToggleZone(bool close, byte zone)
    {
        if (debug)
        {
            Debug.Log("ToggleZoneController RPC called with " +(Zone)zone);
        }
        switch ((Zone)zone)
        {
            case Zone.residential:
                if (debug)
                    Debug.Log("Toggling Residential Zone");
                foreach (Animator animate in residentialZone)
                {
                    animate.SetBool("doorClose", close);
                }

                foreach (GameObject collide in residentialCollider)
                {
                    if (close)
                        collide.SetActive(false);
                    else
                        collide.SetActive(true);
                }
                break;

            case Zone.park:
                if (debug)
                    Debug.Log("Toggling Park Zone");
                foreach (Animator animate in parkZone)
                {
                    animate.SetBool("doorClose", close);
                }

                foreach (GameObject collide in parkCollider)
                {
                    if (close)
                        collide.SetActive(false);
                    else
                        collide.SetActive(true);
                }
                break;

            case Zone.maintenance:

                if (debug)
                    Debug.Log("Toggling Maintenance Zone");
                foreach (Animator animate in maintenanceZone)
                {
                    animate.SetBool("doorClose", close);
                }

                foreach (GameObject collide in maintenanceCollider)
                {
                    if (close)
                        collide.SetActive(false);
                    else
                        collide.SetActive(true);
                }
                break;
            case Zone.retail:

                if (debug)
                    Debug.Log("Toggling Retail Zone");
                foreach (Animator animate in retailZone)
                {
                    animate.SetBool("doorClose", close);
                }

                foreach (GameObject collide in retailCollider)
                {
                    if (close)
                        collide.SetActive(false);
                    else
                        collide.SetActive(true);
                }
                break;
            case Zone.warehouse:

                if (debug)
                    Debug.Log("Toggling Warehouse Zone");
                foreach (Animator animate in warehouseZone)
                {
                    animate.SetBool("doorClose", close);
                }

                foreach (GameObject collide in warehouseCollider)
                {
                    if (close)
                        collide.SetActive(false);
                    else
                        collide.SetActive(true);
                }
                break;
        }
    }
}
