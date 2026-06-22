using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.Game.ResultScripts
{
    public class GOTO_TriggerMultipleQuestsPopup : ResultScript
    {
        void Start()
        {
            // Your code here
            TutorialAlertMessage.instance.OpenMessage("You have multiple quests active at the same time.\n\n\nTo track them you can open your journal with J.\n");
            RCKFunctions.MutateMutable("MutableTriggerMultipleQuests", true);

            // Destroy the script
            Destroy(this);
        }
    }
}