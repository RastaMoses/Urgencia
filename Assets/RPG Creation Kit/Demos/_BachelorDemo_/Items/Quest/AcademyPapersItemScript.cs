using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    public class AcademyPapersItemScript : ItemScript
    {
        public override void OnAdd(Inventory inventory)
        {
            base.OnAdd(inventory);
            if (inventory == Inventory.PlayerInventory)
            {
                RCKFunctions.CompleteQuestStage("MQ_Gratulate", 45);
                RCKFunctions.SetQuestStage("MQ_Gratulate", 50);
            }
        }

    }
}