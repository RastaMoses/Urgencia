using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.BehaviourTree;
using System.Linq;
using RPGCreationKit.Player;

namespace RPGCreationKit.AI
{
    /// <summary>
    /// Adds the Combat System to the AI.
    /// </summary>
    public class AICombatSystem : AIPerception, IMeleeAttacker, IRangedAttacker
    {
        [Header("References")]
        public RuntimeAnimatorController defaultAnimatorController; // The RuntimeAnimatorController that needs to be played when the entity is not in combat

        public WeaponItem defaultWeapon;            // This is the weapon that will be equipped if there's none (usually it is fists)
        public WeaponOnHand defaultWeaponOnHand;
        public SpellsKnowledge spellsKnowledge;

        public GameObject blockingArea;

        public int YNewProperty
        { get; set; }

        public float CurrentHealth
        {
            get
            {
                return attributes.CurHealth;
            }
        }

        public Projectile currentProjectile;
        public AudioSource combatAudioSource;

        // Spells Related
        GameObject spellProjectile;
        SpellProjectile currentSpellProjectile;

        [Space(10)]
        [Header("Status")]
        public float targetDistance2D = 99999.9f;

        public bool weaponDrawn = false;
        public bool canAttack = true;
        public bool isAttacking = false; // Is playing attack animation
        public bool isCastingSpell = false;
        public bool spellsLimitedByMana = false;
        public bool isBlocking = false;
        public bool isUnbalanced = false;

        public bool lastAttackWasCharged;
        public int curAttackType = 0;
        public int curChargedAttackType = 0;
        [HideInInspector] public int lastAttackIndex = 0;
        [HideInInspector] public int lastChargedAttackIndex = 0;

        bool wantsToAttack;

        public bool hasToBlock = false;

        public int combatType = 0;
        public bool helpsMembersOfSameFactions = true;

        public bool wasWalkingBeforeEnteringCombat = true;

        public EntityAttributes mainTargetAttributes;

        bool firearmGoesInCover = true;
        float healthBeforeGoingToCover;

        // Player damage tolerance
        public bool pdtEnabled;
        public float pdtTimeslice = 120.0f;
        public int pdtHitsBeforeAggro = 2;

        public bool pdtCounting = false;
        public float pdtCurTimer = 0.0f;
        public int pdtCurCount = 0;

        // BehaviourTrees

        // Lose Aggro
        private Coroutine combatCoroutine;
        public bool canLoseAggro = false;
        public Vector3 combatAggroStartPos = Vector3.zero;
        public float loseAggroDistFromStart = 25.0f;

        public override void Update()
        {
            base.Update();

            if (isAlive && !isInOfflineMode)
            {
                BlockCheck();

                CheckForEnemies();

                if (m_isInCombat)
                    WhileInCombat();

                // Go to cover cooldown
                if (goToCoverCooldown > 0)
                    goToCoverCooldown -= Time.deltaTime;
                else
                    goToCoverCooldown = -1;

                if(m_isInCombat)
                    curThrowableTimer += 1 * Time.deltaTime;

                if (isInCover)
                    LookAtTarget();

                // PDT (Player Damage Tolerance)
                if(pdtCounting)
                {
                    pdtCurTimer += Time.deltaTime;

                    // Reset
                    if(pdtCurTimer > pdtTimeslice)
                    {
                        pdtCurTimer = 0;
                        pdtCurCount = 0;
                        pdtCounting = false;
                    }
                }
            }
            else if(!isAlive)
            {
                // Force health to be negative if dead
                attributes.CurHealth = -1;
            }
        }

        // Use this for initialization
        public override void Start()
        {
            YNewProperty = 10000;

            base.Start();

            if (!equipment.currentWeapon && defaultWeapon)
            {
                equipment.currentWeapon = defaultWeapon;
                currentWeaponOnHand = defaultWeaponOnHand;
                defaultWeaponOnHand.gameObject.SetActive(true);
                equipment.currentWeaponObject = null;

                canAttack = true;

                if (weaponDrawn)
                {
                    m_Anim.runtimeAnimatorController = equipment.currentWeapon.tpsAnimatorController;
                    m_Anim.SetTrigger("Draw");
                }

                m_Anim.SetBool("isDrawn", weaponDrawn);
            }

            healthBeforeGoingToCover = GetMaxHP() / 2;
        }

        [ContextMenu("Draw Weapon"), AIInvokable]
        public void DrawWeapon()
        {
            combatType = (int)equipment.currentWeapon.weaponType;
            m_Anim.SetInteger("CombatType", combatType);

            //m_Anim.runtimeAnimatorController = currentWeapon.tpsAnimatorController;

            m_Anim.SetTrigger("Draw");
            m_Anim.SetBool("isDrawn", true);

            //FullComboChainReset();
            wantsToAttack = false;
            isDrawingWeapon = true;

            weaponDrawn = true;
        }

        [ContextMenu("Undraw Weapon")]
        public void UndrawWeapon()
        {
            if (weaponDrawn == false || !isAlive)
                return;

            combatType = (int)equipment.currentWeapon.weaponType;
            m_Anim.SetInteger("CombatType", combatType);

            weaponDrawn = false;
            m_Anim.SetBool("isDrawn", false);
            m_Anim.SetTrigger("Undraw");

            wantsToAttack = false;
        }

        /// <summary>
        /// Called by animation event, resets the animator runtime controller to default
        /// </summary>
        public void UndrawWeaponEvent()
        {
            //m_Anim.runtimeAnimatorController = defaultAnimatorController;
            isInCombat = false;
            m_Anim.SetBool("InCombat", false);
            FullComboChainReset();
        }

        /// <summary>
        /// Called by animation event, resets the animator runtime controller to default
        /// </summary>
        public void DrawWeaponEvent()
        {
            //m_Anim.runtimeAnimatorController = defaultAnimatorController;
            isInCombat = true;
            m_Anim.SetBool("InCombat", true);
            FullComboChainReset();

            isSwitchingWeapon = false;
            isDrawingWeapon = false;
        }

        public bool m_isInCombat = false;

        public void EnterInCombatAgainst(Entity _entiy)
        {
            if (m_isInCombat || !isAlive)
                return;

            if (combatCoroutine != null)
                return;
            
            wasWalkingBeforeEnteringCombat = isWalking;

            OnEnterInCombatWeaponsCheck();

            if(aiSounds && aiSounds.onEnterInCombatLines.Length > 0)
            {
                int idx = Random.Range(0, aiSounds.onEnterInCombatLines.Length - 1);
                audioSource.Stop();
                audioSource.clip = aiSounds.onEnterInCombatLines[idx];
                audioSource.Play();

                if (RCKSettings.NPCS_FACIAL_ANIM_ENABLED)
                    QuickDialogueMouthAnim(audioSource.clip);
            }
            
            enterInCombatCalled = true;

            if(combatCoroutine == null)
                combatCoroutine = StartCoroutine(EnterInCombatTask(_entiy));
        }

        // Checks if the entity has the weapons to enter combat, otherwise switch them
        public void OnEnterInCombatWeaponsCheck()
        {
            // Check if this entity has no weapon at all, if it's the case (has been pickpocketed) try to equip a weapon, otherwise equip the unarmed
            if (equipment.currentWeapon == null)
            {
                if (SwitchToBestMeleeWeapon())
                {

                }
                /* to do
                else if(SwitchToBestRangedWeapon())
                {

                }
                */
                else
                {
                    SwitchToDefaultWeapon();
                }
            }
            
            // If the AI has a bow but no ammo equipped
            if (equipment.currentWeapon.weaponType == WeaponType.Bow && (equipment.currentAmmo == null || equipment.currentAmmoObject == null))
            {
                // Try to equip ammo
                if (EquipAmmoForCurrentWeapon() == false)
                {
                    // Otherwise switch to what's available
                    if (SwitchToBestMeleeWeapon())
                    {

                    }
                    /* to do
                    else if(SwitchToBestRangedWeapon())
                    {

                    }
                    */
                    else
                    {
                        SwitchToDefaultWeapon();
                    }
                }
            }
        }

