using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit.Game.ResultScripts
{
    public class SubmachineGunPickupResultScript : ResultScript
    {
        static int weaponCost = 3000;
        static int ammoCost = 1500;
        static int ammoAmount = 150;

        static int ammoCap = 600;

        ZombiesGameMode gameMode;

        void Start()
        {
            // Your code here
            // On use

            if (PlayerCombat.instance.canAttack)
            {
                gameMode = GameObject.Find("CellInfo").GetComponent<ZombiesGameMode>();

                bool hasShotgun = Inventory.PlayerInventory.GetItemCount("9mmsubmachinegun001") > 0;

                if (!hasShotgun)
                {
                    if (gameMode.curPoints >= weaponCost)
                    {
                        ItemInInventory weapon = Inventory.PlayerInventory.AddItem("9mmsubmachinegun001", 1);
                        weapon.metadata.intProperty1 = ((WeaponItem)weapon.item).clipRounds;
                        ItemInInventory ammo = Inventory.PlayerInventory.AddItem("9mmAmmo001", ammoAmount);

                        Equipment.PlayerEquipment.Equip(weapon);

                        Equipment.PlayerEquipment.OnEquipmentChanges();
                        PlayerCombat.instance.OnEquipmentChanges();
                        ThirdPersonPlayer.instance.OnEquipmentChangesHands();
                        PlayerInInventory.instance.OnEquipmentChangesHands();
                        PlayerCombat.instance.SetWeaponUI();

                        gameMode.RemovePoints(weaponCost);

                        GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, AudioClipsDatabase.GetItem("CASH_SOUND"));
                    }
                    else
                    {
                        GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, AudioClipsDatabase.GetItem("UI_007"));
                    }
                }
                else
                {
                    if (gameMode.curPoints >= ammoCost && Inventory.PlayerInventory.GetItemCount("9mmAmmo001") < ammoCap)
                    {
                        ItemInInventory weapon = Inventory.PlayerInventory.GetItem("9mmsubmachinegun001");

                        if (!weapon.isEquipped)
                        {
                            Equipment.PlayerEquipment.OnEquipmentChanges();
                            PlayerCombat.instance.OnEquipmentChanges();
                            ThirdPersonPlayer.instance.OnEquipmentChangesHands();
                            PlayerInInventory.instance.OnEquipmentChangesHands();
                        }

                        ItemInInventory ammo = Inventory.PlayerInventory.AddItem("9mmAmmo001", ammoAmount);

                        // Cap ammo
                        int count = Inventory.PlayerInventory.GetItemCount("9mmAmmo001");
                        if (count > ammoCap)
                        {
                            int excessAmmo = count - ammoCap;
                            Inventory.PlayerInventory.RemoveItem("9mmAmmo001", excessAmmo, false);
                        }

                        PlayerCombat.instance.SetWeaponUI();

                        gameMode.RemovePoints(ammoCost);

                        GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, AudioClipsDatabase.GetItem("CASH_SOUND"));
                    }
                    else
                    {
                        GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, AudioClipsDatabase.GetItem("UI_007"));
                    }
                }
            }

            // Destroy the script
            Destroy(this);
        }
    }
}