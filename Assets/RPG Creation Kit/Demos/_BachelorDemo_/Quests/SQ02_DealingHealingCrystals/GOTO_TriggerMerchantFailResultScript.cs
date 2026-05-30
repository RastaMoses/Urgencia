using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.Game.ResultScripts
{
    public class GOTO_TriggerMerchantFailResultScript : ResultScript
    {
        void Start()
        {
            // Your code here
            if (RCKFunctions.GetStage("SQ_DealingHealingCrystals") == 40 && RCKFunctions.GetStage("SQ_QuackActionRequired") < 60)
            {
                RCKFunctions.FailQuestStage("SQ_QuackActionRequired", RCKFunctions.GetStage("SQ_QuackActionRequired"));
                RCKFunctions.SetQuestStage("SQ_QuackActionRequired", 60);

            }
            if (RCKFunctions.GetStage("SQ_QuackActionRequired") == 50 && RCKFunctions.GetQuest("SQ_DealingHealingCrystals").currentQuestStage < 60)
            {
                RCKFunctions.FailQuestStage("SQ_DealingHealingCrystals", RCKFunctions.GetStage("SQ_DealingHealingCrystals"));
                RCKFunctions.SetQuestStage("SQDealingHealingCrystals", 50);

            }
            

            // Destroy the script
            Destroy(this);
        }
    }
}