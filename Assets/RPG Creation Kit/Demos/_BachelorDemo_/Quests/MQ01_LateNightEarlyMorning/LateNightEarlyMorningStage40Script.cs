using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Quests;
using RPGCreationKit.SaveSystem;
using UnityEngine;

namespace RPGCreationKit.Quests
{
    public class LateNightEarlyMorningStage40Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            MutateGoto();
            InGameHelpUI.instance.TriggerDiaryHelp();

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
        public void MutateGoto()
        {
            Mutable mutable = null;
            if (CellInformation.TryToGetMutable("QuestUpdaterLateNightEarlyMorningMutable2", out mutable))
            {
                mutable.Mutate();
            }
            else // Update the save file directly
            {
                var allMutables = SaveSystemManager.instance.saveFile.MutablesData.allMutables;

                if (allMutables.ContainsKey("QuestUpdaterLateNightEarlyMorningMutable2"))
                    allMutables["QuestUpdaterLateNightEarlyMorningMutable2"].isMutated = true;
                else
                    allMutables.Add("QuestUpdaterLateNightEarlyMorningMutable2", new MutableData(true));
            }
        }
    }

}