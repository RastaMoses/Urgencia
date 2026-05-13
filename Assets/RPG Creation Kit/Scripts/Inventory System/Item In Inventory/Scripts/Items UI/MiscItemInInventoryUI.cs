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
    /// MiscItem UI in Inventory
    /// </summary>
    public class MiscItemInInventoryUI : ItemInInventoryUI, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
    {
        public ItemInInventory miscItemInInventory;
        private MiscItem miscItem;

        // Use this for initialization
        public void Init()
        {
            miscItem = (MiscItem)miscItemInInventory.item;
            UpdateItem();
        }

        public override void UpdateItem()
        {
            base.UpdateItem();

            // Set the UI elements
            Icon.sprite = miscItem.ItemIcon;
            Name.text = miscItem.ItemName;
            DamageVal.text = "-";
            ArmorVal.text = "-";
            WeightVal.text = miscItem.Weight.ToString();
            GoldsVal.text = miscItem.Value.ToString();
            AmountVal.text = "x" + miscItemInInventory.Amount.ToString();

            AmountVal.gameObject.SetActive((miscItemInInventory.Amount > 1 && miscItem.isCumulable) ? true : false);

            stolenIcon.SetActive(miscItemInInventory.metadata.isOwned);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                EquipUnequip();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (!miscItem.isCumulable || miscItemInInventory.Amount <= 1)
                    DropItem();
                else
                    DropCumulableItemPanel();
            }
        }

        /*
        public override void OnClick(bool takeAll = false)
        {
            if (InventoryUI.instance.dropKeyBeingHeld)
            {
                if (!miscItem.isCumulable || miscItemInInventory.Amount <= 1)
                    DropItem();
                else
                    DropCumulableItemPanel();
            }
            else
                EquipUnequip();
        }
        */

        /// <summary>
        /// Mainly used for gamepad or to have a single input that drops the item
        /// </summary>
        public override void OnClickForDrop()
        {
            if (!miscItem.isCumulable || miscItemInInventory.Amount <= 1)
                DropItem();
            else
                DropCumulableItemPanel();
        }

        private void EquipUnequip()
        {

        }

        public override void DropItem()
        {
            base.DropItem();
            // If it is a quest item you can't drop it
            if (miscItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", 3);
                return;
            }

            // Spawn the item in the world
            ItemInWorld itemInWorld = Instantiate(miscItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = miscItemInInventory.metadata;
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
            Inventory.PlayerInventory.RemoveItem(miscItemInInventory, 1);
            miscItemInInventory = null;
            miscItem = null;

            InventoryUI.instance.SelectNextButton();
            InventoryUI.instance.UpdateStatsUI();

            pool.usedObjects.Remove(this);
            pool.MiscsPool.usedObjects.Remove(this);

            // Disable this object
            gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            gameObject.SetActive(false);
        }

        private void DropCumulableItemPanel()
        {
            // If it is a quest item you can't drop it
            if (miscItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", 3);
                return;
            }

            InventoryUI.instance.dropItemsPanel.gameObject.SetActive(true);
            InventoryUI.instance.dropItemsPanel.Init(miscItemInInventory, this);
        }

        public override void ConfirmButtonCumulableItem(int amount)
        {
            // Spawn the item in the world
            ItemInWorld itemInWorld = Instantiate(miscItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = miscItemInInventory.metadata;
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

            Inventory.PlayerInventory.RemoveItem(miscItemInInventory, amount);

            if (miscItemInInventory.Amount < amount)
            {
                //If we've dropped all the items
                // Remove item from inventory (whole)
                miscItemInInventory = null;
                miscItem = null;

                InventoryUI.instance.dropItemsPanel.SetPreviousSelected();
                InventoryUI.instance.SelectNextButton(true);

                pool.usedObjects.Remove(this);
                pool.MiscsPool.usedObjects.Remove(this);

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
            if (miscItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = miscItem.tooltipValue;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (miscItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (miscItem != null && miscItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = miscItem.tooltipValue;
            }
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (miscItem != null && miscItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }
    }
}