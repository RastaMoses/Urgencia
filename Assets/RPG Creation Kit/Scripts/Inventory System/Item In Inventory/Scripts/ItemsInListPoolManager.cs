using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;


namespace RPGCreationKit
{
    /// <summary>
    /// This class manages all the Pools for the Items In List
    /// </summary>
    public class ItemsInListPoolManager : MonoBehaviour
    {
        public Inventory inventoryToPool;

        public List<ItemInInventoryUI> usedObjects;

        [Header("Pools")]
        [SerializeField] public ItemsInListPool AmmosPool;
        [SerializeField] public ItemsInListPool ArmorsPool;
        [SerializeField] public ItemsInListPool BooksPool;
        [SerializeField] public ItemsInListPool ConsumablesPool;
        [SerializeField] public ItemsInListPool KeysPool;
        [SerializeField] public ItemsInListPool MiscsPool;
        [SerializeField] public ItemsInListPool WeaponsPool;


        public void ResetPools()
        {
            // Restore all pools
            AmmosPool.ResetPool();
            ArmorsPool.ResetPool();
            BooksPool.ResetPool();
            ConsumablesPool.ResetPool();
            KeysPool.ResetPool();
            MiscsPool.ResetPool();
            WeaponsPool.ResetPool();

            usedObjects.Clear();
        }

        public void ShowAll()
        {
            AmmosPool.Content.gameObject.SetActive(true);
            ArmorsPool.Content.gameObject.SetActive(true);
            BooksPool.Content.gameObject.SetActive(true);
            ConsumablesPool.Content.gameObject.SetActive(true);
            KeysPool.Content.gameObject.SetActive(true);
            MiscsPool.Content.gameObject.SetActive(true);
            WeaponsPool.Content.gameObject.SetActive(true);

            ShowAmmos();
            ShowArmorItems();
            ShowBookItems();
            ShowConsumableItems();
            ShowKeyItems();
            ShowMiscItems();
            ShowWeaponItems();
        }

        public void WeaponsTab()
        {
            AmmosPool.Content.gameObject.SetActive(true);
            ArmorsPool.Content.gameObject.SetActive(false);
            BooksPool.Content.gameObject.SetActive(false);
            ConsumablesPool.Content.gameObject.SetActive(false);
            KeysPool.Content.gameObject.SetActive(false);
            MiscsPool.Content.gameObject.SetActive(false);
            WeaponsPool.Content.gameObject.SetActive(true);

            ShowAmmos();
            ShowWeaponItems();
        }

        public void ArmorsTab()
        {
            AmmosPool.Content.gameObject.SetActive(false);
            ArmorsPool.Content.gameObject.SetActive(true);
            BooksPool.Content.gameObject.SetActive(false);
            ConsumablesPool.Content.gameObject.SetActive(false);
            KeysPool.Content.gameObject.SetActive(false);
            MiscsPool.Content.gameObject.SetActive(false);
            WeaponsPool.Content.gameObject.SetActive(false);

            ShowArmorItems();
        }

        public void MiscTab()
        {
            AmmosPool.Content.gameObject.SetActive(false);
            ArmorsPool.Content.gameObject.SetActive(false);
            BooksPool.Content.gameObject.SetActive(true);
            ConsumablesPool.Content.gameObject.SetActive(true);
            KeysPool.Content.gameObject.SetActive(false);
            MiscsPool.Content.gameObject.SetActive(true);
            WeaponsPool.Content.gameObject.SetActive(false);

            ShowMiscItems();
            ShowBookItems();
            ShowConsumableItems();
        }

        public void KeysTab()
        {
            AmmosPool.Content.gameObject.SetActive(false);
            ArmorsPool.Content.gameObject.SetActive(false);
            BooksPool.Content.gameObject.SetActive(false);
            ConsumablesPool.Content.gameObject.SetActive(false);
            KeysPool.Content.gameObject.SetActive(true);
            MiscsPool.Content.gameObject.SetActive(false);
            WeaponsPool.Content.gameObject.SetActive(false);

            ShowKeyItems();
        }

