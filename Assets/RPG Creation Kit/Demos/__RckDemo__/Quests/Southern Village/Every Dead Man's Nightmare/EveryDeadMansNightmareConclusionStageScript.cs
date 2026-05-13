using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class EveryDeadMansNightmareConclusionStageScript : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            RckAI motherNeb = null;
            CellInformation.TryToGetAI("MotherNebivia001", out motherNeb);

            if(motherNeb != null)
            {
                motherNeb.ChangeDialogueGraph("MotherNebivia001_EDNConclusion");
            }
            else
            {
                var savedata = SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiDictionary;

                var motherNebData = savedata["MotherNebivia001"];
                motherNebData.currentDialogueID = "MotherNebivia001_EDNConclusion";
            }

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}