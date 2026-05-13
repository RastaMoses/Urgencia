using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.AI
{
    /// <summary>
    /// Allows the AI to have an usable Inventory and use Equipment, Looting Points and is essential for setting up the 
    /// </summary>
    [RequireComponent(typeof(Inventory), typeof(Equipment))]
    public class AIInventoryEquipment : AIMovements
    {
        // The Array of items the AI will equip as soon as it loads for the first time
        [SerializeField]
        public Item[] equippedItems;

        public Inventory inventory;
        public Equipment equipment;

        public LootingPoint lootingPoint;
        [SerializeField] public bool allowsLootWhenDead = true;
        [SerializeField] public bool allowsLootOfEquipment = true;


        public WeaponOnHand currentWeaponOnHand;

        public bool isUsingMeleeWeapon;
        public bool isUsingRangedWeapon;

        public bool setInventoryOwnershipOnStart = false;

        public override void Start()
        {
            base.Start();
            inventory.InitializeInventory();

            if(setInventoryOwnershipOnStart)
                for (int i = 0; i < inventory.Items.Count; i++)
                    if(inventory.Items[i].metadata.IsEmpty())
                    {
                        inventory.Items[i].metadata.isOwned = true;
                        inventory.Items[i].metadata.ownerID = entityID;
                    }

            EquipDelayed();
        }

        /// <summary>
        /// Equips the default items
        /// </summary>
        void EquipDelayed()
        {
            // Equip after everything else
            List<Item> toEquipAfter = new List<Item>();


            for (int i = 0; i < equippedItems.Length; i++)
            {
                if (inventory.HasItem(equippedItems[i].ItemID))
                {
                    if (equippedItems[i].itemType == ItemTypes.ArmorItem && (RCKFunctions.ContainsBiped(((ArmorItem)(equippedItems[i])).Bipeds, BipedObject.Shield) || RCKFunctions.ContainsBiped(((ArmorItem)(equippedItems[i])).Bipeds, BipedObject.Torch)))
                    {
                        // Delay the equipment of shields
                        toEquipAfter.Add(equippedItems[i]);
                    }
                    else
                        equipment.Equip(equippedItems[i].ItemID);
                }
            }

            OnEquipmentChangesHands();
            OnEquipmentChangesAmmo();

            for (int i = 0; i < toEquipAfter.Count; i++)
            {
                equipment.Equip(toEquipAfter[i]);
            }
        }

        public override void Die()
        {
            base.Die();

            if(allowsLootWhenDead)
            {
                lootingPoint.enabled = true;
                lootingPoint.ID = entityName + " LootingPoint";
                lootingPoint.pointName = entityName;
                lootingPoint.lCollider.enabled = true;
            }

            // Remove ownership on all items
            for(int i = 0; i < inventory.Items.Count; i++)
            {
                inventory.Items[i].metadata.Clear();
            }
        }

        /// <summary>
        /// Called when this entity changes its equipment that involves the hands
        /// </summary>
        public void OnEquipmentChangesHands()
        {
            // If there is a weapon equipped
            if (equipment.itemsEquipped[(int)EquipmentSlots.RHand] != null && equipment.itemsEquipped[(int)EquipmentSlots.RHand].item != null)
            {
                // Destroy previous weapon equipped if there was a swap
                if (equipment.currentWeapon != null)
                {
#if UNITY_EDITOR
                    DestroyImmediate(equipment.currentWeaponObject);
#else
                    Destroy(equipment.currentWeaponObject);
#endif
                    equipment.currentWeapon = null;
                }

                // Instantiate Weapon on hand, assign references
                equipment.currentWeapon = (WeaponItem)equipment.itemsEquipped[(int)EquipmentSlots.RHand].item;

                if (equipment.currentWeapon.weaponType == WeaponType.Bow)
                    equipment.currentWeaponObject = Instantiate(equipment.currentWeapon.WeaponOnHand, bodyData.lHand);
                else
                    equipment.currentWeaponObject = Instantiate(equipment.currentWeapon.WeaponOnHand, bodyData.rHand);

                currentWeaponOnHand = equipment.currentWeaponObject.GetComponent<WeaponOnHand>();
                equipment.currentWeaponAnimator = equipment.currentWeaponObject.GetComponent<Animator>();

                if(currentWeaponOnHand != null)
                    currentWeaponOnHand.thisEntity = this;


                if (equipment.currentWeapon.WeaponSheathed != null)
                {
                    if(equipment.currentWeapon.weaponType == WeaponType.Bow ||
                       equipment.currentWeapon.weaponType == WeaponType.BladeTwoHands ||
                       equipment.currentWeapon.weaponType == WeaponType.BluntTwoHands ||
                       equipment.currentWeapon.weaponType == WeaponType.FIREARM_TwoHanded_1)
                        equipment.currentWeaponOnHip = Instantiate(equipment.currentWeapon.WeaponSheathed, bodyData.upperChest);
                    else
                        equipment.currentWeaponOnHip = Instantiate(equipment.currentWeapon.WeaponSheathed, bodyData.hips);
                }
                equipment.currentWeaponObject.SetActive(false);
                equipment.currentWeaponOnHip.SetActive(true);
                
                // Determine what the AI is using
                isUsingMeleeWeapon = (equipment.currentWeapon.weaponType != WeaponType.Bow && equipment.currentWeapon.weaponType != WeaponType.Crossbow);

                isUsingRangedWeapon = (equipment.currentWeapon.weaponType == WeaponType.Bow || equipment.currentWeapon.weaponType == WeaponType.Crossbow);

            }
            else if (equipment.itemsEquipped[(int)EquipmentSlots.RHand] == null && equipment.currentWeapon != null)
            {
                // Unequip = destroy previous equipped weapon
#if UNITY_EDITOR
                DestroyImmediate(equipment.currentWeaponObject);
                DestroyImmediate(equipment.currentWeaponOnHip);
                equipment.currentWeapon = null;
#else
                    Destroy(equipment.currentWeaponObject);
                    Destroy(equipment.currentWeaponOnHip);
                    equipment.currentWeapon = null;
#endif

                equipment.currentWeapon = null;
            }
        }


        [AIInvokable]
        public void UpdateWhichWeaponIsUsing()
        {
            // Determine what the AI is using
            isUsingMeleeWeapon = (equipment.currentWeapon.weaponType != WeaponType.Bow && equipment.currentWeapon.weaponType != WeaponType.Crossbow);

            isUsingRangedWeapon = (equipment.currentWeapon.weaponType == WeaponType.Bow || equipment.currentWeapon.weaponType == WeaponType.Crossbow);
        }

        /// <summary>
        /// Called when this entity changes its equipment that involves the ammo
        /// </summary>
        public void OnEquipmentChangesAmmo()
        {
            if (equipment.itemsEquipped[(int)EquipmentSlots.Ammo] != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item != null)
            {
                if (equipment.currentAmmoObject != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item != equipment.currentAmmoObject)
                {
#if UNITY_EDITOR
                    DestroyImmediate(equipment.currentAmmoObject);
#else
                    Destroy(equipment.currentAmmoObject);
#endif
                    equipment.currentAmmo = null;
                }

                // Instantiate Weapon on hand, assign references
                equipment.currentAmmo = (AmmoItem)equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item;
                equipment.currentAmmoObject = Instantiate(equipment.currentAmmo.bagOnBody, bodyData.upperChest);
            }
            else if (equipment.currentAmmo != null)
            {
                // Unequip = destroy previous equipped weapon
#if UNITY_EDITOR
                DestroyImmediate(equipment.currentAmmoObject);
#else
                    Destroy(equipment.currentAmmoObject);
#endif
                equipment.currentAmmo = null;
            }

            if (equipment.itemsEquipped[(int)EquipmentSlots.Ammo] == null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo] != null)
            {
                // Unequip = destroy previous equipped weapon
#if UNITY_EDITOR
                DestroyImmediate(equipment.currentAmmoObject);
#else
                    Destroy(equipment.currentAmmoObject);
#endif

                equipment.itemsEquipped[(int)EquipmentSlots.Ammo] = null;
            }
        }
    }
}