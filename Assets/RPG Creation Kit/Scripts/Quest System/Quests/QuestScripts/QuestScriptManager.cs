using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class QuestScriptManager : MonoBehaviour
    {
        #region singleton
        public static QuestScriptManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Debug.LogError("Anomaly detected with the singleton pattern of 'QuestScriptManager', are you using multiple 'QuestScriptManager'?");
        }
        #endregion

        // Where the quest scripts needs to be assigned as components (Abstract -> QuestScripts)
        public GameObject scriptsHolder;
    }
}