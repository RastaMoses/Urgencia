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
    /// KeyItem UI in Inventory
    /// </summary>
    public class KeyItemInInventoryUI : ItemInInventoryUI, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
    {
        public ItemInInventory keyItemInInventory;
        private KeyItem keyItem;

        // Use this for initialization
        public void Init()
        {
            keyItem = (KeyItem)keyItemInInventory.item;
            UpdateItem();
        }

        public override void UpdateItem()
        {
            base.UpdateItem();

            // Set the UI elements
            Icon.sprite = keyItem.ItemIcon;
            Name.text = keyItem.ItemName;
            DamageVal.text = "-";
            ArmorVal.text = "-";
            WeightVal.text = keyItem.Weight.ToString();
            GoldsVal.text = keyItem.Value.ToString();
            AmountVal.text = "x" + keyItemInInventory.Amount.ToString();

            AmountVal.gameObject.SetActive((keyItemInInventory.Amount > 1 && keyItem.isCumulable) ? true : false);

            stolenIcon.SetActive(keyItemInInventory.metadata.isOwned);

        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                EquipUnequip();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                DropItem();
            }
        }

        /*
        public override void OnClick(bool takeAll = false)
        {
            if (InventoryUI.instance.dropKeyBeingHeld)
                DropItem();
            else
                EquipUnequip();
        }
        */

        /// <summary>
        /// Mainly used for gamepad or to have a single input that drops the item
        /// </summary>
        public override void OnClickForDrop()
        {
            if (!keyItem.isCumulable || keyItemInInventory.Amount <= 1)
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
            if (keyItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", 3);
                return;
            }

            // Spawn the item in the world
            ItemInWorld itemInWorld = Instantiate(keyItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = keyItemInInventory.metadata;
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
            Inventory.PlayerInventory.RemoveItem(keyItemInInventory, 1);
            keyItemInInventory = null;
            keyItem = null;

            //InventoryUI.instance.ShowTabItems();
            InventoryUI.instance.UpdateStatsUI();

            pool.usedObjects.Remove(this);
            pool.KeysPool.usedObjects.Remove(this);

            // Disable this object
            gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            gameObject.SetActive(false);
            InventoryUI.instance.SelectNextButton();
        }

        private void DropCumulableItemPanel()
        {
            // If it is a quest item you can't drop it
            if (keyItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", 3);
                return;
            }

            InventoryUI.instance.dropItemsPanel.gameObject.SetActive(true);
            InventoryUI.instance.dropItemsPanel.Init(keyItemInInventory, this);
        }

        public override void ConfirmButtonCumulableItem(int amount)
        {
            // Spawn the item in the world
            ItemInWorld itemInWorld = Instantiate(keyItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = keyItemInInventory.metadata;
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

            Inventory.PlayerInventory.RemoveItem(keyItemInInventory, amount);

            if (keyItemInInventory.Amount < amount)
            {
                //If we've dropped all the items
                // Remove item from inventory (whole)
                keyItemInInventory = null;
                keyItem = null;

                InventoryUI.instance.dropItemsPanel.SetPreviousSelected();
                InventoryUI.instance.SelectNextButton(true);

                pool.usedObjects.Remove(this);
                pool.KeysPool.usedObjects.Remove(this);

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
            if (keyItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = keyItem.tooltipValue;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (keyItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (keyItem != null && keyItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = keyItem.tooltipValue;
            }
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (keyItem != null && keyItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }
    }
}