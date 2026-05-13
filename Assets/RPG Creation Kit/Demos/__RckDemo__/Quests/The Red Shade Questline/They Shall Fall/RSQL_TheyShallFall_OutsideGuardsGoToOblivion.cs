using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSQL_TheyShallFall_OutsideGuardsGoToOblivion : ResultScript
    {
        void Start()
        {
            // Send Mack the guards to THE OBLIVION

            RckAI guard1 = null;
            CellsSystem.CellInformation.TryToGetAI("TheyShallFall_CityGuardsOutside1", out guard1);

            RckAI guard2 = null;
            CellsSystem.CellInformation.TryToGetAI("TheyShallFall_CityGuardsOutside2", out guard2);

            guard1.purposeState.ClearPurpose();

            guard1.Movements_SwitchToRun();
            guard1.SetNewBehaviourTree(false, "BTP_FollowMainTargetOnly");
            guard1.SwitchBehaviourTree(false);
            guard1.dialogueSystemEnabled = false;

            guard1.MoveToCell("OBLIVION_001", new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));


            guard1.AssignPurpose(guard1, guard1.mainTarget.gameObject, PurposeClearTypes.ClearOnTeleportToCell, new PurposeStateClearsOnData("OBLIVION_001"));

            guard2.purposeState.ClearPurpose();

            guard2.Movements_SwitchToRun();
            guard2.SetNewBehaviourTree(false, "BTP_FollowMainTargetOnly");
            guard2.SwitchBehaviourTree(false);
            guard2.dialogueSystemEnabled = false;

            guard2.MoveToCell("OBLIVION_001", new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));


            guard2.AssignPurpose(guard1, guard1.mainTarget.gameObject, PurposeClearTypes.ClearOnTeleportToCell, new PurposeStateClearsOnData("OBLIVION_001"));

            // Destroy the script
            Destroy(this);
        }
    }
}