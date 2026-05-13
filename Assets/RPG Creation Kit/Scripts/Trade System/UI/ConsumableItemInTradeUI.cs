using RPGCreationKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGCreationKit
{
    public class ConsumableItemInTradeUI : ConsumableItemInInventoryUI
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                OnClick(false);
        }

        public override void UpdateItem()
        {
            base.UpdateItem();

            // check if the owner of the item is the one that is selling it, if it is, hide the stolen icon
            if (TradeSystemUI.instance.isBuyingOrDrawing && consumableItemInInventory.metadata.isOwned)
            {
                if (TradeSystemUI.instance.curMerchant != null && TradeSystemUI.instance.curMerchant.ID == consumableItemInInventory.metadata.ownerID)
                    stolenIcon.SetActive(false);
            }
        }

        public override void OnClick(bool takeAll = false)
        {
            if (TradeSystemUI.instance.isBuyingOrDrawing)
            {
                if (TradeSystemUI.instance.isMerchant)
                {
                    int adjustedGolds = Mathf.RoundToInt(base.consumableItemInInventory.item.Value * TradeSystemUI.instance.curMerchant.sellMultiplier);

                    // if the amount is 1, add it one time
                    if (base.consumableItemInInventory.Amount <= 1)
                    {
                        TradeSystemUI.instance.confirmTradePanel.gameObject.SetActive(true);
                        TradeSystemUI.instance.confirmTradePanel.Init(base.consumableItemInInventory, this, "Are you sure you want to buy 1 " + base.consumableItemInInventory.item.ItemName + " for " + adjustedGolds + " golds?");
                        TradeSystemUI.instance.confirmTradePanel.continueButton.onClick.AddListener(() => ConfirmTrade(1, ConfirmTradeAction.BuyFromMerchant));
                    }
                    else if (base.consumableItemInInventory.item.isCumulable && base.consumableItemInInventory.Amount > 1)
                    {
                        TradeSystemUI.instance.takeDepositItemsPanel.gameObject.SetActive(true);
                        TradeSystemUI.instance.takeDepositItemsPanel.Init(consumableItemInInventory, this);
                    }
                }
                else
                {
                    // we're drawing from a friendly npc
                }
            }
            else // we're selling
            {
                if (base.consumableItemInInventory.item.QuestItem)
                {
                    AlertMessage.instance.InitAlertMessage("You can't sell Quest Items", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                    return;
                }

                // if the amount is 1, add it one time
                if (TradeSystemUI.instance.isMerchant)
                {
                    int adjustedGolds = Mathf.RoundToInt(base.consumableItemInInventory.item.Value * TradeSystemUI.instance.curMerchant.buyMultiplier);

                    // if the amount is 1, add it one time
                    if (base.consumableItemInInventory.Amount <= 1)
                    {
                        TradeSystemUI.instance.confirmTradePanel.gameObject.SetActive(true);
                        TradeSystemUI.instance.confirmTradePanel.Init(base.consumableItemInInventory, this, "Are you sure you want to sell 1 " + base.consumableItemInInventory.item.ItemName + " for " + adjustedGolds + " golds?");
                        TradeSystemUI.instance.confirmTradePanel.continueButton.onClick.AddListener(() => ConfirmTrade(1, ConfirmTradeAction.SellToMerchant));
                    }
                    else if (base.consumableItemInInventory.item.isCumulable && base.consumableItemInInventory.Amount > 1)
                    {
                        TradeSystemUI.instance.takeDepositItemsPanel.gameObject.SetActive(true);
                        TradeSystemUI.instance.takeDepositItemsPanel.Init(consumableItemInInventory, this);
                    }
                }
            }
        }

        public override void ConfirmButtonCumulableItem(int amount)
        {
            if (TradeSystemUI.instance.isBuyingOrDrawing)
            {
                if (TradeSystemUI.instance.isMerchant)
                {
                    int adjustedGolds = Mathf.RoundToInt((base.consumableItemInInventory.item.Value * TradeSystemUI.instance.curMerchant.sellMultiplier)) * amount;

                    TradeSystemUI.instance.confirmTradePanel.gameObject.SetActive(true);
                    TradeSystemUI.instance.confirmTradePanel.Init(base.consumableItemInInventory, this, "Arze you sure you want to buy " + amount + " " + base.consumableItemInInventory.item.ItemName + " for " + adjustedGolds + " golds?", true);
                    TradeSystemUI.instance.confirmTradePanel.continueButton.onClick.AddListener(() => ConfirmTrade(amount, ConfirmTradeAction.BuyFromMerchant));
                }
                else
                {
                    // friendly npc
                }

            }
            else // we're selling
            {
                if (TradeSystemUI.instance.isMerchant)
                {
                    int adjustedGolds = Mathf.RoundToInt((base.consumableItemInInventory.item.Value * TradeSystemUI.instance.curMerchant.buyMultiplier)) * amount;

                    TradeSystemUI.instance.confirmTradePanel.gameObject.SetActive(true);
                    TradeSystemUI.instance.confirmTradePanel.Init(base.consumableItemInInventory, this, "Azre you sure you want to sell " + amount + " " + base.consumableItemInInventory.item.ItemName + " for " + adjustedGolds + " golds?", true);
                    TradeSystemUI.instance.confirmTradePanel.continueButton.onClick.AddListener(() => ConfirmTrade(amount, ConfirmTradeAction.SellToMerchant));
                }
                else
                {
                    //friendly npc
                }
            }
        }

        public override void ConfirmTrade(int _amount, ConfirmTradeAction _action)
        {
            int totalGoldsAmount = (base.consumableItemInInventory.item.Value * _amount);

            if (_action == ConfirmTradeAction.BuyFromMerchant)
            {
                totalGoldsAmount = Mathf.RoundToInt(base.consumableItemInInventory.item.Value * TradeSystemUI.instance.curMerchant.sellMultiplier) * _amount;

                if (Inventory.PlayerInventory.GetItemCount("Gold001") >= totalGoldsAmount)
                {
                    // Clear the ownership of the item, it now belongs to the player
                    base.consumableItemInInventory.metadata.Clear();

                    Inventory.PlayerInventory.AddItem(base.consumableItemInInventory.item, base.consumableItemInInventory.metadata, _amount);

                    Inventory.PlayerInventory.RemoveItem("Gold001", totalGoldsAmount);
                    TradeSystemUI.instance.curMerchant.inventory.AddItem("Gold001", totalGoldsAmount);


                    // Remove the item from the merchant inventory
                    TradeSystemUI.instance.curMerchant.inventory.RemoveItem(base.consumableItemInInventory, _amount);

                    TradeSystemUI.instance.PlaySound(GoldsSounds.Spend);
                    TradeSystemUI.instance.confirmTradePanel.CancelButton();
                    AlertMessage.instance.InitAlertMessage("Transaction Completed!", 1.5f);

                    TradeSystemUI.instance.UpdateStatsUI();

                    // if the merchant's item is out of stocks, remove the item

                    if (base.consumableItemInInventory.Amount <= 0)
                    {
                        TradeSystemUI.instance.SelectNextButton();

                        // Disable this object
                        pool.usedObjects.Remove(this);
                        pool.ConsumablesPool.usedObjects.Remove(this);

                        gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                        gameObject.SetActive(false);
                    }
                    else
                    {
                        Init();
                    }
                }
                else
                {
                    TradeSystemUI.instance.confirmTradePanel.CancelButton();
                    AlertMessage.instance.InitAlertMessage("You don't have enough golds.", 1.5f);
                }
            }
            else if (_action == ConfirmTradeAction.SellToMerchant)
            {
                totalGoldsAmount = Mathf.RoundToInt(base.consumableItemInInventory.item.Value * TradeSystemUI.instance.curMerchant.buyMultiplier) * _amount;

                if (TradeSystemUI.instance.curMerchant.inventory.GetItemCount("Gold001") >= totalGoldsAmount)
                {
                    // Clear the ownership of the item, it now belongs to the player
                    base.consumableItemInInventory.metadata.Clear();

                    TradeSystemUI.instance.curMerchant.inventory.AddItem(base.consumableItemInInventory.item, base.consumableItemInInventory.metadata, _amount);
                    Inventory.PlayerInventory.RemoveItem(base.consumableItemInInventory, _amount);

                    TradeSystemUI.instance.curMerchant.inventory.RemoveItem("Gold001", totalGoldsAmount);
                    Inventory.PlayerInventory.AddItem("Gold001", totalGoldsAmount);

                    TradeSystemUI.instance.PlaySound(GoldsSounds.Gain);
                    TradeSystemUI.instance.confirmTradePanel.CancelButton();
                    AlertMessage.instance.InitAlertMessage("Transaction Completed!", 1.5f);

                    TradeSystemUI.instance.UpdateStatsUI();

                    // if the merchant's item is out of stocks, remove the item

                    if (base.consumableItemInInventory.Amount <= 0)
                    {
                        TradeSystemUI.instance.SelectNextButton();

                        // Disable this object
                        pool.usedObjects.Remove(this);
                        pool.ConsumablesPool.usedObjects.Remove(this);

                        gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                        gameObject.SetActive(false);
                    }
                    else
                    {
                        Init();
                    }
                }
                else
                {
                    TradeSystemUI.instance.confirmTradePanel.CancelButton();
                    AlertMessage.instance.InitAlertMessage("The merchant doesn't have enough golds.", 1.5f);
                }
            }
        }
    }
}