using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit.AI
{
    /// <summary>
    /// Determines the status of the AI
    /// </summary>
    public class AIStatus : AIBase, IHittable, IDamageable
    {
        public bool isAlive = true;
        public bool isHostile = false;
        public bool isHostileAgainstPC = false;
        public bool isInCombat = false;
        public bool isDrawingWeapon = false;

        public bool isFleeing = false;
        public bool isUnconscious = false;
        public bool enterInCombatCalled = false; // called as soon as the ai gets hit
        public bool isInOfflineMode = false;    // Defines if this AI is in Offline mode or not

        public bool canBePickPocketed = true;

        // If this is set to true, the AI Agent cannot die
        public bool isEssential = false;

        public EntityAttributes attributes;

        [Space(5)]
        public bool usesRagdoll;
        public Ragdoll ragdoll;

        [Space(10)]

        [SerializeField] Events onDeathEvents;
        private bool recoveringStamina;
        private float recoverStaminaAmount = 10f;
        private float recoverAfterActionDelay = 2.5f;

        public bool awardsXPOnKill = true;

        public virtual void Damage(DamageContext context)
        {
            if (!isAlive)
                return;

            isHostile = true;
            attributes.CurHealth -= context.amount;

            if(context.sender != null && context.sender.CompareTag("Player"))
            {
                RckPlayer.instance.OnPlayerHits();
            }

            // Check for essential
            if (attributes.CurHealth <= 0)
            {
                if (!isEssential)
                {
                    PlayDeathSound();
                    Die();

                    if(context.sender.CompareTag("Player") && awardsXPOnKill)
                    {
                        ulong xpAwardedWhenKilled = RCKSettings.CalculateXPGainedOnAIKill(attributes.curLevel);
                        xpAwardedWhenKilled = (ulong)(xpAwardedWhenKilled * attributes.xpMultiplierOnDeath);

                        AlertMessage.instance.InitAlertMessage("+" + xpAwardedWhenKilled.ToString()+" XP.", 1.5f);
                        EntityAttributes.PlayerAttributes.GainXP(xpAwardedWhenKilled);
                    }
                }
                else
                    attributes.CurHealth = attributes.MaxHealth;
            }

            m_Anim.SetTrigger("HasBeenHit");
        }

        public void PlayDeathSound()
        {
            if (aiSounds != null)
            {
                if (aiSounds.deathSounds.Length > 0)
                {
                    int n = Random.Range(0, aiSounds.deathSounds.Length);

                    if (aiSounds.deathSounds[n] != null)
                    {
                        audioSource.clip = aiSounds.deathSounds[n];
                        audioSource.Play();
                    }
                }
            }
        }

        public virtual void DamageBlocked(DamageContext damageContext)
        {

        }


        public void InvokeResetRecover(bool _customAmount = false, float _amount = 0.0f)
        {
            if (!_customAmount)
                _amount = recoverAfterActionDelay;

            Invoke("ResetRecover", _amount);
        }

        public void ResetRecover()
        {
            recoveringStamina = true;
            StartCoroutine("RecoverStamina");
        }

        public IEnumerator RecoverStamina()
        {
            while (recoveringStamina)
            {
                if (attributes.CurStamina < attributes.MaxStamina)
                {
                    attributes.CurStamina += recoverStaminaAmount * Time.deltaTime;
                    attributes.CurStamina = Mathf.Clamp(attributes.CurStamina, 0, attributes.MaxStamina);
                    yield return null;
                }
                else
                    yield return null;
            }
        }

        public void StopRecoveringStamina()
        {
            StopCoroutine("RecoverStamina");
            CancelInvoke("ResetRecover");
            recoveringStamina = false;
        }


        public virtual void Die()
        {
            if(!isEssential)
            {
                isAlive = false;
                isHostile = false;
                isHostileAgainstPC = false;
                isInCombat = false;

                if(onDeathEvents.EvaluateConditions())
                {
                    if(onDeathEvents.questDealers.Count > 0)
                        QuestManager.instance.QuestDealerActivator(onDeathEvents.questDealers);

                    if (onDeathEvents.questUpdaters.Count > 0)
                        QuestManager.instance.QuestUpdaterActivator(onDeathEvents.questUpdaters);

                    if (onDeathEvents.consequences.Count > 0)
                        ConsequenceManager.instance.ConsequencesActivator(gameObject, onDeathEvents.consequences);
                }

                if (usesRagdoll)
                    ragdoll.ToggleRagdoll(false);
                else
                    m_Anim.SetTrigger("Die");
            }
            else
            {
                isUnconscious = true;
                isHostile = false;
                isHostileAgainstPC = false;
                isInCombat = false;

                if (usesRagdoll)
                    ragdoll.ToggleRagdoll(false);
                else
                    m_Anim.SetTrigger("Die");

                // Getup after x seconds? TODO
            }
        }

        public void GenerateBlood()
        {
            
        }

        public void GenerateDecal()
        {
            
        }

        public float GetCurrentHP()
        {
            return attributes.CurHealth;
        }

        public string GetEntityID()
        {
            return entityID;
        }

        public string GetEntityName()
        {
            return entityName;
        }

        public float GetMaxHP()
        {
            return attributes.MaxHealth;
        }

        public void Hit(Vector3 hitDir, float forceAmount = 25, DamageContext damageContext = null)
        {

        }

        public bool IsAlive()
        {
            return isAlive;
        }

        public bool IsHostile()
        {
            return isHostile;
        }

        public bool IsHostileAgainstPC()
        {
            return isHostileAgainstPC;
        }

        public bool IsUnconscious()
        {
            return isUnconscious;
        }

        public bool ShouldDisplayHealthIfHostile()
        {
            return true;
        }

        bool IDamageable.Bleeds()
        {
            return attributes.bleeds;
        }

        void IDamageable.InstantiateBlood(Vector3 pos)
        {
            var blood = Instantiate(attributes.bloodPrefab, pos, Quaternion.identity, this.transform);
            Destroy(blood, 2.5f);
        }

        public Entity GetEntity()
        {
            return this;
        }

        public bool IsRckAI()
        {
            return true;
        }
    }
}