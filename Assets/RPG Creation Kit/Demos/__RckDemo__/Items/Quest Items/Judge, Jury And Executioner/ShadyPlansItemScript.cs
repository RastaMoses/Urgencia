using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class ShadyPlansItemScript : ItemScript
    {
        public override void OnAdd(Inventory inventory)
        {
            if (inventory == Inventory.PlayerInventory && RCKFunctions.GetStage("TKQL_JudgeJuryAndExecutioner") == 15)
            {
                RCKFunctions.SetQuestStage("TKQL_JudgeJuryAndExecutioner", 20);
                RCKFunctions.CompleteQuestStage("TKQL_JudgeJuryAndExecutioner", 15);
            }
        }
    }
}