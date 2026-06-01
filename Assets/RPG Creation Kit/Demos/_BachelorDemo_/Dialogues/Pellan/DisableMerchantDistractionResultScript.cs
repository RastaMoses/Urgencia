using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class DisableMerchantDistractionResultScript : ResultScript
    {
        void Start()
        {
            // Your code here
            if (RCKFunctions.IsQuestCompleted("LateNightEarlyMorning"));
            {
                CellInformation.TryToGetAI("Lanius001", out RckAI lanius);
                if (lanius != null) { lanius.DestroyThis(); }
                
            }
            // Destroy the script
            Destroy(this);
        }
    }
}