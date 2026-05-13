using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class QuestScript : MonoBehaviour
    {
        public Quest quest; // Automatically assigned when the quest script is added in the scene
        [HideInInspector] public float lastIntervalRate = 0.0f;

        public virtual void RunQuestScript()
        {
            lastIntervalRate = quest.questScriptExecutionDelay;
            InvokeRepeating("CustomUpdate", 0, quest.questScriptExecutionDelay);
        }

        public virtual void CustomUpdate()
        {
            // If the interval rate is changed in the Quest, run this script at the selected delay.
            if (lastIntervalRate != quest.questScriptExecutionDelay)
            {
                CancelInvoke("CustomUpdate");

                if (lastIntervalRate > 0) // Setting the questScriptExecutionDelay to 0 means stop the script
                {
                    lastIntervalRate = quest.questScriptExecutionDelay;
                    InvokeRepeating("CustomUpdate", 0, quest.questScriptExecutionDelay);
                }
            }
        }
    }
}