using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class JudgeJuryAndExecutionerQuestScript : QuestScript
    {
        RckAI mack = null;

        // This will start running the CustomUpdate as soon as the quest starts.
        public void Start()
        {
            quest.questScriptExecutionDelay = .25f;
            RunQuestScript();
        }

        // CustomUpdate runs once every (quest.questScriptExecutionDelay) seconds
        public override void CustomUpdate()
        {
            base.CustomUpdate();

            // Your code here
            if(quest.currentQuestStage == 10)
            {
                if(mack == null)
                    CellInformation.TryToGetAI("Mack001", out mack);

                if (mack != null)
                {
                    if(!mack.isAlive)
                    {
                        RCKFunctions.SetQuestStage(quest.questID, 15);
                        RCKFunctions.CompleteQuestStage(quest.questID, 10);
                    }
                }
            }

        }
    }
}