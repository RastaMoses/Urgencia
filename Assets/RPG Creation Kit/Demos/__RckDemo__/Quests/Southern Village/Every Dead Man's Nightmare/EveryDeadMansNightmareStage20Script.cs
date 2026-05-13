using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class EveryDeadMansNightmareStage20Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            RCKFunctions.SpawnAIInCell("ThiefOfTheDead001", "Virrihael(2,0)", new Vector3(282f, 0.1f, 13.52f), Quaternion.Euler(0, 90, 0));

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}