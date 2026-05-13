using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Game.ResultScripts
{
    public class CountTurnsHostileAgainstPlayer : ResultScript
    {
        void Start()
        {
            // Your code here

            // Load the Count AI
            RckAI count = null;
            CellInformation.TryToGetAI("CountTheveninThibault001", out count);

            Debug.Log(count);
            if (count != null)
            {
                Debug.Log("Executing script body.");
                count.EnterInCombatAgainst(Player.RckPlayer.GetPlayerEntity());
            }

            Debug.Log("Script done");

            // Destroy the script
            Destroy(this);
        }
    }
}