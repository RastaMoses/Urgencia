using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.SaveSystem;

namespace RPGCreationKit.SaveSystem
{

    [System.Serializable]
    public class ItemsInWorldDictionary : SerializableDictionary<string, ItemInWorldSaveData> { }

    /// <summary>
    /// Represent a single item in world
    /// </summary>
    [System.Serializable]
    public class ItemInWorldSaveData
    {
        public string worldspaceID;
        public ItemMetadata metadata;
        public Vector3 position;
        public Quaternion rotation;
        public bool isPickedUp = false;
        public bool isKinematic = false;
    }

    /// <summary>
    /// Represent the collection of all items in world in the game
    /// </summary>
    [System.Serializable]
    public class ItemsInWorldData
    {
        [SerializeField] public ItemsInWorldDictionary allItemsInWorld = new ItemsInWorldDictionary();
    }
}