using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Quests;
using TreeEditor;
using UnityEngine;

namespace RPGCreationKit.Quests
{
    public class DealingHealingCrystalsQuestScript : QuestScript
    {
        // This will start running the CustomUpdate as soon as the quest starts.
        public void Start()
        {
            quest.questScriptExecutionDelay = 1f;
            RunQuestScript();
        }

        // CustomUpdate runs once every (quest.questScriptExecutionDelay) seconds
        public override void CustomUpdate()
        {
            base.CustomUpdate();

            // Your code here
            if (Inventory.PlayerInventory.GetItemCount("ManaCrystal001") >= 5 && RCKFunctions.GetStage("SQ_DealingHealingCrystals") == 30)
            {
                //Advance Quest Stage
                RCKFunctions.CompleteQuestStage("SQ_DealingHealingCrystals", 30);
                RCKFunctions.SetQuestStage("SQ_DealingHealingCrystals", 40);
            }
            if (RCKFunctions.GetStage("SQ_DealingHealingCrystals") == 40 && RCKFunctions.GetStage("SQ_QuackActionRequired") < 60)
            {
                RCKFunctions.FailQuestStage("SQ_QuackActionRequired", RCKFunctions.GetStage("SQ_QuackActionRequired"));
                RCKFunctions.SetQuestStage("SQ_QuackActionRequired", 60);

            }
            if (RCKFunctions.GetStage("SQ_DealingHealingCrystals") == 50)
            {
                CellInformation.TryToGetAI("MagicMerchant001", out RckAI merchant);
                if (merchant != null) { merchant.DestroyThis(); }
                RCKFunctions.MutateMutable("Mutable_MagicMerchantFailedQuest", false);
            }
        }
    }
}