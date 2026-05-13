using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSQL_OneStepForward_MartinFinishesTalkingToGroupAssault : ResultScript
    {
        /// <summary>
        /// CHECK RSQL_OneStepForward_PlayerApproachFortressWithoutTalkingToMartin too
        /// </summary>
        /// 



        void Start()
        {
            // Your code here

            // Progress with quest
            RCKFunctions.SetQuestStage("RSMQ_OneStepForward", 40);
            RCKFunctions.CompleteQuestStage("RSMQ_OneStepForward", 30);

            // Set new behaviours to all red shade group
            RckAI martin = null;
            CellInformation.TryToGetAI("Martin001", out martin);

            RckAI vera = null;
            CellInformation.TryToGetAI("Vera001", out vera);

            RckAI follower1 = null;
            CellInformation.TryToGetAI("RedShadeFollower001", out follower1);

            RckAI follower2 = null;
            CellInformation.TryToGetAI("RedShadeFollower002", out follower2);


            if(martin != null)
            {
                martin.SetNewBehaviourTree(false, "BTree_PReachGuardsOutpost001");
                martin.SwitchBehaviourTree(false);
                martin.dialogueSystemEnabled = false;
            }

            if (vera != null)
            {
                vera.purposeState.ClearPurpose();
                vera.SetNewBehaviourTree(false, "BTree_PReachGuardsOutpost004");
                vera.SwitchBehaviourTree(false);

                // RESTORE VERA VISION TO DEFAULT
                vera.radius = 80f;
                vera.sphereForwardOffset = 78f;
                vera.dialogueSystemEnabled = false;

                vera.attributes.derivedAttributes.runSpeed = 7;
                vera.maxSpeed = 7;
                vera.currentSpeed = 7;

                // Make Vera not essential anymore
                vera.isEssential = false;
            }

            if (follower1 != null)
            {
                follower1.SetNewBehaviourTree(false, "BTree_PReachGuardsOutpost002");
                follower1.SwitchBehaviourTree(false);
                follower1.dialogueSystemEnabled = false;
            }

            if (follower2 != null)
            {
                follower2.SetNewBehaviourTree(false, "BTree_PReachGuardsOutpost003");
                follower2.SwitchBehaviourTree(false);
                follower2.dialogueSystemEnabled = false;
            }


            // Enable the Mutables Mutable_MartinSpawns Mutable_PlayerSkipsMartinDialogue
            RCKFunctions.MutateMutable("Mutable_PlayerSkipsMartinDialogue", false);

            // Destroy the script
            Destroy(this);
        }
    }
}