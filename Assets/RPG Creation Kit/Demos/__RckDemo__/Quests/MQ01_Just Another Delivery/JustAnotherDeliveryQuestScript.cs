using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.SaveSystem;

namespace RPGCreationKit.Quests
{
    public class JustAnotherDeliveryQuestScript : QuestScript
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