        public void PopulateAll(bool hideEquipped = false)
        {
            PopulateAmmos(hideEquipped);
            PopulateArmorItems(hideEquipped);
            PopulateBookItems(hideEquipped);
            PopulateConsumableItems(hideEquipped);
            PopulateKeyItems(hideEquipped);
            PopulateMiscItems(hideEquipped);
            PopulateWeaponItems(hideEquipped);
        }
        

        public void PopulateAmmos(bool hideEquipped = false)
        {
            bool incomplete = false; // Can we show all items with the current pool or we have to add other elements?

            int items = inventoryToPool.subLists[(int)ItemTypes.AmmoItem].Count; // shortcut

            // If we can't show all the items with the avaiable objects in pool mark this as incomplete
            if (items > AmmosPool.AbsPooledObjects.Count)
                incomplete = true;

            if(incomplete)
            {
                int Missing = items - AmmosPool.AbsPooledObjects.Count;

                if(Missing > 0)
                    AmmosPool.AddInPool(Missing);
            }

            // Enable & Use
            for(int i = 0; i < items; i++)
            {
                AmmoItemInInventoryUI newItem;
                newItem = AmmosPool.AbsPooledObjects[i].GetComponent<AmmoItemInInventoryUI>();
                newItem.ammoItemInInventory = inventoryToPool.subLists[(int)ItemTypes.AmmoItem][i];

                if (hideEquipped && newItem.ammoItemInInventory.isEquipped)
                    continue;
                
                //newItem.GetComponent<Button>().onClick.AddListener(() => newItem.OnClick());
                newItem.pool = this;

                newItem.Init();

                AmmosPool.usedObjects.Add(newItem);
                usedObjects.Add(newItem);
            }
        }

        public void PopulateArmorItems(bool hideEquipped = false)
        {
            bool incomplete = false; // Can we show all items with the current pool or we have to add other elements?

            int items = inventoryToPool.subLists[(int)ItemTypes.ArmorItem].Count; // shortcut

            // If we can't show all the items with the avaiable objects in pool mark this as incomplete
            if (items > ArmorsPool.AbsPooledObjects.Count)
                incomplete = true;

            if (incomplete)
            {
                int Missing = items - ArmorsPool.AbsPooledObjects.Count;

                if (Missing > 0)
                    ArmorsPool.AddInPool(Missing);
            }

            // Enable & Use
            for (int i = 0; i < items; i++)
            {
                ArmorItemInInventoryUI newItem;
                newItem = ArmorsPool.AbsPooledObjects[i].GetComponent<ArmorItemInInventoryUI>();
                newItem.armorItemInInventory = inventoryToPool.subLists[(int)ItemTypes.ArmorItem][i];

                if (hideEquipped && newItem.armorItemInInventory.isEquipped)
                    continue;

                //newItem.GetComponent<Button>().onClick.AddListener(() => newItem.OnClick());
                newItem.pool = this;

                newItem.Init();

                ArmorsPool.usedObjects.Add(newItem);
                usedObjects.Add(newItem);
            }

        }

        public void PopulateBookItems(bool hideEquipped = false)
        {
            bool incomplete = false; // Can we show all items with the current pool or we have to add other elements?

            int items = inventoryToPool.subLists[(int)ItemTypes.BookItem].Count; // shortcut

            // If we can't show all the items with the avaiable objects in pool mark this as incomplete
            if (items > BooksPool.AbsPooledObjects.Count)
                incomplete = true;

            if (incomplete)
            {
                int Missing = items - BooksPool.AbsPooledObjects.Count;

                if (Missing > 0)
                    BooksPool.AddInPool(Missing);
            }

            // Enable & Use
            for (int i = 0; i < items; i++)
            {
                BookItemInInventoryUI newItem;
                newItem = BooksPool.AbsPooledObjects[i].GetComponent<BookItemInInventoryUI>();
                newItem.bookItemInInventory = inventoryToPool.subLists[(int)ItemTypes.BookItem][i];

                if (hideEquipped && newItem.bookItemInInventory.isEquipped)
                    continue;

                //newItem.GetComponent<Button>().onClick.AddListener(() => newItem.OnClick());
                newItem.pool = this;

                newItem.Init();

                BooksPool.usedObjects.Add(newItem);
                usedObjects.Add(newItem);
            }
        }

