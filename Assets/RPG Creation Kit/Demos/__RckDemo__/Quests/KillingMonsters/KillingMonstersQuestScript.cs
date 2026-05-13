using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class KillingMonstersQuestScript : QuestScript
    {
        RckAI vampire1;
        RckAI vampire2;

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

            if (quest.currentQuestStage == 20)
            {
                if (vampire1 == null || vampire2 == null)
                {
                    CellInformation.TryToGetAI("VampireInCave001", out vampire1);
                    CellInformation.TryToGetAI("VampireInCave002", out vampire2);
                }
                else
                {
                    if (vampire1.isAlive == false && vampire2.isAlive == false)
                    {
                        RCKFunctions.CompleteQuestStage(quest.questID, 20);
                    }
                }
            }
        }
    }
}