using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit
{

    /// <summary>
    /// Allows us to draw and control the QuestManagerUI (diary in the game)
    /// </summary>
    public class QuestManagerUI : MonoBehaviour
    {
        #region Singleton
        public static QuestManagerUI instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Debug.LogError("Anomaly detected with the Singleton Pattern of 'QuestManagerUI', are you using multple QuestManagerUI?");
                Destroy(this);
            }
        }
        #endregion

        [HideInInspector]
        public bool isUIopened = false;

        /// All the references for canvas elements
        [Header("Settings")]
        public GameObject QuestPanelUI;

        public QuestManagerUI_QuestTab activeQuestTab;
        public QuestManagerUI_QuestTab completedQuestTab;

        private QuestManager questManager;

        [Space(10)]

        [Header("Quest Tab")]
        public ToggleGroup activeQuestsInListGroup;
        public ToggleGroup completedQuestInListGroup;

        [Space(5)]

        // References to prefabs
        [Header("Prefabs")]
        public GameObject QuestInListPrefab;
        public GameObject CurrentQuestObjectivePrefab;
        public GameObject CompletedQuestObjectivePrefab;
        public GameObject FailedQuestObjectivePrefab;



        private void Start()
        {
            // Take the questManager reference from the singleton of the quest manager
            questManager = QuestManager.instance;
        }

        private void Update()
        {
            if (RckPlayer.instance.input.currentActionMap.name == "Player" || RckPlayer.instance.input.currentActionMap.name == "QuestJournalUI")
            {
                // Check for input
                if (RckPlayer.instance.input.currentActionMap.FindAction("QuestJournal").triggered &&
                    !InventoryUI.instance.isOpen && !RckPlayer.instance.isInConversation && !RckPlayer.instance.isInteracting &&
                    !RckPlayer.instance.isReadingBook && !RckPlayer.instance.isLooting && !TradeSystemUI.instance.isOpen && !TutorialAlertMessage.instance.messageOpened && !RckPlayer.instance.isInDeathSequence)
                    OpenClose();
            }
        }


        public void OpenClose()
        {
            // If it is not active (so it will be on this run)
            if (!QuestPanelUI.activeInHierarchy)
            {
                UpdateCompletedQuestList(); // Update the CompletedUI before showing it up
                UpdateActiveQuestList();    // Update the UI before showing it up
            }

            // Enable/Disable the panel
            QuestPanelUI.SetActive(!QuestPanelUI.activeInHierarchy);

            // If is active
            if (QuestPanelUI.activeInHierarchy)
            {
                // Set the controls & cursor
                RckPlayer.instance.EnableDisableControls(false);

                isUIopened = true;
                Time.timeScale = 0.0f;

                // Update the Interfaces
                UpdateCompletedQuestsInterface();
                UpdateActiveQuestsInterface();
                RckPlayer.instance.input.SwitchCurrentActionMap("QuestJournalUI");
            }
            else
            {
                // Set the controls & cursor
                RckPlayer.instance.EnableDisableControls(true);

                isUIopened = false;
                Time.timeScale = 1.0f;

                // Check if an alert was spawned (by switching quests) and still if is valid
                if (!questManager.CurrentQuest)
                {
                    // If it isn't valid, destroy all alerts
                    QuestAlertManager.instance.DestroyAllQuestObjectivesAlert();
                    QuestManager.instance.OnCurrentQuestChanges();
                }

                RckPlayer.instance.input.SwitchCurrentActionMap("Player");

            }

        }

        // Update the "Quest Tab" list of quests
        private void UpdateActiveQuestList()
        {
            // Clear the list
            foreach (Transform t in activeQuestTab.QuestsContent)
                Destroy(t.gameObject);

            bool questOnFound = false;
            // Create the list
            for (int i = 0; i < questManager.ActiveQuests.Count; i++)
            {
                // We instantiate the quest in list and get the references
                QuestInList questInList =
                    Instantiate(QuestInListPrefab, activeQuestTab.QuestsContent).GetComponent<QuestInList>();

                Toggle questInListToggle = questInList.GetComponent<Toggle>();

                // Set the correct group
                questInListToggle.group = activeQuestsInListGroup;

                // Then we assign the QuestInList data
                questInList.quest = questManager.ActiveQuests[i];
                questInList.questName.text = questManager.ActiveQuests[i].questName;

                // Check the active quest to set the right "yellow dot" (used when we open/close the UI)
                if (questManager.CurrentQuest == questInList.quest)
                {
                    questInListToggle.isOn = true;

                    questOnFound = true;
                    questInListToggle.Select();
                }

                // Set the listener to allow "CurrentQuest" switching
                questInListToggle.onValueChanged.AddListener(questInList.OnValueChanges);
            }

            if(!questOnFound)
            {
                if (activeQuestTab.QuestsContent.childCount > 0 && activeQuestTab.QuestsContent.GetChild(0) != null)
                    activeQuestTab.QuestsContent.GetChild(0).GetComponent<Toggle>().Select();
            }
        }


        [HideInInspector]
        public Quest completedSelectedQuest = null;
        // Update the "Quest Tab" list of quests
        private void UpdateCompletedQuestList()
        {
            // Clear the list
            foreach (Transform t in completedQuestTab.QuestsContent)
                Destroy(t.gameObject);

            bool questOnFound = false;
            // Create the list
            for (int i = 0; i < questManager.CompletedQuests.Count; i++)
            {
                // We instantiate the quest in list and get the references
                QuestInList questInList =
                    Instantiate(QuestInListPrefab, completedQuestTab.QuestsContent).GetComponent<QuestInList>();

                Toggle questInListToggle = questInList.GetComponent<Toggle>();

                // Set the correct group
                questInListToggle.group = completedQuestInListGroup;

                // Then we assign the QuestInList data
                questInList.quest = questManager.CompletedQuests[i];
                questInList.questName.text = questManager.CompletedQuests[i].questName;

                if (completedSelectedQuest == null && i == 0)
                    completedSelectedQuest = questInList.quest;

                // Check the active quest to set the right "yellow dot" (used when we open/close the UI)
                if (completedSelectedQuest == questInList.quest)
                {
                    questInListToggle.isOn = true;

                    questOnFound = true;
                    questInListToggle.Select();
                }

                // Set the listener to allow "CurrentQuest" switching
                questInListToggle.onValueChanged.AddListener(questInList.OnCompletedQuestValueChanges);
            }

            if (!questOnFound)
            {
                if (completedQuestTab.QuestsContent.childCount > 0 && completedQuestTab.QuestsContent.GetChild(0) != null)
                    completedQuestTab.QuestsContent.GetChild(0).GetComponent<Toggle>().Select();
            }
        }

        // Update the quest description & objectives
        public void UpdateActiveQuestsInterface()
        {
            if (questManager.CurrentQuest != null)
            {
                Quest curQuest = questManager.CurrentQuest;

                // Header and description
                activeQuestTab.questNameHeader.text = "Quest: " + curQuest.questName;
                activeQuestTab.questObjectivesHeader.text = "Quest Objectives:";

                activeQuestTab.questDescription.text = curQuest.questDescription;

                //  Quest Objectives

                // Clear the list
                foreach (Transform t in activeQuestTab.QuestObjectivesContent)
                    Destroy(t.gameObject);


                for (int i = 0; i < curQuest.allStagesActive.Count; i++)
                {
                    // Spawn the current objective
                    QuestStageInList curQuestObjective =
                        Instantiate(CurrentQuestObjectivePrefab, activeQuestTab.QuestObjectivesContent).GetComponent<QuestStageInList>();


                    // Set the description
                    curQuestObjective.description.text = curQuest.GetStage(curQuest.allStagesActive[i]).description;
                }

                // Spawn the completed objectives 
                List<int> completedQuestStages = curQuest.GetAllCompletedStagesAsInt();

                foreach(int qs in completedQuestStages)
                {
                    if (!curQuest.GetStage(qs).displayLogEntry) // skip for hidden objectives
                        continue;

                    QuestStageInList completedQuestObjective =
                    Instantiate(CompletedQuestObjectivePrefab, activeQuestTab.QuestObjectivesContent).GetComponent<QuestStageInList>();

                    completedQuestObjective.description.text = curQuest.GetStage(qs).description;
                }

                List<int> failedQuestsStages = curQuest.GetAllFailedStagesAsInt();

                foreach (int qs in failedQuestsStages)
                {
                    QuestStageInList failedQuestObjective =
                    Instantiate(FailedQuestObjectivePrefab, activeQuestTab.QuestObjectivesContent).GetComponent<QuestStageInList>();

                    failedQuestObjective.description.text = curQuest.GetStage(qs).description;
                }
            }
            else // if there is no quest selected
            {
                // Clear the list of quest objectives
                foreach (Transform t in activeQuestTab.QuestObjectivesContent)
                    Destroy(t.gameObject);

                // Dont display informations
                activeQuestTab.questNameHeader.text = "";
                activeQuestTab.questObjectivesHeader.text = "";
                activeQuestTab.questDescription.text = "";
            }
        }

        /// <summary>
        /// Update the completed quest description & objectives
        /// </summary>
        public void UpdateCompletedQuestsInterface()
        {
            if (completedSelectedQuest != null)
            {
                Quest curQuest = completedSelectedQuest;

                // Header and description
                completedQuestTab.questNameHeader.text = "Quest: " + curQuest.questName;
                completedQuestTab.questObjectivesHeader.text = "Quest Objectives:";

                completedQuestTab.questDescription.text = curQuest.questDescription;


                //  Quest Objectives

                // Clear the list
                foreach (Transform t in completedQuestTab.QuestObjectivesContent)
                    Destroy(t.gameObject);


                List<int> completedQuestStages = completedSelectedQuest.GetAllCompletedStagesAsInt();

                foreach (int qs in completedQuestStages)
                {
                    if (!curQuest.GetStage(qs).displayLogEntry) // skip for hidden objectives
                        continue;

                    QuestStageInList completedQuestObjective =
                    Instantiate(CompletedQuestObjectivePrefab, completedQuestTab.QuestObjectivesContent).GetComponent<QuestStageInList>();

                    completedQuestObjective.description.text = curQuest.GetStage(qs).description;
                }

                List<int> failedQuestsStages = completedSelectedQuest.GetAllFailedStagesAsInt();

                foreach (int qs in failedQuestsStages)
                {
                    QuestStageInList failedQuestObjective =
                    Instantiate(FailedQuestObjectivePrefab, activeQuestTab.QuestObjectivesContent).GetComponent<QuestStageInList>();

                    failedQuestObjective.description.text = curQuest.GetStage(qs).description;
                }

                /*
                // Spawn the completed objectives 
                for (int i = curQuest.questStages.Count - 1; i >= 0; i--)
                {
                    QuestStageInList completedQuestObjective =
                    Instantiate(CompletedQuestObjectivePrefab, completedQuestTab.QuestObjectivesContent).GetComponent<QuestStageInList>();

                    completedQuestObjective.description.text = curQuest.questStages[i].description;
                }
                */
            }
            else // if there is no quest selected
            {
                // Clear the list of quest objectives
                foreach (Transform t in completedQuestTab.QuestObjectivesContent)
                    Destroy(t.gameObject);

                // Dont display informations
                completedQuestTab.questNameHeader.text = "";
                completedQuestTab.questObjectivesHeader.text = "";
                completedQuestTab.questDescription.text = "";
            }
        }

        // Iterate through all toggles to see if no one is enabled, if so, no current quest is active
        public void CheckCurrentQuestEnabled()
        {
            // if no "QuestInList" toggle is enabled, no current quest is selected. Here we remove the reference
            if (!activeQuestsInListGroup.ActiveToggles().Any())
                questManager.CurrentQuest = null;

        }


        // Iterate through all toggles to see if no one is enabled, if so, no current quest is active
        public void CheckCompletedQuestEnabled()
        {
            // if no "QuestInList" toggle is enabled, no current quest is selected. Here we remove the reference
            if (!completedQuestInListGroup.ActiveToggles().Any())
                completedSelectedQuest = null;
        }

    }
}