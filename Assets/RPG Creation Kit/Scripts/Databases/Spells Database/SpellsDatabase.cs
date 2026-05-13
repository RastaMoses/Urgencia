using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;


namespace RPGCreationKit
{
    public class SpellsDatabase : MonoBehaviour
    {
        public static SpellsDatabase instance;
        private void Awake()
        {
            instance = this;

            sFile = file;
        }

        public SpellsDatabaseFile file;
        public static SpellsDatabaseFile sFile;

        public void InsertAllProjectItems()
        {
            // Find all Projects Items and fill the allItems List
        }

        private void FillAllItemsSubLists()
        {
            // Fills all sublists
        }

        public void InsertNewItem(Spell _item)
        {
            // Insert a single item in the allItems list and fill in sublist
        }

        public static Spell GetSpell(string _spellID)
        {
            // Find the item in the database
            // return it
            //for (int i = 0; i < sFile.allItems.Count; i++)
            //    if (sFile.allItems[i].ItemID == _itemID)
            //        return sFile.allItems[i];

            Spell item;
            sFile.dictionary.TryGetValue(_spellID, out item);
            return item;
        }
    }
}