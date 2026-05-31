using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class TutorialBandit001SayLine : ResultScript
    {
        void Start()
        {
            // Your code here
            RckAI bandit = null;
            CellInformation.TryToGetAI("BanditInFirstDungeon001", out bandit);

            if (bandit != null)
            {
                if (!bandit.isInCombat && !bandit.isDrawingWeapon && bandit.isAlive)
                {
                    RCKFunctions.MakeAISpeakLine(bandit, "TUTORIAL_BANDIT_LINE_001");
                    RCKFunctions.DisplayHeardLine("Bandit: These fancy lookin' papers sure could be worth somethin'...", 4.5f);
                }
            }

            RCKFunctions.MutateMutable("Mutable_TutorialDungeonBanditSaysLine", false);

            InGameHelpUI.instance.TriggerSneakAttackHelp();

            // Destroy the script
            Destroy(this);
        }
    }
}
