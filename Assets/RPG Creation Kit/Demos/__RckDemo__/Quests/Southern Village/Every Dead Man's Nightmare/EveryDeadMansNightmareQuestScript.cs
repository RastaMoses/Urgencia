using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class EveryDeadMansNightmareQuestScript : QuestScript
    {
        bool doonce = false;
        bool doonce1 = false;
        bool doonce2 = false;

        RckAI thiefofthedead = null;

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

            if (thiefofthedead == null)
                CellInformation.TryToGetAI("ThiefOfTheDead001", out thiefofthedead);
            else
            {
                if(quest.currentQuestStage == 20 && !thiefofthedead.isAlive && !doonce)
                {
                    // Progress with quest
                    RCKFunctions.SetQuestStage(quest.questID, 40);
                    RCKFunctions.CompleteQuestStage(quest.questID, 30);
                    RCKFunctions.CompleteQuestStage(quest.questID, 20);


                    doonce = true;
                }
                else if(quest.currentQuestStage == 30 && !thiefofthedead.isAlive && !doonce1)
                {
                    // Progress with quest
                    RCKFunctions.SetQuestStage(quest.questID, 40);
                    RCKFunctions.CompleteQuestStage(quest.questID, 30);

                    doonce1 = true;
                }
                else if(quest.currentQuestStage == 50 && !thiefofthedead.isAlive && !doonce2)
                {
                    RCKFunctions.FailQuestStage(quest.questID, 50);
                    RCKFunctions.SetQuestStage(quest.questID, 40);

                    RCKFunctions.CompleteQuestStage(quest.questID, 30, true);

                    doonce2 = true;
                }
            }

        }
    }
}