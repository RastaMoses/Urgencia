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
    /// AmmoItem UI in Inventory
    /// </summary>
    public class AmmoItemInInventoryUI : ItemInInventoryUI, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
    {
        public ItemInInventory ammoItemInInventory;
        private AmmoItem ammoItem;

        // Use this for initialization
        public void Init()
        {
            ammoItem = (AmmoItem)ammoItemInInventory.item;
            UpdateItem();
        }

        public override void UpdateItem()
        {
            base.UpdateItem();

            // Set the UI elements
            Icon.sprite = ammoItem.ItemIcon;
            Name.text = ammoItem.ItemName;
            DamageVal.text = ammoItem.Damage.ToString();
            ArmorVal.text = "-";
            WeightVal.text = ammoItem.Weight.ToString();
            GoldsVal.text = ammoItem.Value.ToString();
            AmountVal.text = "x" + ammoItemInInventory.Amount.ToString();

            AmountVal.gameObject.SetActive((ammoItemInInventory.Amount > 1 && ammoItem.isCumulable) ? true : false);

            equippedIcon.SetActive(ammoItemInInventory.isEquipped);
            stolenIcon.SetActive(ammoItemInInventory.metadata.isOwned);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                EquipUnequip();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if ((ammoItem) && (!ammoItem.isCumulable || ammoItemInInventory.Amount <= 1))
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
                if (!ammoItem.isCumulable || ammoItemInInventory.Amount <= 1)
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
            if (!ammoItem.isCumulable || ammoItemInInventory.Amount <= 1)
                DropItem();
            else
                DropCumulableItemPanel();
        }

        private void EquipUnequip()
        {
            if (!ammoItem.canBeEquipped)
                return;

            if (!ammoItemInInventory.isEquipped)
            {
                if (!Equipment.PlayerEquipment.Equip(ammoItemInInventory))
                {
                    AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                    return;
                }

                if (ammoItem.sOnEquipOrUse)
                    GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, ammoItem.sOnEquipOrUse);
            }
            else
            {
                if (!Equipment.PlayerEquipment.Unequip(ammoItemInInventory))
                {
                    AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                    return;
                }

                if (ammoItem.sOnUnEquip)
                    GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, ammoItem.sOnUnEquip);
            }

            PlayerInInventory.instance.OnEquipmentChangesAmmo();


            UpdateItem();

            InventoryUI.instance.UpdateStatsUI();
            InventoryUI.instance.UpdateAllItems();
        }

        public override void DropItem()
        {
            if (PlayerCombat.instance.isAttacking)
            {
                AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                return;
            }

            base.DropItem();

            // If it is a quest item you can't drop it
            if (ammoItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", 3);
                return;
            }


            // Spawn the item in the world
            ItemInWorld itemInWorld = Instantiate(ammoItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = ammoItemInInventory.metadata;
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

            if (ammoItemInInventory.isEquipped)
            {
                Equipment.PlayerEquipment.Unequip(ammoItemInInventory);
                PlayerCombat.instance.OnEquipmentChanges();
                PlayerInInventory.instance.OnEquipmentChangesHands();
                PlayerInInventory.instance.OnEquipmentChangesAmmo();
            }

            // Remove item from inventory
            Inventory.PlayerInventory.RemoveItem(ammoItemInInventory, 1);
            ammoItemInInventory = null;
            ammoItem = null;

            InventoryUI.instance.SelectNextButton();
            InventoryUI.instance.UpdateStatsUI();

            pool.usedObjects.Remove(this);
            pool.AmmosPool.usedObjects.Remove(this);

            // Disable this object
            gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            gameObject.SetActive(false);



        }

        private void DropCumulableItemPanel()
        {
            if (PlayerCombat.instance.isAttacking)
            {
                AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                return;
            }

            // If it is a quest item you can't drop it
            if (ammoItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", 3);
                return;
            }

            InventoryUI.instance.dropItemsPanel.gameObject.SetActive(true);
            InventoryUI.instance.dropItemsPanel.Init(ammoItemInInventory, this);
        }

        public override void ConfirmButtonCumulableItem(int amount)
        {
            // Spawn the item in the world
            ItemInWorld itemInWorld = Instantiate(ammoItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = ammoItemInInventory.metadata;
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

            if(ammoItemInInventory.Amount <= amount)
                if (ammoItemInInventory.isEquipped)
                {
                    Equipment.PlayerEquipment.Unequip(ammoItemInInventory);
                    PlayerCombat.instance.OnEquipmentChanges();
                    PlayerInInventory.instance.OnEquipmentChangesHands();
                    PlayerInInventory.instance.OnEquipmentChangesAmmo();
                }

            Inventory.PlayerInventory.RemoveItem(ammoItemInInventory, amount);

            if (ammoItemInInventory.Amount <= 0)
            {
                InventoryUI.instance.dropItemsPanel.SetPreviousSelected();
                InventoryUI.instance.SelectNextButton(true);

                //If we've dropped all the items
                pool.usedObjects.Remove(this);
                pool.AmmosPool.usedObjects.Remove(this);

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
            if (ammoItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = ammoItem.tooltipValue;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (ammoItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (ammoItem != null && ammoItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = ammoItem.tooltipValue;
            }
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (ammoItem != null && ammoItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }
    }
}