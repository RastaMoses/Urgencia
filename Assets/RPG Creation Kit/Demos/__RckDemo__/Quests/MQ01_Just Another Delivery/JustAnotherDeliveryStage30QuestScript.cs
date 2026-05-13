using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.AI;

namespace RPGCreationKit.Quests
{
    public class JustAnotherDeliveryStage30QuestScript : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            // Get Mack
            RckAI mack = null;
            CellsSystem.CellInformation.TryToGetAI("Mack001", out mack);

            if(mack != null)
            {
                // Change his dialogue and behaviour directly
                mack.currentDialogueGraph = DialoguesDatabase.GetItem("DIALOGUEMQ_MackTalksToPlayerInvite001");

                mack.SetNewBehaviourTree(false, "BTP_SpeakToPlayer");
                mack.SwitchBehaviourTree(false);

                // Move behind the house
                mack.transform.position = new Vector3(30f, 0.65f, 11.67f);
            }
            
            // Ensure mack is saved, this happens while Mack is in offline mode so if the player saves inside the palace Mack won't be saved, so let's do it manually.

            // Try to get from savefile
            var allAI = SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiDictionary;

            if(allAI.ContainsKey("Mack001"))
            {
                SaveSystem.AISaveData mackData = allAI["Mack001"];
                mackData.currentDialogueID = "DIALOGUEMQ_MackTalksToPlayerInvite001";
                mackData.purposeBehaviourTreeID = "BTP_SpeakToPlayer";
                mackData.curBehaviourTreeID = "BTP_SpeakToPlayer";
                mackData.position = new Vector3(30f, 0.65f, 11.67f);
            }


            // Locks the main gate door to prevent the player to run away before Mack approaches him
            RCKFunctions.LockDoor("MainDoorToCityExterior", DoorLockLevel.Impossible);
            // The door will be unlocked by Mack's Dialogue result script "MQRS_MackAfterInvitesPlayer"

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}