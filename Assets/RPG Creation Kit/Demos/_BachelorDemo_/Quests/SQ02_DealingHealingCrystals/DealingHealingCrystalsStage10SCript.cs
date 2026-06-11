using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class DealingHealingCrystalsStage10SCript : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            RCKFunctions.MutateMutable("MutableTriggerMultipleQuests", false);
            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}