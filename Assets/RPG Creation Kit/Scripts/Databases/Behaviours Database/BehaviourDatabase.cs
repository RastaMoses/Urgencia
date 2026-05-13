using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.BehaviourTree;

namespace RPGCreationKit
{
    public class BehaviourDatabase : MonoBehaviour
    {
        public static BehaviourDatabase instance;
        private void Awake()
        {
            instance = this;

            sFile = file;
        }

        public BehaviourDatabaseFile file;
        public static BehaviourDatabaseFile sFile;

        public void InsertAllProjectItems()
        {
            // Find all Projects Items and fill the allItems List
        }

        private void FillAllItemsSubLists()
        {
            // Fills all sublists
        }

        public void InsertNewItem(RPGCK_BT _item)
        {
            // Insert a single item in the allItems list and fill in sublist
        }

        public static RPGCK_BT GetItem(string _itemID)
        {
            // Find the item in the database
            // return it
            //for (int i = 0; i < sFile.allItems.Count; i++)
            //    if (sFile.allItems[i].ItemID == _itemID)
            //        return sFile.allItems[i];

            RPGCK_BT item;
            sFile.dictionary.TryGetValue(_itemID, out item);

            return item;
        }
    }
}