        bool doonce = false;
        public IEnumerator EnterInCombatTask(Entity _entiy)
        {
            yield return new WaitForEndOfFrame();

            if (isUsingActionPoint)
            {
                if (!doonce)
                {
                    shouldUseNPCActionPoint = false;
                    isReachingActionPoint = false;
                    StopUsingNPCActionPoint();
                    doonce = true;
                }

                while (isUsingActionPoint)
                {
                    yield return null;
                }
                agent.enabled = true;
            }

            if (isInConversation)
                while (isInConversation)
                    yield return null;


            doonce = false;
            // Movements
            StopFollowingPath();

            ResetActionPointAgentState();

            // Stop following the current NavMeshPath
            if (navmeshPath == null)
                navmeshPath = new UnityEngine.AI.NavMeshPath();

            if (agent.isActiveAndEnabled)
                agent.CalculatePath(agent.transform.position + (transform.forward * 0.1f), navmeshPath);

            // Set the main target
            SetTarget(_entiy.gameObject);

            // Try to get the target attributes
            mainTargetAttributes = mainTarget.GetComponent<EntityAttributes>();

            enemyTargets.Add(new VisibleEnemy(_entiy, mainTargetAttributes, new AggroInfo(50)));
            m_isInCombat = true;

            // Initialize throwable timer
            curThrowableTimer = Random.Range(0, throwableCooldown + (throwableCooldown / 3));

            OnEnterInCombat();
            combatCoroutine = null;
            yield break;
        }

        /// TO ALLOW OVERRIDE THE TASK OR AT LEAST RUN CODE AT THE END
        public virtual void OnEnterInCombat()
        {

        }

        /// <summary>
        /// Returns true if the Entity given is an Enemy Target
        /// </summary>
        /// <returns></returns>
        public bool IsFightingEntity(Entity _entity)
        {
            for (int i = 0; i < enemyTargets.Count; i++)
                if (enemyTargets[i].m_entity == _entity)
                    return true;

            return false;
        }

        public int SortByAggro(VisibleEnemy a1, VisibleEnemy a2)
        {
            return a1.m_aggro.aggroValue.CompareTo(a2.m_aggro.aggroValue);
        }

        [ContextMenu("Leave Combat")]
        public virtual void LeaveCombat()
        {
            if (!isAlive || !isInCombat)
                return;

            ClearTarget();
            mainTargetAttributes = null;
            SetLookAtTarget(false);
            hasToBlock = false;

            //if (setMainTarget)
            //    mainTarget = setMainTarget;

            // Stop following the current NavMeshPath
            try
            {
                if(agent.isOnNavMesh)
                    agent.CalculatePath(agent.transform.position + (transform.forward * 0.1f), navmeshPath);
            }
            catch
            {

            }

            isInAttackMovementLock = false;
            isInAttackRotationLock = false;

            m_isInCombat = false;
            enterInCombatCalled = false;
            // Undraw
            UndrawWeapon();

            // Restore walking to before combat
            if(wasWalkingBeforeEnteringCombat)
                Movements_SwitchToWalk();

            // Default is seek
            selectedSteeringBehaviour = SteeringBehaviours.Seek;

        }

        public float curCombatPulse = 999999; // initialize at this value to have an instant pulse
        public void WhileInCombat()
        {
            if (!m_isInCombat)
                return;

            if(canLoseAggro)
            {
                /*
                for(int i = enemyTargets.Count-1; i >= 0; i--)
                {
                    if(Vector3.Distance(transform.position, enemyTargets[i].m_entity.transform.position) > loseAggroDistFromStart)
                        enemyTargets.RemoveAt(i);
                }

                if (enemyTargets.Count == 0)
                {
                    LeaveCombat();

                    if(combatCoroutine != null)
                        StopCoroutine(combatCoroutine);

                    m_isInCombat = false;
                    return;
                }
                */
            }

            curCombatPulse += AggroSettings.COMBAT_PULSE_RATE * Time.deltaTime;
            if (curCombatPulse >= AggroSettings.COMBAT_PULSE_RATE)
            {
                // Update aggro pulse and list
                for (int i = 0; i < enemyTargets.Count; i++)
                {
                    if (enemyTargets[i].m_entity == null|| enemyTargets[i].m_entity == this || enemyTargets[i].m_entityAttributes.CurHealth <= 0)
                    {
                        enemyTargets.RemoveAt(i);
                        continue;
                    }

                    bool removedInFactionCheck = false;
                    for(int j = 0; j < belongsToFactions.Count; j++)
                    {
                        if (GetInFaction(belongsToFactions[j].ID) && (enemyTargets[i].m_entity as EntityFactionable).GetInFaction(belongsToFactions[j].ID) && !belongsToFactions[j].friendlyFire && enemyTargets[i].m_entity != Entity.GetPlayerEntity())
                        {
                            enemyTargets.RemoveAt(i);
                            removedInFactionCheck = true;
                            break;
                        }
                    }
                    if (removedInFactionCheck)
                        continue;

                    enemyTargets[i].m_aggro.CombatPulse();
                    enemyTargets[i].m_aggro.AlterAggroValue(AggroSettings.Modifier.Distance, Vector3.Distance(transform.position, enemyTargets[i].m_entity.transform.position));
                }

                if(enemyTargets.Count > 0)
                    enemyTargets = enemyTargets.OrderByDescending(a => a.m_aggro.aggroValue).ToList();

                curCombatPulse = 0;

                // Always set the most threating enemy as mainTarget
                if (enemyTargets.Count > 0)
                {
                    if (enemyTargets[0].m_entity != null)
                    {
                        SetTarget(enemyTargets[0].m_entity.gameObject);

                        if (aiLookAt != null && enemyTargets[0].m_entity.entityFocusPart != null)
                            aiLookAt.ForceToLookAtTarget(enemyTargets[0].m_entity.entityFocusPart.transform);

                        mainTargetAttributes = enemyTargets[0].m_entityAttributes;
                    }
                }
                else
                {
                    if (aiLookAt != null)
                        aiLookAt.StopForcingLookAtTarget();

                    LeaveCombat();
                }
            }

            // if is using bow and is standing still always face target
            if(currentWeaponOnHand.weaponItem.weaponType == WeaponType.Bow || currentWeaponOnHand.weaponItem.weaponType == WeaponType.Crossbow)
            {
                if(isStopped && mainTarget != null)
                {
                    // Rotate torwards target
                    var lookAt = Quaternion.LookRotation(mainTarget.position - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, Time.deltaTime * generalRotationSpeed);
                }
            }
        }

