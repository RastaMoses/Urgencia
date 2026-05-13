using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RPGCreationKit.SaveSystem
{
    [Serializable]
    public class QuestIDQuestDataDictionary : SerializableDictionary<string, QuestData> { }

    /// <summary>
    /// Represents a single quest data saved in the game
    /// </summary>
    [Serializable]
    public class QuestData
    {
        public float questScriptExecutionDelay;

        public int currentQuestStage;
        public bool questCompleted;
        public bool questFailed;

        public List<int> allStagesActive = new List<int>();
        public List<int> allStagesCompleted = new List<int>();
        public List<int> allStagesFailed = new List<int>();
    }

    /// <summary>
    /// Represents and contains all the Quests saved in the game.
    /// </summary>
    [Serializable]
    public class QuestsData
    {
        public string currentActiveQuestID;
        public QuestIDQuestDataDictionary allActiveQuests = new QuestIDQuestDataDictionary();

        public QuestIDQuestDataDictionary allCompletedQuests = new QuestIDQuestDataDictionary();

    }
}