        public void PopulateConsumableItems(bool hideEquipped = false)
        {
            bool incomplete = false; // Can we show all items with the current pool or we have to add other elements?

            int items = inventoryToPool.subLists[(int)ItemTypes.ConsumableItem].Count; // shortcut

            // If we can't show all the items with the avaiable objects in pool mark this as incomplete
            if (items > ConsumablesPool.AbsPooledObjects.Count)
                incomplete = true;

            if (incomplete)
            {
                int Missing = items - ConsumablesPool.AbsPooledObjects.Count;

                if (Missing > 0)
                    ConsumablesPool.AddInPool(Missing);
            }

            // Enable & Use
            for (int i = 0; i < items; i++)
            {
                ConsumableItemInInventoryUI newItem;
                newItem = ConsumablesPool.AbsPooledObjects[i].GetComponent<ConsumableItemInInventoryUI>();
                newItem.consumableItemInInventory = inventoryToPool.subLists[(int)ItemTypes.ConsumableItem][i];

                if (hideEquipped && newItem.consumableItemInInventory.isEquipped)
                    continue;

                //newItem.GetComponent<Button>().onClick.AddListener(() => newItem.OnClick());
                newItem.pool = this;

                newItem.Init();

                ConsumablesPool.usedObjects.Add(newItem);
                usedObjects.Add(newItem);
            }
        }

        public void PopulateKeyItems(bool hideEquipped = false)
        {
            bool incomplete = false; // Can we show all items with the current pool or we have to add other elements?

            int items = inventoryToPool.subLists[(int)ItemTypes.KeyItem].Count; // shortcut

            // If we can't show all the items with the avaiable objects in pool mark this as incomplete
            if (items > KeysPool.AbsPooledObjects.Count)
                incomplete = true;

            if (incomplete)
            {
                int Missing = items - KeysPool.AbsPooledObjects.Count;

                if (Missing > 0)
                    KeysPool.AddInPool(Missing);
            }

            // Enable & Use
            for (int i = 0; i < items; i++)
            {
                KeyItemInInventoryUI newItem;
                newItem = KeysPool.AbsPooledObjects[i].GetComponent<KeyItemInInventoryUI>();

                if (hideEquipped && newItem.keyItemInInventory.isEquipped)
                    continue;

                newItem.keyItemInInventory = inventoryToPool.subLists[(int)ItemTypes.KeyItem][i];
                //newItem.GetComponent<Button>().onClick.AddListener(() => newItem.OnClick());
                newItem.pool = this;

                newItem.Init();

                KeysPool.usedObjects.Add(newItem);
                usedObjects.Add(newItem);
            }
        }

        public void PopulateMiscItems(bool hideEquipped = false)
        {
            bool incomplete = false; // Can we show all items with the current pool or we have to add other elements?

            int items = inventoryToPool.subLists[(int)ItemTypes.MiscItem].Count; // shortcut

            // If we can't show all the items with the avaiable objects in pool mark this as incomplete
            if (items > MiscsPool.AbsPooledObjects.Count)
                incomplete = true;

            if (incomplete)
            {
                int Missing = items - MiscsPool.AbsPooledObjects.Count;

                if (Missing > 0)
                    MiscsPool.AddInPool(Missing);
            }

            // Enable & Use
            for (int i = 0; i < items; i++)
            {
                MiscItemInInventoryUI newItem;
                newItem = MiscsPool.AbsPooledObjects[i].GetComponent<MiscItemInInventoryUI>();
                newItem.miscItemInInventory = inventoryToPool.subLists[(int)ItemTypes.MiscItem][i];

                if (hideEquipped && newItem.miscItemInInventory.isEquipped)
                    continue;

                // Mainly used for not showing golds inside the player inventory, but to show them in looting points
                MiscItem itemz = (MiscItem)newItem.miscItemInInventory.item;
                if (!itemz.showInPlayerInventory && inventoryToPool == Inventory.PlayerInventory || !itemz.showInPlayerInventory && TradeSystemUI.instance.isOpen && TradeSystemUI.instance.isBuyingOrDrawing && TradeSystemUI.instance.isMerchant)
                    continue; // Skip that item

                //newItem.GetComponent<Button>().onClick.AddListener(() => newItem.OnClick());
                newItem.pool = this;

                newItem.Init();

                MiscsPool.usedObjects.Add(newItem);
                usedObjects.Add(newItem);
            }
        }

