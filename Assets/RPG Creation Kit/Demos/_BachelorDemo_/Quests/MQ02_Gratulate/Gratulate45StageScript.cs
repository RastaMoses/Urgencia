using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class Gratulate45StageScript : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            RCKFunctions.MutateMutable("Mutable_BellRingingCity", true);
            RCKFunctions.MutateMutable("Mutable_BellRingingTavern", true);
            RCKFunctions.MutateMutable("Mutable_BellRingingTavernCellar", true);
            RCKFunctions.MutateMutable("Mutable_BellRingingCityExterior", true);

            RCKFunctions.UnlockDoor("UrgenciaToTavernCellar");
            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}