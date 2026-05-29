using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class DealingHealingCrystalsStage40StageScript : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            RCKFunctions.SetQuestStage("SQ_QuackActionRequired", 60);
            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}