        public void CheckForEnemies()
        {
            if (!isLoaded || isInConversation) // if this is not loaded skip until it is
                return;

            for(int i = 0; i < visibleAI.Count; i++)
            {
                if (visibleAI[i] == null || !visibleAI[i].isLoaded) // If the entity is not loaded we have no way to tell if he's dead or not
                    continue;

                if (visibleAI[i].isAlive && !enemyTargets.Any(t => t.m_entity == visibleAI[i]) && Faction.AreHostile(this.belongsToFactions, visibleAI[i].belongsToFactions))
                {
                    if (!m_isInCombat)
                        EnterInCombatAgainst(visibleAI[i]);

                    enemyTargets.Add(new VisibleEnemy(visibleAI[i], visibleAI[i].GetComponent<EntityAttributes>(), new AggroInfo(50)));
                }
                else if (!m_isInCombat && enemyTargets.Any(t => t.m_entity == visibleAI[i]))
                    EnterInCombatAgainst(visibleAI[i]);

                // check if we're not in combat and if a memeber of our faction is fighting against an enemy
                if(helpsMembersOfSameFactions)
                {
                    if(visibleAI[i].isInCombat && !isInCombat && 
                            visibleAI[i].belongsToFactions.Any(t => belongsToFactions.Contains(FactionsDatabase.GetFaction(t.ID))))
                    {
                        if (visibleAI[i].enemyTargets.Count >= 1)
                        {
                            EnterInCombatAgainst(visibleAI[i].enemyTargets[0].m_entity);
                        }
                    }
                }
            }

            try
            {
                if (!m_isInCombat && enemyTargets.Any(t => t.m_entity.CompareTag("Player")) && visibleTargets.Any(t => t.tr.CompareTag("Player"))
                    || visibleTargets.Any(t => t.tr.CompareTag("Player") && !enemyTargets.Any(t => t.m_entity.CompareTag("Player")) && Faction.AreHostile(this.belongsToFactions, RckPlayer.instance.belongsToFactions)
                    && RckPlayer.instance.isAlive))
                {
                    if (enemyTargets.Any(t => t.m_entity == Entity.GetPlayerEntity()))
                        return;
                    enemyTargets.Add(new VisibleEnemy(Entity.GetPlayerEntity(), RckPlayer.instance.playerAttributes, new AggroInfo(50)));

                    EnterInCombatAgainst(Entity.GetPlayerEntity());
                }
            }
            catch { }
        }


        public void ResetComboChain()
        {
            curAttackType = 0;
        }

        public void ResetComboChainCharged()
        {
            curChargedAttackType = 0;
        }

        public void FullComboChainReset()
        {
            ResetComboChain();
            ResetComboChainCharged();
            CancelInvoke("ResetComboChain");
            CancelInvoke("ResetComboChainCharged");
        }

        public void AttackAnimationEvent()
        {
            currentWeaponOnHand.StartCasting(false, lastAttackIndex);
        }

        public void ChargedAttackAnimationEvent()
        {
            currentWeaponOnHand.StartCasting(true, lastChargedAttackIndex);
        }

        public void EndAttackAnimationEvent()
        {
            currentWeaponOnHand.StopCasting();
        }

        public void PlayAttackSound()
        {
            currentWeaponOnHand.PlayOneShot(currentWeaponOnHand.weaponItem.weaponAttacks[curAttackType].attackSound);
        }

        public void PlayChargedAttackSound()
        {
            currentWeaponOnHand.PlayOneShot(currentWeaponOnHand.weaponItem.weaponChargedAttacks[curChargedAttackType].attackSound);
        }

        public void ResetAttackStateEvent()
        {
            isInAttackMovementLock = false;
            isInAttackRotationLock = false;

            isAttacking = false;
            canAttack = true;
        }

        public void SetAttackMovementLock(int val)
        {
            isInAttackMovementLock = (val == 0 ? false : true);
        }

        public void SetAttackRotationLock(int val)
        {
            isInAttackRotationLock = (val == 0 ? false : true);
        }

        public void DrawWeaponAnimationEvent()
        {
            equipment.currentWeaponOnHip.SetActive(false);
            currentWeaponOnHand.gameObject.SetActive(true);
        }

        public void UndrawWeaponAnimationEvent()
        {
            currentWeaponOnHand.gameObject.SetActive(false);
            equipment.currentWeaponOnHip.SetActive(true);
        }

        public void MeleeChargedAttack()
        {
            // Perform a charged attack
            isAttacking = true;
            // Check if the fatigue is enough for the current attack
            if (attributes.CurStamina < equipment.currentWeapon.weaponChargedAttacks[curChargedAttackType].StaminaAmount)
                curChargedAttackType = 0;

            m_Anim.ResetTrigger("Attack");
            m_Anim.ResetTrigger("ChargedAttack");

            // Set the animation from the AnimatorController, the AnimationsEvents on the 'Swing' animation will do the job
            m_Anim.SetTrigger("ChargedAttack");
            m_Anim.SetInteger("ChargedAttackType", curChargedAttackType);

            lastChargedAttackIndex = curChargedAttackType;
            //attributes.AttackFatigue(currentWeapon.weaponChargedAttacks[curChargedAttackType].StaminaAmount); remove stamina!!

            // To reset the combo if the attack is not done fast
            if (curChargedAttackType + 1 < equipment.currentWeapon.chargedAttackTypes)
                curChargedAttackType++;
            else
                curChargedAttackType = 0;

            CancelInvoke("ResetComboChainCharged");

            if (curChargedAttackType != 0)
                Invoke("ResetComboChainCharged", equipment.currentWeapon.chargedAttackChainTime);


            // audio
            if (equipment.currentWeapon.weaponChargedAttacks[curChargedAttackType].attackSound != null)
            {
                combatAudioSource.clip = equipment.currentWeapon.weaponChargedAttacks[curChargedAttackType].attackSound;
                combatAudioSource.Play();
            }

            canAttack = false;
            wantsToAttack = false;
            lastAttackWasCharged = true;
        }

        [AIInvokable]
        public void MeleeAttack()
        {
            isAttacking = true;
            // Check if the fatigue is enough for the current attack
            //if (attributes.CurStamina < equipment.currentWeapon.weaponAttacks[curAttackType].StaminaAmount)
            //    curAttackType = 0;

            // curAttackType constraints
            if (currentWeaponOnHand.weaponItem.weaponType == WeaponType.Unarmed && equipment.isUsingTorch)
                curAttackType = 0;

            m_Anim.ResetTrigger("Attack");

            // Set the animation from the AnimatorController, the AnimationsEvents on the 'Swing' animation will do the job
            m_Anim.SetTrigger("Attack");
            m_Anim.SetInteger("AttackType", curAttackType);

            lastAttackIndex = curAttackType;
            attributes.DamageStamina(equipment.currentWeapon.weaponAttacks[lastAttackIndex].StaminaAmount, true, RCKSettings.DRAIN_STAMINA_ON_ATTACK_SPEEDAMOUNT);
            StopRecoveringStamina();
            InvokeResetRecover();

            // To reset the combo if the attack is not done fast
            if (curAttackType + 1 < equipment.currentWeapon.AttackTypes)
                curAttackType++;
            else
                curAttackType = 0;

            CancelInvoke("ResetComboChain");

            if (curAttackType != 0)
                Invoke("ResetComboChain", equipment.currentWeapon.attackChainTime);


            canAttack = false;
            wantsToAttack = false;
            lastAttackWasCharged = false;
        }

        [AIInvokable]
        public void CastSpell()
        {
            if (spellsKnowledge != null)
            {
                if(spellsKnowledge.spellInUse != null)
                {
                    if((spellsLimitedByMana && RckPlayer.instance.playerAttributes.CurMana >= spellsKnowledge.spellInUse.manaCost) || !spellsLimitedByMana)
                    {
                        isAttacking = true;
                        isCastingSpell = true;

                        int castHand = (equipment.isUsingShield || equipment.isUsingTorch || (equipment.currentWeapon != null &&
                                        equipment.currentWeapon.weaponType == WeaponType.Bow)) ? 1 : 0;

                        m_Anim.ResetTrigger("Attack");
                        m_Anim.ResetTrigger("CastSpell");

                        m_Anim.SetTrigger("CastSpell");
                        m_Anim.SetInteger("CastType", (int)spellsKnowledge.spellInUse.mode);
                        m_Anim.SetInteger("CastHand", castHand);

                        attributes.DamageMana(spellsKnowledge.spellInUse.manaCost, true, RCKSettings.DRAIN_MANA_ON_CAST_SPEEDAMOUNT);

                        canAttack = false;
                        wantsToAttack = false;
                        lastAttackWasCharged = false;
                    }
                    // else not enough mana
                } //else no spell equipped
            }
            else
                Debug.LogWarning("AI " + entityID + " tried to cast spell, but no Spell Knowledge was set. You may have to assign Spells Knowledge in the Combat Tab.");
        }

