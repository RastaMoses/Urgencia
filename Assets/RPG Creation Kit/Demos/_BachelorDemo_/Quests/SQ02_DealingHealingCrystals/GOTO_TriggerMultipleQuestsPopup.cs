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
            TutorialAlertMessage.instance.OpenMessage("You have multiple different quests active at the same time.\nThere is still some time left until your graduation starts.\nUse it wisely!\n\n\nTo track one of the quests you can open your journal with 'J'\n");
            RCKFunctions.MutateMutable("MutableTriggerMultipleQuests", true);

            // Destroy the script
            Destroy(this);
        }
    }
}