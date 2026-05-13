using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSTK_TheSouthernHarvest_RyanQuickDialogueAtFarmInside : ResultScript
    {
        RckAI ryan;

        void Start()
        {
            StartCoroutine(ScriptExecution());
        }

        public IEnumerator ScriptExecution()
        {
            while(GameStatus.instance.AnyLoading())
                yield return new WaitForEndOfFrame();

            RckAI ryan = null;
            CellsSystem.CellInformation.TryToGetAI("TKQL_Ryan", out ryan);

            if (ryan != null)
                ryan.dialogueSystemEnabled = false;

            yield return new WaitForSeconds(1.5f);

            // Your code here
            if (ryan != null)
                RCKFunctions.MakeAISpeakLine(ryan, "DCLIP_SPECIAL_RYAN_02");
            RCKFunctions.DisplayHeardLine("Ryan: By the Gods... look at this mess..", 2.6f);

            yield return new WaitForSeconds(2.75f);

            if (ryan != null)
                RCKFunctions.MakeAISpeakLine(ryan, "DCLIP_SPECIAL_RYAN_03");
            RCKFunctions.DisplayHeardLine("Ryan: Take a look around... we may find something.", 3.5f);

            if(RCKFunctions.GetStageCompleted("TKQL_TheSouthernHarvest", 30) == 0)
                RCKFunctions.SetQuestStage("TKQL_TheSouthernHarvest", 30);

            RCKFunctions.CompleteQuestStage("TKQL_TheSouthernHarvest", 20);

            RCKFunctions.MutateMutable("Mutable_TheSouthernHarvest_RyanDialogInside", false);

            if (ryan != null)
                ryan.dialogueSystemEnabled = true;

            // Destroy the script
            Destroy(this);
        }
    }
}