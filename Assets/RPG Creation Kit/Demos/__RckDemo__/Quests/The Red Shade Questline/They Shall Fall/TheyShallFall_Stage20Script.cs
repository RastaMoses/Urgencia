using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;

namespace RPGCreationKit.Quests
{
    public class TheyShallFall_Stage20Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here

            // Boost the player health
            //EntityAttributes.PlayerAttributes.MaxHealth = 550.0f;
            //EntityAttributes.PlayerAttributes.CurHealth = 550.0f;

            //EntityAttributes.PlayerAttributes.MaxStamina = 300.0f;
            //EntityAttributes.PlayerAttributes.CurStamina = 300.0f;

            RCKFunctions.LockDoor("CityInteriorToArmorShop", CellsSystem.DoorLockLevel.Impossible);
            RCKFunctions.LockDoor("CityInteriorToGeneralGoodsStore", CellsSystem.DoorLockLevel.Impossible);
            RCKFunctions.LockDoor("CityInteriorToBlacksmithShop", CellsSystem.DoorLockLevel.Impossible);


            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}