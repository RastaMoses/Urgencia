using RPGCreationKit;
using RPGCreationKit.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class StopGuardFollowResultScript : ResultScript
    {
        void Start()
        {
            // Your code here
            RckAI mack = null;
            CellsSystem.CellInformation.TryToGetAI("CityGuard003", out mack);

            if (mack != null)
            {
                

                mack.SetNewBehaviourTree(false, "SBTP_UseCurrentActionPoint");
                mack.SwitchBehaviourTree(false);

                // Move behind the house
                //mack.transform.position = new Vector3(30f, 0.65f, 11.67f);
            }

            // Ensure mack is saved, this happens while Mack is in offline mode so if the player saves inside the palace Mack won't be saved, so let's do it manually.

            // Try to get from savefile
            var allAI = SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiDictionary;

            if (allAI.ContainsKey("CityGuard003"))
            {
                SaveSystem.AISaveData mackData = allAI["CityGuard003"];
                mackData.purposeBehaviourTreeID = "SBTP_UseCurrentActionPoint";
                mackData.curBehaviourTreeID = "SBTP_UseCurrentActionPoint";
            }

            // Destroy the script
            Destroy(this);
        }
    }
}