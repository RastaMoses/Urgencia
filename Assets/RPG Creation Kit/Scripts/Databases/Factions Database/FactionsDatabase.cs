using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class FactionsDatabase : MonoBehaviour
    {
        public static FactionsDatabase instance;
        private void Awake()
        {
            instance = this;

            sFile = file;
        }

        public FactionsDatabaseFile file;
        public static FactionsDatabaseFile sFile;

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

        public static Faction GetFaction(string _factionID)
        {
            // Find the item in the database
            // return it
            //for (int i = 0; i < sFile.allItems.Count; i++)
            //    if (sFile.allItems[i].ItemID == _itemID)
            //        return sFile.allItems[i];

            Faction faction;
            sFile.dictionary.TryGetValue(_factionID, out faction);

            return faction;
        }
    }
}