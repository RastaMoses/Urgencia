using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class HangingNote001ItemScript : ItemScript
    {
        public override void OnAdd(Inventory inventory)
        {
            base.OnAdd(inventory);

            if(inventory == Inventory.PlayerInventory && RCKFunctions.GetStage("SQ_EveryDeadMansNightmare") == 0)
            {
                RCKFunctions.AddQuest("SQ_EveryDeadMansNightmare");
            }

        }

    }
}