        public bool isThrowingGrenade = false;
        public bool grenadeThrown = false;

        [ContextMenu("ThrowNede")] [AIInvokable]
        public void ThrowGrenade()
        {
            if (canAttack && equipment.itemsEquipped[(int)EquipmentSlots.Throwable] != null && equipment.itemsEquipped[(int)EquipmentSlots.Throwable].item != null)
            {
                isAttacking = true;
                isThrowingGrenade = true;
                grenadeThrown = false;
                canAttack = false;
                m_Anim.SetTrigger("StartThrowingGrenade");
                m_Anim.SetTrigger("ThrowGrenade");
                curThrowableTimer = 0;
                m_Anim.Update(0);
            }
        }

        [AIInvokable]
        public void KeepFiringAtTarget()
        {
            // Aim only when stopped
            isBlocking = isStopped;

            // Force aim fire in cover
            if (isInCover)
                isBlocking = true;

            bool canShoot = false;

            if (mainTarget.CompareTag("Player") || mainTarget.CompareTag("RPG Creation Kit/AI"))
            {
                foreach (VisibleTarget t in visibleTargets)
                    if ((mainTarget.CompareTag("Player") && t.tr.CompareTag("Player")) || (mainTarget.CompareTag("RPG Creation Kit/AI") && t.tr.name == "HeadPos"))
                    {
                        RckAI aiComponent = t.tr.GetComponentInParent<RckAI>();
                        if (mainTarget.CompareTag("RPG Creation Kit/AI") && aiComponent != null && aiComponent.transform != mainTarget)
                            continue;

                        canShoot = true;

                        LookAtTarget();

                        if (equipment.itemsEquipped.Length < 12)
                            Debug.LogError("AI IS USING LESS THAN 12 EQUIPMENT SLOTS, IT SHOULD BE AT LEAST 12 FROM UPDATE 1.7");

                        // Throw throwable?
                        if (curThrowableTimer >= throwableCooldown && equipment.itemsEquipped[(int)EquipmentSlots.Throwable] != null)
                            ThrowGrenade();

                        break;
                    }

                // Follow player
                if (!canShoot && !isInCover)
                {
                    stoppingDistance = 2.2225f; // set low stopping distance
                    FollowTargetThroughNavmeshPath();
                }
            }
            // check sight with NPC
            //else if(isHostile)
            //{
            // can shoot = true
            //}
            else
            {
                // Target not in sight
                stoppingDistance = 2.2225f; // set low stopping distance
                FollowTargetThroughNavmeshPath();
            }


            if (canShoot && !isGoingToCover)
            {
                if (!isInCover)
                    stoppingDistance = currentWeaponOnHand.weaponItem.Reach / 4;

                bool isInReach = false;
                // Check reach
                if (!isInCover)
                {
                    if (targetDistance2D < currentWeaponOnHand.weaponItem.Reach / 2.8f)
                        isInReach = true;
                }
                else
                {
                    if (targetDistance2D < currentWeaponOnHand.weaponItem.Reach)
                        isInReach = true;
                }


                // Check rate of fire
                if (weaponDrawn && !isDrawingWeapon && isInReach && isInCombat && canAttack && curFireRate + currentWeaponOnHand.weaponItem.fireRate < Time.time)
                    if (equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 > 0) // todo shortcut to itemequipped
                        FireWeaponEvent();
                    else
                        FirearmReload();
            }
        }

        [AIInvokable]
        public void CheckIfHasToCover()
        {
            // Check cover
            if (firearmGoesInCover && attributes.CurHealth < healthBeforeGoingToCover && currentCover == null && 
                goToCoverCooldown < 0 && !isPerformingRandomPlay)
            {
                FindClosestCover();
            }
        }

        // FIrearm
        public float curFireRate = 0.0f;
        public bool isAiming = false;
        public void FireWeaponEvent()
        {
            curFireRate = Time.time;

            // Check if aiming downsight

            // Check if the fatigue is enough for the current attack
            //if (RckPlayer.instance.playerAttributes.CurStamina < equipment.currentWeapon.weaponAttacks[curAttackType].StaminaAmount)
            //    curAttackType = 0;

            //fpcAnim.ResetTrigger("Attack");

            // Set the animation from the AnimatorController, the AnimationsEvents on the 'Swing' animation will do the job
            //fpcAnim.SetTrigger("Attack");
            //curAttackType = Random.Range(0, currentWeaponOnHand.weaponItem.nAnimFireFromHip); // fire anim
            //fpcAnim.SetInteger("AttackType", curAttackType);

            currentWeaponOnHand.audioSource.clip = currentWeaponOnHand.weaponItem.fireSound;
            currentWeaponOnHand.audioSource.Play();


            // Third Person
            if (!isBlocking)
                m_Anim.Play(currentWeaponOnHand.weaponItem.weaponType.ToString() + "_Hipfire_1", 1, 0.0f);
            else
                m_Anim.Play(currentWeaponOnHand.weaponItem.weaponType.ToString() + "_Aimfire_1", 1, 0.0f);

            equipment.currentWeaponAnimator.Play("Shoot", 0, 0.0f);

            // Force immediate update
            m_Anim.Update(0);

            lastAttackIndex = curAttackType;

            canAttack = true;

            // Remove one round from mag
            equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 -= currentWeaponOnHand.weaponItem.ammoPerShot;

            for (int i = 0; i < currentWeaponOnHand.weaponItem.projectilesPerShot; i++)
            {
                float randomSpreadX = 0.0f;
                float randomSpreadY = 0.0f;

                
                randomSpreadX = Random.Range(-currentWeaponOnHand.weaponItem.hipSpread, currentWeaponOnHand.weaponItem.hipSpread) * RCKSettings.RECOIL_AI_SPREAD_MULTIPLIER;
                randomSpreadY = Random.Range(-currentWeaponOnHand.weaponItem.hipSpread, currentWeaponOnHand.weaponItem.hipSpread) * RCKSettings.RECOIL_AI_SPREAD_MULTIPLIER;

                // Add entity-based accuracy modifiers
                randomSpreadX *= attributes.derivedAttributes.firearmSpreadMultiplier;
                randomSpreadY *= attributes.derivedAttributes.firearmSpreadMultiplier;

                Vector3 rayStart = currentWeaponOnHand.muzzlePos.position;
                Vector3 rayDirection;
                if (mainTarget.CompareTag("Player"))
                    rayDirection = (mainTarget.position - rayStart).normalized + new Vector3(randomSpreadX, randomSpreadY, 0);
                else // when fighting ai, aim from mainTarget.position (feet) and a bit up
                    rayDirection = ((mainTarget.position + mainTarget.up) - rayStart).normalized + new Vector3(randomSpreadX, randomSpreadY, 0);

                Ray ray = new Ray(rayStart, rayDirection);

                Debug.DrawRay(rayStart, rayDirection * currentWeaponOnHand.weaponItem.Reach, Color.cyan, 3);

                currentWeaponOnHand.FireRaycast(this, ray);
            }
        }

