using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class KillingMonstersNoteItemScript : ItemScript
    {
        public override void OnAdd(Inventory inventory)
        {
            base.OnAdd(inventory);

            if (inventory == Inventory.PlayerInventory && RCKFunctions.GetStage("SQ_KillingMonsters") == 0)
            {
                RCKFunctions.AddQuest("SQ_KillingMonsters");
            }

        }

    }
}