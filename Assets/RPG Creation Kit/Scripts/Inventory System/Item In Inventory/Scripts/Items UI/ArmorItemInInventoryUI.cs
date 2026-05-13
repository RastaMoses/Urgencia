using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using UnityEngine.EventSystems;
using RPGCreationKit.Player;
using Unity.VisualScripting;

namespace RPGCreationKit
{
    /// <summary>
    /// ArmorItem UI in Inventory
    /// </summary>
    public class ArmorItemInInventoryUI : ItemInInventoryUI, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
    {
        public ItemInInventory armorItemInInventory;
        private ArmorItem armorItem;

        // Use this for initialization
        public void Init()
        {
            armorItem = (ArmorItem)armorItemInInventory.item;
            UpdateItem();
        }

        public override void UpdateItem()
        {
            base.UpdateItem();

            // Set the UI elements
            Icon.sprite = armorItem.ItemIcon;
            Name.text = armorItem.ItemName;
            DamageVal.text = "-";
            ArmorVal.text = armorItem.ArmorRating.ToString();
            WeightVal.text = armorItem.Weight.ToString();
            GoldsVal.text = armorItem.Value.ToString();
            AmountVal.text = "x" + armorItemInInventory.Amount.ToString();

            AmountVal.gameObject.SetActive((armorItemInInventory.Amount > 1 && armorItem.isCumulable) ? true : false);

            equippedIcon.SetActive(armorItemInInventory.isEquipped);
            stolenIcon.SetActive(armorItemInInventory.metadata.isOwned);
        }

        
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                EquipUnequip();
            }
            else if(eventData.button == PointerEventData.InputButton.Right)
            {
                if ((armorItem) && (!armorItem.isCumulable || armorItemInInventory.Amount <= 1))
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
                if ((armorItem) && (!armorItem.isCumulable || armorItemInInventory.Amount <= 1))
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
            if (!armorItem.isCumulable || armorItemInInventory.Amount <= 1)
                DropItem();
            else
                DropCumulableItemPanel();
        }

        private void EquipUnequip()
        {
            if (!armorItemInInventory.isEquipped)
            {
                if (!Equipment.PlayerEquipment.Equip(armorItemInInventory))
                {
                    AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                    return;
                }

                if (armorItem.sOnEquipOrUse)
                    GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, armorItem.sOnEquipOrUse);
            }
            else
            {
                if(!Equipment.PlayerEquipment.Unequip(armorItemInInventory))
                {
                    AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                    return;
                }

                if (armorItem.sOnUnEquip)
                    GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, armorItem.sOnUnEquip);
            }


            UpdateItem();
            InventoryUI.instance.UpdateStatsUI();
            InventoryUI.instance.UpdateAllItems();
        }

        public override void DropItem()
        {
            base.DropItem();

            if (PlayerCombat.instance.isAttacking)
            {
                AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                return;
            }

            // If it is a quest item you can't drop it
            if (armorItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                return;
            }

            // Spawn the item in the world
            ItemInWorld itemInWorld = Instantiate(armorItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = armorItemInInventory.metadata;
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

            if (armorItemInInventory.isEquipped)
                Equipment.PlayerEquipment.Unequip(armorItemInInventory);

            // Remove item from inventory
            Inventory.PlayerInventory.RemoveItem(armorItemInInventory, 1);
            armorItemInInventory = null;
            armorItem = null;
           
            InventoryUI.instance.SelectNextButton();
            InventoryUI.instance.UpdateStatsUI();

            pool.usedObjects.Remove(this);
            pool.ArmorsPool.usedObjects.Remove(this);

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
            if (armorItem.QuestItem)
            {
                AlertMessage.instance.InitAlertMessage("You can't drop Quest Items", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                return;
            }

            InventoryUI.instance.dropItemsPanel.gameObject.SetActive(true);
            InventoryUI.instance.dropItemsPanel.Init(armorItemInInventory, this);
        }

        public override void ConfirmButtonCumulableItem(int amount)
        {
            ItemInWorld itemInWorld = Instantiate(armorItem.itemInWorld).GetComponent<ItemInWorld>();
            itemInWorld.metadata = armorItemInInventory.metadata;
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

            if(armorItemInInventory.Amount <= amount)
                if (armorItemInInventory.isEquipped)
                    Equipment.PlayerEquipment.Unequip(armorItemInInventory);

            Inventory.PlayerInventory.RemoveItem(armorItemInInventory, amount);

            if (armorItemInInventory.Amount <= 0)
            {
                InventoryUI.instance.dropItemsPanel.SetPreviousSelected();
                InventoryUI.instance.SelectNextButton(true);

                //If we've dropped all the items
                pool.usedObjects.Remove(this);
                pool.ArmorsPool.usedObjects.Remove(this);


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
            if (armorItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = armorItem.tooltipValue;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (armorItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (armorItem != null && armorItem.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = armorItem.tooltipValue;
            }
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (armorItem != null && armorItem.usesTooltip)
                tooltipGameObject.SetActive(false);
        }
    }
}