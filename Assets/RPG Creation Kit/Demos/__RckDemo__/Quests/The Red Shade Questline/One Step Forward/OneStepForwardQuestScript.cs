using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class OneStepForwardQuestScript : QuestScript
    {

        RckAI guard1 = null;
        RckAI guard2 = null;
        RckAI vera = null;
        RckAI martin = null;

        bool Doonce;
        bool Doonce2;
        bool Doonce3;

        RckAI outpGuard1 = null;
        RckAI outpGuard2 = null;
        RckAI outpGuard3 = null;
        RckAI outpGuard4 = null;
        RckAI outpGuard5 = null;

        RckAI generalPhillida = null;

        // This will start running the CustomUpdate as soon as the quest starts.
        public void Start()
        {
            quest.questScriptExecutionDelay = 1f;
            RunQuestScript();
        }

        // CustomUpdate runs once every (quest.questScriptExecutionDelay) seconds
        public override void CustomUpdate()
        {
            base.CustomUpdate();

            // Your code here

            // Wait for the stage 20
            if(quest.currentQuestStage == 20)
            {
                quest.questScriptExecutionDelay = .25f;

                // Check if both guards are dead

                if(guard1 == null)
                    CellsSystem.CellInformation.TryToGetAI("GuardInstVeraFarmHouse01", out guard1);

                if (guard2 == null)
                    CellsSystem.CellInformation.TryToGetAI("GuardInstVeraFarmHouse02", out guard2);

                if (vera == null)
                    CellsSystem.CellInformation.TryToGetAI("Vera001", out vera);

                if (guard1 != null && guard2 != null && vera != null && !Doonce)    // If all of them are loaded in the quest script
                {
                    if(!guard1.isAlive && !guard2.isAlive && !vera.isInCombat && !vera.m_isInCombat) // if both guards are dead
                    {
                        // Progress
                        RCKFunctions.CompleteQuestStage(quest.questID, 20);

                        // Set new vera dialogue and send her to speak to the player
                        vera.ChangeDialogueGraph("RSDIALOGUE_Vera001AfterAmbush");
                        vera.dialogueSystemEnabled = true;

                        vera.SetTarget(RPGCreationKit.Player.RckPlayer.instance.gameObject);

                        vera.SetNewBehaviourTree(false, "BTP_SpeakToPlayerWalk");
                        vera.SwitchBehaviourTree(false);

                        // Heal vera
                        vera.attributes.CurHealth = vera.attributes.MaxHealth;

                        Doonce = true;
                    }
                }
            }


            // Both in stage 30 and 40 we need the outpost guard references, so get them
            if(quest.currentQuestStage >= 30 && quest.currentQuestStage <= 50)
            {
                // Get the guards
                if (outpGuard1 == null)
                    CellsSystem.CellInformation.TryToGetAI("OutpostGuard001", out outpGuard1);

                if (outpGuard2 == null)
                    CellsSystem.CellInformation.TryToGetAI("OutpostGuard002", out outpGuard2);

                if (outpGuard3 == null)
                    CellsSystem.CellInformation.TryToGetAI("OutpostGuard003", out outpGuard3);

                if (outpGuard4 == null)
                    CellsSystem.CellInformation.TryToGetAI("OutpostGuard004", out outpGuard4);

                if (outpGuard5 == null)
                    CellsSystem.CellInformation.TryToGetAI("OutpostGuard005", out outpGuard5);

                if (generalPhillida == null)
                    CellsSystem.CellInformation.TryToGetAI("GeneralPhillida001", out generalPhillida);

                if (martin == null)
                    CellsSystem.CellInformation.TryToGetAI("Martin001", out martin);

                if (vera == null)
                    CellsSystem.CellInformation.TryToGetAI("Vera001", out vera);
            }

            if(quest.currentQuestStage == 30 && !Doonce2)
            {
                // If the player attacks with the bow the guards, they are going to come after the player, so let's trigger the group

                // If the any of the guards targets the player as enemy, trigger the sequence
                if (outpGuard1 != null && outpGuard1.IsFightingEntity(Player.RckPlayer.GetPlayerEntity()) ||
                    outpGuard2 != null && outpGuard2.IsFightingEntity(Player.RckPlayer.GetPlayerEntity()) ||
                    outpGuard3 != null && outpGuard3.IsFightingEntity(Player.RckPlayer.GetPlayerEntity()) ||
                    outpGuard4 != null && outpGuard4.IsFightingEntity(Player.RckPlayer.GetPlayerEntity()) ||
                    outpGuard5 != null && outpGuard5.IsFightingEntity(Player.RckPlayer.GetPlayerEntity()))
                {
                    RCKFunctions.ExecuteScript("RPGCreationKit.Game.ResultScripts.RSQL_OneStepForward_PlayerApproachFortressWithoutTalkingToMartin");
                    Doonce2 = true;
                }
            }

            // Check for guard dead
            if(quest.currentQuestStage == 40)
            {
                // Prevent Martin's death
                if (martin == null)
                    CellsSystem.CellInformation.TryToGetAI("OutpostGuard001", out outpGuard1);
                
                if (outpGuard1 != null && !outpGuard1.isAlive &&
                   outpGuard2 != null && !outpGuard2.isAlive &&
                   outpGuard3 != null && !outpGuard3.isAlive &&
                   outpGuard4 != null && !outpGuard4.isAlive &&
                   generalPhillida != null && !generalPhillida.isAlive)
                {
                    RCKFunctions.SetQuestStage(quest.questID, 50);
                    RCKFunctions.CompleteQuestStage(quest.questID, 40);
                }
            }

            // Restore all AI dialogue and make martin carry the story
            if(quest.currentQuestStage == 50 && !Doonce3)
            {
                RckAI follower1 = null;
                CellInformation.TryToGetAI("RedShadeFollower001", out follower1);

                RckAI follower2 = null;
                CellInformation.TryToGetAI("RedShadeFollower002", out follower2);

                if(follower1 != null && follower1.isAlive)
                {
                    follower1.ChangeDialogueGraph("RSDIALOGUE_FollowerAfterAssault");
                    follower1.dialogueSystemEnabled = true;
                }

                if (follower2 != null && follower2.isAlive)
                {
                    follower2.ChangeDialogueGraph("RSDIALOGUE_FollowerAfterAssault");
                    follower2.dialogueSystemEnabled = true;
                }

                if(martin != null)
                {
                    martin.ChangeDialogueGraph("RSDIALOGUE_Martin001AfterAssault");
                    martin.dialogueSystemEnabled = true;
                }

                if(vera != null && vera.isAlive)
                {
                    vera.ChangeDialogueGraph("RSDIALOGUE_Vera001AfterAssault");
                    vera.dialogueSystemEnabled = true;
                }

                Doonce3 = true;
            }
        }
    }
}