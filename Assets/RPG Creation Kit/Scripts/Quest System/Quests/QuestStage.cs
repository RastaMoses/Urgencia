using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;
using RPGCreationKit;

namespace RPGCreationKit
{

    /// <summary>
    /// ScriptableObject for creating QuestObjectives for quests
    /// </summary>
    [System.Serializable]
    public class QuestStage 
    {
        [Space(5)]
        [Header("Quest Objective settings")]

        public int index = 0;
        [TextArea] public string description = "Insert the quest objective description.";

        public Condition[] stageConditions;
        public string resultScript;

        public bool completeQuest;
        public bool failQuest;

        public bool displayLogEntry = true;

        [HideInInspector] public bool stageCompleted = false;
        [HideInInspector] public bool stageFailed = false;

    }
}