using RPGCreationKit;
using RPGCreationKit.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class Goto_TriggerLate : ResultScript
    {
        void Start()
        {
            // Your code here
            if (RCKFunctions.GetStage("SQ_QuackActionRequired") == 20 || RCKFunctions.GetStage("SQ_DealingHealingCrystals") == 20) 
            { 
                RckPlayer.instance.DisplayHeardLine("Such a stressful day again...I gotta hurry up!", 5f);
                var gotoComp = GetComponent<Goto>();
                if (gotoComp != null) { gotoComp.AllowMultipleOnEnterTriggering = false; }
            }

            // Destroy the script
            Destroy(this);
        }
    }
}