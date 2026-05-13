using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RPGCreationKit.SaveSystem;
using RPGCreationKit;

namespace RPGCreationKit.SaveSystem
{
    [Serializable]
    public class SpellsKnowledgeSaveData
    {
        public string equippedSpell = string.Empty;
        public List<string> items = new List<string>();

        public void RetrieveItemsFromDatabase()
        {
            for (int i = 0; i < items.Count; i++)
                items[i] = SpellsDatabase.GetSpell(items[i]).spellID;
        }
    }
}