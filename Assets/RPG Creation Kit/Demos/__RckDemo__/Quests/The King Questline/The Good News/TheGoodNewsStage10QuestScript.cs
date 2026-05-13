using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class TheGoodNewsStage10QuestScript : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            RCKFunctions.SendIntoOblivion("Mack001");

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}