using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class DialoguesDatabase : MonoBehaviour
    {
        public static DialoguesDatabase instance;
        private void Awake()
        {
            instance = this;

            sFile = file;
        }

        public DialoguesDatabaseFile file;
        public static DialoguesDatabaseFile sFile;

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

        public static DialogueSystem.DialogueGraph GetItem(string _itemID)
        {
            // Find the item in the database
            // return it
            //for (int i = 0; i < sFile.allItems.Count; i++)
            //    if (sFile.allItems[i].ItemID == _itemID)
            //        return sFile.allItems[i];

            DialogueSystem.DialogueGraph graph;
            sFile.dictionary.TryGetValue(_itemID, out graph);

            return graph;
        }
    }
}