using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class TheyShallFallQuestScript : QuestScript
    {
        RckAI king = null;
        RckAI adamus = null;
        RckAI virgilia = null;

        bool Doonce = false;
        bool Doonce2 = false;

        // This will start running the CustomUpdate as soon as the quest starts.
        public void Start()
        {
            RunQuestScript();
        }

        // CustomUpdate runs once every (quest.questScriptExecutionDelay) seconds
        public override void CustomUpdate()
        {
            base.CustomUpdate();

            // Your code here

            if(quest.currentQuestStage == 20)
            {
                quest.questScriptExecutionDelay = .25f;

                // Get 
                CellInformation.TryToGetAI("AdamusLatinius001", out adamus);
                CellInformation.TryToGetAI("TheKing001", out king);
                CellInformation.TryToGetAI("VirgiliaValera001", out virgilia);

                if(adamus != null && king != null && virgilia != null)
                {
                    if(!adamus.isAlive && !king.isAlive && !virgilia.isAlive && !Doonce)
                    {
                        RCKFunctions.SetQuestStage(quest.questID, 30);
                        RCKFunctions.CompleteQuestStage(quest.questID, 20);
                        Doonce = true;
                    }
                }
            }

            // Play ""Cutscene""
            if(quest.currentQuestStage == 30 && !Doonce2)
            {
                RCKFunctions.MutateMutable("Mutable_RedShadeFinalCutscene", false);
                Doonce2 = true;
            }

        }
    }
}