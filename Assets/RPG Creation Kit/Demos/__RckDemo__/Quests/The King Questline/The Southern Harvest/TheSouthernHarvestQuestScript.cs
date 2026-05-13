using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class TheSouthernHarvestQuestScript : QuestScript
    {

        RckAI follower1 = null;
        RckAI follower2 = null;
        RckAI vera = null;
        RckAI martin = null;

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

            if(quest.currentQuestStage == 50)
            {
                quest.questScriptExecutionDelay = 1f;

                // Get AI if not assigned

                if (follower1 == null)
                    CellInformation.TryToGetAI("RedShadeFollower001", out follower1);

                if(follower2 == null)
                    CellInformation.TryToGetAI("RedShadeFollower002", out follower2);

                if(vera == null)
                    CellInformation.TryToGetAI("TKQL_Vera", out vera);

                if (martin == null)
                    CellInformation.TryToGetAI("TKQL_Martin", out martin);

                
                if(follower1 != null && follower2 != null && vera != null && martin != null)
                {
                    if(!follower1.isAlive && !follower2.isAlive && !vera.isAlive && !martin.isAlive)
                    {
                        // Progress with the quest
                        RCKFunctions.SetQuestStage(quest.questID, 60);
                        RCKFunctions.CompleteQuestStage(quest.questID, 50);

                        // Other changes carried by TheSoutherHarvestStage60Script
                    }
                }
            }

        }
    }
}