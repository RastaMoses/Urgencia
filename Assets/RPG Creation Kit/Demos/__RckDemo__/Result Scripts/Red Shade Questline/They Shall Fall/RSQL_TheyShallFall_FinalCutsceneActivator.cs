using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Player;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSQL_TheyShallFall_FinalCutsceneActivator : ResultScript
    {
        void Start()
        {
            RCKFunctions.MutateMutable("Mutable_RedShadeFinalCutscene", true);
            StartCoroutine(ScriptHandler());
        }

        IEnumerator ScriptHandler()
        {
            while (!Player.RckPlayer.instance.charController.isGrounded)
                yield return new WaitForEndOfFrame();


            // If Rck RedShadeFollowers were recruited by the player, place them in their places
            RckAI follower1 = null;
            CellInformation.TryToGetAI("TheyShallFall_RedShadeFollower1", out follower1);

            if (follower1 != null && follower1.isAlive && follower1.purposeBehaviourTree.ID == "BTP_FollowerDynamic")
            {
                follower1.SetNewBehaviourTree(false, "TheyShallFall_Follower1WaitInPalace");
                follower1.SwitchBehaviourTree(false);
                follower1.aiLookAt.isEnabled = false;
            }

            RckAI follower2 = null;
            CellInformation.TryToGetAI("TheyShallFall_RedShadeFollower2", out follower2);

            if (follower2 != null && follower2.isAlive && follower2.purposeBehaviourTree.ID == "BTP_FollowerDynamic")
            {
                follower2.SetNewBehaviourTree(false, "TheyShallFall_Follower2WaitInPalace");
                follower2.SwitchBehaviourTree(false);
                follower2.aiLookAt.isEnabled = false;
            }

            RckPlayer.instance.EnterInCutsceneMode();

            yield return new WaitForSeconds(3f);

            int veraWasAlive = RCKFunctions.IsAIAlive("Vera001");

            // Send original Vera and Martin into oblivion
            RCKFunctions.SendIntoOblivion("Martin001");
            RCKFunctions.SendIntoOblivion("Vera001");

            // Spawn post-game vera and martin if vera was alive
            var newMartin = RCKFunctions.SpawnAIInCurrentCell("Martin_RedShadePostGame", new Vector3(4.05f, 0f, -11.19f), Quaternion.Euler(-0.393f, 182.406f, 0f));
            newMartin.aiLookAt.isEnabled = false;


            RckAI newVera = null;
            if (veraWasAlive != 0)
            {
                newVera = RCKFunctions.SpawnAIInCurrentCell("Vera_RedShadePostGame", new Vector3(5.95f, 0f, -11.19f), Quaternion.Euler(-0.393f, 182.406f, 0f));
                newVera.aiLookAt.isEnabled = false;
            }

            yield return new WaitForSeconds(2f);

            RCKFunctions.MakeAISpeakLine(newMartin, "DCLIP_SPECIAL_MARTIN_001");
            RCKFunctions.DisplayHeardLine("Martin: Finally. The place I deserve.", 3.5f);

            yield return new WaitForSeconds(4.5f);

            RCKFunctions.MakeAISpeakLine(newMartin, "DCLIP_SPECIAL_MARTIN_002");
            RCKFunctions.DisplayHeardLine("Martin: After all this time, it's mine..", 4f);

            yield return new WaitForSeconds(4.5f);

            RCKFunctions.MakeAISpeakLine(newMartin, "DCLIP_SPECIAL_MARTIN_003");
            RCKFunctions.DisplayHeardLine("Martin: And they now burn...", 3f);

            yield return new WaitForSeconds(3.5f);

            yield return new WaitForSeconds(4f);

            // Change Martin's and Vera's dialogues

            newMartin.agent.enabled = false;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            newMartin.agent.enabled = true;

            if(veraWasAlive != 0)
            {
                newVera.agent.enabled = false;
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                newVera.agent.enabled = true;
            }


            RckPlayer.instance.LeaveCutsceneMode();

            if(follower1 != null)
                follower1.dialogueSystemEnabled = false;
            
            if(follower2 != null)
                follower2.dialogueSystemEnabled = false;
            
            newMartin.aiLookAt.isEnabled = true;

            if(newVera != null)
                newVera.aiLookAt.isEnabled = true;


            // Update quest
            RCKFunctions.SetQuestStage("RSQL_TheyShallFall", 40);
            RCKFunctions.CompleteQuestStage("RSQL_TheyShallFall", 30);

            Destroy(this);
        }
    }
}