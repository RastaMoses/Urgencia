using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class AudioClipsDatabase : MonoBehaviour
    {
        public static AudioClipsDatabase instance;

        private void Awake()
        {
            instance = this;

            sFile = file;
        }

        public AudioClipsDatabaseFile file;
        public static AudioClipsDatabaseFile sFile;

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

        public static AudioClip GetItem(string _itemID)
        {
            // Find the item in the database
            // return it
            //for (int i = 0; i < sFile.allItems.Count; i++)
            //    if (sFile.allItems[i].ItemID == _itemID)
            //        return sFile.allItems[i];

            AudioClip clip;
            sFile.dictionary.TryGetValue(_itemID, out clip);
            return clip;
        }
    }
}