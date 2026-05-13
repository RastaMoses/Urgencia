using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using UnityEngine.EventSystems;

namespace RPGCreationKit
{
    public class AmmoItemInLootingUI_DrawDeposit : AmmoItemInInventoryUI
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
                if (base.ammoItemInInventory.Amount <= 1)
                {
                    // Check for failure
                    if (LootingInventoryUI.instance.isPickPocketing)
                    {
                        float dotProduct = Vector3.Dot(LootingInventoryUI.instance.pickPocketingAI.transform.position - Player.RckPlayer.instance.transform.position, LootingInventoryUI.instance.pickPocketingAI.transform.forward);
                        float chance = RCKFunctions.CalculatePickpocketChance(dotProduct > 0, ammoItemInInventory.item.Weight, Player.RckPlayer.instance.playerAttributes.attributes.Dexterity, ammoItemInInventory.isEquipped);

                        int result = Random.Range(0, 100);

                        // Fail check
                        if (result > chance)
                        {
                            AI.RckAI ai = LootingInventoryUI.instance.pickPocketingAI;

                            ai.EnterInCombatAgainst(Player.RckPlayer.GetPlayerEntity());

                            if(RCKSettings.PICKPOCKET_DIALOGUE_CLIP_PLAYS)
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

                    Inventory.PlayerInventory.AddItem(base.ammoItemInInventory.item, base.ammoItemInInventory.metadata, 1, !takeAll);

                    if (base.ammoItemInInventory.isEquipped)
                        LootingInventoryUI.instance.curLootingPoint.equipment.Unequip(base.ammoItemInInventory);

                    // Remove the item from the current loot inventory
                    LootingInventoryUI.instance.curLootingPoint.inventory.RemoveItem(base.ammoItemInInventory, 1);

                    LootingInventoryUI.instance.SelectNextButton();

                    // Disable this object
                    pool.usedObjects.Remove(this);
                    pool.AmmosPool.usedObjects.Remove(this);

                    gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                    gameObject.SetActive(false);

                    // Check steal
                    if (!LootingInventoryUI.instance.isPickPocketing && RCKFunctions.CheckPlayerStealsItem(base.ammoItemInInventory))
                    {
                        // Close looting ui
                        LootingInventoryUI.instance.CloseUI();
                    }
                }
                else if (base.ammoItemInInventory.item.isCumulable && base.ammoItemInInventory.Amount > 1)
                {
                    if (!takeAll)
                    {
                        LootingInventoryUI.instance.takeDepositItemsPanel.gameObject.SetActive(true);
                        LootingInventoryUI.instance.takeDepositItemsPanel.Init(ammoItemInInventory, this);
                    }
                    else
                    {
                        ConfirmButtonCumulableItem(base.ammoItemInInventory.Amount);
                    }
                }
            } else // we're depositing
            {
                if(base.ammoItemInInventory.item.QuestItem)
                {
                    AlertMessage.instance.InitAlertMessage("You can't leave Quest Items", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                    return;
                }

                // Make sure weapons are not removed if being used
                if (!PlayerCombat.instance.isAttacking && ammoItemInInventory.isEquipped)
                {
                    // if the amount is 1, add it one time
                    if (base.ammoItemInInventory.Amount <= 1)
                    {
                        // Remove the item from the current loot inventory
                        LootingInventoryUI.instance.curLootingPoint.inventory.AddItem(base.ammoItemInInventory.item, base.ammoItemInInventory.metadata, 1);

                        if (ammoItemInInventory.isEquipped)
                        {
                            Equipment.PlayerEquipment.Unequip(ammoItemInInventory);
                            PlayerCombat.instance.OnEquipmentChanges();
                            PlayerInInventory.instance.OnEquipmentChangesHands();
                            PlayerInInventory.instance.OnEquipmentChangesAmmo();
                        }

                        Inventory.PlayerInventory.RemoveItem(base.ammoItemInInventory, 1);

                        LootingInventoryUI.instance.SelectNextButton();

                        // Disable this object
                        pool.usedObjects.Remove(this);
                        pool.AmmosPool.usedObjects.Remove(this);

                        gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                        gameObject.SetActive(false);
                    }
                    else if (base.ammoItemInInventory.item.isCumulable && base.ammoItemInInventory.Amount > 1)
                    {
                        LootingInventoryUI.instance.takeDepositItemsPanel.gameObject.SetActive(true);
                        LootingInventoryUI.instance.takeDepositItemsPanel.Init(ammoItemInInventory, this);
                    }
                } 
                else
                {
                    AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                    return;
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
                    float chance = RCKFunctions.CalculatePickpocketChance(dotProduct > 0, ammoItemInInventory.item.Weight, Player.RckPlayer.instance.playerAttributes.attributes.Dexterity, ammoItemInInventory.isEquipped, amount);

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
                Inventory.PlayerInventory.AddItem(base.ammoItemInInventory.item, base.ammoItemInInventory.metadata, amount);

                if (base.ammoItemInInventory.Amount <= amount)
                    if (base.ammoItemInInventory.isEquipped)
                        LootingInventoryUI.instance.curLootingPoint.equipment.Unequip(base.ammoItemInInventory);

                // Remove from Loot Inventory
                LootingInventoryUI.instance.curLootingPoint.inventory.RemoveItem(base.ammoItemInInventory, amount);

                if (ammoItemInInventory.Amount <= 0)
                {
                    LootingInventoryUI.instance.takeDepositItemsPanel.SetPreviousSelected();
                    LootingInventoryUI.instance.SelectNextButton(true);

                    //If we've took all the items
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

                // Check steal
                if (!LootingInventoryUI.instance.isPickPocketing && RCKFunctions.CheckPlayerStealsItem(base.ammoItemInInventory))
                {
                    // Close looting ui
                    LootingInventoryUI.instance.CloseUI();
                }
            }
            else // we're depositing
            {
                // Add it to Loot Inventory
                LootingInventoryUI.instance.curLootingPoint.inventory.AddItem(base.ammoItemInInventory.item, base.ammoItemInInventory.metadata, amount);

                if (base.ammoItemInInventory.Amount <= amount)
                    if (ammoItemInInventory.isEquipped)
                    {
                        Equipment.PlayerEquipment.Unequip(ammoItemInInventory);
                        PlayerCombat.instance.OnEquipmentChanges();
                        PlayerInInventory.instance.OnEquipmentChangesHands();
                        PlayerInInventory.instance.OnEquipmentChangesAmmo();
                    }

                // Remove in PlayerInventory
                Inventory.PlayerInventory.RemoveItem(base.ammoItemInInventory, amount);

                if (ammoItemInInventory.Amount <= 0)
                {
                    LootingInventoryUI.instance.takeDepositItemsPanel.SetPreviousSelected();
                    LootingInventoryUI.instance.SelectNextButton(true);

                    //If we've took all the items
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
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (LootingInventoryUI.instance.isPickPocketing)
            {
                // If pickpocketing, show/hide the chance
                float dotProduct = Vector3.Dot(LootingInventoryUI.instance.pickPocketingAI.transform.position - Player.RckPlayer.instance.transform.position, LootingInventoryUI.instance.pickPocketingAI.transform.forward);
                float chance = RCKFunctions.CalculatePickpocketChance(dotProduct > 0, ammoItemInInventory.item.Weight, Player.RckPlayer.instance.playerAttributes.attributes.Dexterity, ammoItemInInventory.isEquipped);
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
                float chance = RCKFunctions.CalculatePickpocketChance(dotProduct > 0, ammoItemInInventory.item.Weight, Player.RckPlayer.instance.playerAttributes.attributes.Dexterity, ammoItemInInventory.isEquipped);
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