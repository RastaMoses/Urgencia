using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSQL_OneStepForward_VeraEquipsMeleeAndFollow : ResultScript
    {
        void Start()
        {
            // Your code here

            // Get Vera, she is loaded
            RckAI vera = null;
            CellsSystem.CellInformation.TryToGetAI("Vera001", out vera);

            // Set Vera's behaviour
            vera.SetTarget(Player.RckPlayer.instance.gameObject);

            vera.SetNewBehaviourTree(false, "BTP_FollowerDynamic");
            vera.SwitchBehaviourTree(false);

            // The purpose will be cleared manually
            vera.AssignPurpose(vera, Player.RckPlayer.instance.gameObject, PurposeClearTypes.Undefined, null);


            // Make Vera vision less far away - RESTORED IN RSQL_OneStepForward_MartinFinishesTalkingToGroupAssault
            vera.radius = 40;
            vera.sphereForwardOffset = 38.78f;

            // Give the player bow and arrows
            Inventory.PlayerInventory.AddItem("IronBow001", 1);
            Inventory.PlayerInventory.AddItem("SteelArrowAmmo", 25);

            AlertMessage.instance.InitAlertMessage("Iron Bow added!", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
            AlertMessage.instance.InitAlertMessage("25 Steel Arrow added!", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);


            // Enable the Mutables Mutable_MartinSpawns Mutable_PlayerSkipsMartinDialogue
            RCKFunctions.MutateMutable("Mutable_MartinSpawns", false);

            // Destroy the script
            Destroy(this);
        }
    }
}