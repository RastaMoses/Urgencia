using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class TheyShallFall_Stage30Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            RCKFunctions.LockDoor("KingsPalaceToCityInterior", DoorLockLevel.Impossible);

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}