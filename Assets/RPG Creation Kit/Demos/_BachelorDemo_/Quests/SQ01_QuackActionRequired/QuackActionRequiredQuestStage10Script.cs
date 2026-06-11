using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class QuackActionRequiredQuestStage10Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            TimeOfDayManager.instance.SetTime(13.0f);
            TimeOfDayManager.instance.currentTimeScale = 0.07f;
            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}