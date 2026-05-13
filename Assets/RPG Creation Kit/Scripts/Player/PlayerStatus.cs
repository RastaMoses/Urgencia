using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.Player
{
    public class PlayerStatus : PlayerBase, IHittable, IDamageable
    {
        public bool isAlive = true;
        [SerializeField] GameObject bloodParticlePrefab;


        [SerializeField] bool recoveringStamina = false;
        [SerializeField] public float recoverStaminaAmount = 10f;
        [SerializeField] public float recoverAfterActionDelay = 2.5f;

        [SerializeField] bool recoveringMana = true;
        [SerializeField] public float recoverManaAmount = 5.0f;
        [SerializeField] public float recoverAfterManaUseDelay = 2.5f;

        [SerializeField] bool recoveringHealth = false;
        [SerializeField] public float recoverHealthAmount = 0;
        [SerializeField] public float recoverAfterHitDelay = 2.5f;


        [HideInInspector] private IDamageable PlayerDamageable;

        public override void Start()
        {
            base.Start();

            // Load/Set initial stats
            playerAttributes.CurHealth = playerAttributes.MaxHealth;
            playerAttributes.CurStamina = playerAttributes.MaxStamina;

            PlayerDamageable = GetComponent<IDamageable>();

            // Recover mana if set to
            if (recoveringMana)
                StartCoroutine(RecoverMana());
        }

        public void InvokeResetRecover(bool _customAmount = false, float _amount = 0.0f)
        {
            if (!_customAmount)
                _amount = recoverAfterActionDelay;

            Invoke("ResetRecover", _amount);
        }

        public void InvokeResetRecoverHealth()
        {
            Invoke("ResetRecoverHealth", recoverAfterHitDelay);
        }

        public void InvokeResetRecoverMana()
        {
            Invoke("ResetRecoverMana", recoverAfterManaUseDelay);
        }

        public override void Update()
        {
            base.Update();

            /*
            if ((!isAlive || playerAttributes.CurHealth <= 0) && !PlayerDeathScreen.instance.ui.activeInHierarchy)
            {
                isAlive = false;
                PlayerDeathScreen.instance.ShowDeathScreen();
            }
            */

            if (isAlive && playerAttributes.CurHealth <= 0.1f)
            {
                PlayerDeath();
            }
        }

        public void ResetRecover()
        {
            recoveringStamina = true;
            StartCoroutine(RecoverStamina());
        }

        public IEnumerator RecoverStamina()
        {
            while (recoveringStamina && isAlive)
            {
                if (playerAttributes.CurStamina < playerAttributes.MaxStamina)
                {
                    playerAttributes.CurStamina += recoverStaminaAmount * Time.deltaTime;
                    playerAttributes.CurStamina = Mathf.Clamp(playerAttributes.CurStamina, 0, playerAttributes.MaxStamina);
                    yield return null;
                }
                else
                    yield return null;
            }
        }

        public IEnumerator RecoverMana()
        {
            while (recoveringMana && isAlive)
            {
                if (playerAttributes.CurMana < playerAttributes.MaxMana)
                {
                    playerAttributes.CurMana += recoverManaAmount * Time.deltaTime;
                    playerAttributes.CurMana = Mathf.Clamp(playerAttributes.CurMana, 0, playerAttributes.MaxMana);
                    yield return null;
                }
                else
                    yield return null;
            }
        }

        public IEnumerator RecoverHealth()
        {
            while (recoveringHealth && isAlive)
            {
                if (playerAttributes.CurHealth < playerAttributes.MaxHealth)
                {
                    playerAttributes.CurHealth += recoverHealthAmount * Time.deltaTime;
                    playerAttributes.CurHealth = Mathf.Clamp(playerAttributes.CurHealth, 0, playerAttributes.MaxHealth);
                    yield return null;
                }
                else
                    yield return null;
            }
        }
        public void ResetRecoverHealth()
        {
            recoveringHealth = true;
            StartCoroutine(RecoverHealth());
        }

        public void ResetRecoverMana()
        {
            recoveringMana = true;
            StartCoroutine(RecoverStamina());
        }

        public void StopRecoveringStamina()
        {
            StopCoroutine(RecoverStamina());
            CancelInvoke("ResetRecover");
            recoveringStamina = false;
        }

        public void StopRecoveringHealth()
        {
            StopCoroutine(RecoverHealth());
            CancelInvoke("ResetRecoverHealth");
            recoveringHealth = false;
        }

        public void StopRecoveringMana()
        {
            StopCoroutine(RecoverMana());
            CancelInvoke("ResetRecoverMana");
            recoveringMana = false;
        }

        public bool hasBeenDamaged = false; // used to broadcast to third person animator
        void IDamageable.Damage(DamageContext damageContext)
        {
            if (!isAlive)
                return;

            // TODO do the equipment decrese of the damage (also have to do this for npcs)
            float finalDamage = damageContext.amount;
            finalDamage -=  (finalDamage * equipment.ArmorRate) / 100;

            if (finalDamage <= 0)
                finalDamage = 1;

            playerAttributes.DamageHealth(finalDamage, true, RCKSettings.PLAYER_DAMAGE_SPEED);
            
            if(!damageContext.wasBlocked)
                BloodScreenEffect.instance.OnHit();

            StopRecoveringHealth();
            InvokeResetRecoverHealth();

            // Play damage sound
            GameAudioManager.instance.PlayRandomPlayerDamageSound();

            hasBeenDamaged = true;

            if (playerAttributes.CurHealth - finalDamage <= 0.1f)
            {
                PlayerDeath();
            }
        }

        void IDamageable.DamageBlocked(DamageContext damageContext)
        {
            if (!isAlive)
                return;

            // TODO do the equipment decrese of the damage (also have to do this for npcs)
            float baseDamage = damageContext.amount;
            float finalDamage = damageContext.amount;
            finalDamage -= (finalDamage * equipment.ArmorRate) / 100;

            finalDamage *= (equipment.isUsingShield) ? equipment.currentShield.blockingMultiplier : equipment.currentWeapon.BlockingMultiplier;

            if (finalDamage <= 0)
                finalDamage = 0;

            float staminaDmg = baseDamage;
            staminaDmg *= (equipment.isUsingShield) ? equipment.currentShield.staminaDrainMultiplier : equipment.currentWeapon.StaminaDrainMultiplierOnBlock;
            playerAttributes.DamageStamina(staminaDmg, true, RCKSettings.DRAIN_STAMINA_ON_ATTACKBLOCKED_SPEEDAMOUNT);
            RckPlayer.instance.StopRecoveringStamina();
            RckPlayer.instance.InvokeResetRecover();

            PlayerCombat.instance.fpcAnim.SetTrigger("hasBlockedAttack");
            PlayerCombat.instance.tpsAnim.SetTrigger("hasBlockedAttack");

            PlayerCombat.instance.fpcAnim.Update(0);
            PlayerCombat.instance.tpsAnim.Update(0);

            AudioClip blockingSound = (equipment.isUsingShield) ? equipment.currentShield.blockingSound : equipment.currentWeapon.blockSound;

            if (blockingSound != null)
                PlayerCombat.instance.currentWeaponOnHand.PlayOneShot(blockingSound);

            playerAttributes.DamageHealth(finalDamage, true, RCKSettings.PLAYER_DAMAGE_SPEED);

            StopRecoveringHealth();
            InvokeResetRecoverHealth();

            if (playerAttributes.CurHealth - finalDamage <= 0.1f)
                PlayerDeath();

            // Check if stamina is zeroed, if so, play animation
            if(playerAttributes.CurStamina - staminaDmg <= 0)
            {
                PlayerCombat.instance.OnAttackIsBlocked();
            }
        }

        public virtual void PlayerDeath()
        {
            isAlive = false;

            if (ThirdPersonPlayer.instance.currentWeaponOnHand != null)
                ThirdPersonPlayer.instance.currentWeaponOnHand.DisableCollisionWithOwner();
        }

        void IDamageable.Die()
        {
            throw new System.NotImplementedException();
        }

        void IDamageable.GenerateBlood()
        {

        }

        void IHittable.GenerateDecal()
        {

        }

        float IDamageable.GetCurrentHP()
        {
            return playerAttributes.CurHealth;
        }

        string IDamageable.GetEntityID()
        {
            return entityID;
        }

        string IDamageable.GetEntityName()
        {
            return entityName;
        }

        float IDamageable.GetMaxHP()
        {
            return playerAttributes.MaxHealth;
        }

        void IHittable.Hit(Vector3 hitDir, float forceAmount, DamageContext damageContext)
        {
            
        }

        #region Unused
        bool IDamageable.IsAlive()
        {
            return isAlive;
        }

        bool IDamageable.IsHostile()
        {
            return false;
        }

        bool IDamageable.IsHostileAgainstPC()
        {
            return false;
        }

        bool IDamageable.IsUnconscious()
        {
            return false;
        }

        bool IDamageable.ShouldDisplayHealthIfHostile()
        {
            return false;
        }
        #endregion

        public bool Bleeds()
        {
            return bloodParticlePrefab != null && isAlive;
        }

        public void InstantiateBlood(Vector3 pos)
        {
            var blood = Instantiate(bloodParticlePrefab, pos, Quaternion.identity, null);
            Destroy(blood, 2.5f);
        }

        public IDamageable GetIDamageable()
        {
            return PlayerDamageable;
        }

        public Entity GetEntity()
        {
            return this;
        }

        public bool IsRckAI()
        {
            return false;
        }
    }
}