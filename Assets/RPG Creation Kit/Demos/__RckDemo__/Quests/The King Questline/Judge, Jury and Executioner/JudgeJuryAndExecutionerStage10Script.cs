using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;

namespace RPGCreationKit.Quests
{
    public class JudgeJuryAndExecutionerStage10Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            // Adds the player to the Guards Faction
            Player.RckPlayer.instance.AddToFaction("GuardsFaction");

            // Set Mack to be not essential anymore as he has to be killed by the player, we know he will not be loaded so modify the AI data directly
            var aiData = SaveSystem.SaveSystemManager.instance.saveFile.AIData;

            var mackData = aiData.aiDictionary["Mack001"];
            mackData.currentDialogueID = "DIALOGUEMQ_MackRSDialogue002TKQuestline";
            mackData.isEssential = false;

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}