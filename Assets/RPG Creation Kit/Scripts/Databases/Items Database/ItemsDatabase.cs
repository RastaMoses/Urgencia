using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class ItemsDatabase : MonoBehaviour
    {
        public static ItemsDatabase instance;
        private void Awake()
        {
            instance = this;

            sFile = file;
        }

        public ItemsDatabaseFile file;
        public static ItemsDatabaseFile sFile;

        public void InsertAllProjectItems()
        {
            // Find all Projects Items and fill the allItems List
        }

        private void FillAllItemsSubLists()
        {
            // Fills all sublists
        }

        public void InsertNewItem(Item _item)
        {
            // Insert a single item in the allItems list and fill in sublist
        }

        public static Item GetItem(string _itemID)
        {
            // Find the item in the database
            // return it
            //for (int i = 0; i < sFile.allItems.Count; i++)
            //    if (sFile.allItems[i].ItemID == _itemID)
            //        return sFile.allItems[i];

            Item item;
            sFile.dictionary.TryGetValue(_itemID, out item);

            return item;
        }
    }
}