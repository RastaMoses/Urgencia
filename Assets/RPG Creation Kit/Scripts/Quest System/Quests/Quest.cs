using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.SaveSystem;

namespace RPGCreationKit
{
    /// <summary>
    /// ScriptableObject for creating quests
    /// </summary>
    [CreateAssetMenu(fileName = "New Quest", menuName = "RPG Creation Kit/Quests/New Quest", order = 0)]
    public class Quest : ScriptableObject
    {
        public bool _IS_DEMO_FILE = true;

        // The ID
        public string questID = ""; 
        public string questName = "New Quest";

        [Space(5)]

        [TextArea] public string questDescription = "Insert the quest description...";

        // List of Objectives
        public List<QuestStage> questStages;
        public List<int> allStagesActive = new List<int>();
        public List<int> allStagesCompleted = new List<int>();
        public List<int> allStagesFailed = new List<int>();

        [Space(5)]

        public int questPriority = 50;
        public string questScript;

        [Space(5)]

        public bool allowRepeatedStages = false;
        public bool firstStageIsDefaultStage = true;

        public float questScriptExecutionDelay = 5.0f;

        // The current QuestObjective index.
        [HideInInspector] public int currentQuestStage = 0;

        [HideInInspector] public bool questCompleted = false;
        [HideInInspector] public bool questFailed = false;

        public Condition[] questConditions;

        // XP
        public ulong questXpReward;

        // Get QuestStage from stage index
        public QuestStage GetStage(int _stageIndex)
        {
            for(int i = 0; i < questStages.Count; i++)
            {
                if (questStages[i].index == _stageIndex)
                    return questStages[i];
            }

            return null;
        }

        public List<QuestStage> GetAllCompletedStages()
        {
            List<QuestStage> completedStages = new List<QuestStage>();

            for(int i = questStages.Count - 1; i >= 0; i--)
            {
                if (questStages[i].stageCompleted)
                    completedStages.Add(questStages[i]);
            }

            return completedStages;
        }

        public List<int> GetAllCompletedStagesAsInt()
        {
            return allStagesCompleted;
        }

        public List<int> GetAllFailedStagesAsInt()
        {
            return allStagesFailed;
        }

        public QuestData ToSaveData()
        {
            QuestData newData = new QuestData()
            {
                currentQuestStage = currentQuestStage,
                questCompleted = questCompleted,
                questFailed = questFailed,
                questScriptExecutionDelay = questScriptExecutionDelay,
                allStagesCompleted = allStagesCompleted,
                allStagesActive = allStagesActive,
                allStagesFailed = allStagesFailed
            };

            return newData;
        }
    }
}
