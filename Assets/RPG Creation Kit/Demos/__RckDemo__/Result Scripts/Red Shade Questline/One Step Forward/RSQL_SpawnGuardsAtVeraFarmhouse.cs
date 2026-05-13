using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSQL_SpawnGuardsAtVeraFarmhouse : ResultScript
    {
        void Start()
        {
            // Your code here
            RckAI vera = null;
            CellsSystem.CellInformation.TryToGetAI("Vera001", out vera);

            if(vera != null)
            {
                // Prevent Vera from being talkable
                vera.dialogueSystemEnabled = false;
            }

            // Set the player in the Red Shade and lock the city gate
            Player.RckPlayer.instance.AddToFaction("RedShade");
            RCKFunctions.LockDoor("MainDoorToCityInterior", CellsSystem.DoorLockLevel.Impossible);

            RCKFunctions.SpawnAIInCurrentCell("GuardInstVeraFarmHouse01", new Vector3(9.76f, 1.0f, 231.1f), Quaternion.Euler(0, -7.5f, 0));
            var guardAI = RCKFunctions.SpawnAIInCurrentCell("GuardInstVeraFarmHouse02", new Vector3(4.27f, 1.0f, 228.94f), Quaternion.Euler(0, -7.5f, 0));

            RCKFunctions.MakeAISpeakLine(guardAI, "DCLIP_SPECIAL_GUARD_KILLHIM");
            RCKFunctions.DisplayHeardLine("Guard: He works for them! Kill him!", 4f);

            // Destroy the script
            Destroy(this);
        }
    }
}