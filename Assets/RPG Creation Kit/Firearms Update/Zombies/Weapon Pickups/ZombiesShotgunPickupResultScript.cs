using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit.Game.ResultScripts
{
    public class ZombiesShotgunPickupResultScript : ResultScript
    {
        static int shotgunCost = 1500;
        static int shotgunAmmoCost = 1000;
        static int shotgunAmmoAmount = 40;

        static int ammoCap = 80;

        ZombiesGameMode gameMode;

        void Start()
        {
            // Your code here
            // On use

            if (PlayerCombat.instance.canAttack)
            {
                gameMode = GameObject.Find("CellInfo").GetComponent<ZombiesGameMode>();

                bool hasShotgun = Inventory.PlayerInventory.GetItemCount("Shotgun001") > 0;

                if (!hasShotgun)
                {
                    if (gameMode.curPoints >= shotgunCost)
                    {
                        ItemInInventory shotgun = Inventory.PlayerInventory.AddItem("Shotgun001", 1);
                        shotgun.metadata.intProperty1 = ((WeaponItem)shotgun.item).clipRounds;
                        ItemInInventory ammo = Inventory.PlayerInventory.AddItem("12gaAmmo001", shotgunAmmoAmount);

                        Equipment.PlayerEquipment.Equip(shotgun);

                        Equipment.PlayerEquipment.OnEquipmentChanges();
                        PlayerCombat.instance.OnEquipmentChanges();
                        ThirdPersonPlayer.instance.OnEquipmentChangesHands();
                        PlayerInInventory.instance.OnEquipmentChangesHands();
                        PlayerCombat.instance.SetWeaponUI();

                        gameMode.RemovePoints(shotgunCost);

                        GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, AudioClipsDatabase.GetItem("CASH_SOUND"));
                    }
                    else
                    {
                        GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, AudioClipsDatabase.GetItem("UI_007"));
                    }
                }
                else
                {
                    if (gameMode.curPoints >= shotgunAmmoCost && Inventory.PlayerInventory.GetItemCount("12gaAmmo001") < ammoCap)
                    {
                        ItemInInventory shotgun = Inventory.PlayerInventory.GetItem("Shotgun001");

                        if (!shotgun.isEquipped)
                        {
                            Equipment.PlayerEquipment.OnEquipmentChanges();
                            PlayerCombat.instance.OnEquipmentChanges();
                            ThirdPersonPlayer.instance.OnEquipmentChangesHands();
                            PlayerInInventory.instance.OnEquipmentChangesHands();
                        }

                        ItemInInventory ammo = Inventory.PlayerInventory.AddItem("12gaAmmo001", shotgunAmmoAmount);

                        // Cap ammo
                        int count = Inventory.PlayerInventory.GetItemCount("12gaAmmo001");
                        if (count > ammoCap)
                        {
                            int excessAmmo = count - ammoCap;
                            Inventory.PlayerInventory.RemoveItem("12gaAmmo001", excessAmmo, false);
                        }

                        PlayerCombat.instance.SetWeaponUI();

                        gameMode.RemovePoints(shotgunAmmoCost);

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