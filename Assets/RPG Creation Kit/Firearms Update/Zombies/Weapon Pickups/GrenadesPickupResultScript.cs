using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit.Game.ResultScripts
{
    public class GrenadesPickupResultScript : ResultScript
    {
        static int ammoCost = 1500;
        static int ammoAmount = 3;

        static int ammoCap = 3;

        ZombiesGameMode gameMode;

        void Start()
        {
            // Your code here
            // On use

            if (PlayerCombat.instance.canAttack)
            {
                gameMode = GameObject.Find("CellInfo").GetComponent<ZombiesGameMode>();

                if (gameMode.curPoints >= ammoCost && Inventory.PlayerInventory.GetItemCount("FragGrenadeN") < ammoCap)
                {
                    ItemInInventory ammo = Inventory.PlayerInventory.AddItem("FragGrenadeN", ammoAmount);

                    // Cap ammo
                    int count = Inventory.PlayerInventory.GetItemCount("FragGrenadeN");
                    if (count > ammoCap)
                    {
                        int excessAmmo = count - ammoCap;
                        Inventory.PlayerInventory.RemoveItem("FragGrenadeN", excessAmmo, false);
                    }

                    ItemInInventory weapon = Inventory.PlayerInventory.GetItem("FragGrenadeN");

                    if (!weapon.isEquipped)
                    {
                        Equipment.PlayerEquipment.Equip(weapon);

                        Equipment.PlayerEquipment.OnEquipmentChanges();
                        PlayerCombat.instance.OnEquipmentChanges();
                        ThirdPersonPlayer.instance.OnEquipmentChangesHands();
                        PlayerInInventory.instance.OnEquipmentChangesHands();
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

            // Destroy the script
            Destroy(this);
        }
    }
}