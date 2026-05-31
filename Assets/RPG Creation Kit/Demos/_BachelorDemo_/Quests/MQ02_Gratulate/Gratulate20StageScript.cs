using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class Gratulate20StageScript : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            RCKFunctions.MutateMutable("Mutable_BellRingingCity", false);
            RCKFunctions.MutateMutable("Mutable_BellRingingTavern", false);
            RCKFunctions.MutateMutable("Mutable_BellRingingTavernCellar", false); 
            RCKFunctions.MutateMutable("Mutable_BellRingingCityExterior", false);
            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}