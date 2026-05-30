using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class QuackActionRequiredQuestScript : QuestScript
    {
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
            
            if (RCKFunctions.GetStage("SQ_QuackActionRequired") == 60)
            {

                RCKFunctions.MutateMutable("Mutable_FrogQuestFail", false);
                CellInformation.TryToGetAI("Pellan001", out RckAI pellan);
                if (pellan != null) { pellan.DestroyThis(); }
                //if (pellan != null) { RCKFunctions.SpawnAIInCell("Frog001", "TomsTavern", new Vector3(6.59600019f, 0.737999976f, 5.1079998f), new Quaternion(1.40390298e-07f, -0.72203207f, -8.20759638e-09f, 0.691859663f)); }

            }
        }
    }
}