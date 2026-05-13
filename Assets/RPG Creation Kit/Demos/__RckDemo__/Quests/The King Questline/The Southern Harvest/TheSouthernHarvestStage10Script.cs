using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class TheSouthernHarvestStage10Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            // Spawn Ryan at the door 
            RCKFunctions.SpawnAIInCurrentCell("TKQL_Ryan", new Vector3(2f, 0, -15f), Quaternion.Euler(0, 170.671f, 0));

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}