        public void PopulateWeaponItems(bool hideEquipped = false)
        {
            bool incomplete = false; // Can we show all items with the current pool or we have to add other elements?

            int items = inventoryToPool.subLists[(int)ItemTypes.WeaponItem].Count; // shortcut

            // If we can't show all the items with the avaiable objects in pool mark this as incomplete
            if (items > WeaponsPool.AbsPooledObjects.Count)
                incomplete = true;

            if (incomplete)
            {
                int Missing = items - WeaponsPool.AbsPooledObjects.Count;

                if (Missing > 0)
                    WeaponsPool.AddInPool(Missing);
            }

            // Enable & Use
            for (int i = 0; i < items; i++)
            {
                WeaponItemInInventoryUI newItem;
                newItem = WeaponsPool.AbsPooledObjects[i].GetComponent<WeaponItemInInventoryUI>();
                newItem.weaponItemInInventory = inventoryToPool.subLists[(int)ItemTypes.WeaponItem][i];

                if (hideEquipped && newItem.weaponItemInInventory.isEquipped)
                    continue;

                //newItem.GetComponent<Button>().onClick.AddListener(() => newItem.OnClick());
                newItem.pool = this;

                newItem.Init();

                WeaponsPool.usedObjects.Add(newItem);
                usedObjects.Add(newItem);
            }
        }

        public void ShowAmmos()
        {
            for(int i = 0; i < AmmosPool.usedObjects.Count; i++)
                AmmosPool.usedObjects[i].gameObject.SetActive(true);
        }

        public void ShowArmorItems()
        {
            for (int i = 0; i < ArmorsPool.usedObjects.Count; i++)
                ArmorsPool.usedObjects[i].gameObject.SetActive(true);
        }

        public void ShowBookItems()
        {
            for (int i = 0; i < BooksPool.usedObjects.Count; i++)
                BooksPool.usedObjects[i].gameObject.SetActive(true);
        }

        public void ShowConsumableItems()
        {
            for (int i = 0; i < ConsumablesPool.usedObjects.Count; i++)
                ConsumablesPool.usedObjects[i].gameObject.SetActive(true);
        }

        public void ShowKeyItems()
        {
            for (int i = 0; i < KeysPool.usedObjects.Count; i++)
                KeysPool.usedObjects[i].gameObject.SetActive(true);
        }

        public void ShowMiscItems()
        {
            for (int i = 0; i < MiscsPool.usedObjects.Count; i++)
                MiscsPool.usedObjects[i].gameObject.SetActive(true);
        }

        public void ShowWeaponItems()
        {
            for (int i = 0; i < WeaponsPool.usedObjects.Count; i++)
                WeaponsPool.usedObjects[i].gameObject.SetActive(true);
        }

        /// <summary>
        /// Specifically called for showing Player's items that a merchant wants to buy.
        /// </summary>
        /// <param name="items"></param>
        public void MerchantTradeShowAll(TradableItems items)
        {
            if (items.HasFlag(TradableItems.Ammos))
            {
                AmmosPool.Content.gameObject.SetActive(true);
                ShowAmmos();
            }

            if (items.HasFlag(TradableItems.Armors))
            {
                ArmorsPool.Content.gameObject.SetActive(true);
                ShowArmorItems();
            }

            if (items.HasFlag(TradableItems.Books))
            {
                BooksPool.Content.gameObject.SetActive(true);
                ShowBookItems();
            }

            if (items.HasFlag(TradableItems.Consumables))
            {
                ConsumablesPool.Content.gameObject.SetActive(true);
                ShowConsumableItems();
            }

            if (items.HasFlag(TradableItems.Keys))
            {
                KeysPool.Content.gameObject.SetActive(true);
                ShowKeyItems();
            }

            if (items.HasFlag(TradableItems.Miscs))
            {
                MiscsPool.Content.gameObject.SetActive(true);
                ShowMiscItems();
            }

            if (items.HasFlag(TradableItems.Weapons))
            {
                WeaponsPool.Content.gameObject.SetActive(true);
                ShowWeaponItems();
            }
        }

