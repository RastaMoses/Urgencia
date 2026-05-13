using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RPGCreationKit;
using RPGCreationKit.SaveSystem;
using RPGCreationKit.Player;

namespace RPGCreationKit
{

    /// <summary>
    /// Class that keeps track of every Active and Completed quests, Add quests and update them.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {

        #region Singleton
        public static QuestManager instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Debug.LogError("Anomaly detected with the Singleton Pattern of 'QuestManager', are you using multple QuestManager?");
                Destroy(this);
            }
        }
        #endregion

        [Header("Quest Manager")]

        [Space(5)]

        // The current Quest. Displays the current Quest Objective position with the 2D Compass
        public Quest CurrentQuest;

        // Lists of active quests and completed ones.
        public List<Quest> ActiveQuests;
        public List<Quest> CompletedQuests;

        // For updating compass
        public delegate void QuestStageChanges();
        public static event QuestStageChanges OnQuestStageChanges;

        public AudioClip onQuestAddSound;
        public AudioClip onQuestCompletedSound;
        public AudioClip onStageUpdateSound;

        public void Start()
        {
            
        }


        // When we'll want to add a new Quest, we'll always call this method 
        public Quest AddQuest(Quest newQuest, int atIndex = 0, bool hideAlert = false, bool _comesFromLoad = false, bool _comesFromLoadAndCompleted = false)
        {
            // As written in the documentation, we will always work with Instances of the Quests, never with the real reference
            newQuest = Instantiate(newQuest);

            // Set the correct initial index
            newQuest.currentQuestStage = atIndex;



            // Before going furhter, process the conditions on this quest.
            bool conditionsSatisfied = RCKFunctions.VerifyConditions(newQuest.questConditions);
            
            if(!conditionsSatisfied)
            {
                Debug.Log("The condition was not satisfied.");
                Destroy(newQuest);
                return null;
            }

            // If the quest has just started - set the first objective.
            
            if (newQuest.firstStageIsDefaultStage)
            {
                if (newQuest.currentQuestStage == 0)
                {
                    newQuest.currentQuestStage = newQuest.questStages[0].index;
                }

                /*
                // Trigger OnStart of QuestStageScript if present
                if (!string.IsNullOrEmpty(newQuest.GetStage(newQuest.currentQuestStage).resultScript))
                {
                    // Start running the QuestScript if it exists
                    if (newQuest.questScript != null)
                    {
                        QuestStageScript qScript =
                            (QuestStageScript)QuestScriptManager.instance.scriptsHolder.AddComponent(System.Type.GetType(newQuest.GetStage(newQuest.currentQuestStage).resultScript));
                    }
                }
                */
            }

            // Add the quest in the active list
            ActiveQuests.Add(newQuest);

            // If no current quest is active at the moment
            if (!CurrentQuest && !_comesFromLoad)
            {
                CurrentQuest = ActiveQuests.Find(x => x.questID == newQuest.questID); // set the one just added
            }


            if(!_comesFromLoad)
                SetQuestStage(newQuest.questID, newQuest.currentQuestStage, false, _comesFromLoad);
            else
                SetQuestStage(newQuest.questID, newQuest.currentQuestStage, true, _comesFromLoad);


            if (!hideAlert)
                RCKFunctions.InitQuestAlert(newQuest, false);

            // Start running the QuestScript if it exists
            if(!string.IsNullOrEmpty(newQuest.questScript) && !_comesFromLoadAndCompleted)
            {
                QuestScript qScript = (QuestScript)QuestScriptManager.instance.scriptsHolder.AddComponent(System.Type.GetType(newQuest.questScript));
                qScript.quest = newQuest;
            }

            // Play sound
            if (!_comesFromLoad && !hideAlert)
                GameAudioManager.instance.PlayOneShot(AudioSources.UISounds, onQuestAddSound);

            return newQuest;
        }

        // When we'll want to add a new Quest, we'll always call this method
        public Quest AddQuest(string newQuestID, int atIndex = 0, bool hideAlert = false, bool _comesFromLoad = false, bool _comesFromLoadAndCompleted = false)
        {
            Quest newQuest = RCKFunctions.GetQuest(newQuestID);

            if (!newQuest)
            {
                Debug.LogWarning("Tried to add the quest with id: " + newQuestID + " but the Quest is not present in the current Quest Database File.");
                return null;
            }

            return AddQuest(newQuest, atIndex, hideAlert, _comesFromLoad, _comesFromLoadAndCompleted);
        }


        // This method is used to execute QuestDealer from scripts as Goto, NPCDialogueLine and so on and so forth
        public void QuestDealerActivator(List<QuestDealer> questDealer)
        {
            // We just add as many quests as the QuestDealer list of the caller has
            for (int i = 0; i < questDealer.Count; i++)
                AddQuest(questDealer[i].newQuest, questDealer[i].atIndex, questDealer[i].hideAlert);
        }

        public void QuestUpdaterActivator(List<QuestUpdater> questUpdater)
        {
            // We just add as many quests as the QuestDealer list of the caller has
            for (int i = 0; i < questUpdater.Count; i++)
            {
                if (questUpdater[i].completed)
                    CompleteQuestStage(questUpdater[i].questToUpdate.questID, questUpdater[i].questStage, questUpdater[i].hideLog);
                else
                    SetQuestStage(questUpdater[i].questToUpdate.questID, questUpdater[i].questStage, questUpdater[i].hideLog);
            }
        }

        // This method is used to complete Quest objectives of a quest:
        public void SetQuestStage(string _questID, int _stageIndex, bool hideLog = false, bool _comesFromLoad = false)
        {
            // Needed database of quests to find the object by id
            Quest quest = RCKFunctions.GetQuest(_questID);

            if (quest)
            {
                QuestStage stage = quest.GetStage(_stageIndex);

                if (stage != null)
                    SetQuestStage(quest, stage, hideLog, _comesFromLoad);
                else
                    Debug.LogWarning("Stage: " + _stageIndex + " does not exist for the quest " + quest.questName + "ID:" + _questID);
            }
            else
                Debug.LogWarning("Quest with ID: " + _questID + " does not exist or is not included in the currently used Database File.");

        }
        public void SetQuestStage(Quest _quest, QuestStage _questStage, bool hideLog = false, bool _comesFromLoad = false)
        {
            // Find and keep the index of the quest
            Quest quest = null;
            bool questFound = false;

            for (int i = 0; i < ActiveQuests.Count; i++)
            {
                if (_quest.questID == ActiveQuests[i].questID)
                {
                    // Set the flag
                    questFound = true;
                    // Keep the reference of the quest
                    quest = ActiveQuests[i];
                    break;
                }
            }

            if (questFound)
            {
                int previousQuestStage = quest.currentQuestStage;
                //quest.allStagesCompleted.Add(completedStage);

                /*
                bool repeatedQuestStage = false;

                // This is a redo of stage
                if(_questStage.index < quest.currentQuestStage)
                {
                    if (!quest.allowRepeatedStages)
                        return;
                    else
                        repeatedQuestStage = true;
                }

                */

                //if (!repeatedQuestStage)
                //    quest.GetStage(quest.currentQuestStage).stageCompleted = true;

                // Set the new stage
                quest.currentQuestStage = _questStage.index;

                if(!_comesFromLoad)
                    quest.allStagesActive.Add(_questStage.index);

                //if(repeatedQuestStage)
                    quest.GetStage(quest.currentQuestStage).stageCompleted = false;


                if (!hideLog)
                {
                    // Update the UI with the new current quest objective
                    QuestAlertManager.instance.InitQuestObjectiveAlert(quest.GetStage(_questStage.index).description, QuestObjectiveAlertType.Current);

                    // Update the UI with the completed quest objective
                    //if (quest.GetStage(previousQuestStage) != null && !repeatedQuestStage)
                    //    QuestAlertManager.instance.InitQuestObjectiveAlert(quest.GetStage(previousQuestStage).description, true);
                }

                // Trigger OnStart of QuestStageScript if present
                if (!_comesFromLoad && !string.IsNullOrEmpty(quest.GetStage(quest.currentQuestStage).resultScript))
                {
                    // Start running the QuestScript if it exists
                    if (quest.questScript != null)
                    {
                        QuestStageScript qScript =
                            (QuestStageScript)QuestScriptManager.instance.scriptsHolder.AddComponent(System.Type.GetType(quest.GetStage(quest.currentQuestStage).resultScript));
                    }
                }
            }
            else
            {
                if (!_questStage.completeQuest)
                {
                    // We add the quest in the list starting from the next quest objective from this one
                    AddQuest(_quest, _questStage.index, false);

                    // Update the UI with the completed quest objective (Already done in AddQuest)
                    //QuestAlertManager.instance.DisplayCurrentQuestObjective();

                } else
                {
                    // With this quest objective we've completed the quest.
                    AddQuest(_quest, _quest.questStages.Count - 1, true);
                    CompleteQuest(_quest);
                }
            }

            // Update the compass
            if (OnQuestStageChanges != null)
            {
                if (RCKSettings.ROUND_COMPASS_ENABLED)
                    Compass.instance.OnQuestUpdate();

                if (RCKSettings.HORIZONTAL_COMPASS_ENABLED)
                    HorizontalCompass.instance.OnQuestUpdate();
                OnQuestStageChanges();
            }

            if (!_comesFromLoad && !hideLog)
                GameAudioManager.instance.PlayOneShot(AudioSources.UISounds, onStageUpdateSound);

        }

        public void CompleteQuestStage(string _questID, int _stageIndex, bool hideLog = false, bool _comesFromLoad = false)
        {
            // Needed database of quests to find the object by id
            Quest quest = RCKFunctions.GetQuest(_questID);

            if (quest)
            {
                QuestStage stage = quest.GetStage(_stageIndex);

                if (stage != null)
                    CompleteQuestStage(quest, stage, hideLog, _comesFromLoad);
                else
                    Debug.LogWarning("Stage: " + _stageIndex + " does not exist for the quest " + quest.questName + "ID:" + _questID);
            }
            else
                Debug.LogWarning("Quest with ID: " + _questID + " does not exist or is not included in the currently used Database File.");
        }

        public void CompleteQuestStage(Quest _quest, QuestStage _questStage, bool hideLog = false, bool _comesFromLoad = false)
        {
            // Find and keep the index of the quest
            Quest quest = null;
            bool questFound = false;

            for (int i = 0; i < ActiveQuests.Count; i++)
            {
                if (_quest.questID == ActiveQuests[i].questID)
                {
                    // Set the flag
                    questFound = true;
                    // Keep the reference of the quest
                    quest = ActiveQuests[i];
                    break;
                }
            }

            if (questFound)
            {
                quest.allStagesActive.Remove(_questStage.index);

                quest.GetStage(_questStage.index).stageCompleted = true;

                if (quest.GetStage(_questStage.index).completeQuest)
                {
                    CompleteQuest(quest);

                    // Update the UI with the last completed quest objective
                    if (!hideLog)
                        QuestAlertManager.instance.InitQuestObjectiveAlert(_questStage.description, QuestObjectiveAlertType.Completed);
                }
                else
                {
                    // Update the UI with the completed quest objective
                    if (!hideLog)
                        QuestAlertManager.instance.InitQuestObjectiveAlert(_questStage.description, QuestObjectiveAlertType.Completed);
                }
            }
            else
            {
                if (!_questStage.completeQuest)
                {
                    // We add the quest in the list starting from the next quest objective from this one
                    AddQuest(_quest, _questStage.index, false);

                    // Update the UI with the completed quest objective (Already done in AddQuest)
                    //QuestAlertManager.instance.DisplayCurrentQuestObjective();

                }
                else
                {
                    // With this quest objective we've completed the quest.
                    AddQuest(_quest, _quest.questStages.Count - 1, true);
                    CompleteQuest(_quest);
                }
            }

            // Update the compass
            if (OnQuestStageChanges != null)
            {
                if (RCKSettings.ROUND_COMPASS_ENABLED)
                    Compass.instance.OnQuestUpdate();

                if (RCKSettings.HORIZONTAL_COMPASS_ENABLED)
                    HorizontalCompass.instance.OnQuestUpdate();
                OnQuestStageChanges();
            }

            if(!_comesFromLoad)
                quest.allStagesCompleted.Add(_questStage.index);
        }

        public void FailQuestStage(string _questID, int _stageIndex, bool hideLog = false, bool _comesFromLoad = false)
        {
            // Needed database of quests to find the object by id
            Quest quest = RCKFunctions.GetQuest(_questID);

            if (quest)
            {
                QuestStage stage = quest.GetStage(_stageIndex);

                if (stage != null)
                    FailQuestStage(quest, stage, hideLog, _comesFromLoad);
                else
                    Debug.LogWarning("Stage: " + _stageIndex + " does not exist for the quest " + quest.questName + "ID:" + _questID);
            }
            else
                Debug.LogWarning("Quest with ID: " + _questID + " does not exist or is not included in the currently used Database File.");
        }

        public void FailQuestStage(Quest _quest, QuestStage _questStage, bool hideLog = false, bool _comesFromLoad = false)
        {
            // Find and keep the index of the quest
            Quest quest = null;
            bool questFound = false;

            for (int i = 0; i < ActiveQuests.Count; i++)
            {
                if (_quest.questID == ActiveQuests[i].questID)
                {
                    // Set the flag
                    questFound = true;
                    // Keep the reference of the quest
                    quest = ActiveQuests[i];
                    break;
                }
            }

            if (questFound)
            {
                quest.allStagesActive.Remove(_questStage.index);

                quest.GetStage(_questStage.index).stageFailed = true;

                // Update the UI with the completed quest objective
                if (!hideLog)
                    QuestAlertManager.instance.InitQuestObjectiveAlert(_questStage.description, QuestObjectiveAlertType.Failed);
            }
            else
            {
                
            }

            // Update the compass
            if (OnQuestStageChanges != null)
            {
                if (RCKSettings.ROUND_COMPASS_ENABLED)
                    Compass.instance.OnQuestUpdate();

                if (RCKSettings.HORIZONTAL_COMPASS_ENABLED)
                    HorizontalCompass.instance.OnQuestUpdate();
                OnQuestStageChanges();
            }

            if (!_comesFromLoad)
                quest.allStagesFailed.Add(_questStage.index);
        }

        // hideAlert is true when the quest is completed to recover state (like from a savegame)
        // If hideAlert is false, it means it's the first time the player actually completes the quest, if so, award the XP and set the UI
        public void CompleteQuest(Quest _quest, bool _hideAlert = false)
        {
            Quest quest = null;

            // Find the quest in the list
            for (int i = 0; i < ActiveQuests.Count; i++)
            {
                if (_quest.questID == ActiveQuests[i].questID)
                {
                    // Keep the reference of the quest
                    quest = ActiveQuests[i];
                    break;
                }
            }

            if (!_hideAlert)
            {
                // Quest completed!
                QuestAlertManager.instance.InitQuestAlert(quest.questName, false);
            }

            // Add this quest to the completed ones
            CompletedQuests.Add(quest);

            // Remove this quest from the actives one
            ActiveQuests.Remove(quest);

            // If this was the current quest, clear the reference
            if (CurrentQuest == quest)
                CurrentQuest = null;

            // Add xp, rewards and stuff...

            // Start running the QuestScript if it exists
            if (!string.IsNullOrEmpty(quest.questScript))
            {
                QuestScript qScript = (QuestScript)QuestScriptManager.instance.scriptsHolder.GetComponent(System.Type.GetType(quest.questScript));

                if(qScript != null)
                    Destroy(qScript);
            }

            if (!_hideAlert)
                GameAudioManager.instance.PlayOneShot(AudioSources.UISounds, onQuestCompletedSound);

            // Add XP
            if (!_hideAlert)
            {
                AlertMessage.instance.InitAlertMessage("+" + quest.questXpReward.ToString() + " XP.", 1.5f);
                EntityAttributes.PlayerAttributes.GainXP(quest.questXpReward);
            }
        }

        // Called when the current quest changes by the QuestManagerUI, used to display the current objective
        public void OnCurrentQuestChanges()
        {
            if (CurrentQuest != null)
                QuestAlertManager.instance.DisplayCurrentQuestObjective(false);


            // Update the compass
            if (OnQuestStageChanges != null)
            {
                if (RCKSettings.ROUND_COMPASS_ENABLED)
                    Compass.instance.OnQuestUpdate();

                if (RCKSettings.HORIZONTAL_COMPASS_ENABLED)
                    HorizontalCompass.instance.OnQuestUpdate();
                OnQuestStageChanges();
            }
        }

        public Quest GetQuestRef(string _questID)
        {
            for (int i = 0; i < ActiveQuests.Count; i++)
            {
                if (_questID == ActiveQuests[i].questID)
                    return ActiveQuests[i];
            }

            return null;
        }

        public int GetStage(string _questID)
        {
            for(int i = 0; i < ActiveQuests.Count; i++)
            {
                if(ActiveQuests[i].questID == _questID)
                    return ActiveQuests[i].currentQuestStage;
            }

            for (int i = 0; i < CompletedQuests.Count; i++)
            {
                if (CompletedQuests[i].questID == _questID)
                    return CompletedQuests[i].currentQuestStage;
            }

            return 0;
        }

        public int GetStageCompleted(string _questID, int _stage)
        {
            Quest quest = GetQuestRef(_questID);

            if (quest != null)
            {
                if (quest.allStagesCompleted.Contains(_stage))
                    return 1;
                else
                    return 0;
            }
            else
                Debug.Log("QUEST NULL");

            return 0;
        }

        public int GetStageFailed(string _questID, int _stage)
        {
            Quest quest = GetQuestRef(_questID);

            if (quest != null)
            {
                if (quest.allStagesFailed.Contains(_stage))
                    return 1;
                else
                    return 0;
            }

            return 0;
        }

        /// <summary>
        /// Returns if a given quest is completed or not.
        /// </summary>
        /// <param name="_questID"></param>
        /// <returns></returns>
        public bool IsQuestCompleted(string _questID)
        {
            for (int i = 0; i < CompletedQuests.Count; i++)
            {
                if (CompletedQuests[i].questID == _questID)
                    return true;
            }

            return false;
        }


        public void LoadQuestDataFromSavefile()
        {
            var questData = SaveSystemManager.instance.saveFile.QuestData;

            // Active quests
            foreach (KeyValuePair<string, QuestData> quest in questData.allActiveQuests)
            {
                var addedQuest = AddQuest(quest.Key, 0, true, true);

                addedQuest.questScriptExecutionDelay = quest.Value.questScriptExecutionDelay;
                addedQuest.questCompleted = quest.Value.questCompleted;
                addedQuest.questFailed = quest.Value.questFailed;

                addedQuest.currentQuestStage = quest.Value.currentQuestStage;
                addedQuest.allStagesCompleted = quest.Value.allStagesCompleted;
                addedQuest.allStagesActive = quest.Value.allStagesActive;
                addedQuest.allStagesFailed = quest.Value.allStagesFailed;

                for(int i = 0; i < addedQuest.allStagesActive.Count; i++)
                    SetQuestStage(addedQuest.questID, addedQuest.allStagesActive[i], true, true);

                for (int i = 0; i < addedQuest.allStagesFailed.Count; i++)
                    FailQuestStage(addedQuest.questID, addedQuest.allStagesFailed[i], true, true);

                for (int i = 0; i < addedQuest.allStagesCompleted.Count; i++)
                    CompleteQuestStage(addedQuest.questID, addedQuest.allStagesCompleted[i], true, true);
            }

            // Completed quests
            foreach (KeyValuePair<string, QuestData> quest in questData.allCompletedQuests)
            {
                var addedQuest = AddQuest(quest.Key, quest.Value.currentQuestStage, true, true, true);
                addedQuest.questScriptExecutionDelay = quest.Value.questScriptExecutionDelay;
                addedQuest.questCompleted = quest.Value.questCompleted;
                addedQuest.questFailed = quest.Value.questFailed;

                addedQuest.currentQuestStage = quest.Value.currentQuestStage;
                addedQuest.allStagesCompleted = quest.Value.allStagesCompleted;

                CompleteQuest(addedQuest, true);
            }

            if (!string.IsNullOrEmpty(questData.currentActiveQuestID))
            {
                CurrentQuest = ActiveQuests.Find(x => x.questID == questData.currentActiveQuestID);
                OnCurrentQuestChanges();
            }
            else
                CurrentQuest = null;
        }

        public QuestsData ToSaveData()
        {
            QuestsData newData = new QuestsData();

            // Fill/Update ActiveQuests
            for (int i = 0; i < ActiveQuests.Count; i++)
            {
                var quest = ActiveQuests[i];
                QuestData questData = quest.ToSaveData();

                // Update the dictionary

                // If the quest was already here
                if (newData.allActiveQuests.ContainsKey(quest.questID))
                    newData.allActiveQuests[quest.questID] = questData;
                else
                    newData.allActiveQuests.Add(quest.questID, questData);
            }

            // FIll/Update CompletedQuests
            for (int i = 0; i < CompletedQuests.Count; i++)
            {
                var quest = CompletedQuests[i];
                QuestData questData = quest.ToSaveData();

                // Update the dictionary

                // If the quest was already here
                if (newData.allCompletedQuests.ContainsKey(quest.questID))
                    newData.allCompletedQuests[quest.questID] = questData;
                else
                    newData.allCompletedQuests.Add(quest.questID, questData);
            }

            // Save active quest if any
            if (CurrentQuest != null)
                newData.currentActiveQuestID = CurrentQuest.questID;
            else
                newData.currentActiveQuestID = string.Empty;

            return newData;
        }

    }
}
