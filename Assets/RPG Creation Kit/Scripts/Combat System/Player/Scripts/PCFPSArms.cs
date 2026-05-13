using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;


namespace RPGCreationKit
{
    /// <summary>
    /// This class is used on the WeaponInHand of the Player to send a message to PlayerCombat when an animation event is triggered.
    /// </summary>
    public class PCFPSArms : MonoBehaviour
    {
        public Transform rHand;
        public Transform lHand;

        public SkinnedMeshRenderer hands;
        public SkinnedMeshRenderer arms;

        PlayerCombat playerCombat;
        GameObject projectile;
        GameObject spellProjectile;

        Equipment playerEquipment;
        // Use this for initialization
        void Start()
        {
            playerCombat = PlayerCombat.instance;
            playerEquipment = Equipment.PlayerEquipment;
        }

        public void AttackAnimationEvent()
        {
            playerCombat.currentWeaponOnHand.StartCasting(false, playerCombat.lastAttackIndex);
        }

        public void ChargedAttackAnimationEvent()
        {
            if (!RPGCreationKit.Player.RckPlayer.instance.isInThirdPerson && playerCombat.currentWeaponOnHand != playerCombat.defaultWeapon)
                playerCombat.currentWeaponOnHand.StartCasting(true, playerCombat.lastChargedAttackIndex);
        }

        public void EndAttackAnimationEvent()
        {
            playerCombat.currentWeaponOnHand.StopCasting();
        }

        public void PlayAttackSound()
        {
            if(!RPGCreationKit.Player.RckPlayer.instance.isInThirdPerson && playerCombat.currentWeaponOnHand != playerCombat.defaultWeapon)
                playerCombat.currentWeaponOnHand.PlayOneShot(playerCombat.currentWeaponOnHand.weaponItem.weaponAttacks[playerCombat.curAttackType].attackSound);
        }

        public void PlayChargedAttackSound()
        {
            if (!RPGCreationKit.Player.RckPlayer.instance.isInThirdPerson && playerCombat.currentWeaponOnHand != playerCombat.defaultWeapon)
                playerCombat.currentWeaponOnHand.PlayOneShot(playerCombat.currentWeaponOnHand.weaponItem.weaponChargedAttacks[playerCombat.curChargedAttackType].attackSound);
        }

        public void ResetAttackStateEvent()
        {
            playerCombat.ResetAttackStateEvent();
            playerCombat.isAttacking = false;
        }

        public void FirearmReloadEvent()
        {
            playerCombat.FirearmReloadEvent();
        }

        public void PlaySoundFromDatabase(string _id)
        {
            playerCombat.currentWeaponOnHand.audioSource.PlayOneShot(AudioClipsDatabase.GetItem(_id));
        }

        public void PutOneBulletAnim()
        {
            playerEquipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1++;
            Inventory.PlayerInventory.RemoveItem(playerEquipment.currentWeapon.ammo, 1, false);
            playerCombat.SetWeaponUI();
        }

        public void SetIsBlockingEvent()
        {
            playerCombat.isBlocking = true;
        }

        public void ResetIsUnbalancedEvent()
        {
            playerCombat.isUnbalanced = false;
        }

        /// <summary>
        /// Called when the player sheats the sword 
        /// </summary>
        public void OnDrawWeaponEvent()
        {
            if (playerEquipment == null)
                playerEquipment = Equipment.PlayerEquipment;

            if(playerEquipment.currentWeaponObject != null)
                playerEquipment.currentWeaponObject.SetActive(true);

            if (PlayerCombat.instance.currentWeaponOnHand.weaponItem.sOnDraw)
                PlayerCombat.instance.currentWeaponOnHand.PlayOneShot(PlayerCombat.instance.currentWeaponOnHand.weaponItem.sOnDraw);

        }

        /// <summary>
        /// Called when the player unsheats the sword
        /// </summary>
        public void OnUndrawWeaponEvent()
        {
            if (playerEquipment == null)
                playerEquipment = Equipment.PlayerEquipment;

            if (playerEquipment.currentWeaponObject != null)
                playerEquipment.currentWeaponObject.SetActive(false);

            playerCombat.isUndrawingWeapon = false;
        }

        /// <summary>
        /// Called when the sword has been sheated and is ready to attack
        /// </summary>
        public void OnWeaponIsDrawn()
        {
            playerCombat.weaponDrawn = true;
            playerCombat.isDrawingWeapon = false;
        }

        public void FPSSpawnAmmoOnRHand()
        {
            projectile = Instantiate(((AmmoItem)Equipment.PlayerEquipment.itemsEquipped[(int)EquipmentSlots.Ammo].item).projectile, rHand);
            projectile.SetLayer(RCKLayers.FirstPerson);
            playerCombat.currentProjectile = projectile.GetComponent<Projectile>();
        }

        public void SetProjectileOnHolder()
        {
            playerCombat.currentProjectile.transform.SetParent(playerCombat.projectileHolder);
            CrossbowReferences cbR = playerEquipment.currentWeaponObject.GetComponent<CrossbowReferences>();
            playerCombat.currentProjectile.transform.localPosition = cbR.projectileInHolderPos;
            playerCombat.currentProjectile.transform.localRotation = Quaternion.Euler(cbR.projectileInHolderRot);
        }

        public void FPSAmmoReady()
        {
            playerCombat.projectileNocked = true;
        }

