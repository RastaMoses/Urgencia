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
    /// WeaponItem UI in Inventory
    /// </summary>
    public class WeaponItemInInventoryUI : ItemInInventoryUI, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
    {
        public ItemInInventory weaponItemInInventory;
        private WeaponItem weaponItem;

        // Use this for initialization
        public void Init()
        {
            weaponItem = (WeaponItem)weaponItemInInventory.item;
            UpdateItem();
        }

        public override void UpdateItem()
        {
            base.UpdateItem();

            // Set the UI elements
            Icon.sprite = weaponItem.ItemIcon;
            Name.text = weaponItem.ItemName;
            DamageVal.text = weaponItem.Damage.ToString();
            ArmorVal.text = "-";
            WeightVal.text = weaponItem.Weight.ToString();
            GoldsVal.text = weaponItem.Value.ToString();
            AmountVal.text = "x" + weaponItemInInventory.Amount.ToString();

            AmountVal.gameObject.SetActive((weaponItemInInventory.Amount > 1 && weaponItem.isCumulable) ? true : false);

            equippedIcon.SetActive(weaponItemInInventory.isEquipped);

            stolenIcon.SetActive(weaponItemInInventory.metadata.isOwned);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                EquipUnequip();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (!weaponItem.isCumulable || weaponItemInInventory.Amount <= 1)
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
                if (!weaponItem.isCumulable || weaponItemInInventory.Amount <= 1)
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
            if (!weaponItem.isCumulable || weaponItemInInventory.Amount <= 1)
                DropItem();
            else
                DropCumulableItemPanel();
        }

        bool swap = false;
        private void EquipUnequip()
        {
            if (!weaponItemInInventory.isEquipped)
            {
                if (!Equipment.PlayerEquipment.Equip(weaponItemInInventory))
                {
                    AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_SHORT + 1);
                    return;
                }

                if (weaponItem.sOnEquipOrUse)
                    GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, weaponItem.sOnEquipOrUse);
            }
            else
            {
                if (!Equipment.PlayerEquipment.Unequip(weaponItemInInventory))
                {
                    AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_SHORT + 1);
                    return;
                }

                if (weaponItem.sOnUnEquip)
                    GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, weaponItem.sOnUnEquip);
            }


            PlayerCombat.instance.OnEquipmentChanges();
            PlayerInInventory.instance.OnEquipmentChangesHands();

            InventoryUI.instance.UpdateStatsUI();
            InventoryUI.instance.UpdateAllItems();
        }

        public override void DropItem()
        {
            if (PlayerCombat.instance.isAttacking)
            {
                AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_SHORT + 1);
                return;
            }

            base.DropItem();
            // If it is a quest item you can't drop it
            if (weaponItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", AlertMessage.DEFAULT_MESSAGE_DURATION_SHORT);
                return;
            }

            // Spawn the item in the world
            ItemInWorld itemInWorld = Instantiate(weaponItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = weaponItemInInventory.metadata;
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

            if (weaponItemInInventory.isEquipped)
            {
                Equipment.PlayerEquipment.Unequip(weaponItemInInventory);
                PlayerCombat.instance.OnEquipmentChanges();
                PlayerInInventory.instance.OnEquipmentChangesHands();
            }

            Inventory.PlayerInventory.RemoveItem(weaponItemInInventory, 1);
            weaponItemInInventory = null;
            weaponItem = null;

            InventoryUI.instance.SelectNextButton();
            InventoryUI.instance.UpdateStatsUI();

            PlayerInInventory.instance.OnEquipmentChangesHands();
            if(Equipment.PlayerEquipment.itemsEquipped[(int)EquipmentSlots.RHand] == weaponItemInInventory)
                PlayerCombat.instance.RemoveCurrentWeapon();

            pool.usedObjects.Remove(this);
            pool.WeaponsPool.usedObjects.Remove(this);

            // Disable this object
            gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            gameObject.SetActive(false);

        }

        private void DropCumulableItemPanel()
        {
            if (PlayerCombat.instance.isAttacking)
            {
                AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_SHORT + 1);
                return;
            }

            // If it is a quest item you can't drop it
            if (weaponItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", 3);
                return;
            }

            InventoryUI.instance.dropItemsPanel.gameObject.SetActive(true);
            InventoryUI.instance.dropItemsPanel.Init(weaponItemInInventory, this);
        }

        public override void ConfirmButtonCumulableItem(int amount)
        {
            // Spawn the item in the world
            ItemInWorld itemInWorld = Instantiate(weaponItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = weaponItemInInventory.metadata;
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

            if (weaponItemInInventory.Amount <= amount)
            {
                if (weaponItemInInventory.isEquipped)
                {
                    Equipment.PlayerEquipment.Unequip(weaponItemInInventory);
                    PlayerCombat.instance.OnEquipmentChanges();
                    PlayerInInventory.instance.OnEquipmentChangesHands();
                }
            }

            Inventory.PlayerInventory.RemoveItem(weaponItemInInventory, amount);

            if (weaponItemInInventory.Amount < amount)
            {
                //If we've dropped all the items
                // Remove item from inventory (whole)
                weaponItemInInventory = null;
                weaponItem = null;

                InventoryUI.instance.dropItemsPanel.SetPreviousSelected();
                InventoryUI.instance.SelectNextButton(true);

                pool.usedObjects.Remove(this);
                pool.WeaponsPool.usedObjects.Remove(this);

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
            if (weaponItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = weaponItem.tooltipValue;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (weaponItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (weaponItem != null && weaponItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = weaponItem.tooltipValue;
            }
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (weaponItem != null && weaponItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }
    }
}