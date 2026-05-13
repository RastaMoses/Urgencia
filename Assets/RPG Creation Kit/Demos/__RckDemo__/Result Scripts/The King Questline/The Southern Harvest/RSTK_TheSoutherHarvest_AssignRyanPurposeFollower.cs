using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSTK_TheSoutherHarvest_AssignRyanPurposeFollower : ResultScript
    {
        void Start()
        {
            // Your code here
            RckAI ryan = null;
            CellsSystem.CellInformation.TryToGetAI("TKQL_Ryan", out ryan);

            if (ryan != null)
                ryan.AssignPurpose(ryan, Player.RckPlayer.instance.gameObject, PurposeClearTypes.Undefined, null, null);

            // Destroy the script
            Destroy(this);
        }
    }
}