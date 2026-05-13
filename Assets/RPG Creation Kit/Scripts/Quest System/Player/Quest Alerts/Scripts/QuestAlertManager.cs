using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGCreationKit;


namespace RPGCreationKit
{

    public enum QuestObjectiveAlertType
    {
        Current = 0,
        Completed = 1,
        Failed = 2
    }
    /// <summary>
    /// Class that keeps track of all Alerts of quests, completed or added and Quest Objectives
    /// </summary>
    public class QuestAlertManager : MonoBehaviour
    {

        #region Singleton
        public static QuestAlertManager instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Debug.LogError("Anomaly detected with the Singleton Pattern of 'QuestAlertManager', are you using multple QuestAlertManager?");
                Destroy(this);
            }
        }
        #endregion


        /// <summary>
        /// To keep the data of a queued alert
        /// </summary>
        [System.Serializable]
        public struct QueuedAlert
        {
            public string QuestName;
            public bool isNew;

            // Constructor
            public QueuedAlert(string _questName, bool _isNew)
            {
                QuestName = _questName;
                isNew = _isNew;
            }
        }

        // References
        [Header("Quest Alert")]
        public GameObject QuestAlertPrefab;

        [Space(5)]
        [Header("Quest Objectives Alert")]
        public Transform QuestObjectivesContainer;
        public GameObject CurrentQuestObjectiveAlertPrefab;
        public GameObject QuestObjectiveAlertCompletedPrefab;
        public GameObject FailedQuestObjectiveAlertCompletedPrefab;

        public List<QueuedAlert> queuedAlerts = new List<QueuedAlert>();


        /// <summary>
        /// Instantiate a new QuestAlert text
        /// </summary>
        /// <param name="QuestName">The quest name</param>
        /// <param name="isNew">Is it a new quest or a completed one?</param>
        public void InitQuestAlert(string QuestName, bool isNew)
        {
            // If there is no other
            if (transform.childCount == 0)
            {
                // Instantiate the Quest Alert
                GameObject newQuestAlert = Instantiate(QuestAlertPrefab, transform);

                // Set the correct text
                if (isNew)
                    newQuestAlert.GetComponent<TextMeshProUGUI>().text = QuestName + ": Started!";
                else
                    newQuestAlert.GetComponent<TextMeshProUGUI>().text = QuestName + ": Completed!";

                // Destroy it after the lenght of the animation clip of the text (Fade In Text - 5 Seconds)
                Destroy(newQuestAlert, newQuestAlert.GetComponent<Animation>().clip.length);
            }
            else
            {
                // Add in the queued list
                queuedAlerts.Add(new QueuedAlert(QuestName, isNew));

                // Wait for the current alert to finish
                StartCoroutine("WaitForEndOfAlert");
            }
        }

        IEnumerator WaitForEndOfAlert()
        {
            yield return new WaitForEndOfFrame();

            // If there is no queued alerts, break!
            if (queuedAlerts.Count == 0) yield break;

            // If there is still some other alerts
            while (transform.childCount > 0)
            {
                // just wait a bit more
                yield return null;
            }
            // When there are no other alerts, init the first alert added in the list
            InitQuestAlert(queuedAlerts[0].QuestName, queuedAlerts[0].isNew);

            // and then remove it from the list
            queuedAlerts.RemoveAt(0);
        }

        /// <summary>
        /// Called when we have to show a QuestObjective, completed or not
        /// </summary>
        /// <param name="_questObjective">The quest objective description</param>
        /// <param name="isCompleted">Is this quest objective completed?</param>
        public void InitQuestObjectiveAlert(string _questObjective, QuestObjectiveAlertType type)
        {
            InGameQuestObjectiveAlert newObjectiveAlert = null;


            switch (type)
            {
                case QuestObjectiveAlertType.Current:
                    newObjectiveAlert = Instantiate(CurrentQuestObjectiveAlertPrefab, QuestObjectivesContainer).GetComponent<InGameQuestObjectiveAlert>();

                    break;
                case QuestObjectiveAlertType.Completed:
                    newObjectiveAlert = Instantiate(QuestObjectiveAlertCompletedPrefab, QuestObjectivesContainer).GetComponent<InGameQuestObjectiveAlert>();
                    break;
                case QuestObjectiveAlertType.Failed:
                    newObjectiveAlert = Instantiate(FailedQuestObjectiveAlertCompletedPrefab, QuestObjectivesContainer).GetComponent<InGameQuestObjectiveAlert>();
                    break;
                default:
                    break;
            }


            newObjectiveAlert.Initialize(_questObjective);
        }


        /// <summary>
        /// Called when we switch the quests in the QuestManagerUI, display only the current objective
        /// </summary>
        public void DisplayCurrentQuestObjective(bool isCompleted)
        {
            QuestObjectiveAlertType alertType = (isCompleted) ? QuestObjectiveAlertType.Completed : QuestObjectiveAlertType.Current;
            // Clear the objectives list
            DestroyAllQuestObjectivesAlert();

            var allStagesActive = QuestManager.instance.CurrentQuest.allStagesActive;
            for (int i = 0; i < allStagesActive.Count; i++)
                InitQuestObjectiveAlert(QuestManager.instance.CurrentQuest.GetStage(allStagesActive[i]).description, alertType);
        }

        public void DisplayQuestObjective(string questID, int stageIndex, bool isCompleted)
        {
            QuestObjectiveAlertType alertType = (isCompleted) ? QuestObjectiveAlertType.Completed : QuestObjectiveAlertType.Current;

            Quest quest = RCKFunctions.GetQuest(questID);

            for (int i = 0; i < quest.allStagesActive.Count; i++)
            {
                InitQuestObjectiveAlert(quest.GetStage(quest.allStagesActive[i]).description, alertType);
            }
        }

        public void DisplayQuestObjective(Quest quest, int stageIndex, bool isCompleted)
        {
            //InitQuestObjectiveAlert(quest.GetStage(stageIndex).description, isCompleted);
            DisplayQuestObjective(quest.questID, stageIndex, isCompleted);
        }

        /// <summary>
        /// Clear all the quest objectives alerts
        /// </summary>
        public void DestroyAllQuestObjectivesAlert()
        {
            foreach (Transform t in QuestObjectivesContainer)
                Destroy(t.gameObject);
        }

    }
}