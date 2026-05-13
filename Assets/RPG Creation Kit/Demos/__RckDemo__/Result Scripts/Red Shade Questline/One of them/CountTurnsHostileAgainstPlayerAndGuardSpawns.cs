using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Game.ResultScripts
{
    public class CountTurnsHostileAgainstPlayerAndGuardSpawns : ResultScript
    {
        void Start()
        {
            // Your code here

            // Load the Count AI
            RckAI count = null;
            CellInformation.TryToGetAI("CountTheveninThibault001", out count);

            if (count != null)
            {
                count.EnterInCombatAgainst(Player.RckPlayer.GetPlayerEntity());

                // Spawn guards and lock doors
                RCKFunctions.SpawnAIInCurrentCell("CityGuardInstHostile001", new Vector3(-35.64f, 1.0f, 22.96f), Quaternion.Euler(0, -88.5f, 0));
                RCKFunctions.SpawnAIInCurrentCell("CityGuardInstHostile002", new Vector3(-42.47f, 1.0f, 16.74f), Quaternion.Euler(0, -13.58f, 0));

                RCKFunctions.MakeAISpeakLine(count, "DCLIP_SPECIAL_COUNT_LOCKDOORS");
                RCKFunctions.DisplayHeardLine("Count: Lock the doors! Get him!", 3.5f);

                // Lock doors
                RCKFunctions.LockDoor("CountsHouseDoor1ToCityExterior", DoorLockLevel.Impossible);
                RCKFunctions.LockDoor("CountsHouseDoor2ToCityExterior", DoorLockLevel.Impossible);
                RCKFunctions.LockDoor("CountsHouseToCountsPrivateRoom001", DoorLockLevel.Impossible);
            }

            // Destroy the script
            Destroy(this);
        }
    }
}