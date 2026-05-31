using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class TriggerGuardTalksToPlayerResultScript : ResultScript
    {
        void Start()
        {
            // Your code here
            // Get Mack
            RckAI mack = null;
            CellsSystem.CellInformation.TryToGetAI("CityGuard003", out mack);

            if (mack != null && RCKFunctions.GetStage("MQ_Gratulate")==20)
            {
                // Change his dialogue and behaviour directly
                //mack.currentDialogueGraph = DialoguesDatabase.GetItem("DIALOGUE_CeremonyGuardHaltsPlayer");
                mack.ChangeDialogueGraph("DIALOGUE_CeremonyGuardHaltsPlayer");

                mack.SetNewBehaviourTree(false, "BTP_SpeakToPlayer");
                mack.SwitchBehaviourTree(false);

                // Move behind the house
                //mack.transform.position = new Vector3(30f, 0.65f, 11.67f);
            }

            // Ensure mack is saved, this happens while Mack is in offline mode so if the player saves inside the palace Mack won't be saved, so let's do it manually.

            // Try to get from savefile
            var allAI = SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiDictionary;

            if (allAI.ContainsKey("CityGuard003"))
            {
                SaveSystem.AISaveData mackData = allAI["CityGuard003"];
                mackData.currentDialogueID = "DIALOGUE_CeremonyGuardHaltsPlayer";
                mackData.purposeBehaviourTreeID = "BTP_SpeakToPlayer";
                mackData.curBehaviourTreeID = "BTP_SpeakToPlayer";
                mackData.position = new Vector3(30f, 0.65f, 11.67f);
            }


            // Locks the main gate door to prevent the player to run away before Mack approaches him
            RCKFunctions.LockDoor("CityInteriorToKingsPalace", DoorLockLevel.Impossible);
            // The door will be unlocked by Mack's Dialogue result script "MQRS_MackAfterInvitesPlayer"

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}