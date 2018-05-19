using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Inventory : MonoBehaviour {



	private const int SLOTS = 5;

	private List<InventoryItem> Items = new List<InventoryItem>();

	public event EventHandler<InventoryEventArgs> ItemAdded;

	public void AddItem(InventoryItem item)
	{
		if (Items.Count < SLOTS)
		{
			Collider collider = (item as MonoBehaviour).GetComponent<Collider>();

			if (collider.enabled)
			{
				collider.enabled = false;

				Items.Add(item);

				item.OnPickup();

				if (ItemAdded != null)
				{
					ItemAdded(this, new InventoryEventArgs(item));
				}
			}
		}
	}
}
