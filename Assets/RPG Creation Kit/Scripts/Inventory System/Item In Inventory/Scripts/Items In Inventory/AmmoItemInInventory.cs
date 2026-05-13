using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Class for Ammo Items in the Inventory, keeps track of the Item and the Amount of it in the inventory
    /// </summary>

    [System.Serializable]
    public class AmmoItemInInventory : ItemInInventory
    {
        public ItemInInventory listItem; // the item in the abs list
        // public AmmoItem ammoItem;
    }
}