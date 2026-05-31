using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.AI;

namespace RPGCreationKit.Game.ResultScripts
{
    public class DisablePellanResultScript : ResultScript
    {
        void Start()
        {
            if (RCKFunctions.GetStage("MQ_Gratulate") != 40) { Destroy(this);  return; }
            RckAI pellan;
            // Your code here
            CellInformation.TryToGetAI("Pellan001", out pellan);
            if (pellan != null) 
            {
                pellan.transform.position = new Vector3(-1, 1, 25);
                pellan.SetNewBehaviourTree(false, "BTP_EmptyStatePurpose");
                pellan.SwitchBehaviourTree(false);
            }

            var allAI = SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiDictionary;

            if (allAI.ContainsKey("Pellan001"))
            {
                SaveSystem.AISaveData mackData = allAI["Pellan001"];
                mackData.purposeBehaviourTreeID = "BTP_EmptyStatePurpose";
                mackData.curBehaviourTreeID = "BTP_EmptyStatePurpose";
                mackData.position = new Vector3(-1, 1, 25);
            }
            // Destroy the script
            Destroy(this);
        }
    }
}