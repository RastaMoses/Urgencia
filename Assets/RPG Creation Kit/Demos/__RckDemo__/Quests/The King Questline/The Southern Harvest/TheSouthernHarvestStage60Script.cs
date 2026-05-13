using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class TheSouthernHarvestStage60Script : QuestStageScript
    {
        RckAI ryan = null;
        RckAI generalPhillida = null;


        private void Start()
        {
            // Your code here
            CellInformation.TryToGetAI("TKQL_Ryan", out ryan);

            if (ryan != null)
            {
                ryan.ChangeDialogueGraph("TKQLDialogue_RyanAfterAmbush");
                ryan.SetNewBehaviourTree(false, "BTP_SpeakToPlayerWalkRyan");
                ryan.SwitchBehaviourTree(false);
            }


            CellInformation.TryToGetAI("GeneralPhillida001", out generalPhillida);

            if (generalPhillida != null)
                generalPhillida.ChangeDialogueGraph("TKQLDialogue_GeneralPhillidaAfterAmbush");

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}