using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Base class for Items in Inventory
    /// </summary>
    [System.Serializable]
    public class ItemInInventory
    {
        public string externalItemID; // item.ItemID deatached from the reference, used for external use (like loading the actual item)
        public Item item;
        public ItemMetadata metadata;
        public int Amount = 1;

        public bool isEquipped = false;
    }
}