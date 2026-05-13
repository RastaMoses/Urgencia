using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;

namespace RPGCreationKit.Quests
{
    public class TheSouthernHarvestStage20Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here

            // Enables Ryan's quickline at the farmhouse
            RCKFunctions.MutateMutable("Mutable_TheSouthernHarvest_RyanDialog", false);
            RCKFunctions.UnlockDoor("Virrihael02ToVeraFarmhouse");

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}