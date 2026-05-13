using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit
{
    public class HalfWrittenLetterItemScript : ItemScript
    {
        public override void OnAdd(Inventory inventory)
        {
            if (inventory == Inventory.PlayerInventory && (RCKFunctions.GetStage("TKQL_TheSouthernHarvest") == 20 || RCKFunctions.GetStage("TKQL_TheSouthernHarvest") == 30))
            {
                RCKFunctions.CompleteQuestStage("TKQL_TheSouthernHarvest", 30);

                RckAI ryan = null;
                CellsSystem.CellInformation.TryToGetAI("TKQL_Ryan", out ryan);

                if(ryan != null)
                {
                    // Set Ryan new dialogue and behaviour
                    ryan.ChangeDialogueGraph("TKQL_TheSouthernHarvest_RyanSpeaksToPlayerAfterNote");
                    ryan.SetNewBehaviourTree(false, "BTP_SpeakToPlayer");
                    ryan.SwitchBehaviourTree(false);
                }
            }
        }
    }
}