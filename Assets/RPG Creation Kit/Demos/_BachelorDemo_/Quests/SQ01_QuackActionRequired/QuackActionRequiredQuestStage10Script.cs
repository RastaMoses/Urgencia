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
            InGameHelpUI.instance.TriggerDiaryHelp();
            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}