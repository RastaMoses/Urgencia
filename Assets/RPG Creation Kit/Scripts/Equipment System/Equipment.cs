using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    public enum EquipmentSlots
    {
        Head = 0,
        Upperbody = 1,
        Lowerbody = 2,
        Hands = 3,
        Feets = 4,
        RHand = 5,
        LHand = 6,
        RRing = 7,
        LRing = 8,
        Amulet = 9,
        Ammo = 10,
        Throwable = 11,
    }


    /// <summary>
    /// Class for keeping Equipment both for Player & NPCs
    /// </summary>
    [RequireComponent(typeof(Inventory))]
    public class Equipment : MonoBehaviour
    {
        public readonly static int equipmentSlotsLength = 12;

        [Header("PLAYER")]
        public SkinnedMeshRenderer FP_Arms;

        [Space(10)]

        public BodyData characterModel;

        public static Equipment PlayerEquipment;
        private void Awake()
        {
            // Find the player inventory and set reference
            if (!PlayerEquipment)
                if (gameObject.CompareTag("Player") && gameObject.GetComponent<Equipment>() != null)
                    PlayerEquipment = this;
        }


        // References
        [SerializeField] public Inventory inv;

        // To instatiate meshes of armors
        private List<ItemInInventory> armorsEquipped = new List<ItemInInventory>();
        private List<GameObject> armorsMeshes = new List<GameObject>();

        [Header("Equipment")]
        public ItemInInventory[] itemsEquipped = new ItemInInventory[Equipment.equipmentSlotsLength];


        [Space(5)]
        [Header("Stats")]
        public float MeleeDamageOutput = 0;
        public float RangedDamageOutput = 0;
        public float ArmorRate = 0;
        public bool isUsingShield = false;
        public bool isUsingTorch = false;
        public bool doingOneHWeaponSwap = false;


        public WeaponItem currentWeapon;
        public GameObject currentWeaponObject;
        public GameObject currentWeaponOnHip;
        public Animator currentWeaponAnimator;


        public AmmoItem currentAmmo;
        public GameObject currentAmmoObject;

        public bool preventShieldDrop = false; // same as preventTorchDrop
        public ArmorItem currentShield;
        public GameObject currentShieldObject;
        public GameObject currentFPShieldObject;

        public bool preventTorchDrop = false; // prevent NPCs from dropping torches on the ground when dead. This is useful for killing NPCs via scripts and preventing torches to stay on ground
        public ArmorItem currentTorch;
        public GameObject currentTorchInHand;

        public bool Equip(string itemID)
        {
            ItemInInventory itemToEquip;
            // Check if item exists in inventory
            if ((itemToEquip = inv.GetItem(itemID)) != null)
            {
                return Equip(itemToEquip.item);
            }
            else
                return false;
        }

        public bool Equip(Item itemToEquip)
        {
            var itemInSubInv = inv.GetItem(itemToEquip);

            if (itemInSubInv != null)
                return Equip(itemInSubInv);
            else
                return false;
        }

        /// <summary>
        /// Returns true if the new item was successfully equipped.
        /// </summary>
        /// <param name="_item"></param>
        /// <returns></returns>
        public bool Equip(ItemInInventory _item)
        {
            if (PlayerEquipment != null && PlayerEquipment == this)
            {
                if (PlayerCombat.instance.isAttacking)
                {
                    AlertMessage.instance.InitAlertMessage("You cannot change weapons or armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_SHORT);
                    return false;
                }
            }

            if (_item != null && _item.item == null)
                return false;

            if (_item.item.itemType == ItemTypes.ArmorItem)
            {
                ArmorItem item = (ArmorItem)_item.item;

                for (int i = 0; i < item.Bipeds.Length; i++)
                {
                    switch (item.Bipeds[i])
                    {
                        case BipedObject.Head:

                            if (itemsEquipped[(int)EquipmentSlots.Head] != null)
                                Unequip(EquipmentSlots.Head);

                            _item.isEquipped = true;
                            itemsEquipped[(int)EquipmentSlots.Head] = _item;
                            armorsEquipped.Add(_item);
                            break;

                        case BipedObject.UpperBody:
                            if (itemsEquipped[(int)EquipmentSlots.Upperbody] != null)
                                Unequip(EquipmentSlots.Upperbody);

                            // Disable default upperbody
                            if (characterModel.defaultUpperbody != null)
                                characterModel.defaultUpperbody.gameObject.SetActive(false);

                            _item.isEquipped = true;
                            itemsEquipped[(int)EquipmentSlots.Upperbody] = _item;
                            armorsEquipped.Add(_item);
                            break;

                        case BipedObject.LowerBody:

                            if (itemsEquipped[(int)EquipmentSlots.Lowerbody] != null)
                                Unequip(EquipmentSlots.Lowerbody);

                            if (characterModel.defaultLowerbody != null)
                                characterModel.defaultLowerbody.gameObject.SetActive(false);

                            _item.isEquipped = true;
                            itemsEquipped[(int)EquipmentSlots.Lowerbody] = _item;
                            armorsEquipped.Add(_item);

                            break;

                        case BipedObject.Hand:

                            if (itemsEquipped[(int)EquipmentSlots.Hands] != null)
                                Unequip(EquipmentSlots.Hands);

                            _item.isEquipped = true;
                            itemsEquipped[(int)EquipmentSlots.Hands] = _item;
                            armorsEquipped.Add(_item);

                            break;

                        case BipedObject.Foot:

                            if (itemsEquipped[(int)EquipmentSlots.Feets] != null)
                                Unequip(EquipmentSlots.Feets);

                            _item.isEquipped = true;
                            itemsEquipped[(int)EquipmentSlots.Feets] = _item;
                            armorsEquipped.Add(_item);

                            break;


                        case BipedObject.RightRing:

                            if (itemsEquipped[(int)EquipmentSlots.RRing] != null)
                                Unequip(EquipmentSlots.RRing);

                            _item.isEquipped = true;
                            itemsEquipped[(int)EquipmentSlots.RRing] = _item;
                            armorsEquipped.Add(_item);

                            break;

                        case BipedObject.LeftRing:

                            if (itemsEquipped[(int)EquipmentSlots.LRing] != null)
                                Unequip(EquipmentSlots.LRing);

                            _item.isEquipped = true;
                            itemsEquipped[(int)EquipmentSlots.LRing] = _item;
                            armorsEquipped.Add(_item);

                            break;

                        case BipedObject.Amulet:

                            if (itemsEquipped[(int)EquipmentSlots.Amulet] != null)
                                Unequip(EquipmentSlots.Amulet);

                            _item.isEquipped = true;
                            itemsEquipped[(int)EquipmentSlots.Amulet] = _item;
                            armorsEquipped.Add(_item);

                            break;


                        case BipedObject.Shield:

                            if (CompareTag("RPG Creation Kit/PlayerInInventory") && (itemsEquipped[(int)EquipmentSlots.RHand]) != null)
                                currentWeapon = (WeaponItem)(itemsEquipped[(int)EquipmentSlots.RHand].item);

                            if (currentWeapon != null &&
                                (currentWeapon.weaponType == WeaponType.BladeOneHand || currentWeapon.weaponType == WeaponType.BluntOneHand ||
                                currentWeapon.weaponType == WeaponType.DaggerOneHand))
                            {
                                if (itemsEquipped[(int)EquipmentSlots.LHand] != null)
                                    Unequip(EquipmentSlots.LHand);

                                _item.isEquipped = true;
                                itemsEquipped[(int)EquipmentSlots.LHand] = _item;
                                armorsEquipped.Add(_item);

                                currentShield = (ArmorItem)_item.item;

                            }
                            else if (PlayerEquipment == this)
                                AlertMessage.instance.InitAlertMessage("You can't use a shield\n with the current weapon.", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM, false);

                            break;

                        case BipedObject.Torch:

                            if (CompareTag("RPG Creation Kit/PlayerInInventory") && (itemsEquipped[(int)EquipmentSlots.RHand]) != null)
                                currentWeapon = (WeaponItem)(itemsEquipped[(int)EquipmentSlots.RHand].item);

                            if (CompareTag("RPG Creation Kit/PlayerInInventory") && currentWeapon == null)
                                currentWeapon = Equipment.PlayerEquipment.currentWeapon;

                            if (currentWeapon != null &&
                                (currentWeapon.weaponType == WeaponType.BladeOneHand || currentWeapon.weaponType == WeaponType.BluntOneHand ||
                                currentWeapon.weaponType == WeaponType.DaggerOneHand || currentWeapon.weaponType == WeaponType.Unarmed))
                            {
                                if (itemsEquipped[(int)EquipmentSlots.LHand] != null)
                                    Unequip(EquipmentSlots.LHand);

                                _item.isEquipped = true;
                                itemsEquipped[(int)EquipmentSlots.LHand] = _item;
                                armorsEquipped.Add(_item);

                                currentTorch = (ArmorItem)_item.item;
                            }
                            else if (PlayerEquipment == this)
                                AlertMessage.instance.InitAlertMessage("You can't use a torch\n with the current weapon.", AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM, false);

                            break;
                    }
                }

                // Hide body parts if we have to
                if (characterModel != null)
                {
                    if (item.HideHair && characterModel.hair != null)
                        characterModel.hair.gameObject.SetActive(false);
                    if (item.hideHead)
                        characterModel.head.gameObject.SetActive(false);
                    if (item.hideUpperbody)
                        characterModel.upperbody.gameObject.SetActive(false);
                    if (item.hideArms)
                        characterModel.arms.gameObject.SetActive(false);
                    if (item.hideHands)
                        characterModel.hands.gameObject.SetActive(false);
                    if (item.hideLegs)
                        characterModel.lowerbody.gameObject.SetActive(false);
                    if (item.hideFeet)
                        characterModel.feet.gameObject.SetActive(false);

                    if (PlayerEquipment == this && item.hideArms)
                        PlayerCombat.instance.fpsArms.arms.gameObject.SetActive(false);

                    if (PlayerEquipment == this && item.hideHands)
                        PlayerCombat.instance.fpsArms.hands.gameObject.SetActive(false);
                }
            }
            else if (_item.item.itemType == ItemTypes.AmmoItem)
            {
                if (itemsEquipped[(int)EquipmentSlots.Ammo] != null)
                    Unequip(EquipmentSlots.Ammo);

                _item.isEquipped = true;
                itemsEquipped[(int)EquipmentSlots.Ammo] = _item;
            }
            else if (_item.item.itemType == ItemTypes.WeaponItem)
            {
                WeaponItem item = (WeaponItem)_item.item;

                if (item.weaponClass == WeaponClass.Throwable)
                {
                    // Equip Throwable
                    if (itemsEquipped[(int)EquipmentSlots.Throwable] != null)
                        Unequip(EquipmentSlots.Throwable);

                    _item.isEquipped = true;
                    itemsEquipped[(int)EquipmentSlots.Throwable] = _item;
                }
                else
                {
                    if (itemsEquipped[(int)EquipmentSlots.RHand] != null)
                    {
                        if (item.weaponType == WeaponType.BladeOneHand ||
                            item.weaponType == WeaponType.BluntOneHand)
                            doingOneHWeaponSwap = true;

                        Unequip(EquipmentSlots.RHand);

                        doingOneHWeaponSwap = false;
                    }

                    if (itemsEquipped[(int)EquipmentSlots.LHand] != null)
                    {
                        if (item.weaponType == WeaponType.BladeTwoHands ||
                            item.weaponType == WeaponType.BluntTwoHands ||
                            item.weaponType == WeaponType.Bow)
                            Unequip(EquipmentSlots.LHand);
                    }

                    _item.isEquipped = true;
                    itemsEquipped[(int)EquipmentSlots.RHand] = _item;
                }
            }


            // Remove duplicates armorsEquipped
            armorsEquipped = armorsEquipped.Distinct().ToList();

            CalculateStats();
            SpawnArmorsMeshes();
            OnEquipmentChanges();

            if (PlayerEquipment == this)
                PlayerCombat.instance.SetWeaponUI();

            if (PlayerEquipment != null && PlayerEquipment == this)
                // Broadcast to PlayerInInventory
                PlayerInInventory.instance.equipment.Equip(_item);

                return true;
        }

        public bool Unequip(ItemInInventory _item)
        {
            if (PlayerEquipment != null && PlayerEquipment == this)
            {
                // TOCHANGE IF ENTITY IS ATTACKING.
                if (PlayerCombat.instance.isAttacking)
                {
                    AlertMessage.instance.InitAlertMessage("You cannot change weapons and armor while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_SHORT);
                    return false;
                }
            }

            if (_item == null)
                return false;

            if (_item != null && _item.item == null)
                return false;

            if (_item.item.itemType == ItemTypes.ArmorItem)
            {
                ArmorItem item = (ArmorItem)_item.item;

                for (int i = 0; i < item.Bipeds.Length; i++)
                {
                    switch (item.Bipeds[i])
                    {
                        case BipedObject.Head:
                            _item.isEquipped = false;
                            armorsEquipped.Remove(_item);
                            itemsEquipped[(int)EquipmentSlots.Head] = null;
                            break;

                        case BipedObject.UpperBody:

                            // Enable default upperbody
                            if (characterModel.defaultUpperbody != null)
                                characterModel.defaultUpperbody.gameObject.SetActive(true);

                            _item.isEquipped = false;
                            armorsEquipped.Remove(_item);
                            itemsEquipped[(int)EquipmentSlots.Upperbody] = null;
                            break;

                        case BipedObject.LowerBody:

                            // Enable default upperbody
                            if (characterModel.defaultLowerbody != null)
                                characterModel.defaultLowerbody.gameObject.SetActive(true);

                            _item.isEquipped = false;
                            armorsEquipped.Remove(_item);
                            itemsEquipped[(int)EquipmentSlots.Lowerbody] = null;

                            break;

                        case BipedObject.Hand:
                            _item.isEquipped = false;
                            armorsEquipped.Remove(_item);
                            itemsEquipped[(int)EquipmentSlots.Hands] = null;

                            break;

                        case BipedObject.Foot:
                            _item.isEquipped = false;
                            armorsEquipped.Remove(_item);
                            itemsEquipped[(int)EquipmentSlots.Feets] = null;

                            break;

                        case BipedObject.RightRing:
                            _item.isEquipped = false;
                            armorsEquipped.Remove(_item);
                            itemsEquipped[(int)EquipmentSlots.RRing] = null;

                            break;

                        case BipedObject.LeftRing:
                            _item.isEquipped = false;
                            armorsEquipped.Remove(_item);
                            itemsEquipped[(int)EquipmentSlots.LRing] = null;

                            break;

                        case BipedObject.Amulet:
                            _item.isEquipped = false;
                            armorsEquipped.Remove(_item);
                            itemsEquipped[(int)EquipmentSlots.Amulet] = null;

                            break;


                        case BipedObject.Shield:
                            _item.isEquipped = false;
                            armorsEquipped.Remove(_item);
                            itemsEquipped[(int)EquipmentSlots.LHand] = null;
                            currentShield = (ArmorItem)_item.item;
                            break;

                        case BipedObject.Torch:
                            _item.isEquipped = false;
                            armorsEquipped.Remove(_item);
                            itemsEquipped[(int)EquipmentSlots.LHand] = null;
                            break;
                    }
                }


                // Hide body parts if we have to
                if (characterModel != null)
                {
                    if (item.HideHair && characterModel.hair != null)
                        characterModel.hair.gameObject.SetActive(true);
                    if (item.hideHead)
                        characterModel.head.gameObject.SetActive(true);
                    if (item.hideUpperbody)
                        characterModel.upperbody.gameObject.SetActive(true);
                    if (item.hideArms)
                        characterModel.arms.gameObject.SetActive(true);
                    if (item.hideHands)
                        characterModel.hands.gameObject.SetActive(true);
                    if (item.hideLegs)
                        characterModel.lowerbody.gameObject.SetActive(true);
                    if (item.hideFeet)
                        characterModel.feet.gameObject.SetActive(true);

                    if (PlayerEquipment == this && item.hideArms)
                        PlayerCombat.instance.fpsArms.arms.gameObject.SetActive(true);

                    if (PlayerEquipment == this && item.hideHands)
                        PlayerCombat.instance.fpsArms.hands.gameObject.SetActive(true);
                }
            }
            else if(_item.item.itemType == ItemTypes.AmmoItem)
            {
                _item.isEquipped = false;
                itemsEquipped[(int)EquipmentSlots.Ammo] = null;
            }
            else if(_item.item.itemType == ItemTypes.WeaponItem)
            {
                WeaponItem weaponitem = _item.item as WeaponItem;

                if (weaponitem.weaponClass == WeaponClass.Throwable)
                {
                    _item.isEquipped = false;
                    itemsEquipped[(int)EquipmentSlots.Throwable] = null;
                }
                else
                {
                    _item.isEquipped = false;
                    itemsEquipped[(int)EquipmentSlots.RHand] = null;
                }
            }

                // Remove duplicates armorsEquipped
                armorsEquipped = armorsEquipped.Distinct().ToList();

                CalculateStats();
                SpawnArmorsMeshes();
                OnEquipmentChanges();

                if (PlayerEquipment == this)
                    PlayerCombat.instance.SetWeaponUI();

            if (PlayerEquipment != null && PlayerEquipment == this)
                PlayerInInventory.instance.equipment.Unequip(_item);

                return true;
        }

        [ContextMenu("OnEquipmentChanges")]
        public void OnEquipmentChanges()
        {
            // Broadcast to PlayerInInventory
            if (PlayerEquipment != null && PlayerEquipment == this)
                PlayerInInventory.instance.equipment.OnEquipmentChanges();

            // Check for L hand
            if (itemsEquipped[(int)EquipmentSlots.LHand] != null && itemsEquipped[(int)EquipmentSlots.LHand].item != null)
            {
                if(itemsEquipped[(int)EquipmentSlots.LHand].item.itemType == ItemTypes.ArmorItem)
                {
                    ArmorItem armorItem = (ArmorItem)itemsEquipped[(int)EquipmentSlots.LHand].item;

                    isUsingShield = (armorItem.Bipeds.Contains(BipedObject.Shield));
                    isUsingTorch = (armorItem.Bipeds.Contains(BipedObject.Torch));


                    if (isUsingShield) // If we're using a shield we must have a one handed weapon equipped
                    {
                        if (itemsEquipped[(int)EquipmentSlots.RHand] != null && itemsEquipped[(int)EquipmentSlots.RHand].item != null)
                        {
                            if (itemsEquipped[(int)EquipmentSlots.RHand].item.itemType == ItemTypes.WeaponItem)
                            {
                                WeaponItem currentWeap = (WeaponItem)itemsEquipped[(int)EquipmentSlots.RHand].item;

                                // If we're using an one handed weapon
                                if (currentWeap.weaponType == WeaponType.BladeOneHand || currentWeap.weaponType == WeaponType.BluntOneHand || currentWeap.weaponType == WeaponType.DaggerOneHand)
                                {
                                    // ALL GOOD
                                    if (PlayerEquipment == this)
                                    {
                                        PlayerCombat.instance.fpcAnim.SetBool("isUsingShield", isUsingShield);
                                        Player.ThirdPersonPlayer.instance.m_Animator.SetBool("isUsingShield", isUsingShield);
                                    }
                                    else
                                    {
                                        Animator anim = GetComponent<Animator>();
                                        if (anim != null)
                                            anim.SetBool("isUsingShield", isUsingShield);
                                    }
                                }
                            }
                        }
                        else if(!doingOneHWeaponSwap)
                        {
                            // Unequip the shield
                            Unequip(EquipmentSlots.LHand);
                            
                            if(PlayerEquipment == this)
                                AlertMessage.instance.InitAlertMessage("You can't equip a shield without an\none-handed weapon.", AlertMessage.DEFAULT_MESSAGE_DURATION_SHORT);
                        }
                    }

                    if (isUsingTorch) 
                    {
                        if (currentWeaponObject == null || (itemsEquipped[(int)EquipmentSlots.RHand] != null && itemsEquipped[(int)EquipmentSlots.RHand].item != null))
                        {
                            if (currentWeaponObject == null || (itemsEquipped[(int)EquipmentSlots.RHand].item.itemType == ItemTypes.WeaponItem))
                            {
                                WeaponItem currentWeap = currentWeaponObject != null ? (WeaponItem)itemsEquipped[(int)EquipmentSlots.RHand].item : null;

                                // If we're using an one handed weapon
                                if (currentWeaponObject == null || (currentWeap.weaponType == WeaponType.BladeOneHand || currentWeap.weaponType == WeaponType.BluntOneHand || currentWeap.weaponType == WeaponType.DaggerOneHand))
                                {
                                    // ALL GOOD
                                    if (PlayerEquipment == this)
                                    {
                                        PlayerCombat.instance.fpcAnim.SetBool("isUsingTorch", isUsingTorch);
                                        Player.ThirdPersonPlayer.instance.m_Animator.SetBool("isUsingTorch", isUsingTorch);
                                        PlayerInInventory.instance.m_Animator.SetBool("isUsingTorch", isUsingTorch);
                                    }
                                    else
                                    {
                                        Animator anim = GetComponent<Animator>();
                                        if (anim != null)
                                            anim.SetBool("isUsingTorch", isUsingTorch);
                                    }
                                }
                            }
                        }
                        else if (!doingOneHWeaponSwap)
                        {
                            
                        }
                    }
                }
            } else
            {
                isUsingShield = false;
                isUsingTorch = false;

                if (PlayerEquipment == this)
                {
                    PlayerCombat.instance.fpcAnim.SetBool("isUsingShield", isUsingShield);
                    Player.ThirdPersonPlayer.instance.m_Animator.SetBool("isUsingShield", isUsingShield);

                    PlayerCombat.instance.fpcAnim.SetBool("isUsingTorch", isUsingTorch);
                    Player.ThirdPersonPlayer.instance.m_Animator.SetBool("isUsingTorch", isUsingTorch);
                    PlayerInInventory.instance.m_Animator.SetBool("isUsingTorch", isUsingTorch);
                }
                else
                {
                    Animator anim = GetComponent<Animator>();
                    if(anim != null)
                    {
                        anim.SetBool("isUsingTorch", isUsingShield);
                        anim.SetBool("isUsingShield", isUsingShield);
                    }
                }
            }

            if (itemsEquipped[(int)EquipmentSlots.RHand] == null && currentWeapon != null && currentWeapon.weaponType != WeaponType.Unarmed)
            {
                // Unequip = destroy previous equipped weapon
                Destroy(currentWeaponObject);
                Destroy(currentWeaponOnHip);

                currentWeapon = null;
            }

            if (itemsEquipped[(int)EquipmentSlots.Ammo] == null && currentAmmo != null)
            {
                // Unequip = destroy previous equipped weapon
                Destroy(currentAmmoObject);

                itemsEquipped[(int)EquipmentSlots.Ammo] = null;
            }

            // Broadcast to PlayerInInventory
            if (PlayerEquipment != null && PlayerEquipment == this)
            {
                if (Player.RckPlayer.instance.isInThirdPerson)
                    Player.RckPlayer.instance.SwitchToThirdPerson();
                else
                    Player.RckPlayer.instance.SwitchToFirstPerson();
            }
        }

        /// <summary>
        /// Calculate Armor rate, meele damage output and ranged damage output in base of our gear.
        /// </summary>
        private void CalculateStats()
        {
            // Reset Stats
            ArmorRate = 0;
            MeleeDamageOutput = 0;
            RangedDamageOutput = 0;

            // A single armor can be present (can occupy slots) in different parts of the body
            // But we need to calculate the stats only once
            List<ItemInInventory> ArmorsEquippedItems = new List<ItemInInventory>();

            if (itemsEquipped[(int)EquipmentSlots.Head] != null)
                if (!ArmorsEquippedItems.Contains(itemsEquipped[(int)EquipmentSlots.Head]))
                    ArmorsEquippedItems.Add(itemsEquipped[(int)EquipmentSlots.Head]);

            if (itemsEquipped[(int)EquipmentSlots.Upperbody] != null)
                if (!ArmorsEquippedItems.Contains(itemsEquipped[(int)EquipmentSlots.Upperbody]))
                    ArmorsEquippedItems.Add(itemsEquipped[(int)EquipmentSlots.Upperbody]);

            if (itemsEquipped[(int)EquipmentSlots.Lowerbody] != null)
                if (!ArmorsEquippedItems.Contains(itemsEquipped[(int)EquipmentSlots.Lowerbody]))
                    ArmorsEquippedItems.Add(itemsEquipped[(int)EquipmentSlots.Lowerbody]);

            if (itemsEquipped[(int)EquipmentSlots.Hands] != null)
                if (!ArmorsEquippedItems.Contains(itemsEquipped[(int)EquipmentSlots.Hands]))
                    ArmorsEquippedItems.Add(itemsEquipped[(int)EquipmentSlots.Hands]);

            if (itemsEquipped[(int)EquipmentSlots.Feets] != null)
                if (!ArmorsEquippedItems.Contains(itemsEquipped[(int)EquipmentSlots.Feets]))
                    ArmorsEquippedItems.Add(itemsEquipped[(int)EquipmentSlots.Feets]);

            if (itemsEquipped[(int)EquipmentSlots.LHand] != null)
                if (!ArmorsEquippedItems.Contains(itemsEquipped[(int)EquipmentSlots.LHand]))
                    ArmorsEquippedItems.Add(itemsEquipped[(int)EquipmentSlots.LHand]);

            /* Debug of ArmorsEquipped
            Debug.LogWarning("-----------");
            for (int i = 0; i < ArmorsEquippedItems.Count; i++)
            {
                Debug.Log(ArmorsEquippedItems[i].item);
            }
            Debug.LogWarning("-----------");
            */

            // Calculate armor stats
            for (int i = 0; i < ArmorsEquippedItems.Count; i++)
            {
                if (ArmorsEquippedItems[i].item != null)
                {
                    ArmorItem item = (ArmorItem)ArmorsEquippedItems[i].item;
                    ArmorRate += item.ArmorRating;
                }
            }

            // Calculate Melee Damage Output
            if (itemsEquipped[(int)EquipmentSlots.RHand] != null && itemsEquipped[(int)EquipmentSlots.RHand].item != null)
            {
                WeaponItem item = (WeaponItem)itemsEquipped[(int)EquipmentSlots.RHand].item;

                if (item.weaponType != WeaponType.Bow)
                    MeleeDamageOutput += item.Damage;
                else
                    RangedDamageOutput += item.Damage;

                // You can add here other buffs, like character strenght, dexterity etc
            }

            // Calculate Ranged Damage Output
            if (itemsEquipped[(int)EquipmentSlots.Ammo] != null && itemsEquipped[(int)EquipmentSlots.Ammo].item != null)
            {
                // If we've got a bow equipped
                if (RangedDamageOutput >= 0.1)
                {
                    // Add ammo damage
                    AmmoItem item = (AmmoItem)itemsEquipped[(int)EquipmentSlots.Ammo].item;
                    RangedDamageOutput += item.Damage;
                }
            }

        }


        private void SpawnArmorsMeshes()
        {
            for (int i = 0; i < armorsMeshes.Count; i++)
            {
#if UNITY_EDITOR
                if(!Application.isPlaying)
                    DestroyImmediate(armorsMeshes[i].gameObject);
                else
                    Destroy(armorsMeshes[i].gameObject);
#else
                Destroy(armorsMeshes[i].gameObject);
#endif
            }

            armorsMeshes.Clear();

            for(int i = 0; i < armorsEquipped.Count; i++)
            {
                ArmorItem armorItem = (ArmorItem)armorsEquipped[i].item;

                if (!armorItem.isStaticObject)
                {
                    if (armorsEquipped[i].item.useMesh)
                    {
                        armorsMeshes.Add(AttachToMesh(armorsEquipped[i].item).gameObject);

                        if (PlayerEquipment == this)
                        {
                            if ((armorItem).Bipeds.Contains(BipedObject.Hand) ||
                                (armorItem).Bipeds.Contains(BipedObject.UpperBody))
                                armorsMeshes.Add(AttachToFPMesh(armorsEquipped[i].item).gameObject);
                        }
                    }
                } 
                else
                {
                    if(armorItem.Bipeds.Contains(BipedObject.Shield)) 
                    {
                        // Spawn the shield model
                        GameObject shield = SpawnStaticObject(armorsEquipped[i].item, StaticObjectSlot.LeftHand);
                        armorsMeshes.Add(shield);
                        currentShieldObject = shield;

                        if (PlayerEquipment == this)
                        {
                            armorsMeshes.Add(currentFPShieldObject = SpawnStaticFPObject(armorsEquipped[i].item, StaticObjectSlot.LeftHand));
                        }
                    }

                    if (armorItem.Bipeds.Contains(BipedObject.Torch))
                    {
                        // Spawn the torch model
                        GameObject torch = SpawnStaticObject(armorsEquipped[i].item, StaticObjectSlot.LeftHand);
                        armorsMeshes.Add(torch);
                        currentTorchInHand = torch;

                        if (PlayerEquipment == this)
                        {
                            armorsMeshes.Add(currentFPShieldObject = SpawnStaticFPObject(armorsEquipped[i].item, StaticObjectSlot.LeftHand));
                        }
                    }
                }

                // Hide body parts if we have to
                if (characterModel != null)
                {
                    if (armorItem.hideHead)
                        characterModel.head.gameObject.SetActive(false);
                    if (armorItem.hideUpperbody)
                        characterModel.upperbody.gameObject.SetActive(false);
                    if (armorItem.hideArms)
                        characterModel.arms.gameObject.SetActive(false);
                    if (armorItem.hideHands)
                        characterModel.hands.gameObject.SetActive(false);
                    if (armorItem.hideLegs)
                        characterModel.lowerbody.gameObject.SetActive(false);
                    if (armorItem.hideFeet)
                        characterModel.feet.gameObject.SetActive(false);

                    if (PlayerEquipment == this && armorItem.hideArms)
                        PlayerCombat.instance.fpsArms.arms.gameObject.SetActive(false);

                    if (PlayerEquipment == this && armorItem.hideHands)
                        PlayerCombat.instance.fpsArms.hands.gameObject.SetActive(false);
                }
            }
        }

        public SkinnedMeshRenderer AttachToMesh(Item _item)
        {
            ArmorItem item = (ArmorItem)_item;

            SkinnedMeshRenderer getMesh = characterModel.isMale ? item.MaleBipedModel : item.FemaleBipedModel;

            SkinnedMeshRenderer newMesh = Instantiate(getMesh) as SkinnedMeshRenderer;
            newMesh.transform.parent = characterModel.upperbody.transform.parent;

            newMesh.rootBone = characterModel.upperbody.rootBone;
            newMesh.bones = characterModel.upperbody.bones;

            return newMesh;
        }

        public SkinnedMeshRenderer AttachToFPMesh(Item _item)
        {
            if (this != Equipment.PlayerEquipment)
                return null;

            ArmorItem item = (ArmorItem)_item;

            SkinnedMeshRenderer getMesh = (item.useFirstPersonModel) ? item.fpMaleModel : item.MaleBipedModel;

            SkinnedMeshRenderer newMesh = Instantiate(getMesh) as SkinnedMeshRenderer;
            newMesh.updateWhenOffscreen = true;
            newMesh.gameObject.layer = LayerMask.NameToLayer("FirstPerson");
            newMesh.transform.parent = FP_Arms.transform.parent;

            newMesh.rootBone = FP_Arms.rootBone;
            newMesh.bones = FP_Arms.bones;

            return newMesh;
        }

        public GameObject SpawnStaticObject(Item _item, StaticObjectSlot _slot)
        {
            ArmorItem item = (ArmorItem)_item;

            GameObject newObject = Instantiate(item.maleStaticObject, (_slot == StaticObjectSlot.LeftHand) ? characterModel.lHand : characterModel.rHand);

            return newObject;
        }

        public GameObject SpawnStaticFPObject(Item _item, StaticObjectSlot _slot)
        {
            ArmorItem item = (ArmorItem)_item;

            GameObject newObject = Instantiate(item.maleStaticObject, (_slot == StaticObjectSlot.LeftHand) ? PlayerCombat.instance.fpsArms.lHand : PlayerCombat.instance.fpsArms.rHand);
            newObject.SetLayer(RCKLayers.FirstPerson, true);
            return newObject;
        }

        public bool Unequip(EquipmentSlots slot)
        {
            return Unequip(itemsEquipped[(int)slot]);
        }


        public bool IsEquipped(string _itemID)
        {
            for(int i = 0; i < itemsEquipped.Length; i++)
            {
                if (itemsEquipped[i] != null && itemsEquipped[i].item != null)
                {
                    if (itemsEquipped[i].item.ItemID == _itemID)
                    return true;
                }
            }

            return false;
        }
    }


}