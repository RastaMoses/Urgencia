using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class QuestsDatabase : MonoBehaviour
    {
        public static QuestsDatabase instance;
        private void Awake()
        {
            instance = this;

            sFile = file;
        }

        public QuestsDatabaseFile file;
        public static QuestsDatabaseFile sFile;

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

        public static Quest GetQuest(string _questID)
        {
            // Find the item in the database
            // return it
            //for (int i = 0; i < sFile.allItems.Count; i++)
            //    if (sFile.allItems[i].ItemID == _itemID)
            //        return sFile.allItems[i];

            Quest item;
            sFile.dictionary.TryGetValue(_questID, out item);

            return item;
        }
    }
}