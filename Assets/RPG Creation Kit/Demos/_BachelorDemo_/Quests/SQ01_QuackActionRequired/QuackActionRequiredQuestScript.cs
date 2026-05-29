using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class QuackActionRequiredQuestScript : QuestScript
    {
        // This will start running the CustomUpdate as soon as the quest starts.
        public void Start()
        {
            RunQuestScript();
        }

        // CustomUpdate runs once every (quest.questScriptExecutionDelay) seconds
        public override void CustomUpdate()
        {
            base.CustomUpdate();

            // Your code here
            if (RCKFunctions.GetStage("SQ_QuackActionRequired") >= 30 && RCKFunctions.GetQuest("SQ_DealingHealingCrystals").currentQuestStage < 60)
            {
                RCKFunctions.FailQuestStage("SQ_DealingHealingCrystals", RCKFunctions.GetStage("SQ_DealingHealingCrystals"));
                CellInformation.TryToGetAI("MagicMerchant001", out RckAI merchant);
                if (merchant != null) { merchant.DestroyThis(); }
                RCKFunctions.MutateMutable("Mutable_MagicMerchantFailedQuest", false);
            }
        }
    }
}