        public void FirearmReload()
        {
            if (canAttack && currentWeaponOnHand.weaponItem.weaponClass == WeaponClass.Firearm &&  
                equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 < currentWeaponOnHand.weaponItem.clipRounds)
            {
                canAttack = false;
                
                if (currentWeaponOnHand.weaponItem.reloadType == ReloadType.Normal)
                {
                    // Make reload happen
                    if (equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 == 0)
                    {
                        equipment.currentWeaponAnimator.Play("Reload_Empty_TPS", 0, 0.0f);
                    }
                    else
                    {
                        equipment.currentWeaponAnimator.Play("Reload_TPS", 0, 0.0f);
                    }

                    // Make reload happen
                    if (equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 == 0)
                    {
                        m_Anim.Play(currentWeaponOnHand.weaponItem.ItemID + "_Reload_1_Empty", 1, 0.0f);
                    }
                    else
                    {
                        m_Anim.Play(currentWeaponOnHand.weaponItem.ItemID + "_Reload_1", 1, 0.0f);
                    }

                    // Force immediate update
                    m_Anim.Update(0);
                    equipment.currentWeaponAnimator.Update(0);

                    // play sounds

                    // rest is handled by anim events
                }
                else if (currentWeaponOnHand.weaponItem.reloadType == ReloadType.Single)
                {
                    // Handle single reload
                    Debug.Log("RELOADING");
                    StartCoroutine(SingleReloadTask());
                }
            }
        }

        public bool wantsToCancelRelaod = false;
        IEnumerator SingleReloadTask()
        {
            // Start going

            m_Anim.Play(currentWeaponOnHand.weaponItem.ItemID + "_ReloadS_Start", 1, 0.0f);
            equipment.currentWeaponAnimator.Play("Reload_Start", 0, 0.0f);

            m_Anim.Update(0);
            equipment.currentWeaponAnimator.Update(0);

            yield return new WaitForEndOfFrame();

            // Start going in input
            while (m_Anim.GetCurrentAnimatorStateInfo(1).normalizedTime < 1.0f)
                yield return null;

            wantsToCancelRelaod = false;

            do
            {
                m_Anim.Play(currentWeaponOnHand.weaponItem.ItemID + "_ReloadS_PutOneBullet", 1, 0.0f);
                equipment.currentWeaponAnimator.Play("Reload_PutOneBullet", 0, 0.0f);

                m_Anim.Update(0);
                equipment.currentWeaponAnimator.Update(0);

                yield return new WaitForEndOfFrame();

                while (m_Anim.GetCurrentAnimatorStateInfo(1).normalizedTime < 1.0f)
                {
                    if (targetDistance2D < 10.0f)
                        wantsToCancelRelaod = true;

                    yield return null;
                }

                if (equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 >= currentWeaponOnHand.weaponItem.clipRounds)
                    wantsToCancelRelaod = true;
            }
            while (wantsToCancelRelaod == false);

            m_Anim.Play(currentWeaponOnHand.weaponItem.ItemID + "_ReloadS_End", 1, 0.0f);
            equipment.currentWeaponAnimator.Play("Reload_End", 0, 0.0f);

            m_Anim.Update(0);
            equipment.currentWeaponAnimator.Update(0);

            yield return null;
        }

        public void PutOneBulletAnim()
        {
            equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1++;
        }

        public void EndSingleReloadTypeAnimEvent()
        {
            ResetAttackStateEvent();
        }


        public void FirearmReloadEvent()
        {
            // reload the weapon
            equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 = currentWeaponOnHand.weaponItem.clipRounds;
        }

        public void RangedAttack()
        {
            // Check if there is ammo (and if it's of correct type
            if (equipment.itemsEquipped[(int)EquipmentSlots.Ammo] != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].Amount >= 1)
            {
                // Player wants to nock an arrow
                wantsToAttack = true;

                isAttacking = true;

                curAttackType = 0; // you may choose different shot types (ex. for the crouch)

                m_Anim.ResetTrigger("Attack");

                // Set the animation from the AnimatorController, the AnimationsEvents on the 'Swing' animation will do the job
                m_Anim.SetTrigger("Attack");
                m_Anim.SetInteger("AttackType", curAttackType);

                lastAttackIndex = curAttackType;

                //cameraAnim.AttackAnimation(currentWeapon.weaponAttacks[curAttackType].cameraAnim);

                lastAttackIndex = curAttackType;
                //attributes.AttackFatigue(currentWeapon.weaponAttacks[curAttackType].StaminaAmount);

                canAttack = false;
                lastAttackWasCharged = false;
            }
        }

        [AIInvokable]
        public void SetBlocking(bool startBlocking)
        {
            hasToBlock = startBlocking;
            if (!startBlocking) // COMBATFIX1
                canAttack = true;
        }

        public void BlockToggle()
        {
            hasToBlock = !hasToBlock;
        }

        public bool wasBlocking = false;
        public void BlockCheck()
        {
            isBlocking = hasToBlock;

            m_Anim.SetBool("isBlocking", isBlocking);
            //fpcAnim.SetBool("isUnbalanced", isUnbalanced);

            if (isBlocking && !wasBlocking)
            {
                m_Anim.ResetTrigger("hasBlockedAttack");
                m_Anim.SetTrigger("StartBlocking");
            }

            if (isBlocking)
            {
                if (!wasBlocking)
                {
                    //attributes.BlockingFatigue(); TODO
                    wasBlocking = true;
                }
            }
            else
            {
                if(blockingArea)
                    if (blockingArea.activeSelf)
                        blockingArea.SetActive(false);

                if (wasBlocking)
                {
                    //playerVitals.StopBlockingFatigue();
                    wasBlocking = false;
                    isBlocking = false;
                }
            }
        }

        /// <summary>
        /// Called by an Animation event on the block idle that enables the blocking area
        /// </summary>
        public void BlockAnimationEvent()
        {
            blockingArea.SetActive(true);
        }

        public override void DamageBlocked(DamageContext damageContext)
        {
            base.DamageBlocked(damageContext);

            // If the attack was blocked reduce the damage
            float blockingMultiplier = (equipment.isUsingShield) ? ((ArmorItem)equipment.itemsEquipped[(int)EquipmentSlots.LHand].item).blockingMultiplier : equipment.currentWeapon.BlockingMultiplier;

            AudioClip blockingSound = (equipment.isUsingShield) ? equipment.currentShield.blockingSound : equipment.currentWeapon.blockSound;

            if (blockingSound != null)
                currentWeaponOnHand.PlayOneShot(blockingSound);

            damageContext.amount *= blockingMultiplier;

            attributes.DamageStamina(damageContext.amount * blockingMultiplier, true, RCKSettings.DRAIN_STAMINA_ON_ATTACKBLOCKED_SPEEDAMOUNT);

            if (attributes.CurStamina <= 0)
            {
                attributes.CurStamina = 0;
                isUnbalanced = true;
                isBlocking = false;
                m_Anim.SetTrigger("hasBeenUnbalanced");
            }
            else
            {
                m_Anim.SetTrigger("hasBlockedAttack");
            }

            Damage(damageContext);    
        }

