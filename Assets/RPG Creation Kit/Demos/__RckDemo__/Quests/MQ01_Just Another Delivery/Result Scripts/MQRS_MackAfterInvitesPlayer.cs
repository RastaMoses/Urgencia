using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit.Game.ResultScripts
{
    public class MQRS_MackAfterInvitesPlayer : ResultScript
    {
        void Start()
        {
            // Your code here
            RCKFunctions.UnlockDoor("MainDoorToCityExterior");

            // Send Mack to his house (It's safe to assume Mack is loaded)

            RckAI mack = null;
            CellsSystem.CellInformation.TryToGetAI("Mack001", out mack);
            
            mack.purposeState.ClearPurpose();

            mack.Movements_SwitchToWalk();
            mack.SetNewBehaviourTree(false, "BTP_FollowMainTargetOnly");
            mack.SwitchBehaviourTree(false);

            mack.MoveToCell("CityMackHouse", new Vector3(39.6f, 0.4f, 12.416f), Quaternion.Euler(0, -30.74f, 0));


            mack.AssignPurpose(mack, mack.mainTarget.gameObject, PurposeClearTypes.ClearOnTeleportToCell, new PurposeStateClearsOnData("CityMackHouse"));
            
            // Destroy the script
            Destroy(this);
        }
    }
}