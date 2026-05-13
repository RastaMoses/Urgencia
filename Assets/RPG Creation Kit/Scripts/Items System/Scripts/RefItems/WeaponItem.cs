using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    // From Update 1.7, weapons are divided in two classes
    public enum WeaponClass
    {
        Melee,
        Firearm,
        Throwable
    }

    public enum WeaponType
    {
        None = 0,
        BladeOneHand = 1,
        BladeTwoHands = 2,
        BluntOneHand = 3,
        BluntTwoHands = 4,
        Bow = 5,
        Crossbow = 6,
        DaggerOneHand = 7,
        Unarmed = 8,
        FIREARM_OneHanded_1 = 9,
        FIREARM_TwoHanded_1 = 10,
    }


    public enum FirearmFireType
    {
        Auto = 0,
        Semi
    };

    public enum ReloadType
    {
        Normal = 0,
        Single
    };

    /// <summary>
    /// Class for Weapons, like swords and maces.
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon", menuName = "RPG Creation Kit/Items/WeaponItem", order = 1)]
    public class WeaponItem : Item
    {
        public override ItemTypes itemType { get { return ItemTypes.WeaponItem; } }


        public GameObject WeaponOnHand;
        public GameObject WeaponSheathed;

        
        // Shared
        public WeaponClass weaponClass = WeaponClass.Melee;
        public WeaponType weaponType;
        public WeightType weightType;

        public float Health = 100.0f;      // Health of the Item, reduced when the character recieve damage
        public float Reach = 2.0f;         // Distance of hit
        public float Speed = 1.0f;         // Speed of the swing
        public float Damage = 10.0f;
        [Range(0f, 1f)]
        public float BlockingMultiplier = .5f;
        public float StaminaDrainMultiplierOnBlock = 1.5f;

        public AudioClip blockSound;

        public RuntimeAnimatorController fpsAnimatorController;
        public RuntimeAnimatorController tpsAnimatorController;

        // Melee Weapons
        public int AttackTypes = 3;
        public WeaponAttack[] weaponAttacks;
        public int chargedAttackTypes = 3;
        public WeaponAttack[] weaponChargedAttacks;

        public float attackChainTime = 1.5f;
        public float chargedAttackChainTime = 1.5f;

        public float aggroModifier = 1;

        // Firearms
        public FirearmFireType fireType;
        public ReloadType reloadType = ReloadType.Normal;
        public AmmoItem ammo;
        public int clipRounds = 30;
        public int ammoPerShot = 1;
        public int projectilesPerShot = 1;
        public float fireRate = 10; // ms
        public float aimSpread = 0;
        public float hipSpread = 0.5f;
        public float sightFov = 60;
        public float criticalMultiplier = 2.0f;
        public float recoilHit = 2.0f;

        public bool animateAim = true; // true if there are animation for aiming/firing while aiming, it's false for the shotgun

        public int nAnimFireFromHip = 1;
        public int nAnimFireFromSight = 1;

        public Vector3 aimingHolderPos;
        public Vector3 aimingHolderRot;

        public Vector3 aimingHolderPosCrouched;
        public Vector3 aimingHolderRotCrouched;

        public float swayWhileAiming = 0.005f;

        public AudioClip fireSound;
        public AudioClip emptyFireSound;

        [Range(0, 100)]
        public float staggeringChancePerShot = 10; // 0 - 100

        public GameObject shellCase;
        public GameObject defaultHole;
        // Sounds

        // Throwables
        public float explodesAfter;
        public GameObject explosionObject;
        public float explosionRadius;
        public float explosionForce;

        public bool clippingWeapon = false; // set this to true if the weapon clips the body during animations, means when the weapon will be dropped may cause issues with ragdoll

        // Sneak attack
        public float sneakAttackMultiplier = 1.0f;

        public AudioClip sOnDraw;
        public AudioClip sOnSheathe;
    }
}