using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Quests;
using RPGCreationKit.SaveSystem;
using UnityEngine;

namespace RPGCreationKit.Quests
{
    public class LateNightEarlyMorningStage30Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            MutateGoto();

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
        public void MutateGoto()
        {
            Mutable mutable = null;
            if (CellInformation.TryToGetMutable("QuestUpdaterLateNightEarlyMorningMutable", out mutable))
            {
                mutable.Mutate();
            }
            else // Update the save file directly
            {
                var allMutables = SaveSystemManager.instance.saveFile.MutablesData.allMutables;

                if (allMutables.ContainsKey("QuestUpdaterLateNightEarlyMorningMutable"))
                    allMutables["QuestUpdaterLateNightEarlyMorningMutable"].isMutated = true;
                else
                    allMutables.Add("QuestUpdaterLateNightEarlyMorningMutable", new MutableData(true));
            }
        }
    }

}