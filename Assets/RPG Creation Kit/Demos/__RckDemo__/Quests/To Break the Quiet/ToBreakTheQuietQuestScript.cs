using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class ToBreakTheQuietQuestScript : QuestScript
    {
        // This will start running the CustomUpdate as soon as the quest starts.

        RckAI bandit1;
        RckAI bandit2;
        public void Start()
        {
            quest.questScriptExecutionDelay = 1f;
            RunQuestScript();
        }

        // CustomUpdate runs once every (quest.questScriptExecutionDelay) seconds
        public override void CustomUpdate()
        {
            base.CustomUpdate();

            //If current stage is 20
            if (quest.currentQuestStage == 20)
            {
                //Try to get the bandits
                if (bandit1 == null || bandit2 == null)
                {
                    if (bandit1 == null) CellInformation.TryToGetAI("FQKid001", out bandit1);
                    if (bandit2 == null) CellInformation.TryToGetAI("FQKid002", out bandit2);
                }
                else
                {
                    //If both bandits are dead, complete stage 20 and set stage 30
                    if (!bandit1.isAlive && !bandit2.isAlive)
                    {
                        RCKFunctions.CompleteQuestStage(quest.questID, 20);
                        RCKFunctions.SetQuestStage(quest.questID, 30);
                    }
                }
                
            }

            // Your code here
        }
    }
}