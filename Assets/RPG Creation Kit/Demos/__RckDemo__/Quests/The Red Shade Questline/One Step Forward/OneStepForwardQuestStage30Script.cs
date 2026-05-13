using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;

namespace RPGCreationKit.Quests
{
    /// <summary>
    /// Spawns Martin and the Red Shade followers in the cell "Virrihael(-1,2)" to assault the outpost.
    /// </summary>
    public class OneStepForwardQuestStage30Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here
          
            // Spawn Martin and followers
            RCKFunctions.SpawnAIInCell("Martin001", "Virrihael(-1,2)", new Vector3(-135.68f, 1.0f, 270.6f), Quaternion.Euler(0, 115.148f, 0));
            RCKFunctions.SpawnAIInCell("RedShadeFollower001", "Virrihael(-1,2)", new Vector3(-138.36f, 1.0f, 273.46f), Quaternion.Euler(0, -246.842f, 0));
            RCKFunctions.SpawnAIInCell("RedShadeFollower002", "Virrihael(-1,2)", new Vector3(-138.99f, 1.0f, 276.64f), Quaternion.Euler(0, -245.72f, 0));


            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}