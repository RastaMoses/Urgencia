using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSQL_OneStepForward_VeraEquipsBowAndFollow : ResultScript
    {
        void Start()
        {
            // Your code here

            // Get Vera, she is loaded
            RckAI vera = null;
            CellsSystem.CellInformation.TryToGetAI("Vera001", out vera);

            vera.purposeState.ClearPurpose();

            // Set Vera's behaviour
            vera.SetTarget(Player.RckPlayer.instance.gameObject);

            vera.SetNewBehaviourTree(false, "BTP_FollowerDynamic");
            vera.SwitchBehaviourTree(false);

            // The purpose will be cleared manually
            vera.AssignPurpose(vera, Player.RckPlayer.instance.gameObject, PurposeClearTypes.Undefined, null);

            // Add and equip bow and arrows
            vera.inventory.AddItem("IronBow001", 1);
            vera.inventory.AddItem("SteelArrowAmmo", 20);

            vera.equipment.Equip("IronBow001");
            vera.equipment.Equip("SteelArrowAmmo");

            vera.equipment.OnEquipmentChanges();
            vera.OnEquipmentChangesHands();
            vera.OnEquipmentChangesAmmo();

            // Make Vera vision less far away - RESTORED IN RSQL_OneStepForward_MartinFinishesTalkingToGroupAssault
            vera.radius = 40;
            vera.sphereForwardOffset = 38.78f;

            // Enable the Mutables Mutable_MartinSpawns Mutable_PlayerSkipsMartinDialogue
            RCKFunctions.MutateMutable("Mutable_MartinSpawns", false);

            // Destroy the script
            Destroy(this);
        }
    }
}