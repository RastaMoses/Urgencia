using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class GOTO_TriggerMerchantFailResultScript : ResultScript
    {
        void Start()
        {
            // Your code here
            if (RCKFunctions.GetStage("SQ_DealingHealingCrystals") == 40 && RCKFunctions.GetStage("SQ_QuackActionRequired") < 60 && Inventory.PlayerInventory.HasItem("DemoItem"))
            {
                RCKFunctions.FailQuestStage("SQ_QuackActionRequired", RCKFunctions.GetStage("SQ_QuackActionRequired"));
                RCKFunctions.SetQuestStage("SQ_QuackActionRequired", 60);

            }
            if (RCKFunctions.GetStage("SQ_QuackActionRequired") == 50 && RCKFunctions.GetQuest("SQ_DealingHealingCrystals").currentQuestStage < 50 && Inventory.PlayerInventory.HasItem("DemoItem"))
            {
                RCKFunctions.FailQuestStage("SQ_DealingHealingCrystals", RCKFunctions.GetStage("SQ_DealingHealingCrystals"));
                RCKFunctions.SetQuestStage("SQDealingHealingCrystals", 50);
                CellInformation.TryToGetAI("MagicMerchant001", out RckAI merchant);
                if (merchant != null) { merchant.DestroyThis(); }
                RCKFunctions.MutateMutable("Mutable_MagicMerchantFailedQuest", false);
                RCKFunctions.MutateMutable("TriggerFailQuest", false);
            }
            

            // Destroy the script
            Destroy(this);
        }
    }
}