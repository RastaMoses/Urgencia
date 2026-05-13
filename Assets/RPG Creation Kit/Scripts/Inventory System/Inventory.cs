using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RPGCreationKit;
using RPGCreationKit.SaveSystem;

namespace RPGCreationKit
{
    public class Inventory : MonoBehaviour
    {

        public static Inventory PlayerInventory;
        private void Awake()
        {
            // Find the player inventory and set reference
            if (!PlayerInventory)
                if (gameObject.CompareTag("Player"))
                    PlayerInventory = this;

            subLists = new List<ItemInInventory>[Item.typesCount];

            for (int i = 0; i < Item.typesCount; i++)
                subLists[i] = new List<ItemInInventory>();
        }

        [SerializeField] private Equipment equipment;

        [Header("Inventory Lists")]

        [SerializeField]
        public List<ItemInInventory> Items = new List<ItemInInventory>();

        [SerializeField]
        public List<ItemInInventory>[] subLists = new List<ItemInInventory>[Item.typesCount];

        [Space(10)]

        public float CurTotalWeight = 0;

        private void Start()
        {
            equipment = GetComponent<Equipment>();
        }

        public void InitializeInventory()
        {
            ClearSubLists();

            // Fill the sub inventories
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].externalItemID = Items[i].item.ItemID;
                subLists[(int)Items[i].item.itemType].Add(Items[i]);
            }
        }

        public ItemInInventory AddItem(string _itemID, int _amount, bool _playSound = true)
        {
            Item item = ItemsDatabase.GetItem(_itemID);

            if (item == null)
            {
                Debug.LogWarning("Tried to add in an Inventory the item with ID: '" + _itemID + "' but it is not present in the ItemsDatabase, you probably have to update it.");
                return null;
            }

            return AddItem(item, new ItemMetadata(), _amount, _playSound);
        }

        public ItemInInventory AddItem(Item _item, ItemMetadata _metadata, int _amount, bool _playSound = true)
        {
            ItemInInventory newItem = new ItemInInventory();

            // If it's cumulable
            if (_item.isCumulable)
            {
                // Try to get the item
                newItem = GetItem(_item);

                // Golds are a special kind of item, once picked up they have no ownership
                if (newItem != null && newItem.item.ItemID == "Gold001")
                {
                    _metadata.Clear();
                    newItem.metadata.Clear();
                }

                // If you get it, just update the amount and leave
                if(newItem != null && newItem.metadata.Equals(_metadata))
                {
                    newItem.Amount += _amount;

                    CurTotalWeight = CalculateTotalWeight();

                    // If player inventory play sound
                    if (_playSound && PlayerInventory == this)
                    {
                        if (_item.sOnAddedInInventory != null)
                            GameAudioManager.instance.PlayOneShot(AudioSources.UISounds, _item.sOnAddedInInventory);
                    }

                    return newItem;
                }
                else
                {
                    newItem = new ItemInInventory()
                    {
                        item = _item,
                        metadata = _metadata,
                        externalItemID = _item.ItemID,
                        Amount = _amount
                    };

                    Items.Add(newItem);
                    subLists[(int)newItem.item.itemType].Add(newItem);

                    CurTotalWeight = CalculateTotalWeight();

                    // If player inventory play sound
                    if (_playSound && PlayerInventory == this)
                    {
                        if (_item.sOnAddedInInventory != null)
                            GameAudioManager.instance.PlayOneShot(AudioSources.UISounds, _item.sOnAddedInInventory);
                    }

                    return newItem;
                }
            }

            // The code above runs if the item isn't cumulable
            for(int i = 0; i < _amount; i++)
            {
                // Add as many as the amount is
                newItem = new ItemInInventory()
                {
                    item = _item,
                    metadata = _metadata,
                    externalItemID = _item.ItemID,
                    Amount = 1
                };

                Items.Add(newItem);

                subLists[(int)newItem.item.itemType].Add(newItem);
            }

            // If player inventory play sound
            if(_playSound && PlayerInventory == this)
            {
                if (_item.sOnAddedInInventory != null)
                    GameAudioManager.instance.PlayOneShot(AudioSources.UISounds, _item.sOnAddedInInventory);
            }

            // ITEMSCRIPT_FUNC Run OnAdd
            if (!string.IsNullOrEmpty(newItem.item.itemScript))
            {
                ItemScript iScript = (ItemScript)QuestScriptManager.instance.scriptsHolder.AddComponent(System.Type.GetType(newItem.item.itemScript));
                iScript.OnAdd(this);
                Destroy(iScript);
            }

            CurTotalWeight = CalculateTotalWeight();

            return newItem;

        }

        public void RemoveItem(string _itemID, int _amount, bool _playSound = true)
        {
            Item item = ItemsDatabase.GetItem(_itemID);

            if (item == null)
            {
                Debug.LogWarning("Tried to add in an Inventory the item with ID: '" + _itemID + "' but it is not present in the ItemsDatabase, you probably have to update it.");
                return;
            }

            RemoveItem(item, _amount, _playSound);
        }

        public void RemoveItem(Item _item, int _amount, bool _playSound = true)
        {
            ItemInInventory itemToRemove = GetItem(_item);

            if(itemToRemove == null)
                return;

            RemoveItem(itemToRemove, _amount, _playSound);
        }

        public void RemoveItem(ItemInInventory _directItem, int _amount, bool _playSound = true)
        {
            if (_directItem.item.isCumulable && _directItem.Amount > 1 && _directItem.Amount > _amount)
            {
                _directItem.Amount -= _amount;
            }
            else
            {
                _directItem.Amount -= _amount;

                subLists[(int)_directItem.item.itemType].Remove(_directItem);
                Items.Remove(_directItem);
            }


            // If player inventory play sound
            if (_playSound && PlayerInventory == this)
            {
                if (_directItem.item.sOnAddedInInventory != null)
                    GameAudioManager.instance.PlayOneShot(AudioSources.UISounds, _directItem.item.sOnAddedInInventory);
            }

            CurTotalWeight = CalculateTotalWeight();
        }

        public bool HasItem(string _itemID)
        {
            Item item = ItemsDatabase.GetItem(_itemID);

            if (item == null)
            {
                Debug.LogWarning("Tried to add in an Inventory the item with ID '" + _itemID + "' but it is not present in the ItemsDatabase, you probably have to update it.");
                return false;
            }

            return HasItem(item);
        }

        public bool HasItem(Item item)
        {
            for(int i = 0; i < subLists[(int)item.itemType].Count; i++)
            {
                if (subLists[(int)item.itemType][i].externalItemID == item.ItemID)
                    return true;
            }

            return false;
        }


        public int GetItemCountNonStolen(string _itemID)
        {
            ItemInInventory item = GetItemNonStolen(_itemID);

            if (item == null)
                return 0;

            return item.Amount;
        }

        public ItemInInventory GetItemNonStolen(string _itemID)
        {
            Item item = ItemsDatabase.GetItem(_itemID);

            if (item == null)
                return null;

            return GetItemNonStolen(item);
        }

        public ItemInInventory GetItemNonStolen(Item item)
        {
            for (int i = 0; i < subLists[(int)item.itemType].Count; i++)
            {
                if (subLists[(int)item.itemType][i].externalItemID == item.ItemID)
                    if(!subLists[(int)item.itemType][i].metadata.isOwned)
                        return subLists[(int)item.itemType][i];
            }

            return null;
        }

        public int GetItemCountStolen(string _itemID)
        {
            ItemInInventory item = GetItemStolen(_itemID);

            if (item == null)
                return 0;

            return item.Amount;
        }

        public ItemInInventory GetItemStolen(string _itemID)
        {
            Item item = ItemsDatabase.GetItem(_itemID);

            if (item == null)
                return null;

            return GetItemStolen(item);
        }

        public ItemInInventory GetItemStolen(Item item)
        {
            for (int i = 0; i < subLists[(int)item.itemType].Count; i++)
            {
                if (subLists[(int)item.itemType][i].externalItemID == item.ItemID)
                    if (subLists[(int)item.itemType][i].metadata.isOwned)
                        return subLists[(int)item.itemType][i];
            }

            return null;
        }

        public int GetItemCountBothStolenAndNot(string _itemID)
        {
            ItemInInventory stolen = GetItemStolen(_itemID);
            ItemInInventory notStolen = GetItemNonStolen(_itemID);

            int amount = 0;

            if(stolen != null)
                amount += stolen.Amount;

            if (notStolen != null)
                amount += notStolen.Amount;

            return amount;
        }

        // GetItemCount returns the first (may be stolen or not) entry
        public int GetItemCount(string _itemID)
        {
            ItemInInventory item = GetItem(_itemID);

            if (item == null)
                return 0;

            return item.Amount;
        }

        public int GetItemCount(Item _item)
        {
            ItemInInventory item = GetItem(_item.ItemID);

            if (item == null)
                return 0;

            return item.Amount;
        }

        public ItemInInventory GetItem(string _itemID)
        {
            Item item = ItemsDatabase.GetItem(_itemID);

            if (item == null)
            {
                Debug.LogWarning("Tried to add in an Inventory the item with ID: '" + _itemID + "' but it is not present in the ItemsDatabase, you probably have to update it.");
                return null;
            }

            return GetItem(item);
        }

        public ItemInInventory GetItem(Item item)
        {
            if (item == null || subLists[(int)item.itemType] == null)
                return null;

            for (int i = 0; i < subLists[(int)item.itemType].Count; i++)
            {
                if (subLists[(int)item.itemType][i].externalItemID == item.ItemID)
                    return subLists[(int)item.itemType][i];
            }

            return null;
        }

        public void ClearInventory()
        {
            Items.Clear();

            for (int i = 0; i < Item.typesCount; i++)
                subLists[i].Clear();
        }

        public void ClearInventoryAndEquipment()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].isEquipped)
                    equipment.Unequip(Items[i]);
            }

            Items.Clear();

            for (int i = 0; i < Item.typesCount; i++)
                subLists[i].Clear();
        }

        public void ClearSubLists()
        {
            for (int i = 0; i < Item.typesCount; i++)
                subLists[i].Clear();
        }

        public InventorySaveData ToSaveData()
        {
            InventorySaveData saveData = new InventorySaveData();

            for (int i = 0; i < Items.Count; i++)
                saveData.items.Add(Items[i]);

            return saveData;
        }

        public float CalculateTotalWeight()
        {
            float totalWeight = 0;

            for (int i = 0; i < Items.Count; i++)
                totalWeight += (Items[i].item.Weight * Items[i].Amount);

            if (Inventory.PlayerInventory == this && InventoryUI.instance.isOpen)
                InventoryUI.instance.UpdateStatsUI();

            return totalWeight;
        }


        public bool IsOverencumbred()
        {
            return CurTotalWeight > EntityAttributes.PlayerAttributes.derivedAttributes.maxEncumbrance;
        }
    }
}