        public void MerchantTradeShowArmors(TradableItems items)
        {
            if (items.HasFlag(TradableItems.Armors))
            {
                ArmorsPool.Content.gameObject.SetActive(true);
                ShowArmorItems();
            }
        }

        public void MerchantTradeArmorsTab(TradableItems items)
        {
            AmmosPool.Content.gameObject.SetActive(false);
            ArmorsPool.Content.gameObject.SetActive(true);
            BooksPool.Content.gameObject.SetActive(false);
            ConsumablesPool.Content.gameObject.SetActive(false);
            KeysPool.Content.gameObject.SetActive(false);
            MiscsPool.Content.gameObject.SetActive(false);
            WeaponsPool.Content.gameObject.SetActive(false);

            if (items.HasFlag(TradableItems.Armors))
                ShowArmorItems();
        }

        public void MerchantTradeKeysTab(TradableItems items)
        {
            AmmosPool.Content.gameObject.SetActive(false);
            ArmorsPool.Content.gameObject.SetActive(false);
            BooksPool.Content.gameObject.SetActive(false);
            ConsumablesPool.Content.gameObject.SetActive(false);
            KeysPool.Content.gameObject.SetActive(true);
            MiscsPool.Content.gameObject.SetActive(false);
            WeaponsPool.Content.gameObject.SetActive(false);

            if (items.HasFlag(TradableItems.Keys))
                ShowKeyItems();
        }

        public void MerchantTradeMiscTab(TradableItems items)
        {
            AmmosPool.Content.gameObject.SetActive(false);
            ArmorsPool.Content.gameObject.SetActive(false);
            BooksPool.Content.gameObject.SetActive(true);
            ConsumablesPool.Content.gameObject.SetActive(true);
            KeysPool.Content.gameObject.SetActive(false);
            MiscsPool.Content.gameObject.SetActive(true);
            WeaponsPool.Content.gameObject.SetActive(false);

            if (items.HasFlag(TradableItems.Miscs))
                ShowMiscItems();

            if (items.HasFlag(TradableItems.Books))
                ShowBookItems();

            if (items.HasFlag(TradableItems.Consumables))
                ShowConsumableItems();
        }


        public void MerchantTradeWeaponsTab(TradableItems items)
        {
            AmmosPool.Content.gameObject.SetActive(true);
            ArmorsPool.Content.gameObject.SetActive(false);
            BooksPool.Content.gameObject.SetActive(false);
            ConsumablesPool.Content.gameObject.SetActive(false);
            KeysPool.Content.gameObject.SetActive(false);
            MiscsPool.Content.gameObject.SetActive(false);
            WeaponsPool.Content.gameObject.SetActive(true);

            if (items.HasFlag(TradableItems.Ammos))
                ShowAmmos();

            if (items.HasFlag(TradableItems.Weapons))
                ShowWeaponItems();
        }

        public object ReturnItemInInventory(Item _item)
        {
            if (_item is AmmoItem)
            {
                for(int i = 0; i < AmmosPool.AbsPooledObjects.Count; i++)
                {
                    if (_item.ItemID == AmmosPool.AbsPooledObjects[i].GetComponent<AmmoItemInInventoryUI>().ammoItemInInventory.item.ItemID)
                        return AmmosPool.AbsPooledObjects[i].GetComponent<AmmoItemInInventoryUI>();
                }
            }

            //else if (_item is ArmorItem)
            //    armorItems.Remove((ArmorItem)_item);

            //else if (_item is BookItem)
            //    bookItems.Remove((BookItem)_item);

            //else if (_item is BookItem)
            //    bookItems.Remove((BookItem)_item);

            //else if (_item is ConsumableItem)
            //    consumableItems.Remove((ConsumableItem)_item);

            //else if (_item is KeyItem)
            //    keyItems.Remove((KeyItem)_item);

            //else if (_item is MiscItem)
            //    miscItems.Remove((MiscItem)_item);

            //else if (_item is WeaponItem)
            //    weaponItems.Remove((WeaponItem)_item);

            return null;
        }
    }
}