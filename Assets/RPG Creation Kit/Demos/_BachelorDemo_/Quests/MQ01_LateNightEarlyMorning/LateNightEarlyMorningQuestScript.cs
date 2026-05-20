using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Quests;
using RPGCreationKit.SaveSystem;
using UnityEngine;

namespace RPGCreationKit.Quests
{
    public class LateNightEarlyMorningQuestScript : QuestScript
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

        }

        
    }
}