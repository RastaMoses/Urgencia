using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit.Game.ResultScripts
{
    public class Goto_TriggerFailFrogResultScript : ResultScript
    {
        void Start()
        {
            // Your code here
            if (RCKFunctions.GetStage("SQ_DealingHealingCrystals") == 40) { RckPlayer.instance.DisplayHeardLine("I don't think Pellan has a lot more time...", 5f); }
            var gotoComp = GetComponent<Goto>();
            if (gotoComp != null) { gotoComp.AllowMultipleOnEnterTriggering = false; }
            // Destroy the script
            Destroy(this);
        }
    }
}