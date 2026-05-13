using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;


namespace RPGCreationKit
{
    /// <summary>
    /// This class is used in the Quest Manager UI to display quests with toggles
    /// </summary>
    public class QuestInList : MonoBehaviour
    {

        public Quest quest;     // The referred quest
        public Text questName;  // The text that will contain the name of the quest

        /// <summary>
        /// Called when we select a quest from the active tab 
        /// </summary>
        public void OnValueChanges(bool isActive)
        {
            // If we selected this one
            if (GetComponent<Toggle>().isOn)
            {
                // Set this quest to be the current (of the actives)
                QuestManager.instance.CurrentQuest = quest;
            }

            QuestManager.instance.OnCurrentQuestChanges();

            // Check if we should delete the current quest
            QuestManagerUI.instance.CheckCurrentQuestEnabled();

            // Update the interface UI (description & objectives)
            QuestManagerUI.instance.UpdateActiveQuestsInterface();


        }

        /// <summary>
        /// Called when we select a quest from the completed tab 
        /// </summary>
        public void OnCompletedQuestValueChanges(bool isActive)
        {
            if (GetComponent<Toggle>().isOn)
            {
                // Set this quest to be the current (of the completed)
                QuestManagerUI.instance.completedSelectedQuest = quest;
            }

            // Iterate through all toggles to see if no one is enabled, if so, no current completed quest is active
            QuestManagerUI.instance.CheckCompletedQuestEnabled();

            // Update the completed quest description & objectives
            QuestManagerUI.instance.UpdateCompletedQuestsInterface();

        }
    }
}