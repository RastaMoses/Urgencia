using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using RPGCreationKit;

namespace RPGCreationKit
{
    [System.Serializable]
    public struct ItemMetadata : IEquatable<ItemMetadata>
    {
        public bool isOwned;
        public string ownerID;

        // Firearms
        public int intProperty1; // for firearms, this is the number of rounds in the magazine

        public bool Equals(ItemMetadata other)
        {
            return (isOwned.Equals(other.isOwned));
        }

        public void Clear()
        {
            isOwned = false;
            ownerID = string.Empty;
        }

        public bool IsEmpty()
        {
            return (isOwned == false && ownerID == string.Empty);
        }
    }

    public enum ItemTypes
    {
        AmmoItem = 0,
        ArmorItem = 1,
        BookItem = 2,
        ConsumableItem = 3,
        KeyItem = 4,
        MiscItem = 5,
        WeaponItem = 6
    };


    /// <summary>
    /// Primitive item class, contains basic informations about the item
    /// </summary>
    public class Item : ScriptableObject
    {
        public bool _IS_DEMO_FILE = true;

        public readonly static int typesCount = 7; // the number of the elements of the ItemTypes enum

        public string ItemID = "NewItemID";

        public Sprite ItemIcon;

        public virtual ItemTypes itemType { get; }

        public string ItemName = "New Item";

        public GameObject itemInWorld;

        public float Weight = 1.0f;
        public int Value = 0;

        public bool QuestItem = false;

        public bool isCumulable = false;

        public bool useMesh = true;

        public bool usesTooltip = false;
        [TextArea] public string tooltipValue = "";

        // Scripts To Call on events
        public string itemScript;

        // Audio
        public AudioClip sOnPickUp;
        public AudioClip sOnDrop;
        public AudioClip sOnAddedInInventory;
        public AudioClip sOnRemovedFromInventory;
        public AudioClip sOnEquipOrUse;
        public AudioClip sOnUnEquip;
    }
}