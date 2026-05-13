using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit
{
    public class AIDatabase : MonoBehaviour
    {
        public static AIDatabase instance;
        private void Awake()
        {
            instance = this;

            sFile = file;
        }

        public AIDatabaseFile file;
        public static AIDatabaseFile sFile;

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

        public static GameObject GetAI(string _factionID)
        {
            // Find the item in the database
            // return it
            //for (int i = 0; i < sFile.allItems.Count; i++)
            //    if (sFile.allItems[i].ItemID == _itemID)
            //        return sFile.allItems[i];

            GameObject ai;
            sFile.dictionary.TryGetValue(_factionID, out ai);

            return ai;
        }
    }
}