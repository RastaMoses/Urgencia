using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.SaveSystem;
using System.Reflection;
using UnityEditor;
using System;

namespace RPGCreationKit
{
    [System.Serializable]
    public struct LootingPointMetadata : IEquatable<LootingPointMetadata>
    {
        public bool isOwned;
        public string ownerID;

        public bool Equals(LootingPointMetadata other)
        {
            return (isOwned.Equals(other.isOwned) && ownerID.Equals(other.ownerID));
        }
    }

    public class LootingPoint : MonoBehaviour
    {
        public bool hasToInitializeInventory = true;
        public string ID;
        public LootingPointMetadata metadata;
        public string pointName;

        public Collider lCollider;

        public AudioClip openSound;
        public AudioClip closeSound;

        public Inventory inventory;
        public Equipment equipment;

        [HideInInspector] public List<ItemInInventoryUI> items;
        public bool hasToLoadAndSave = true;

        private void Start()
        {
            if (hasToInitializeInventory)
                inventory.InitializeInventory();

            if(hasToLoadAndSave)
                TryLoadFromSavefile();
        }

        [ContextMenu("Regenerate ID")]
        public void RegenerateGUIDStr()
        {
            ID = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        [ContextMenu("Apply LootingPoint Ownership to Items")]
        public void ApplyLootingPointOwnershipToItems()
        {
            for(int i = 0; i < inventory.Items.Count; i++)
            {
                inventory.Items[i].metadata.isOwned = metadata.isOwned;
                inventory.Items[i].metadata.ownerID = metadata.ownerID;
            }
        }

        public bool TryLoadFromSavefile()
        {
            LootingPointSaveData data;
            SaveSystemManager.instance.saveFile.LootingPointsData.allLootingPoints.TryGetValue(ID, out data);

            if(data != null)
            {
                inventory.ClearInventory();

                for(int i = 0; i < data.allItems.Count; i++)
                    inventory.AddItem(data.allItems[i].externalItemID, data.allItems[i].Amount);

                inventory.InitializeInventory();

                return true;
            }
            return false;
        }

        public void SaveOnFile()
        {
            if (hasToLoadAndSave)
            {
                if (SaveSystemManager.instance.saveFile.LootingPointsData.allLootingPoints.ContainsKey(ID))
                {
                    // Update entry
                    SaveSystemManager.instance.saveFile.LootingPointsData.allLootingPoints[ID] = ToSaveData();
                }
                else
                    SaveSystemManager.instance.saveFile.LootingPointsData.allLootingPoints.Add(ID, ToSaveData());
            }
        }

        public LootingPointSaveData ToSaveData()
        {
            LootingPointSaveData data = new LootingPointSaveData
            {
                lootingID = ID,
                allItems = inventory.Items
            };

            return data;
        }
    }
}