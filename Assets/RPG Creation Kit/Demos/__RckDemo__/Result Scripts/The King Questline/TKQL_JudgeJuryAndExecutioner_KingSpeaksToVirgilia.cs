using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Game.ResultScripts
{
    public class TKQL_JudgeJuryAndExecutioner_KingSpeaksToVirgilia : ResultScript
    {
        RckAI king = null;
        RckAI virgilia = null;

        void Start()
        {
            // Your code here
            StartCoroutine(ScriptTask());
        }

        private IEnumerator ScriptTask()
        {
            Player.RckPlayer.instance.EnterInCutsceneMode();

            yield return new WaitForSeconds(0.5f);

            CellInformation.TryToGetAI("TheKing001", out king);
            CellInformation.TryToGetAI("VirgiliaValera001", out virgilia);

            king.SetSpeakerIndex(0);
            virgilia.SetSpeakerIndex(1);

            king.ChangeDialogueGraph("TKQL_JudgeJuryAndExecutioner_KingsSpeakToVirgilia01");

            IDialoguable[] ppl = { king, virgilia };

            king.DialogueLogic(ppl, king.currentDialogueGraph);

            // Wait for the dialogue to finish
            while (king.isInConversation)
                yield return null;

            king.aiLookAt.StopForcingLookAtTarget();

            yield return new WaitForSeconds(1.25f);

            Player.RckPlayer.instance.LeaveCutsceneMode();

            king.ChangeDialogueGraph("TKQL_JudgeJuryAndExecutioner_KingSpeaksBackToPlayer");
            Player.RckPlayer.instance.StartDialogue(king);


            // Destroy the script
            Destroy(this);
        }
    }
}