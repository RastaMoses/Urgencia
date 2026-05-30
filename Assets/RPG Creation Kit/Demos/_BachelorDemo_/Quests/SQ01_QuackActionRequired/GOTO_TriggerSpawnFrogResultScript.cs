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
        void Start()
        {
            // Your code here
            if (RCKFunctions.GetStage("SQ_QuackActionRequired") == 60)
            {

                RCKFunctions.MutateMutable("Mutable_FrogQuestFail", false);
                CellInformation.TryToGetAI("Pellan001", out RckAI pellan);
                if (pellan != null) { pellan.DestroyThis(); }
                //if (pellan != null) { RCKFunctions.SpawnAIInCell("Frog001", "TomsTavern", new Vector3(6.59600019f, 0.737999976f, 5.1079998f), new Quaternion(1.40390298e-07f, -0.72203207f, -8.20759638e-09f, 0.691859663f)); }
                var gotoComp = GetComponent<Goto>();
                if (gotoComp != null) { gotoComp.AllowMultipleOnEnterTriggering = false; }
            }

            // Destroy the script
            Destroy(this);
        }
    }
}