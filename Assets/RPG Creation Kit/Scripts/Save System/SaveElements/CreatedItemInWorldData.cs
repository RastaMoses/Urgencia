using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.SaveSystem;

namespace RPGCreationKit.SaveSystem
{
    [System.Serializable]
    public class CreatedItemsInWorldDictionary : SerializableDictionary<string, CreatedItemInWorldCollection> { }

    /// <summary>
    /// Represent a single item in world
    /// </summary>
    [System.Serializable]
    public class CreatedItemInWorldSaveData
    {
        public string itemID;
        public ItemMetadata metadata;
        public string WorldspaceID;
        public int amount;
        public Vector3 position;
        public Quaternion rotation;
        public bool isKinematic;
    }

    /// <summary>
    /// Represent a single item in world
    /// </summary>
    [System.Serializable]
    public class CreatedItemInWorldCollection
    {
        public List<CreatedItemInWorldSaveData> itemsInThis = new List<CreatedItemInWorldSaveData>();
    }

    /// <summary>
    /// Represent the collection of all items in world in the game
    /// </summary>
    [System.Serializable]
    public class CreatedItemInWorldData
    {
        [SerializeField] public CreatedItemsInWorldDictionary allCreatedItemsInWorld = new CreatedItemsInWorldDictionary();
    }
}