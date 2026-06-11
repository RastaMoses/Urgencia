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
            quest.questScriptExecutionDelay = 1f; // Set the delay between each CustomUpdate execution to 1 second.
        }

        // CustomUpdate runs once every (quest.questScriptExecutionDelay) seconds
        public override void CustomUpdate()
        {
            base.CustomUpdate();

            // Your code here

            //If looking for flower then advance time to a certain point unless time already evening
            if (RCKFunctions.GetStage("SQ_QuackActionRequired") >= 10)
            {
                if(TimeOfDayManager.instance.GetCurrentTime() >= 18f && TimeOfDayManager.instance.currentTimeScale != 0)
                {
                    TimeOfDayManager.instance.currentTimeScale = 0;
                }

            }
        }
    }
}