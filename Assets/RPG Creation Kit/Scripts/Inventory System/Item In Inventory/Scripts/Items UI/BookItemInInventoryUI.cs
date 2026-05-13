using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using UnityEngine.EventSystems;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    /// <summary>
    /// BookItem UI in Inventory
    /// </summary>
    public class BookItemInInventoryUI : ItemInInventoryUI, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
    {
        public ItemInInventory bookItemInInventory;
        private BookItem bookItem;

        // Use this for initialization
        public void Init()
        {
            bookItem = (BookItem)bookItemInInventory.item;
            UpdateItem();
        }

        public override void UpdateItem()
        {
            base.UpdateItem();

            // Set the UI elements
            Icon.sprite = bookItem.ItemIcon;
            Name.text = bookItem.ItemName;
            DamageVal.text = "-";
            ArmorVal.text = "-";
            WeightVal.text = bookItem.Weight.ToString();
            GoldsVal.text = bookItem.Value.ToString();
            AmountVal.text = "x" + bookItemInInventory.Amount.ToString();

            AmountVal.gameObject.SetActive((bookItemInInventory.Amount > 1 && bookItem.isCumulable) ? true : false);

            stolenIcon.SetActive(bookItemInInventory.metadata.isOwned);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (bookItem == null)
                return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                ReadBook();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (!bookItem.isCumulable || bookItemInInventory.Amount <= 1)
                    DropItem();
                else
                    DropCumulableItemPanel();
            }
        }

        /*
        public override void OnClick(bool takeAll = false)
        {
            if (bookItem == null)
                return;

            if (InventoryUI.instance.dropKeyBeingHeld)
            {
                if (!bookItem.isCumulable || bookItemInInventory.Amount <= 1)
                    DropItem();
                else
                    DropCumulableItemPanel();
            }
            else
            {
                ReadBook();
            }
        }
        */

        /// <summary>
        /// Mainly used for gamepad or to have a single input that drops the item
        /// </summary>
        public override void OnClickForDrop()
        {
            if (!bookItem.isCumulable || bookItemInInventory.Amount <= 1)
                DropItem();
            else
                DropCumulableItemPanel();
        }

        private void ReadBook()
        {
            if (bookItem.sOnEquipOrUse)
                GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, bookItem.sOnEquipOrUse);

            BookReaderManager.instance.ReadBook(bookItem, false, null, true);
        }

        public override void DropItem()
        {
            base.DropItem();
            // If it is a quest item you can't drop it
            if (bookItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", 3);
                return;
            }

            // Spawn the item in the world
            ItemInWorld itemInWorld = Instantiate(bookItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = bookItemInInventory.metadata;
            itemInWorld.isCreatedItem = true;
            itemInWorld.gameObject.transform.position = RckPlayer.instance.transform.position + (RckPlayer.instance.transform.forward * 1.5f) + Vector3.up;

            var allItems = SaveSystem.SaveSystemManager.instance.saveFile.CreatedItemsInWorldData.allCreatedItemsInWorld;
            // Add this created item
            if (allItems.ContainsKey(WorldManager.instance.currentCenterCell.ID))
                allItems[WorldManager.instance.currentCenterCell.ID].itemsInThis.Add(itemInWorld.ToCreatedItemSaveData());
            else
            {
                allItems.Add(WorldManager.instance.currentCenterCell.ID, new SaveSystem.CreatedItemInWorldCollection());
                allItems[WorldManager.instance.currentCenterCell.ID].itemsInThis.Add(itemInWorld.ToCreatedItemSaveData());
            }

            // Remove item from inventory
            Inventory.PlayerInventory.RemoveItem(bookItemInInventory, 1);
            bookItemInInventory = null;
            bookItem = null;

            InventoryUI.instance.SelectNextButton();
            InventoryUI.instance.UpdateStatsUI();

            pool.usedObjects.Remove(this);
            pool.BooksPool.usedObjects.Remove(this);

            // Disable this object
            gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            gameObject.SetActive(false);
        }

        private void DropCumulableItemPanel()
        {
            // If it is a quest item you can't drop it
            if (bookItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", 3);
                return;
            }

            InventoryUI.instance.dropItemsPanel.gameObject.SetActive(true);
            InventoryUI.instance.dropItemsPanel.Init(bookItemInInventory, this);
        }

        public override void ConfirmButtonCumulableItem(int amount)
        {
            ItemInWorld itemInWorld = Instantiate(bookItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = bookItemInInventory.metadata;
            itemInWorld.isCreatedItem = true;
            itemInWorld.Amount = amount;
            itemInWorld.gameObject.transform.position = RckPlayer.instance.transform.position + (RckPlayer.instance.transform.forward * 1.5f) + Vector3.up;

            var allItems = SaveSystem.SaveSystemManager.instance.saveFile.CreatedItemsInWorldData.allCreatedItemsInWorld;
            // Add this created item
            if (allItems.ContainsKey(WorldManager.instance.currentCenterCell.ID))
                allItems[WorldManager.instance.currentCenterCell.ID].itemsInThis.Add(itemInWorld.ToCreatedItemSaveData());
            else
            {
                allItems.Add(WorldManager.instance.currentCenterCell.ID, new SaveSystem.CreatedItemInWorldCollection());
                allItems[WorldManager.instance.currentCenterCell.ID].itemsInThis.Add(itemInWorld.ToCreatedItemSaveData());
            }

            itemInWorld.transform.position = RckPlayer.instance.transform.position + (RckPlayer.instance.transform.forward * 1.5f) + Vector3.up;

            Inventory.PlayerInventory.RemoveItem(bookItemInInventory, amount);

            if (bookItemInInventory.Amount <= 0)
            {
                InventoryUI.instance.dropItemsPanel.SetPreviousSelected();
                InventoryUI.instance.SelectNextButton(true);

                //If we've dropped all the items
                pool.usedObjects.Remove(this);
                pool.BooksPool.usedObjects.Remove(this);

                InventoryUI.instance.ShowTabItems();

                // Disable this object
                gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                gameObject.SetActive(false);
            }
            else
            {
                // Update the UI
                Init();
            }

            InventoryUI.instance.UpdateStatsUI();

        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (bookItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = bookItem.tooltipValue;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (bookItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (bookItem != null && bookItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = bookItem.tooltipValue;
            }
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (bookItem != null && bookItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }
    }
}