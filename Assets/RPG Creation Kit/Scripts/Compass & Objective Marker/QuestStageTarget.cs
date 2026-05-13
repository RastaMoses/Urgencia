using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Refer a World Object needed to complete a quest objective, its position will be displayed on the compass.
    /// </summary>
    public class QuestStageTarget : MonoBehaviour
    {
        [SerializeField] private Quest quest;
        [SerializeField] private int questStageIndex;

        void Start()
        {
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            while ( WorldManager.instance == null || Player.RckPlayer.instance == null ||
                WorldManager.instance.isLoading || !Player.RckPlayer.instance.IsControlledByPlayer() || !CellsSystem.CellInformation.AllActiveCellsLoaded())
                yield return new WaitForEndOfFrame();

            QuestManager.OnQuestStageChanges += CheckQuestObjective;
            CheckQuestObjective();

            yield return null;
        }

        /// <summary>
        /// Called from OnQuestObjectiveChanges event (QuestManager.cs), check if there is a new Quest Objective to point
        /// </summary>
        private void CheckQuestObjective()
        {
            try
            {

                // If the current quest & quest objective match this marker
                if (QuestManager.instance != null && QuestManager.instance.CurrentQuest != null && Compass.instance != null && transform != null && quest != null &&
                    QuestManager.instance.CurrentQuest.questID == quest.questID &&
                    QuestManager.instance.CurrentQuest.currentQuestStage == questStageIndex)
                {
                    // Set the compass to look at this 
                    // Need changes when working with cells
                    if (RCKSettings.ROUND_COMPASS_ENABLED)
                        Compass.instance.ChangeQuestObjective(transform);

                    if (RCKSettings.HORIZONTAL_COMPASS_ENABLED)
                        HorizontalCompass.instance.ChangeQuestObjective(transform);
                }

                if (RCKSettings.ROUND_COMPASS_ENABLED)
                    Compass.instance.CheckActiveObjectives();

                if (RCKSettings.HORIZONTAL_COMPASS_ENABLED)
                    HorizontalCompass.instance.CheckActiveObjectives();
            }
            catch
            {

            }
        }
    }
}
