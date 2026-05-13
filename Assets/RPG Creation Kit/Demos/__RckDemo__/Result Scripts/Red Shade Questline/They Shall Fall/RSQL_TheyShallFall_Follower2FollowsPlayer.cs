using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSQL_TheyShallFall_Follower2FollowsPlayer : ResultScript
    {
        void Start()
        {
            // Your code here
            RckAI follower2 = null;
            CellInformation.TryToGetAI("TheyShallFall_RedShadeFollower2", out follower2);

            if (follower2 != null)
            {
                follower2.purposeState.ClearPurpose();

                // Set Vera's behaviour
                follower2.SetTarget(Player.RckPlayer.instance.gameObject);

                follower2.SetNewBehaviourTree(false, "BTP_FollowerDynamic");
                follower2.SwitchBehaviourTree(false);

                follower2.ChangeDialogueGraph("DIALOGUE_RedShadeFollowerDefaultDialogueMale");

                // The purpose will be cleared manually
                follower2.AssignPurpose(follower2, Player.RckPlayer.instance.gameObject, PurposeClearTypes.Undefined, null);

                follower2.followTargetOutsideOfCell = true;
            }

            // Destroy the script
            Destroy(this);
        }
    }
}