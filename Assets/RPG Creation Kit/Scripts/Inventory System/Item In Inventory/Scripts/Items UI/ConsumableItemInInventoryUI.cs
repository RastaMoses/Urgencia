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
    public class ConsumableItemInInventoryUI : ItemInInventoryUI, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
    {
        public ItemInInventory consumableItemInInventory;
        private ConsumableItem consumableItem;

        // Use this for initialization
        public void Init()
        {
            consumableItem = (ConsumableItem)consumableItemInInventory.item;
            UpdateItem();
        }

        public override void UpdateItem()
        {
            base.UpdateItem();

            // Set the UI elements
            Icon.sprite = consumableItem.ItemIcon;
            Name.text = consumableItem.ItemName;
            DamageVal.text = "-";
            ArmorVal.text = "-";
            WeightVal.text = consumableItem.Weight.ToString();
            GoldsVal.text = consumableItem.Value.ToString();
            AmountVal.text = "x" + consumableItemInInventory.Amount.ToString();

            AmountVal.gameObject.SetActive((consumableItemInInventory.Amount > 1 && consumableItem.isCumulable) ? true : false);

            stolenIcon.SetActive(consumableItemInInventory.metadata.isOwned);

        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                UseConsumable();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (!consumableItem.isCumulable || consumableItemInInventory.Amount <= 1)
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
                if (!consumableItem.isCumulable || consumableItemInInventory.Amount <= 1)
                    DropItem();
                else
                    DropCumulableItemPanel();
            }
            else
                UseConsumable();
        }
        */

        /// <summary>
        /// Mainly used for gamepad or to have a single input that drops the item
        /// </summary>
        public override void OnClickForDrop()
        {
            if (!consumableItem.isCumulable || consumableItemInInventory.Amount <= 1)
                DropItem();
            else
                DropCumulableItemPanel();
        }

        private void UseConsumable()
        {
            if (consumableItem.sOnEquipOrUse)
                GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, consumableItem.sOnEquipOrUse);

            // Consume it
            consumableItem.Use(EntityAttributes.PlayerAttributes);

            if (!consumableItem.isCumulable || consumableItem.isCumulable && consumableItemInInventory.Amount-1 <= 0)
            {
                InventoryUI.instance.SelectNextButton();

                pool.usedObjects.Remove(this);
                pool.ConsumablesPool.usedObjects.Remove(this);

                // Disable this object
                gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                gameObject.SetActive(false);

                // Remove item from inventory
                Inventory.PlayerInventory.RemoveItem(consumableItemInInventory, 1, false);
            }
            else
            {
                // Remove item from inventory
                Inventory.PlayerInventory.RemoveItem(consumableItemInInventory, 1, false);

                Init();
            }

        }

        public override void DropItem()
        {
            base.DropItem();
            // If it is a quest item you can't drop it
            if (consumableItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", 3);
                return;
            }

            // Spawn the item in the world
            ItemInWorld itemInWorld = Instantiate(consumableItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = consumableItemInInventory.metadata;
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
            Inventory.PlayerInventory.RemoveItem(consumableItemInInventory, 1);
            consumableItemInInventory = null;
            consumableItem = null;

            InventoryUI.instance.SelectNextButton();
            InventoryUI.instance.UpdateStatsUI();

            // Disable this object
            pool.usedObjects.Remove(this);
            pool.ConsumablesPool.usedObjects.Remove(this);

            gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            gameObject.SetActive(false);
        }

        private void DropCumulableItemPanel()
        {
            // If it is a quest item you can't drop it
            if (consumableItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", 3);
                return;
            }

            InventoryUI.instance.dropItemsPanel.gameObject.SetActive(true);
            InventoryUI.instance.dropItemsPanel.Init(consumableItemInInventory, this);
        }

        public override void ConfirmButtonCumulableItem(int amount)
        {
            // Spawn the item in the world
            ItemInWorld itemInWorld = Instantiate(consumableItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = consumableItemInInventory.metadata;
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

            Inventory.PlayerInventory.RemoveItem(consumableItemInInventory, amount);

            if (consumableItemInInventory.Amount <= 0)
            {
                InventoryUI.instance.dropItemsPanel.SetPreviousSelected();
                InventoryUI.instance.SelectNextButton(true);

                //If we've dropped all the items
                pool.usedObjects.Remove(this);
                pool.ConsumablesPool.usedObjects.Remove(this);

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
            if (consumableItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = consumableItem.tooltipValue;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (consumableItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (consumableItem != null && consumableItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = consumableItem.tooltipValue;
            }
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (consumableItem != null && consumableItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }
    }
}