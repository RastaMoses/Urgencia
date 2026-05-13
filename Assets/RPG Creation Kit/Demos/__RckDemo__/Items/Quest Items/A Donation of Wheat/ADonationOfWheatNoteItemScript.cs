using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class ADonationOfWheatNoteItemScript : ItemScript
    {
        public override void OnAdd(Inventory inventory)
        {
            base.OnAdd(inventory);

            if(inventory == Inventory.PlayerInventory && RCKFunctions.GetStage("SQ_ADonationOfWheat") == 0)
            {
                RCKFunctions.AddQuest("SQ_ADonationOfWheat");
            }

        }

    }
}