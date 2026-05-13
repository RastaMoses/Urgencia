using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    /// <summary>
    /// This script is executed when the Player gets into Mack's house and mack is already in there.
    /// </summary>
    public class TheRedShadeQuestStage20Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here

            // Grab Mack
            RckAI mack = null;
            CellInformation.TryToGetAI("Mack001", out mack);

            if(mack != null)
            {
                // Set empty behaviour
                mack.SetNewBehaviourTree(false, "BTP_EmptyStatePurpose");
                mack.SwitchBehaviourTree(false);

                // Set dialogue
                mack.currentDialogueGraph = DialoguesDatabase.GetItem("DIALOGUEMQ_MackRSDialogue001");
            }

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}