using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Game.ResultScripts
{
    public class TavernAlbertHenryStartDiscussion : ResultScript
    {
        void Start()
        {
            // Your code here
            RckAI henry = null;
            RckAI albert = null;

            CellInformation.TryToGetAI("HenryLowier001", out henry);
            CellInformation.TryToGetAI("AlbertNicetius001", out albert);

            henry.EnterInCombatAgainst(albert);
            albert.EnterInCombatAgainst(henry);

            henry.dialogueSystemEnabled = true;

            henry.ChangeDialogueGraph("HenryAfterDiscussion");

            // Destroy the script
            Destroy(this);
        }
    }
}