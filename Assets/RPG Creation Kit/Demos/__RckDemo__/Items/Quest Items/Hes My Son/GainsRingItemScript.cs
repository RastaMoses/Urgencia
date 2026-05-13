using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class GainsRingItemScript : ItemScript
    {
        public override void OnAdd(Inventory inventory)
        {
            if (inventory == Inventory.PlayerInventory && RCKFunctions.GetStage("SQ_HesMySon001") == 20)
            {
                RCKFunctions.SetQuestStage("SQ_HesMySon001", 30);
                RCKFunctions.CompleteQuestStage("SQ_HesMySon001", 20);
            }
            else if(inventory == Inventory.PlayerInventory && RCKFunctions.GetStage("SQ_HesMySon001") == 10)
            {
                RCKFunctions.SetQuestStage("SQ_HesMySon001", 30);
                RCKFunctions.CompleteQuestStage("SQ_HesMySon001", 10);
                RCKFunctions.CompleteQuestStage("SQ_HesMySon001", 20);
            }
        }
    }
}