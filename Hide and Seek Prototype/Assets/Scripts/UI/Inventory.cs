using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {

    public Image slot1;
    public Text count;
    public GameObject slot;

    public List<Sprite> ItemList = new List<Sprite>();

    void Start()
    {
        count.text = "999";
        slot.SetActive(true);
        slot1.sprite = ItemList[0];
    }

}
