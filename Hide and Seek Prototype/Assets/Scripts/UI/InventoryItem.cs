using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



    public interface InventoryItem
    {

        string Name { get; }

        Sprite Image { get; }
        Sprite Image2 { get; }

    int Count { get; }

        void OnPickup();

    }

    public class InventoryEventArgs : EventArgs
    {
        public InventoryEventArgs(InventoryItem item)
        {

            Debug.Log("Call from InventoryEvent");
            Item = item;
        }

        public InventoryItem Item;
    }



