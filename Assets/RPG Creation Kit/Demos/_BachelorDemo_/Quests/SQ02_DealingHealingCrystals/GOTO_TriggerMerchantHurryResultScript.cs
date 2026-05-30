using RPGCreationKit;
using RPGCreationKit.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class GOTO_TriggerMerchantHurryResultScript : ResultScript
    {
        void Start()
        {
            // Your code here
            if (RCKFunctions.GetStage("SQ_QuackActionRequired") == 40) { RckPlayer.instance.DisplayHeardLine("That merchant won't be there for long...", 5f); }
            var gotoComp = GetComponent<Goto>();
            if (gotoComp != null) { gotoComp.AllowMultipleOnEnterTriggering = false; }
            // Destroy the script
            Destroy(this);
        }
    }
}