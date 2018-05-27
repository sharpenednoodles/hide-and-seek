using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {

    //public Image images;
    public Text count;
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

        
        //image.enabled = true;
        //image.sprite = e.Item.Image;
        //count.text = "999";
        //Transform InventoryHolder = transform.Find("InventoryPanel");
        foreach (Transform slot in InventoryHolder)
        {
            Image image = slot.GetChild(0).GetComponent<Image>();


           
            if (image.enabled && image.sprite == null)
            {

                Debug.Log("Call from Inventory Script if condition");
                image.enabled = true;
                image.sprite = e.Item.Image;

                //to do store a reference to the item

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












}
