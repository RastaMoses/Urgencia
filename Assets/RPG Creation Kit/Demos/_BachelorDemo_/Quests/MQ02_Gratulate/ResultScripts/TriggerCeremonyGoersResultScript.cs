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

            //SPAWN THE CEREMONY GOER
            RckAI cg = RCKFunctions.SpawnAIInCell("CeremonyGoer001", "AcademyCityInterior", new Vector3(12f, 0.269999743f, -4.69999981f), Quaternion.Euler(0, 0, 0));
            //CellsSystem.CellInformation.TryToGetAI("CeremonyGoer001", out cg);

            cg.purposeState.ClearPurpose();

            cg.Movements_SwitchToWalk();
            cg.SetNewBehaviourTree(false, "BTP_FollowMainTargetOnly");
            cg.SwitchBehaviourTree(false);

            cg.MoveToCell("CityKingPalace", new Vector3(3.6f, 1f, -17.416f), Quaternion.Euler(0, -30.74f, 0));


            cg.AssignPurpose(cg, cg.mainTarget.gameObject, PurposeClearTypes.ClearOnTeleportToCell, new PurposeStateClearsOnData("CityKingPalace"));

            //SPAWN THE CEREMONY GOER
            cg = RCKFunctions.SpawnAIInCell("CeremonyGoer002", "AcademyCityInterior", new Vector3(-53.5999985f, 0.270001411f, -30.3999996f), Quaternion.Euler(0, 0, 0));
            //CellsSystem.CellInformation.TryToGetAI("CeremonyGoer001", out cg);

            cg.purposeState.ClearPurpose();

            cg.Movements_SwitchToWalk();
            cg.SetNewBehaviourTree(false, "BTP_FollowMainTargetOnly");
            cg.SwitchBehaviourTree(false);

            cg.MoveToCell("CityKingPalace", new Vector3(3.6f, 1f, -17.416f), Quaternion.Euler(0, -30.74f, 0));


            cg.AssignPurpose(cg, cg.mainTarget.gameObject, PurposeClearTypes.ClearOnTeleportToCell, new PurposeStateClearsOnData("CityKingPalace"));


            //SPAWN THE CEREMONY GOER
            cg = RCKFunctions.SpawnAIInCell("CeremonyGoer003", "AcademyCityInterior", new Vector3(-20.1700001f, 0.27000019f, -6.36000013f), Quaternion.Euler(0, 0, 0));
            //CellsSystem.CellInformation.TryToGetAI("CeremonyGoer001", out cg);

            cg.purposeState.ClearPurpose();

            cg.Movements_SwitchToWalk();
            cg.SetNewBehaviourTree(false, "BTP_FollowMainTargetOnly");
            cg.SwitchBehaviourTree(false);

            cg.MoveToCell("CityKingPalace", new Vector3(3.6f, 1f, -17.416f), Quaternion.Euler(0, -30.74f, 0));


            cg.AssignPurpose(cg, cg.mainTarget.gameObject, PurposeClearTypes.ClearOnTeleportToCell, new PurposeStateClearsOnData("CityKingPalace"));


            //SPAWN THE CEREMONY GOER
            cg = RCKFunctions.SpawnAIInCell("CeremonyGoer004", "AcademyCityInterior", new Vector3(-41.5099983f, 0.269999772f, -5.32000017f), Quaternion.Euler(0, 0, 0));
            //CellsSystem.CellInformation.TryToGetAI("CeremonyGoer001", out cg);

            cg.purposeState.ClearPurpose();

            cg.Movements_SwitchToWalk();
            cg.SetNewBehaviourTree(false, "BTP_FollowMainTargetOnly");
            cg.SwitchBehaviourTree(false);

            cg.MoveToCell("CityKingPalace", new Vector3(3.6f, 1f, -17.416f), Quaternion.Euler(0, -30.74f, 0));


            cg.AssignPurpose(cg, cg.mainTarget.gameObject, PurposeClearTypes.ClearOnTeleportToCell, new PurposeStateClearsOnData("CityKingPalace"));


            //SPAWN THE CEREMONY GOER
            cg = RCKFunctions.SpawnAIInCell("CeremonyGoer005", "AcademyCityInterior", new Vector3(-31.6000004f, 0.270001054f, 21.2999992f), Quaternion.Euler(0, 0, 0));
            //CellsSystem.CellInformation.TryToGetAI("CeremonyGoer001", out cg);

            cg.purposeState.ClearPurpose();

            cg.Movements_SwitchToWalk();
            cg.SetNewBehaviourTree(false, "BTP_FollowMainTargetOnly");
            cg.SwitchBehaviourTree(false);

            cg.MoveToCell("CityKingPalace", new Vector3(3.6f, 1f, -17.416f), Quaternion.Euler(0, -30.74f, 0));


            cg.AssignPurpose(cg, cg.mainTarget.gameObject, PurposeClearTypes.ClearOnTeleportToCell, new PurposeStateClearsOnData("CityKingPalace"));


            //SPAWN THE CEREMONY GOER
            cg = RCKFunctions.SpawnAIInCell("CeremonyGoer006", "AcademyCityInterior", new Vector3(27.8999996f, 0.270000279f, 25.8999996f), Quaternion.Euler(0, 0, 0));
            //CellsSystem.CellInformation.TryToGetAI("CeremonyGoer001", out cg);

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