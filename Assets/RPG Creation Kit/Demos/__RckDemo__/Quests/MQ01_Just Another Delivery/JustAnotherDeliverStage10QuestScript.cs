using NUnit.Framework.Internal;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Quests;
using RPGCreationKit.SaveSystem;
using UnityEngine;

namespace RPGCreationKit.Quests
{
    public class JustAnotherDeliverStage10QuestScript : QuestStageScript
    {
        private void Start()
        {
            // Your code here
            TutorialAlertMessage.instance.OpenMessage("Thank you for downloading the RPG Creation Kit Demo!\n\nYou can view the commands in the Settings.\n\nThis Demo features a branching storyline, I recommend playing with both factions and to not miss secondary quests in order to have a proper overview of what can be done with the RPG Creation Kit.\n\nThank you again, I hope you will enjoy this demo.");

            // Deactivate Mutable Goto so this can never be triggered again
            MutateGoto();

            // Add Quest Item to PlayerInventory
            Inventory.PlayerInventory.AddItem("QI_SealedLetterForKing001", 1, false);
            
            var pots = Inventory.PlayerInventory.AddItem("HealthPot001", 10, false);
            Inventory.PlayerInventory.AddItem("Gold001", 150, false);
            //var torch = Inventory.PlayerInventory.AddItem("ITorch001", 1, false);

            // Set inventory and equipment in base of selected class
            switch (SaveSystemManager.instance.saveFile.PlayerData.selectedClass)
            {
                case 0: // warrior
                    //var wHelmet = Inventory.PlayerInventory.AddItem("IronHelmet001", 1, false);
                    var wCuriass = Inventory.PlayerInventory.AddItem("IronCuriass001", 1, false);
                    //var wGloves = Inventory.PlayerInventory.AddItem("IronGloves001", 1, false);
                    var wPants = Inventory.PlayerInventory.AddItem("IronPants001", 1, false);
                    //var wBoots = Inventory.PlayerInventory.AddItem("IronBoots001", 1, false);
                    var wBoots = Inventory.PlayerInventory.AddItem("PriestsShoes001", 1, false);
                    //var wShield = Inventory.PlayerInventory.AddItem("IRoundShield001", 1, false);
                    //var wAxe = Inventory.PlayerInventory.AddItem("IronAxe001", 1, false);

                    //Equipment.PlayerEquipment.Equip(wHelmet);
                    Equipment.PlayerEquipment.Equip(wCuriass);
                    //Equipment.PlayerEquipment.Equip(wGloves);
                    Equipment.PlayerEquipment.Equip(wPants);
                    Equipment.PlayerEquipment.Equip(wBoots);

                    //Equipment.PlayerEquipment.Equip(wAxe);
                    //Equipment.PlayerEquipment.Equip(wShield);

                    Equipment.PlayerEquipment.OnEquipmentChanges();
                    PlayerCombat.instance.OnEquipmentChanges();
                    PlayerInInventory.instance.OnEquipmentChangesHands();
                    break;

                case 1: // ranger
                    var rHelmet = Inventory.PlayerInventory.AddItem("LeatherCape001", 1, false);
                    var rCuriass = Inventory.PlayerInventory.AddItem("LeatherCuriass001", 1, false);
                    var rGloves = Inventory.PlayerInventory.AddItem("LeatherGloves001", 1, false);
                    var rPants = Inventory.PlayerInventory.AddItem("LeatherPants001", 1, false);
                    var rBoots = Inventory.PlayerInventory.AddItem("LeatherBoots001", 1, false);
                    //var rAmmo = Inventory.PlayerInventory.AddItem("SteelArrowAmmo", 50, false);
                    //var rBow = Inventory.PlayerInventory.AddItem("IronBow001", 1, false);
                    //var rDagger = Inventory.PlayerInventory.AddItem("SilverDagger001", 1, false);

                    //Equipment.PlayerEquipment.Equip(rHelmet);
                    Equipment.PlayerEquipment.Equip(rCuriass);
                    //Equipment.PlayerEquipment.Equip(rGloves);
                    Equipment.PlayerEquipment.Equip(rPants);
                    Equipment.PlayerEquipment.Equip(rBoots);
                    //Equipment.PlayerEquipment.Equip(rAmmo);
                    //Equipment.PlayerEquipment.Equip(rBow);

                    Equipment.PlayerEquipment.OnEquipmentChanges();
                    PlayerCombat.instance.OnEquipmentChanges();
                    PlayerInInventory.instance.OnEquipmentChangesHands();
                    PlayerInInventory.instance.OnEquipmentChangesAmmo();
                    break;

                case 2: // mage
                    //var mHelmet = Inventory.PlayerInventory.AddItem("PriestsCape001", 1, false);
                    var mCuriass = Inventory.PlayerInventory.AddItem("PriestsVest001", 1, false);
                    var mBoots = Inventory.PlayerInventory.AddItem("PriestsShoes001", 1, false);
                    
                    //var mDagger = Inventory.PlayerInventory.AddItem("SilverDagger001", 1, false);

                    //Equipment.PlayerEquipment.Equip(mHelmet);
                    Equipment.PlayerEquipment.Equip(mCuriass);
                    Equipment.PlayerEquipment.Equip(mBoots);
                    //Equipment.PlayerEquipment.Equip(mDagger);

                    //var fireTouch = SpellsKnowledge.Player.LearnSpell("S_Firetouch01");
                    var fireBall = SpellsKnowledge.Player.LearnSpell("S_Fireball01");
                    SpellsKnowledge.Player.LearnSpell("S_LesserHealing001");

                    SpellsKnowledge.Player.EquipSpell(fireBall);

                    Equipment.PlayerEquipment.OnEquipmentChanges();
                    PlayerCombat.instance.OnEquipmentChanges();
                    PlayerInInventory.instance.OnEquipmentChangesHands();
                    PlayerInInventory.instance.OnEquipmentChangesAmmo();

                    break;

                default:
                    break;
            }

            TimeOfDayManager.instance.SetTime(19.0f);
            InGameHelpUI.instance.StartTutorial();

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }


        public void MutateGoto()
        {
            Mutable mutable = null;
            if (CellInformation.TryToGetMutable("QuestDealerJustAnotherDeliveryMutable", out mutable))
            {
                mutable.Mutate();
            }
            else // Update the save file directly
            {
                var allMutables = SaveSystemManager.instance.saveFile.MutablesData.allMutables;

                if (allMutables.ContainsKey("QuestDealerJustAnotherDeliveryMutable"))
                    allMutables["QuestDealerJustAnotherDeliveryMutable"].isMutated = true;
                else
                    allMutables.Add("QuestDealerJustAnotherDeliveryMutable", new MutableData(true));
            }
        }
    }
}
