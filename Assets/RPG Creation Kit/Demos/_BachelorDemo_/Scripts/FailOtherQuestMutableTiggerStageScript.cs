using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class FailOtherQuestMutableTiggerStageScript : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            RCKFunctions.MutateMutable("TriggerFailQuest", false);
            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}