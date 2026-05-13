using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Player;

namespace RPGCreationKit.Quests
{
    public class UntotenStage10 : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            SetArmor();
            SetAttributes();

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }

        private void SetArmor()
        {
            var helmet = Inventory.PlayerInventory.AddItem("IronHelmet001", 1, false);
            var armor = Inventory.PlayerInventory.AddItem("IronCuriass001", 1, false);
            //var gloves = Inventory.PlayerInventory.AddItem("IronGloves001", 1, false);
            var pants = Inventory.PlayerInventory.AddItem("IronPants001", 1, false);
            var boots = Inventory.PlayerInventory.AddItem("IronBoots001", 1, false);

            var startingAmmo = Inventory.PlayerInventory.AddItem("9mmAmmo001", 96, false);
            var pistol = Inventory.PlayerInventory.AddItem("9mmPistol001", 1, false);
            pistol.metadata.intProperty1 = ((WeaponItem)pistol.item).clipRounds;


            Equipment.PlayerEquipment.Equip(helmet);
            Equipment.PlayerEquipment.Equip(armor);
            //Equipment.PlayerEquipment.Equip(gloves);
            Equipment.PlayerEquipment.Equip(pants);
            Equipment.PlayerEquipment.Equip(boots);

            Equipment.PlayerEquipment.Equip(pistol);

            Equipment.PlayerEquipment.OnEquipmentChanges();
            PlayerCombat.instance.OnEquipmentChanges();
            ThirdPersonPlayer.instance.OnEquipmentChangesHands();
            PlayerInInventory.instance.OnEquipmentChangesHands();
        }

        private void SetAttributes()
        {
            //EntityAttributes.PlayerAttributes.MaxHealth = 100.0f;
            //EntityAttributes.PlayerAttributes.CurHealth = 100.0f;

            //EntityAttributes.PlayerAttributes.MaxStamina = 120.0f;
            //EntityAttributes.PlayerAttributes.CurStamina = 120.0f;

            RckPlayer.instance.recoverStaminaAmount = 35.0f;
            RckPlayer.instance.recoverAfterActionDelay = 1.25f;
            RckPlayer.instance.jogSpeed = 6.5f;
            RckPlayer.instance.runSpeed = 10.5f;

            RckPlayer.instance.recoverHealthAmount = 20.0f;
            RckPlayer.instance.recoverAfterHitDelay = 4.0f;

        }
    }
}