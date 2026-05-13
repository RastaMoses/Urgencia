using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.SaveSystem;

namespace RPGCreationKit.SaveSystem
{

    [System.Serializable]
    public class LootingPointsDictionary : SerializableDictionary<string, LootingPointSaveData> { }

    /// <summary>
    /// Represent a single looting point
    /// </summary>
    [System.Serializable]
    public class LootingPointSaveData
    {
        public string lootingID;

        public List<ItemInInventory> allItems = new List<ItemInInventory>();

        public void RetrieveItemsFromDatabase()
        {
            for (int i = 0; i < allItems.Count; i++)
                allItems[i].item = ItemsDatabase.GetItem(allItems[i].externalItemID);
        }
    }

    /// <summary>
    /// Represent the collection of all looting points in the game
    /// </summary>
    [System.Serializable]
    public class LootingPointsData
    {
        [SerializeField] public LootingPointsDictionary allLootingPoints = new LootingPointsDictionary();
    }
}