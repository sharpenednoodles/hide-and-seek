using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {

    //public Image images;
    //public Text count;
    public Transform InventoryHolder;
    /*public GameObject slot;

    public List<Sprite> ItemList = new List<Sprite>();

    void Start()
    {
        count.text = "999";
        slot.SetActive(true);
        slot1.sprite = ItemList[0];
    }

    public void SetImage(Sprite image)
    {
        slot.SetActive(true);
        slot1.sprite = image;
    }*/


    private const int SLOTS = 5;

    private static List<InventoryItem> Items = new List<InventoryItem>();

    public static event EventHandler<InventoryEventArgs> ItemAdded;

    void Start()
    {
        //image = GetComponent<Image>();
        Inventory.ItemAdded += InventoryScript_ItemAdded;
        Debug.Log("Call from Inventory Script Start");
    }


    // Inventory system related code
    private void InventoryScript_ItemAdded(object sender, InventoryEventArgs e)

    {
       
        Debug.Log("Call from Inventory Script Item added");

        foreach (Transform slot in InventoryHolder)
        {
            Image image = slot.GetComponent<Image>();
            Image image2 = slot.GetChild(1).GetComponent<Image>();
            


            if (image.enabled && image.sprite == null)
            {

                Debug.Log("Call from Inventory Script if condition");
                image.enabled = true;
                image.color = Color.white;
                image.sprite = e.Item.Image;
                //if(image.enabled && image.sprite == null)
                //{
                    Debug.Log("Call from Inventory Script second if");
                    image2.sprite = e.Item.Image2;
                    //image2.color = Color.white;
                //}


                break;
            }
        }
    }


    public static void AddItem(InventoryItem item)
    {
        if (Items.Count < SLOTS)
        {
            Debug.Log("Call from Inventory IF ITEM COUNT");
            Collider collider = (item as MonoBehaviour).GetComponent<Collider>();

            if (collider.enabled)
            {
                Debug.Log("Call from Inventory Collider Enabled");
                collider.enabled = false;

                Items.Add(item);

                item.OnPickup();

                if (ItemAdded != null)
                {
                    Debug.Log("Call from Inventory ItemAdded");
                    ItemAdded(Inventory.ItemAdded, new InventoryEventArgs(item));
                }
            }
        }
    }

    /*
        if (Input.GetAxis("Mouse ScrollWheel") > 0f ) // forward
        {
            minimap.orthographicSize++;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f ) // backwards
        {
            minimap.orthographicSize--;

        }
     */

    void Update()
    {
        //int i = 0;
        Image weapon1 = InventoryHolder.GetChild(0).GetChild(1).GetComponent<Image>();
        Image weapon2 = InventoryHolder.GetChild(1).GetChild(1).GetComponent<Image>();
        Image weapon3 = InventoryHolder.GetChild(2).GetChild(1).GetComponent<Image>();
        Image weapon4 = InventoryHolder.GetChild(3).GetChild(1).GetComponent<Image>();
        Image weapon5 = InventoryHolder.GetChild(4).GetChild(1).GetComponent<Image>();
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            weapon1.color = Color.white;
            weapon2.color = Color.clear;
            weapon3.color = Color.clear;
            weapon4.color = Color.clear;
            weapon5.color = Color.clear;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            weapon2.color = Color.white;
            weapon1.color = Color.clear;
            weapon3.color = Color.clear;
            weapon4.color = Color.clear;
            weapon5.color = Color.clear;

        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            weapon3.color = Color.white;
            weapon1.color = Color.clear;
            weapon2.color = Color.clear;
            weapon4.color = Color.clear;
            weapon5.color = Color.clear;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            weapon4.color = Color.white;
            weapon2.color = Color.clear;
            weapon3.color = Color.clear;
            weapon1.color = Color.clear;
            weapon5.color = Color.clear;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            weapon5.color = Color.white;
            weapon2.color = Color.clear;
            weapon3.color = Color.clear;
            weapon4.color = Color.clear;
            weapon1.color = Color.clear;
        }




        //wheel

        /*if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            if (i < 5)
            {
                Debug.Log("Call from Forward If");
                Image weapon1 = InventoryHolder.GetChild(i).GetChild(1).GetComponent<Image>();
                weapon1.color = Color.clear;
                i++;
                Image weapon2 = InventoryHolder.GetChild(i).GetChild(1).GetComponent<Image>();
                weapon2.color = Color.white;
                
            }
            else
            {
                Debug.Log("Call from Forward else");
                i = 0;
            }

        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {
            if (i > 0)
            {
                Debug.Log("Call from Backward If");
                Image weapon1 = InventoryHolder.GetChild(i).GetChild(1).GetComponent<Image>();
                weapon1.color = Color.clear;
                i--;
                Image weapon2 = InventoryHolder.GetChild(i).GetChild(1).GetComponent<Image>();
                weapon2.color = Color.white;
                
            }
            else
            {
                Debug.Log("Call from Backward else");

               i = 5;
            }

        }*/
    }







}
