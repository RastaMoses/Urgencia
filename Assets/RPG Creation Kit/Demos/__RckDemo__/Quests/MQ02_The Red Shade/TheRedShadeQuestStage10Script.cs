using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;

namespace RPGCreationKit.Quests
{
    public class TheRedShadeQuestStage10Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            RCKFunctions.UnlockDoor("CityInteriorToMackHouse");
            
            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}