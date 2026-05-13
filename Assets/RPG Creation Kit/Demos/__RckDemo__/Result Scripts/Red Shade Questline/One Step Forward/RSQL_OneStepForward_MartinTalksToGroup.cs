using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;


namespace RPGCreationKit.Game.ResultScripts
{
    public class RSQL_OneStepForward_MartinTalksToGroup : ResultScript
    {
        void Start()
        {
            // Your code here
            RckAI martin = null;
            CellInformation.TryToGetAI("Martin001", out martin);

            RckAI follower1 = null;
            CellInformation.TryToGetAI("RedShadeFollower001", out follower1);

            RckAI follower2 = null;
            CellInformation.TryToGetAI("RedShadeFollower002", out follower2);

            RckAI vera = null;
            CellInformation.TryToGetAI("Vera001", out vera);

            martin.SetSpeakerIndex(0);
            follower1.SetSpeakerIndex(1);
            follower2.SetSpeakerIndex(2);
            vera.SetSpeakerIndex(3);

            IDialoguable[] ppl = { martin, follower1, follower2, vera };

            martin.DialogueLogic(ppl, martin.currentDialogueGraph);

            // Destroy the script
            Destroy(this);
        }

        void DelayedStart()
        {
            
        }
    }
}