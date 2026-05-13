using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;


namespace RPGCreationKit
{
    public class WorldspaceDatabase : MonoBehaviour
    {
        public static WorldspaceDatabase instance;
        private void Awake()
        {
            instance = this;

            sFile = file;
        }

        public WorldspaceDatabaseFile file;
        public static WorldspaceDatabaseFile sFile;

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

        public static Worldspace GetWorldspace(string _worldspaceID)
        {
            // Find the item in the database
            // return it
            //for (int i = 0; i < sFile.allItems.Count; i++)
            //    if (sFile.allItems[i].ItemID == _itemID)
            //        return sFile.allItems[i];

            Worldspace item;
            sFile.dictionary.TryGetValue(_worldspaceID, out item);
            return item;
        }
    }
}