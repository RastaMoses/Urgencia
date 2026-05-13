using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using UnityEngine.EventSystems;

namespace RPGCreationKit
{
    public class KeyItemInLootingUI_DrawDeposit : KeyItemInInventoryUI
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
                if (base.keyItemInInventory.Amount <= 1)
                {
                    // Check for failure
                    if (LootingInventoryUI.instance.isPickPocketing)
                    {
                        float dotProduct = Vector3.Dot(LootingInventoryUI.instance.pickPocketingAI.transform.position - Player.RckPlayer.instance.transform.position, LootingInventoryUI.instance.pickPocketingAI.transform.forward);
                        float chance = RCKFunctions.CalculatePickpocketChance(dotProduct > 0, keyItemInInventory.item.Weight, Player.RckPlayer.instance.playerAttributes.attributes.Dexterity, keyItemInInventory.isEquipped);

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

                    Inventory.PlayerInventory.AddItem(base.keyItemInInventory.item, base.keyItemInInventory.metadata, 1, !takeAll);

                    // Remove the item from the current loot inventory
                    LootingInventoryUI.instance.curLootingPoint.inventory.RemoveItem(base.keyItemInInventory, 1);

                    LootingInventoryUI.instance.SelectNextButton();

                    // Disable this object
                    pool.usedObjects.Remove(this);
                    pool.KeysPool.usedObjects.Remove(this);

                    gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                    gameObject.SetActive(false);

                    // Check steal
                    if (!LootingInventoryUI.instance.isPickPocketing && RCKFunctions.CheckPlayerStealsItem(base.keyItemInInventory))
                    {
                        // Close looting ui
                        LootingInventoryUI.instance.CloseUI();
                    }
                }
                else if (base.keyItemInInventory.item.isCumulable && base.keyItemInInventory.Amount > 1)
                {
                    if (!takeAll)
                    {
                        LootingInventoryUI.instance.takeDepositItemsPanel.gameObject.SetActive(true);
                        LootingInventoryUI.instance.takeDepositItemsPanel.Init(keyItemInInventory, this);
                    }
                    else
                    {
                        ConfirmButtonCumulableItem(base.keyItemInInventory.Amount);
                    }
                }
            }
            else // we're depositing
            {
                if (base.keyItemInInventory.item.QuestItem)
                {
                    AlertMessage.instance.InitAlertMessage("You can't leave Quest Items", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                    return;
                }

                // if the amount is 1, add it one time
                if (base.keyItemInInventory.Amount <= 1)
                {
                    // Remove the item from the current loot inventory
                    LootingInventoryUI.instance.curLootingPoint.inventory.AddItem(base.keyItemInInventory.item, base.keyItemInInventory.metadata, 1);

                    Inventory.PlayerInventory.RemoveItem(base.keyItemInInventory, 1);

                    LootingInventoryUI.instance.SelectNextButton();

                    // Disable this object
                    pool.usedObjects.Remove(this);
                    pool.KeysPool.usedObjects.Remove(this);

                    gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                    gameObject.SetActive(false);
                }
                else if (base.keyItemInInventory.item.isCumulable && base.keyItemInInventory.Amount > 1)
                {
                    LootingInventoryUI.instance.takeDepositItemsPanel.gameObject.SetActive(true);
                    LootingInventoryUI.instance.takeDepositItemsPanel.Init(keyItemInInventory, this);
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
                    float chance = RCKFunctions.CalculatePickpocketChance(dotProduct > 0, keyItemInInventory.item.Weight, Player.RckPlayer.instance.playerAttributes.attributes.Dexterity, keyItemInInventory.isEquipped, amount);

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
                Inventory.PlayerInventory.AddItem(base.keyItemInInventory.item, base.keyItemInInventory.metadata, amount);

                // Remove from Loot Inventory
                LootingInventoryUI.instance.curLootingPoint.inventory.RemoveItem(base.keyItemInInventory, amount);

                if (keyItemInInventory.Amount <= 0)
                {
                    LootingInventoryUI.instance.takeDepositItemsPanel.SetPreviousSelected();
                    LootingInventoryUI.instance.SelectNextButton(true);

                    //If we've took all the items
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

                // Check steal
                if (!LootingInventoryUI.instance.isPickPocketing && RCKFunctions.CheckPlayerStealsItem(base.keyItemInInventory))
                {
                    // Close looting ui
                    LootingInventoryUI.instance.CloseUI();
                }
            }
            else // we're depositing
            {
                // Remove from Loot Inventory
                LootingInventoryUI.instance.curLootingPoint.inventory.AddItem(base.keyItemInInventory.item, base.keyItemInInventory.metadata, amount);

                // Add in PlayerInventory
                Inventory.PlayerInventory.RemoveItem(base.keyItemInInventory, amount);

                if (keyItemInInventory.Amount <= 0)
                {
                    LootingInventoryUI.instance.takeDepositItemsPanel.SetPreviousSelected();
                    LootingInventoryUI.instance.SelectNextButton(true);

                    //If we've took all the items
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
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (LootingInventoryUI.instance.isPickPocketing)
            {
                // If pickpocketing, show/hide the chance
                float dotProduct = Vector3.Dot(LootingInventoryUI.instance.pickPocketingAI.transform.position - Player.RckPlayer.instance.transform.position, LootingInventoryUI.instance.pickPocketingAI.transform.forward);
                float chance = RCKFunctions.CalculatePickpocketChance(dotProduct > 0, keyItemInInventory.item.Weight, Player.RckPlayer.instance.playerAttributes.attributes.Dexterity, keyItemInInventory.isEquipped);
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
                float chance = RCKFunctions.CalculatePickpocketChance(dotProduct > 0, keyItemInInventory.item.Weight, Player.RckPlayer.instance.playerAttributes.attributes.Dexterity, keyItemInInventory.isEquipped);
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