        public override void Die()
        {
            hasToBlock = false;

            // If the AI had a shield equipped
            if(equipment.isUsingShield && !equipment.preventShieldDrop)
            {
                // Spawn the item in the world
                ItemInWorld itemInWorld = Instantiate(equipment.currentShield.itemInWorld, bodyData.lHand.transform.position, bodyData.lHand.transform.rotation).GetComponent<ItemInWorld>();
                itemInWorld.isCreatedItem = true;
                itemInWorld.metadata.Clear(); // clear dropped item metadata

                var allItems = SaveSystem.SaveSystemManager.instance.saveFile.CreatedItemsInWorldData.allCreatedItemsInWorld;
                // Add this created item
                if (allItems.ContainsKey(WorldManager.instance.currentCenterCell.ID))
                    allItems[WorldManager.instance.currentCenterCell.ID].itemsInThis.Add(itemInWorld.ToCreatedItemSaveData());
                else
                {
                    allItems.Add(WorldManager.instance.currentCenterCell.ID, new SaveSystem.CreatedItemInWorldCollection());
                    allItems[WorldManager.instance.currentCenterCell.ID].itemsInThis.Add(itemInWorld.ToCreatedItemSaveData());
                }

                equipment.currentShieldObject.SetActive(false);
                equipment.Unequip(EquipmentSlots.LHand);
                inventory.RemoveItem(equipment.currentShield.ItemID, 1);
            }

            if(equipment.isUsingTorch && !equipment.preventTorchDrop)
            {
                 // Spawn the item in the world
                ItemInWorld itemInWorld = Instantiate(equipment.currentTorch.itemInWorld, bodyData.lHand.transform.position, bodyData.lHand.transform.rotation).GetComponent<ItemInWorld>();
                itemInWorld.isCreatedItem = true;
                itemInWorld.metadata.Clear(); // clear dropped item metadata

                var allItems = SaveSystem.SaveSystemManager.instance.saveFile.CreatedItemsInWorldData.allCreatedItemsInWorld;
                // Add this created item
                if (allItems.ContainsKey(WorldManager.instance.currentCenterCell.ID))
                    allItems[WorldManager.instance.currentCenterCell.ID].itemsInThis.Add(itemInWorld.ToCreatedItemSaveData());
                else
                {
                    allItems.Add(WorldManager.instance.currentCenterCell.ID, new SaveSystem.CreatedItemInWorldCollection());
                    allItems[WorldManager.instance.currentCenterCell.ID].itemsInThis.Add(itemInWorld.ToCreatedItemSaveData());
                }

                equipment.currentTorchInHand.SetActive(false);
                equipment.Unequip(EquipmentSlots.LHand);
                inventory.RemoveItem(equipment.currentTorch.ItemID, 1);
            }

            // If the AI weapon was drawn
            if (isInCombat && currentWeaponOnHand != null)
            {
                // Stop casting (entity may have died while attacking)
                currentWeaponOnHand.StopCasting();

                // Instantiate the InWorld equivalent 
                if(equipment.currentWeapon.itemInWorld != null)
                {
                    Vector3 pos = bodyData.lHand.transform.position;
                    if (equipment.currentWeapon.clippingWeapon)
                    {
                        pos += transform.forward * 1.15f;
                        m_Anim.SetTrigger("ReturnToDefault");
                        m_Anim.Update(0);
                    }

                    // Spawn the item in the world
                    ItemInWorld itemInWorld = Instantiate(equipment.currentWeapon.itemInWorld, pos, bodyData.lHand.transform.rotation).GetComponent<ItemInWorld>();
                    itemInWorld.metadata.Clear(); // clear dropped item metadata
                    itemInWorld.isCreatedItem = true;

                    var allItems = SaveSystem.SaveSystemManager.instance.saveFile.CreatedItemsInWorldData.allCreatedItemsInWorld;
                    // Add this created item
                    if (allItems.ContainsKey(WorldManager.instance.currentCenterCell.ID))
                        allItems[WorldManager.instance.currentCenterCell.ID].itemsInThis.Add(itemInWorld.ToCreatedItemSaveData());
                    else
                    {
                        allItems.Add(WorldManager.instance.currentCenterCell.ID, new SaveSystem.CreatedItemInWorldCollection());
                        allItems[WorldManager.instance.currentCenterCell.ID].itemsInThis.Add(itemInWorld.ToCreatedItemSaveData());
                    }
                }

                currentWeaponOnHand.gameObject.SetActive(false);
                inventory.RemoveItem(equipment.currentWeapon.ItemID, 1);
            }

            StopAllCoroutines();

            m_isInCombat = false;
            isInCombat = false;

            if (spellProjectile != null)
                Destroy(spellProjectile);

            base.Die();

            if(blockingArea)
                blockingArea.SetActive(false);
        }

        public bool isSwitchingWeapon = false;

        public void ChangeWeapon(WeaponItem _weaponToEquip)
        {
            isSwitchingWeapon = true;

            ItemInInventory itemToEquip;
            // Check if item exists in inventory
            if ((itemToEquip = inventory.GetItem(_weaponToEquip)) != null)
                StartCoroutine(ChangeWeaponTask(itemToEquip));
        }

        public void ChangeWeapon(string weaponID)
        {
            isSwitchingWeapon = true;

            ItemInInventory itemToEquip;
            // Check if item exists in inventory
            if( (itemToEquip = inventory.GetItem(weaponID)) != null)
            {
                StartCoroutine(ChangeWeaponTask(itemToEquip));
            }
        }


        public IEnumerator ChangeWeaponTask(ItemInInventory _itemToEquip)
        {
            hasToBlock = false;

            bool wasDrawn = weaponDrawn;
            bool wasInCombat = m_isInCombat;
            if (isInCombat)
            {
                while (!canAttack || isDrawingWeapon)
                    yield return new WaitForEndOfFrame();

                // Undraw the weapon
                UndrawWeapon();
            }

            while (isInCombat || isDrawingWeapon)
                yield return new WaitForEndOfFrame();

            equipment.Equip(_itemToEquip.item);

            OnEquipmentChangesHands();
            OnEquipmentChangesAmmo();

            if (wasInCombat)
                DrawWeapon();
            else
                isSwitchingWeapon = false;

            yield return null;
        }

