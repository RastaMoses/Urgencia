using RPGCreationKit;
using RPGCreationKit.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class DiaryTriggerResultScript : ResultScript
    {
        void Start()
        {
            if (RCKFunctions.GetStage("MQ_LateNightEarlyMorning") != 40 || RCKFunctions.IsQuestCompleted("MQ_LateNightEarlyMorning") == true) { Destroy(this); return; }
            // Your code here
            InGameHelpUI.instance.TriggerDiaryHelp();
            

            // Destroy the script
            Destroy(this);
        }
    }
}