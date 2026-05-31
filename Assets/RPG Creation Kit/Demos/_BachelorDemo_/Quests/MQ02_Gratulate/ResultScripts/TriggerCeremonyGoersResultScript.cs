using RPGCreationKit;
using RPGCreationKit.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class TriggerCeremonyGoersResultScript : ResultScript
    {
        void Start()
        {
            if (RCKFunctions.GetStage("MQ_Gratulate") != 20)
            {
                Destroy(this);
                return;
            }
            // Your code here
            RCKFunctions.UnlockDoor("CityInteriorToKingsPalace");

            // Send Mack to his house (It's safe to assume Mack is loaded)

            RckAI cg = null;
            CellsSystem.CellInformation.TryToGetAI("CeremonyGoer001", out cg);

            cg.purposeState.ClearPurpose();

            cg.Movements_SwitchToWalk();
            cg.SetNewBehaviourTree(false, "BTP_FollowMainTargetOnly");
            cg.SwitchBehaviourTree(false);

            cg.MoveToCell("CityKingPalace", new Vector3(3.6f, 1f, -17.416f), Quaternion.Euler(0, -30.74f, 0));


            cg.AssignPurpose(cg, cg.mainTarget.gameObject, PurposeClearTypes.ClearOnTeleportToCell, new PurposeStateClearsOnData("CityKingPalace"));

            // Destroy the script
            Destroy(this);
        }
    }
}