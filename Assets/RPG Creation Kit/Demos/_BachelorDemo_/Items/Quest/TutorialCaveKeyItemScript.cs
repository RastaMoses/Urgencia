using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class TutorialCaveKeyItemScript : ItemScript
    {
        public override void OnAdd(Inventory inventory)
        {
            if (inventory == Inventory.PlayerInventory && RCKFunctions.GetStage("MQ_LateNightEarlyMorning") == 10)
            {
                RCKFunctions.SetQuestStage("MQ_LateNightEarlyMorning", 20);
                RCKFunctions.CompleteQuestStage("MQ_LateNightEarlyMorning", 10);
            }
        }

    }
}