using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SYstem for controlling player inventory
/// </summary>
public class InventoryController : MonoBehaviour {

    [System.Serializable]
    public class InventoryItem
    {
        public string name;
        public HideSeek.WeaponController.Weapon.ID ID;
        public Sprite weaponImage;
        public Sprite SelectedImage;
        public int slotNumber;
        public bool isDefault;
        public GameObject prefab;
        //These are used for rendering, no need to assign in inspector
        public GameObject highlight;
        public GameObject nonSelectedSlot;
        public GameObject selectedSlot;
    }
    [Header("Inventory Properties")]
    public List<InventoryItem> inventoryItems;
    public List<InventoryItem> currentInventory;
    [SerializeField] private GameObject InventoryPanel;
    [SerializeField] bool debug = true;

    const int MAX_SLOTS = 5;
    public int currentSlot = 0;
    private int nextSlot = 0;
    private int usedSlots = 0;

    [Header("Additional Parameters")]
    [SerializeField] private Sprite blankNonSelected;
    [SerializeField] private Sprite blankSelected;

    // Use this for initialization
    void Start ()
    {
        currentInventory = new List<InventoryItem>();

        //unused now
        if (currentInventory.Count > 0)
        {
            if (debug)
                Debug.Log("Inventory Already exists, refreshing");
            //ClearPlayerInventory();
        }
        //Initialise the players inventory object
        //InitialiseInventory();
	}

    //Called from Weapon Controller
    public void ScrollDownInput()
    {
        currentSlot += 1;
        if ((int)currentSlot > MAX_SLOTS - 1)
            currentSlot = 0;
        RefreshUI();
    }
    
    //Called from Weapon Controller
    public void ScrollUpInput()
    {
        currentSlot -= (1);
        if ((int)currentSlot < 0)
            currentSlot = MAX_SLOTS - 1;
        RefreshUI();
    }

    //Give our inventory slots data to work with
    private void InitialiseInventory()
    {
        if (debug)
            Debug.Log("Initialise Inventory");
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            //Debug.LogWarning(i);
            GameObject inventorySlot = InventoryPanel.transform.GetChild(i).transform.gameObject;
            InventoryItem itemToAdd = new InventoryItem
            {
                highlight = inventorySlot.transform.GetChild(0).transform.gameObject,
                nonSelectedSlot = inventorySlot.transform.GetChild(1).transform.gameObject,
                selectedSlot = inventorySlot.transform.GetChild(2).transform.gameObject,
                slotNumber = i,
            };
            currentInventory.Add(itemToAdd);
            currentInventory[i].nonSelectedSlot.GetComponent<Image>().sprite = blankNonSelected;
            currentInventory[i].selectedSlot.GetComponent<Image>().sprite = blankSelected;
        }
    }

    public void ClearPlayerInventory()
    {
        nextSlot = 0;
        usedSlots = 0;
        currentInventory.Clear();
        InitialiseInventory();
    }

    //Refreshes whole UI
    public void RefreshUI()
    {
        foreach(InventoryItem invent in currentInventory)
        {
            if (debug)
                Debug.Log("Refreshing UI");
            //Update highlights
            if (invent.slotNumber == currentSlot)
            {
                if (debug)
                    Debug.Log("Slot active");
                invent.highlight.SetActive(true);
                invent.nonSelectedSlot.SetActive(false);
                invent.selectedSlot.SetActive(true);
            }
            else
            {
                if(debug)
                    Debug.Log("Slot unactive");
                invent.highlight.SetActive(false);
                invent.nonSelectedSlot.SetActive(true);
                invent.selectedSlot.SetActive(false);
            }
        }
    }

    public void AddItem(HideSeek.WeaponController.Weapon.ID ID, bool refresh)
    {
        if (debug)
        {
            Debug.Log("Adding "+ID +" to player inventory");
        }
        if (usedSlots >= MAX_SLOTS)
        {
            Debug.Log("Inventory Full");
            return;
        }
        int AddID = WeaponIDToInventory(ID);
        currentInventory[nextSlot].name = inventoryItems[AddID].name;
        currentInventory[nextSlot].ID = inventoryItems[AddID].ID;
        currentInventory[nextSlot].weaponImage = inventoryItems[AddID].weaponImage;
        currentInventory[nextSlot].SelectedImage = inventoryItems[AddID].SelectedImage;
        currentInventory[nextSlot].nonSelectedSlot.GetComponent<Image>().sprite = currentInventory[nextSlot].weaponImage;
        currentInventory[nextSlot].selectedSlot.GetComponent<Image>().sprite = currentInventory[nextSlot].SelectedImage;
        if (refresh)
            RefreshUI();
        nextSlot += 1;
        usedSlots += 1;
    }

    //unimplemented
    public void DropItem()
    {
        //Can't just remove list entry
    }

    //Gives us every weapon
    public void FillInventory()
    {
        if (debug)
            Debug.Log("Filling Inventory");
        //Only works for number of available slots
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (debug)
                Debug.Log("Adding " +(HideSeek.WeaponController.Weapon.ID)i);
            AddItem((HideSeek.WeaponController.Weapon.ID)i, false);
        }
    }

    public void GiveUnarmed()
    {
        AddItem(HideSeek.WeaponController.Weapon.ID.unarmed, true);
    }
    //Returns the inventory item number from inventory item
    private int WeaponIDToInventory(HideSeek.WeaponController.Weapon.ID ID)
    {
        int intID = (int)ID;
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if ((int)inventoryItems[i].ID == intID)
                return i;
        }
        return 0;
    }

    public HideSeek.WeaponController.Weapon.ID GetIDFromSlot()
    {
        HideSeek.WeaponController.Weapon.ID ID = currentInventory[currentSlot].ID;
        return ID;
    }
}
