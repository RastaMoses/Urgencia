using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSTK_TheSouthernHarvest_RyanQuickDialogueAtFarm : ResultScript
    {
        void Start()
        {
            // Your code here

            RckAI ryan = null;
            CellInformation.TryToGetAI("TKQL_Ryan", out ryan);

            if (ryan != null)
            {
                RCKFunctions.MakeAISpeakLine(ryan, "DCLIP_SPECIAL_RYAN_01");
                RCKFunctions.DisplayHeardLine("Ryan: I don't like this... it's too quiet...", 3.5f);
            }

            RCKFunctions.MutateMutable("Mutable_TheSouthernHarvest_RyanDialog", true);

            // Destroy the script
            Destroy(this);
        }
    }
}