using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.SaveSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class DealingHealingCrystalsMutateScrit : ResultScript
    {
        private void Start()
        {
            // Your code here
            if (RCKFunctions.GetStage("SQ_DealingHealingCrystals") == 20)
            {
                RCKFunctions.CompleteQuestStage("SQ_DealingHealingCrystals", 20);
                RCKFunctions.SetQuestStage("SQ_DealingHealingCrystals", 30);

                MutateGoto();
                // Use this line to destroy the script if it is not longer needed.
                Destroy(this);
            }



        }

        public void MutateGoto()
        {
            Mutable mutable = null;
            if (CellInformation.TryToGetMutable("QuestUpdaterDealingHealingCrystals", out mutable))
            {
                mutable.Mutate();
            }
            else // Update the save file directly
            {
                var allMutables = SaveSystemManager.instance.saveFile.MutablesData.allMutables;

                if (allMutables.ContainsKey("QuestUpdaterDealingHealingCrystals"))
                    allMutables["QuestUpdaterDealingHealingCrystals"].isMutated = true;
                else
                    allMutables.Add("QuestUpdaterDealingHealingCrystals", new MutableData(true));
            }
        }
    }
}