using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSQL_TheyShallFall_Follower1FollowsPlayer : ResultScript
    {
        void Start()
        {
            // Your code here
            RckAI follower1 = null;
            CellInformation.TryToGetAI("TheyShallFall_RedShadeFollower1", out follower1);

            if(follower1 != null)
            {
                follower1.purposeState.ClearPurpose();

                // Set Vera's behaviour
                follower1.SetTarget(Player.RckPlayer.instance.gameObject);

                follower1.SetNewBehaviourTree(false, "BTP_FollowerDynamic");
                follower1.SwitchBehaviourTree(false);

                follower1.ChangeDialogueGraph("DIALOGUE_RedShadeFollowerDefaultDialogueMale");

                // The purpose will be cleared manually
                follower1.AssignPurpose(follower1, Player.RckPlayer.instance.gameObject, PurposeClearTypes.Undefined, null);

                follower1.followTargetOutsideOfCell = true;
            }

            // Destroy the script
            Destroy(this);
        }
    }
}