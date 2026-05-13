using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;


namespace RPGCreationKit
{
    /// <summary>
    /// ScriptableObject for updating quests
    /// </summary>
    [System.Serializable]
    public class QuestUpdater
    {
        [SerializeField] public Quest questToUpdate;
        public bool completed;
        public int questStage;
        public bool hideLog = false;
    }
}
