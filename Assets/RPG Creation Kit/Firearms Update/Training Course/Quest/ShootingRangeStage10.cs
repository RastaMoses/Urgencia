using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Player;

namespace RPGCreationKit.Quests
{
    public class ShootingRangeStage10 : QuestStageScript
    {
        private void Start()
        {
            TimeOfDayManager.instance.SetTime(12.0f);

            // Your code here
            var helmet = Inventory.PlayerInventory.AddItem("IronHelmet001", 1, false);
            var armor = Inventory.PlayerInventory.AddItem("IronCuriass001", 1, false);
            var pants = Inventory.PlayerInventory.AddItem("IronPants001", 1, false);
            var boots = Inventory.PlayerInventory.AddItem("IronBoots001", 1, false);

            Equipment.PlayerEquipment.Equip(helmet);
            Equipment.PlayerEquipment.Equip(armor);
            Equipment.PlayerEquipment.Equip(pants);
            Equipment.PlayerEquipment.Equip(boots);

            Equipment.PlayerEquipment.OnEquipmentChanges();
            PlayerCombat.instance.OnEquipmentChanges();
            ThirdPersonPlayer.instance.OnEquipmentChangesHands();
            PlayerInInventory.instance.OnEquipmentChangesHands();

            EntityAttributes.PlayerAttributes.attributes.Constitution = 50;

            EntityAttributes.PlayerAttributes.derivedAttributes.CalculateFromAttributes(EntityAttributes.PlayerAttributes.attributes);
            EntityAttributes.PlayerAttributes.CurHealth = EntityAttributes.PlayerAttributes.MaxHealth;

            RckPlayer.instance.recoverStaminaAmount = 45.0f;
            RckPlayer.instance.recoverAfterActionDelay = 1.25f;

            RckPlayer.instance.recoverHealthAmount = 50.0f;
            RckPlayer.instance.recoverAfterHitDelay = 3.0f;

            RckPlayer.instance.UpdateHealthStaminaGUI();

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }
    }
}