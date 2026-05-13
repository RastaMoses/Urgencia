using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using UnityEngine.EventSystems;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    public class WeaponItemInLootingUI_DrawDeposit : WeaponItemInInventoryUI
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                OnClick(false);
        }

        public override void OnClick(bool takeAll = false)
        {
            if (LootingInventoryUI.instance.isDrawing)
            {
                // if the amount is 1, add it one time
                if (base.weaponItemInInventory.Amount <= 1)
                {
                    // Check for failure
                    if (LootingInventoryUI.instance.isPickPocketing)
                    {
                        float dotProduct = Vector3.Dot(LootingInventoryUI.instance.pickPocketingAI.transform.position - Player.RckPlayer.instance.transform.position, LootingInventoryUI.instance.pickPocketingAI.transform.forward);
                        float chance = RCKFunctions.CalculatePickpocketChance(dotProduct > 0, weaponItemInInventory.item.Weight, Player.RckPlayer.instance.playerAttributes.attributes.Dexterity, weaponItemInInventory.isEquipped);

                        int result = Random.Range(0, 100);

                        // Fail check
                        if (result > chance)
                        {
                            AI.RckAI ai = LootingInventoryUI.instance.pickPocketingAI;

                            ai.EnterInCombatAgainst(Player.RckPlayer.GetPlayerEntity());
                            if (RCKSettings.PICKPOCKET_DIALOGUE_CLIP_PLAYS)
                            {
                                if (ai.bodyData.isMale)
                                    RCKFunctions.MakeAISpeakLine(ai, "DCLIP_SPECIAL_PICKPOCKET_MALE");
                                else
                                    RCKFunctions.MakeAISpeakLine(ai, "DCLIP_SPECIAL_PICKPOCKET_FEMALE");
                            }
                            RCKFunctions.DisplayHeardLine(ai.entityName + ": Help! Pickpocket!", 3.5f);
                            AlertMessage.instance.InitAlertMessage("You have been caught pickpocketing!", 4f);
                            LootingInventoryUI.instance.CloseUI();
                            return;
                        }
                    }

                    Inventory.PlayerInventory.AddItem(base.weaponItemInInventory.item, base.weaponItemInInventory.metadata, 1, !takeAll);

                    if (base.weaponItemInInventory.isEquipped)
                        LootingInventoryUI.instance.curLootingPoint.equipment.Unequip(base.weaponItemInInventory);

                    // Remove the item from the current loot inventory
                    LootingInventoryUI.instance.curLootingPoint.inventory.RemoveItem(base.weaponItemInInventory, 1);

                    LootingInventoryUI.instance.SelectNextButton();

                    // Disable this object
                    pool.usedObjects.Remove(this);
                    pool.WeaponsPool.usedObjects.Remove(this);

                    gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                    gameObject.SetActive(false);

                    // Check steal
                    if (!LootingInventoryUI.instance.isPickPocketing && RCKFunctions.CheckPlayerStealsItem(base.weaponItemInInventory))
                    {
                        // Close looting ui
                        LootingInventoryUI.instance.CloseUI();
                    }
                }
                else if (base.weaponItemInInventory.item.isCumulable && base.weaponItemInInventory.Amount > 1)
                {
                    if (!takeAll)
                    {
                        LootingInventoryUI.instance.takeDepositItemsPanel.gameObject.SetActive(true);
                        LootingInventoryUI.instance.takeDepositItemsPanel.Init(weaponItemInInventory, this);
                    }
                    else
                    {
                        ConfirmButtonCumulableItem(base.weaponItemInInventory.Amount);
                    }
                }
            }
            else // we're depositing
            {
                if (base.weaponItemInInventory.item.QuestItem)
                {
                    AlertMessage.instance.InitAlertMessage("You can't leave Quest Items", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                    return;
                }

                // if the amount is 1, add it one time
                if (base.weaponItemInInventory.Amount <= 1)
                {
                    // Remove the item from the current loot inventory
                    LootingInventoryUI.instance.curLootingPoint.inventory.AddItem(base.weaponItemInInventory.item, base.weaponItemInInventory.metadata, 1);


                    if (base.weaponItemInInventory.isEquipped)
                    {
                        Equipment.PlayerEquipment.Unequip(base.weaponItemInInventory);
                        PlayerCombat.instance.OnEquipmentChanges();
                        PlayerInInventory.instance.OnEquipmentChangesHands();
                    }

                    Inventory.PlayerInventory.RemoveItem(base.weaponItemInInventory, 1);


                    LootingInventoryUI.instance.SelectNextButton();

                    // Disable this object
                    pool.usedObjects.Remove(this);
                    pool.WeaponsPool.usedObjects.Remove(this);

                    gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                    gameObject.SetActive(false);
                }
                else if (base.weaponItemInInventory.item.isCumulable && base.weaponItemInInventory.Amount > 1)
                {
                    LootingInventoryUI.instance.takeDepositItemsPanel.gameObject.SetActive(true);
                    LootingInventoryUI.instance.takeDepositItemsPanel.Init(weaponItemInInventory, this);
                }
            }
        }

        public override void ConfirmButtonCumulableItem(int amount)
        {
            if (LootingInventoryUI.instance.isDrawing)
            {
                // Check for failure
                if (LootingInventoryUI.instance.isPickPocketing)
                {
                    float dotProduct = Vector3.Dot(LootingInventoryUI.instance.pickPocketingAI.transform.position - Player.RckPlayer.instance.transform.position, LootingInventoryUI.instance.pickPocketingAI.transform.forward);
                    float chance = RCKFunctions.CalculatePickpocketChance(dotProduct > 0, weaponItemInInventory.item.Weight, Player.RckPlayer.instance.playerAttributes.attributes.Dexterity, weaponItemInInventory.isEquipped, amount);

                    int result = Random.Range(0, 100);

                    // Fail check
                    if (result > chance)
                    {
                        AI.RckAI ai = LootingInventoryUI.instance.pickPocketingAI;

                        ai.EnterInCombatAgainst(Player.RckPlayer.GetPlayerEntity());
                        if (RCKSettings.PICKPOCKET_DIALOGUE_CLIP_PLAYS)
                        {
                            if (ai.bodyData.isMale)
                                RCKFunctions.MakeAISpeakLine(ai, "DCLIP_SPECIAL_PICKPOCKET_MALE");
                            else
                                RCKFunctions.MakeAISpeakLine(ai, "DCLIP_SPECIAL_PICKPOCKET_FEMALE");
                        }
                        RCKFunctions.DisplayHeardLine(ai.entityName + ": Help! Pickpocket!", 3.5f);
                        AlertMessage.instance.InitAlertMessage("You have been caught pickpocketing!", 4f);
                        LootingInventoryUI.instance.CloseUI();
                        return;
                    }
                }

                // Add in PlayerInventory
                Inventory.PlayerInventory.AddItem(base.weaponItemInInventory.item, base.weaponItemInInventory.metadata, amount);

                if (base.weaponItemInInventory.Amount <= amount)
                    if (base.weaponItemInInventory.isEquipped)
                        LootingInventoryUI.instance.curLootingPoint.equipment.Unequip(base.weaponItemInInventory);

                // Remove from Loot Inventory
                LootingInventoryUI.instance.curLootingPoint.inventory.RemoveItem(base.weaponItemInInventory, amount);

                if (weaponItemInInventory.Amount <= 0)
                {
                    LootingInventoryUI.instance.takeDepositItemsPanel.SetPreviousSelected();
                    LootingInventoryUI.instance.SelectNextButton(true);

                    //If we've took all the items
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

                // Check steal
                if (!LootingInventoryUI.instance.isPickPocketing && RCKFunctions.CheckPlayerStealsItem(base.weaponItemInInventory))
                {
                    // Close looting ui
                    LootingInventoryUI.instance.CloseUI();
                }
            }
            else // we're depositing
            {
                // Remove from Loot Inventory
                LootingInventoryUI.instance.curLootingPoint.inventory.AddItem(base.weaponItemInInventory.item, base.weaponItemInInventory.metadata, amount);

                if(base.weaponItemInInventory.Amount <= amount)
                    if (weaponItemInInventory.isEquipped)
                    {
                        Equipment.PlayerEquipment.Unequip(weaponItemInInventory);
                        PlayerCombat.instance.OnEquipmentChanges();
                        PlayerInInventory.instance.OnEquipmentChangesHands();
                    }

                // Add in PlayerInventory
                Inventory.PlayerInventory.RemoveItem(base.weaponItemInInventory, amount);

                if (weaponItemInInventory.Amount <= 0)
                {
                    LootingInventoryUI.instance.takeDepositItemsPanel.SetPreviousSelected();
                    LootingInventoryUI.instance.SelectNextButton(true);

                    //If we've took all the items
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
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if(LootingInventoryUI.instance.isPickPocketing)
            {
                // If pickpocketing, show/hide the chance
                float dotProduct = Vector3.Dot(LootingInventoryUI.instance.pickPocketingAI.transform.position - Player.RckPlayer.instance.transform.position, LootingInventoryUI.instance.pickPocketingAI.transform.forward);
                float chance = RCKFunctions.CalculatePickpocketChance(dotProduct > 0, weaponItemInInventory.item.Weight, Player.RckPlayer.instance.playerAttributes.attributes.Dexterity, weaponItemInInventory.isEquipped);
                LootingInventoryUI.instance.pickpocketChanceText.text = chance + "%";
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (LootingInventoryUI.instance.isPickPocketing)
            {
                // If pickpocketing, show/hide the chance
                LootingInventoryUI.instance.pickpocketChanceText.text = "?";
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (LootingInventoryUI.instance.isPickPocketing)
            {
                // If pickpocketing, show/hide the chance
                float dotProduct = Vector3.Dot(LootingInventoryUI.instance.pickPocketingAI.transform.position - Player.RckPlayer.instance.transform.position, LootingInventoryUI.instance.pickPocketingAI.transform.forward);
                float chance = RCKFunctions.CalculatePickpocketChance(dotProduct > 0, weaponItemInInventory.item.Weight, Player.RckPlayer.instance.playerAttributes.attributes.Dexterity, weaponItemInInventory.isEquipped);
                LootingInventoryUI.instance.pickpocketChanceText.text = chance + "%";
            }
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            if (LootingInventoryUI.instance.isPickPocketing)
            {
                // If pickpocketing, show/hide the chance
                LootingInventoryUI.instance.pickpocketChanceText.text = "?";
            }
        }
    }
}