        /// <summary>
        /// Returns true if the caller has ammo for the equipped weapon and equips it, otherwise it returns false without equipping anything.
        /// </summary>
        /// <returns></returns>
        public bool EquipAmmoForCurrentWeapon()
        {
            AmmoItem curAmmo = null;

            switch(currentWeaponOnHand.weaponItem.weaponType)
            {
                case WeaponType.Bow:
                    for(int i = 0; i < inventory.subLists[(int)ItemTypes.AmmoItem].Count; i++)
                    {
                        curAmmo = inventory.subLists[(int)ItemTypes.AmmoItem][i].item as AmmoItem;
                        if (curAmmo.type == AmmoItem.AmmoType.Arrow)
                        {
                            equipment.Equip(curAmmo);
                            return true;
                        }
                    }
                    break;

                case WeaponType.Crossbow:
                    for (int i = 0; i < inventory.subLists[(int)ItemTypes.AmmoItem].Count; i++)
                    {
                        curAmmo = inventory.subLists[(int)ItemTypes.AmmoItem][i].item as AmmoItem;
                        if (curAmmo.type == AmmoItem.AmmoType.Dart)
                        {
                            equipment.Equip(curAmmo);
                            return true;
                        }
                    }
                    break;

                default:
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the caller has ammo for the equipped weapon and equips it, otherwise it returns false without equipping anything.
        /// </summary>
        /// <returns></returns>
        public bool EquipAmmoForWeapon(WeaponItem _weapon)
        {
            AmmoItem curAmmo = null;

            switch (_weapon.weaponType)
            {
                case WeaponType.Bow:
                    for (int i = 0; i < inventory.subLists[(int)ItemTypes.AmmoItem].Count; i++)
                    {
                        curAmmo = inventory.subLists[(int)ItemTypes.AmmoItem][i].item as AmmoItem;
                        if (curAmmo.type.HasFlag(AmmoItem.AmmoType.Arrow))
                        {
                            equipment.Equip(curAmmo);
                            return true;
                        }
                    }
                    break;

                case WeaponType.Crossbow:
                    for (int i = 0; i < inventory.subLists[(int)ItemTypes.AmmoItem].Count; i++)
                    {
                        curAmmo = inventory.subLists[(int)ItemTypes.AmmoItem][i].item as AmmoItem;
                        if (curAmmo.type.HasFlag(AmmoItem.AmmoType.Dart))
                        {
                            equipment.Equip(curAmmo);
                            return true;
                        }
                    }
                    break;

                default:
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Returns the AmmoItem if the caller has ammo for the equipped weapon, otherwise it returns null.
        /// </summary>
        /// <returns></returns>
        public AmmoItem HasAmmoForCurrentWeapon()
        {
            AmmoItem curAmmo = null;

            switch (currentWeaponOnHand.weaponItem.weaponType)
            {
                case WeaponType.Bow:
                    for (int i = 0; i < inventory.subLists[(int)ItemTypes.AmmoItem].Count; i++)
                    {
                        curAmmo = inventory.subLists[(int)ItemTypes.AmmoItem][i].item as AmmoItem;
                        if (curAmmo.type == AmmoItem.AmmoType.Arrow)
                        {
                            return curAmmo;
                        }
                    }
                    break;

                case WeaponType.Crossbow:
                    for (int i = 0; i < inventory.subLists[(int)ItemTypes.AmmoItem].Count; i++)
                    {
                        curAmmo = inventory.subLists[(int)ItemTypes.AmmoItem][i].item as AmmoItem;
                        if (curAmmo.type == AmmoItem.AmmoType.Dart)
                        {
                            return(curAmmo);
                        }
                    }
                    break;

                default:
                    return null;
            }

            return null;
        }

        /// <summary>
        /// Returns the AmmoItem if the caller has ammo for the weapon passed, otherwise it returns null.
        /// </summary>
        /// <returns></returns>
        public AmmoItem HasAmmoForWeapon(WeaponItem _weapon)
        {
            AmmoItem curAmmo = null;

            switch (_weapon.weaponType)
            {
                case WeaponType.Bow:
                    for (int i = 0; i < inventory.subLists[(int)ItemTypes.AmmoItem].Count; i++)
                    {
                        curAmmo = inventory.subLists[(int)ItemTypes.AmmoItem][i].item as AmmoItem;
                        if (curAmmo.type.HasFlag(AmmoItem.AmmoType.Arrow))
                        {
                            return curAmmo;
                        }
                    }
                    break;

                case WeaponType.Crossbow:
                    for (int i = 0; i < inventory.subLists[(int)ItemTypes.AmmoItem].Count; i++)
                    {
                        curAmmo = inventory.subLists[(int)ItemTypes.AmmoItem][i].item as AmmoItem;
                        if (curAmmo.type.HasFlag(AmmoItem.AmmoType.Dart))
                        {
                            return (curAmmo);
                        }
                    }
                    break;

                default:
                    return null;
            }

            return null;
        }

        /// TODO
        public bool SwitchToBestMeleeWeapon()
        {
            bool hasAMeleeWeapon = false;
            WeaponItem weaponItem = null;
            ItemInInventory itemToEquip = null;
            int maxDmgWeap = -1;

            for (int i = 0; i < inventory.subLists[(int)ItemTypes.WeaponItem].Count; i++)
            {
                weaponItem = (WeaponItem)inventory.subLists[(int)ItemTypes.WeaponItem][i].item;

                if (weaponItem.weaponType == WeaponType.BladeOneHand || weaponItem.weaponType == WeaponType.BluntOneHand ||
                    weaponItem.weaponType == WeaponType.BladeTwoHands || weaponItem.weaponType == WeaponType.BluntTwoHands ||
                    weaponItem.weaponType == WeaponType.DaggerOneHand)
                {
                    hasAMeleeWeapon = true;
                    
                    // We found a stronger weapon
                    if(weaponItem.Damage > maxDmgWeap)
                        itemToEquip = inventory.subLists[(int)ItemTypes.WeaponItem][i];
                }
            }

            if(hasAMeleeWeapon)
            {
                equipment.Equip(itemToEquip);

                equipment.OnEquipmentChanges();
                OnEquipmentChangesHands();
                OnEquipmentChangesAmmo();
            }

            return hasAMeleeWeapon;
        }

        public bool SwitchToBestRangedWeapon()
        {
            return false;
        }

        public bool SwitchToBestAmmo()
        {
            return false;
        }

        public bool EquipShieldIfHeCan()
        {
            return false;
        }

        public void SwitchToDefaultWeapon()
        {
            equipment.currentWeapon = defaultWeapon;
            currentWeaponOnHand = defaultWeaponOnHand;
        }

        GameObject projectile;
        public void SpawnAmmoOnRHand()
        {
            projectile = Instantiate(((AmmoItem)equipment.currentAmmo).projectile, bodyData.rHand);
            currentProjectile = projectile.GetComponent<Projectile>();
            currentProjectile.shooterEntity = this;
        }

        public bool projectileNocked = false;
        public void OnAmmoIsReady()
        {
            projectileNocked = true;
        }

        [AIInvokable]
        public void RangedLoadAndAim()
        {
            StopCoroutine(RangedLoadAndAimTask());
            StartCoroutine(RangedLoadAndAimTask());
        }

        private IEnumerator RangedLoadAndAimTask()
        {
            // Player wants to nock an arrow
            wantsToAttack = true;

            isAttacking = true;

            curAttackType = 0; // you may choose different shot types (ex. for the crouch)

            m_Anim.ResetTrigger("Attack");

            // Set the animation from the AnimatorController
            m_Anim.SetTrigger("Attack");
            m_Anim.SetInteger("AttackType", curAttackType);

            equipment.currentWeaponAnimator.ResetTrigger("Load");
            equipment.currentWeaponAnimator.Play("Load");

            //currentWeaponAnimator.SetTrigger("Load");

            //fpcAnim.Play("Load");

            //cameraAnim.AttackAnimation(currentWeapon.weaponAttacks[curAttackType].cameraAnim);

            lastAttackIndex = curAttackType;
            //playerVitals.AttackFatigue(currentWeapon.weaponAttacks[curAttackType].StaminaAmount);

            // audio
            //if (currentWeapon.weaponAttacks[curAttackType].attackSound != null)
            //{
            //    combatAudioSource.clip = currentWeapon.weaponAttacks[curAttackType].attackSound;
            //    combatAudioSource.Play();
            //}

            canAttack = false;
            lastAttackWasCharged = false;

            //aiLookAt.ForceToLookAtTarget(mainTarget);

            Vector3 _shootDir = (mainTarget.transform.position - this.transform.position).normalized;

            Vector3 noiseDir = Vector3.zero;
            bool abort = false;
            while (!projectileNocked && !abort)
            {
                // Calculate some noise
                noiseDir = new Vector3(Random.Range(-1f, 1f) / attributes.derivedAttributes.rangedCombatAccuracy * 100,
                                       Random.Range(-1f, 1f) / attributes.derivedAttributes.rangedCombatAccuracy * 100,
                                       0);
                // Aim preditct etc

                if (mainTarget == null || !m_isInCombat)
                {
                    abort = true;
                    break;
                }

                if (mainTarget.CompareTag("Player"))
                {
                    _shootDir = ((mainTarget.transform.position - this.transform.position) + noiseDir + (Vector3.down * (Player.RckPlayer.instance.IsCrouching ? 2.25f : 1.5f))).normalized;
                }
                else
                    _shootDir = ((mainTarget.transform.position - this.transform.position) + noiseDir).normalized;

                yield return new WaitForSeconds(0.1f);
            }

            if(abort)
            {
                m_Anim.SetTrigger("AbortShoot");
                if(currentProjectile != null)
                    Destroy(currentProjectile.gameObject);
                projectileNocked = false;
                wantsToAttack = false;
                isAttacking = false;
                canAttack = true;
            }

            // Aim
            RangedShoot(_shootDir);
        }

        public void RangedShoot(Vector3 _direction)
        {
            m_Anim.ResetTrigger("Attack");

            m_Anim.SetTrigger("Attack");
            m_Anim.SetInteger("AttackType", 1);

            equipment.currentWeaponAnimator.ResetTrigger("Shoot");
            equipment.currentWeaponAnimator.SetTrigger("Shoot");

            projectileNocked = false;
            wantsToAttack = false;
            isAttacking = false;

            // Shoot the arrow
            currentProjectile.Shoot(equipment.currentWeapon, (AmmoItem)equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item, this, _direction, false);
            //inventory.RemoveItem()

            // audio
            if (equipment.currentWeapon.weaponAttacks[0] != null)
            {
                currentWeaponOnHand.PlayOneShot(equipment.currentWeapon.weaponAttacks[0].attackSound);
            }

            aiLookAt.Invoke("StopForcingLookAtTarget", 1f);
        }

        /// <summary>
        /// Those callbacks will be called by the functionalities of the BehaviourTree.
        /// The actual implementations and behaviours of those must be defined per each entity.
        /// </summary>

        void IMeleeAttacker.BlockEvent()
        {
            throw new System.NotImplementedException();
        }

        void IRangedAttacker.BlockEvent()
        {
            throw new System.NotImplementedException();
        }

        void IMeleeAttacker.DrawWeapon()
        {
            throw new System.NotImplementedException();
        }

        void IRangedAttacker.DrawWeapon()
        {
            throw new System.NotImplementedException();
        }

        void IMeleeAttacker.MeleeAttackCheck()
        {
            throw new System.NotImplementedException();
        }

        void IMeleeAttacker.MeleeAttackEvent()
        {
            throw new System.NotImplementedException();
        }

        void IMeleeAttacker.OnEnterCombat()
        {
            throw new System.NotImplementedException();
        }

        void IRangedAttacker.OnEnterCombat()
        {
            throw new System.NotImplementedException();
        }

        void IRangedAttacker.RangedAttackCheck()
        {
            throw new System.NotImplementedException();
        }

        void IRangedAttacker.RangedAttackEvent()
        {
            throw new System.NotImplementedException();
        }

        void IMeleeAttacker.UndrawWeapon()
        {
            throw new System.NotImplementedException();
        }

        void IRangedAttacker.UndrawWeapon()
        {
            throw new System.NotImplementedException();
        }

        void IMeleeAttacker.UseMelee()
        {
            throw new System.NotImplementedException();
        }

        void IRangedAttacker.UseRanged()
        {
            throw new System.NotImplementedException();
        }

        public void OnAttackIsBlocked()
        {
            canAttack = false;
            wantsToAttack = false;
            isAttacking = true;
            isBlocking = false;
            FullComboChainReset();

            m_Anim.SetTrigger("hasBeenBlocked");
        }

        [AIInvokable]
        public void Get2DTargetDistance()
        {
            if (mainTarget != null)
                targetDistance2D = Calculate2DTargetDistance(transform.position, mainTarget.transform.position);
            else 
                targetDistance2D = 999999.9f;
        }

        private float Calculate2DTargetDistance(Vector3 v1, Vector3 v2)
        {
            float xDiff = v1.x - v2.x;
            float zDiff = v1.z - v2.z;
            return Mathf.Sqrt((xDiff * xDiff) + (zDiff * zDiff));
        }

        // Spells, Update 1.3 - Anim callbacks

        public void CastSpellInit()
        {
            Spell curSpell = spellsKnowledge.spellInUse;

            Transform handT = (equipment.isUsingShield || equipment.isUsingTorch || (equipment.currentWeapon != null && equipment.currentWeapon.weaponType == WeaponType.Bow)) ? bodyData.rHand : bodyData.lHand;

            switch (curSpell.mode)
            {
                case SpellModes.Projectile:
                    spellProjectile = Instantiate(curSpell.magicProjectile, handT);
                    spellProjectile.SetLayer(RCKLayers.Default, true);
                    currentSpellProjectile = spellProjectile.GetComponent<SpellProjectile>();
                    currentSpellProjectile.Init(this, false);
                    break;

                case SpellModes.Touch:
                    spellProjectile = Instantiate(curSpell.magicProjectile, handT);
                    spellProjectile.SetLayer(RCKLayers.Default, true);
                    currentSpellProjectile = spellProjectile.GetComponent<SpellProjectile>();
                    currentSpellProjectile.Init(this, false);
                    break;

                case SpellModes.Self:
                    spellProjectile = Instantiate(curSpell.magicProjectile, handT);
                    spellProjectile.SetLayer(RCKLayers.Default, true);
                    currentSpellProjectile = spellProjectile.GetComponent<SpellProjectile>();
                    currentSpellProjectile.Init(this, false);
                    break;
            }
        }

        public void CastSpellAttackEvent()
        {
            // Instantiate/Do Stuff
            Spell curSpell = spellsKnowledge.spellInUse;
            Vector3 _shootDir;
            switch (curSpell.mode)
            {
                case SpellModes.Projectile:

                    if(mainTarget != null)
                        _shootDir = ((mainTarget.transform.position - this.transform.position) + (Vector3.down * (Player.RckPlayer.instance.IsCrouching ? 2.25f : 1.5f))).normalized;
                    else
                        _shootDir = (((transform.forward * 2) - this.transform.position) + (Vector3.down * (Player.RckPlayer.instance.IsCrouching ? 2.25f : 1.5f))).normalized;

                    currentSpellProjectile.Shoot(this, _shootDir, false);
                    currentSpellProjectile.ExecuteOnCasterEffects();
                    break;

                case SpellModes.Touch:
                    if(mainTarget != null)
                        _shootDir = ((mainTarget.transform.position - this.transform.position) + (Vector3.down * (Player.RckPlayer.instance.IsCrouching ? 2.25f : 1.5f))).normalized;
                    else
                        _shootDir = (((transform.forward*2) - this.transform.position) + (Vector3.down * (Player.RckPlayer.instance.IsCrouching ? 2.25f : 1.5f))).normalized;

                    if (currentSpellProjectile != null)
                    {
                        currentSpellProjectile.Shoot(this, _shootDir, false);
                        currentSpellProjectile.ExecuteOnCasterEffects();
                    }
                    break;

                case SpellModes.Self:
                    currentSpellProjectile.Explode();
                    currentSpellProjectile.ExecuteOnCasterEffects();
                    break;
            }
        }

        public float curThrowableTimer = 0;
        public float throwableCooldown = 30;
        public void CastSpellResetEvent()
        {
            ResetAttackStateEvent();
            isAttacking = false;
            isCastingSpell = false;
        }

        public Throwable curThrowable;
        public void SpawnThowableOnLHand()
        {
            if (curThrowable != null)
                Destroy(curThrowable);

            curThrowable = Instantiate(((WeaponItem)equipment.itemsEquipped[(int)EquipmentSlots.Throwable].item).WeaponOnHand, bodyData.lHand).GetComponent<Throwable>();
            curThrowable.GetComponent<Throwable>().dummy = false;

        }

        public void ThrowGrenadeEvent()
        {
            if(mainTarget != null)
            {
                // Throw towards target
                curThrowable.Throw(this, (mainTarget.position - transform.position).normalized, false);
            }
            else
                curThrowable.Throw(this, transform.forward * 5, false);

            curThrowableTimer = 0;

            // Remove 1 thrwoable from inventory
            inventory.RemoveItem(equipment.itemsEquipped[(int)EquipmentSlots.Throwable], 1);

            // Check unequip
            if (equipment.itemsEquipped[(int)EquipmentSlots.Throwable].Amount == 0)
            {
                equipment.itemsEquipped[(int)EquipmentSlots.Throwable].isEquipped = false;
                equipment.itemsEquipped[(int)EquipmentSlots.Throwable] = null;
            }
        }

        public void ResetThrowGrenadeEvent()
        {
            isAttacking = false;
            isThrowingGrenade = false;
            grenadeThrown = false;
            canAttack = true;
        }
    }
}