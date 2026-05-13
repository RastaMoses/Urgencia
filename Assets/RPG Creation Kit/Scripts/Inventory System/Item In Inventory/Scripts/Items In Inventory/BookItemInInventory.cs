using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Class for Armor Items in the Inventory, keeps track of the Item and the Amount of it in the inventory
    /// </summary>

    [System.Serializable]
    public class BookItemInInventory : ItemInInventory
    {
        // public BookItem bookItem;
        public ItemInInventory listItem; // the item in the abs list
    }
}