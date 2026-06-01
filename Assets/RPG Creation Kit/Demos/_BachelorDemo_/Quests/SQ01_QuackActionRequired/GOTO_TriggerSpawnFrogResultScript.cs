using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class GOTO_TriggerSpawnFrogResultScript : ResultScript
    {
        RckAI frogPellan;
        void Start()
        {
            
            // Your code here
            if (RCKFunctions.GetStage("SQ_QuackActionRequired") == 60)
            {
                
                Debug.Log("TriggerSpawnFrogResultScript");
                frogPellan = RCKFunctions.SpawnAIInCurrentCell("Frog001", new Vector3(-2.45f, 0.153330505f, 17.65f), new Quaternion(1.4014681e-07f, -0.738781095f, -1.16478223e-08f, 0.673945487f));
                Debug.Log(frogPellan.transform.position);
                
                var allAI = SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiDictionary;
               
                if (allAI.ContainsKey("Frog001"))
                {
                    SaveSystem.AISaveData mackData = allAI["Frog001"];
                    mackData.purposeBehaviourTreeID = "BTP_EmptyStatePurpose";
                    mackData.curBehaviourTreeID = "BTP_EmptyStatePurpose";
                    mackData.position = new Vector3(-2.45f, 0.153330505f, 17.65f);
                }
                Debug.Log(frogPellan.transform.position);

                RckAI pellan;
                // Your code here
                CellInformation.TryToGetAI("Pellan001", out pellan);
                if (pellan != null)
                {
                    pellan.DestroyThis();
                }
                RCKFunctions.MutateMutable("Mutable_FrogQuestFail", false);
            }

            // Destroy the script
            Destroy(this);
        }
    }
}