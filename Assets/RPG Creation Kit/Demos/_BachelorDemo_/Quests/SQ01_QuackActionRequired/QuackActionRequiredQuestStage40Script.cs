using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class QuackActionRequiredQuestStage40Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            RCKFunctions.SetQuestStage("SQ_DealingHealingCrystals", 50);
            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}