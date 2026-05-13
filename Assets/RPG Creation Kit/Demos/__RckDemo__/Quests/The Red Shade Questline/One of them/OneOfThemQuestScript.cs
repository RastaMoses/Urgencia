using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    /// <summary>
    /// Controls the quest "One of them"
    /// </summary>
    public class OneOfThemQuestScript : QuestScript
    {
        bool Doonce = false; // Used to trigger hostile count behaviour if the player is not wearing the guard set

        bool Doonce2 = false; // Used to unlock the count's house door

        bool Doonce3 = false; // Used to update the quest when the count dies

        // This will start running the CustomUpdate as soon as the quest starts.
        public void Start()
        {
            quest.questScriptExecutionDelay = 0.25f; // We need a fast iteration
            RunQuestScript();
        }

        // CustomUpdate runs once every (quest.questScriptExecutionDelay) seconds
        public override void CustomUpdate()
        {
            base.CustomUpdate();

            // Your code here

            if (quest.currentQuestStage >= 20 && !Doonce2) 
            {
                RCKFunctions.UnlockDoor("CityExteriorToCountsHouseDoor1");
                RCKFunctions.UnlockDoor("CityExteriorToCountsHouseDoor2");
                Doonce2 = true;
            }

            if (quest.currentQuestStage < 40)
            {
                // Load the Count AI
                RckAI count = null;
                CellInformation.TryToGetAI("CountTheveninThibault001", out count);

                if(count != null)
                {
                    bool playerWearingSet = RCKFunctions.GetPlayerEquippedBool("GuardBoots001") && RCKFunctions.GetPlayerEquippedBool("GuardCuriass001") &&
                                            RCKFunctions.GetPlayerEquippedBool("GuardsGloves001") && RCKFunctions.GetPlayerEquippedBool("GuardsHelemt") &&
                                            RCKFunctions.GetPlayerEquippedBool("GuardsPants001");

                    // Check if player is wearing the whole set
                    if (!playerWearingSet && !Doonce)
                    {
                        // Trigger alarm

                        // The count speaks to the player and turns hostile
                        count.currentDialogueGraph = DialoguesDatabase.GetItem("DIALOGUE_CountSpeaksToPlayerBlewCoverage");
                        count.SetNewBehaviourTree(false, "BTP_SpeakToPlayer");
                        count.SwitchBehaviourTree(false);

                        // The code continues with CountTurnsHostileAgainstPlayerAndGuardSpawns result script, executed after the DIALOGUE_CountSpeaksToPlayerBlewCoverage
                        // ends

                        Doonce = true;
                    }

                    if(Doonce) // if the alarm was triggered
                    {
                        // This code runs if the player kills every guard and the count while in the house
                        // It shouldn't happen ever but if someone manages to do that he can leave the house instead of being stuck inside forever.

                        // if all guards are dead unlock doors
                    }

                    if(!count.isAlive && !Doonce3 && quest.currentQuestStage < 40)
                    {
                        RCKFunctions.SetQuestStage(quest.questID, 40);
                        RCKFunctions.CompleteQuestStage(quest.questID, 20);
                        Doonce3 = true;
                    }

                }
            }

        }
    }
}