        /// <summary>
        /// With the crossbow we can jump, run and undraw even if the weapon is loaded
        /// </summary>
        public void FPSAmmoReadyCrossbow()
        {
            playerCombat.projectileNocked = true;
            playerCombat.isAttacking = false;
            playerCombat.canAttack = true;
        }

        public void PlayCastSpellSound()
        {
            playerCombat.spellsAudioSource.PlayOneShot(SpellsKnowledge.Player.spellInUse.sOnCast);
        }

        public void CastSpellInit()
        {
            Spell curSpell = SpellsKnowledge.Player.spellInUse;

            Transform handT = (Equipment.PlayerEquipment.isUsingShield || Equipment.PlayerEquipment.isUsingTorch || (Equipment.PlayerEquipment.currentWeapon != null && Equipment.PlayerEquipment.currentWeapon.weaponType == WeaponType.Bow)) ? rHand : lHand;

            switch (curSpell.mode)
            {
                case SpellModes.Projectile:
                    spellProjectile = Instantiate(curSpell.magicProjectile, handT);
                    spellProjectile.SetLayer(RCKLayers.FirstPerson, true);
                    playerCombat.currentSpellProjectile = spellProjectile.GetComponent<SpellProjectile>();
                    playerCombat.currentSpellProjectile.Init(Entity.GetPlayerEntity(), true);
                    break;

                case SpellModes.Touch:
                    spellProjectile = Instantiate(curSpell.magicProjectile, handT);
                    spellProjectile.SetLayer(RCKLayers.FirstPerson, true);
                    playerCombat.currentSpellProjectile = spellProjectile.GetComponent<SpellProjectile>();
                    playerCombat.currentSpellProjectile.Init(Entity.GetPlayerEntity(), true);
                    break;

                case SpellModes.Self:
                    spellProjectile = Instantiate(curSpell.magicProjectile, handT);
                    spellProjectile.SetLayer(RCKLayers.FirstPerson, true);
                    playerCombat.currentSpellProjectile = spellProjectile.GetComponent<SpellProjectile>();
                    playerCombat.currentSpellProjectile.Init(Entity.GetPlayerEntity(), true);
                    break;
            }
        }

        public void CastSpellAttackEvent()
        {
            // Instantiate/Do Stuff
            Spell curSpell = SpellsKnowledge.Player.spellInUse;
            Ray ray;
            switch (curSpell.mode)
            {
                case SpellModes.Projectile:
                    ray = Player.RckPlayer.instance.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                    playerCombat.currentSpellProjectile.Shoot(Entity.GetPlayerEntity(), ray.direction, true);
                    playerCombat.currentSpellProjectile.ExecuteOnCasterEffects();
                    break;

                case SpellModes.Touch:
                    ray = Player.RckPlayer.instance.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                    if(playerCombat.currentSpellProjectile != null)
                    {
                        playerCombat.currentSpellProjectile.Shoot(Entity.GetPlayerEntity(), ray.direction, true);
                        playerCombat.currentSpellProjectile.ExecuteOnCasterEffects();
                    }
                    break;

                case SpellModes.Self:
                    playerCombat.currentSpellProjectile.Explode();
                    playerCombat.currentSpellProjectile.ExecuteOnCasterEffects();
                    break;
            }
        }

        public void CastSpellEndAndDestroy()
        {

        }

        public void CastSpellResetEvent()
        {
            playerCombat.ResetAttackStateEvent();
            playerCombat.isAttacking = false;
            playerCombat.isCastingSpell = false;
        }

        public void ResetGrenadeStateEvent()
        {
            playerCombat.ResetAttackStateEvent();
            playerCombat.isAttacking = false;

            playerCombat.isThrowingGrenade = false;
            playerCombat.grenadeInput = false;
            playerCombat.grenadeThrown = false;
        }

        public void ThrowGrenadeEvent()
        {
            playerCombat.ThrowGrenadeEvent();
        }

        public void FPSSpawnGrenadeOnHand()
        {
            playerCombat.currentThrowable = Instantiate(((WeaponItem)Equipment.PlayerEquipment.itemsEquipped[(int)EquipmentSlots.Throwable].item).WeaponOnHand, lHand).GetComponent<Throwable>();
            RCKLayers.SetLayer(playerCombat.currentThrowable.gameObject, RCKLayers.FirstPerson, true);
            playerCombat.currentThrowable.thrownByPlayer = true;

            if (RCKSettings.TRIGGER_EXPLOSIVE_AS_SOON_AS_HOLD)
            {
                if(playerCombat.currentThrowable.throwableType == ThrowableType.Explosive)
                    playerCombat.currentThrowable.TriggerExplosive();
            }
        }

        public void SetControlledByAnim(int val)
        {
            RckPlayer.instance.controlledByAnim = (val == 0 ? false : true);
        }

        public void SetControlledByAnimXVal(float val)
        {
            RckPlayer.instance.controlledByAnimX = val;
        }

        public void SetControlledByAnimZVal(float val)
        {
            RckPlayer.instance.controlledByAnimZ = val;
        }

        public void SetControlledByRunSpeed(int val)
        {
            RckPlayer.instance.runSpeedIfControlledByAnim = (val == 0 ? false : true);
        }

        public void SetStopPlayerInputByAnim(int val)
        {
            RckPlayer.instance.stopPlayerInputByAnim = (val == 0 ? false : true);
        }

        public void RestoreAnimatorLayerWeights()
        {
            PlayerCombat.instance.RestoreLayerWeight();
        }
    }
}