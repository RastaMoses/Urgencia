using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Quests
{
    public class QuackActionRequiredQuestStage30Script : QuestStageScript
    {
        [SerializeField] GameObject flowers;
        private void Start()
        {
            // Your code here
            //Spawn Flower Interactive Object
            Instantiate(flowers);
            //Disable Merchant


            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}