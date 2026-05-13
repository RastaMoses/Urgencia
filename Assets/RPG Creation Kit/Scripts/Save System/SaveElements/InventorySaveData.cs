using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RPGCreationKit.SaveSystem;
using RPGCreationKit;

namespace RPGCreationKit.SaveSystem
{
    [Serializable]
    public class InventorySaveData
    {
        public List<ItemInInventory> items = new List<ItemInInventory>();

        public void RetrieveItemsFromDatabase()
        {
            for(int i = 0; i < items.Count; i++)
                items[i].item = ItemsDatabase.GetItem(items[i].externalItemID);
        }
    }
}