using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RPGCreationKit;
using UnityEngine.UI;
using RPGCreationKit.Player;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem;

namespace RPGCreationKit
{
    /// <summary>
    /// Combat system controller for the Player
    /// </summary>
    public class PlayerCombat : MonoBehaviour, IMeleeAttacker 
    {
        #region Singleton
        public static PlayerCombat instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Debug.LogError("Anomaly detected with the Singleton Pattern of 'PlayerCombat', are you using multple PlayerCombats?");
                Destroy(this);
            }
        }
        #endregion

        public bool fpsAssigned = false;

        [Header("References")]
        public WeaponItem defaultWeapon;            // This is the weapon that will be equipped if there's none (usually it is fists)
        public WeaponOnHand defaultWeaponOnHand;

        public SpellsKnowledge spellKnowledge;
        public SpellProjectile currentSpellProjectile;

        public Transform fpsTransform;
        public PCFPSArms fpsArms;
        public GameObject fpsArmsObject;            // This is a reference to the first person arms model

        [Space(10)]

        public Transform m_Camera;
        public CameraAnimation cameraAnim;
        public Animator fpcAnim;
        public Animator tpsAnim;
        public Equipment equipment;

        public RuntimeAnimatorController defaultCameraAnimator;

        [Space(5)]

        public PCWeaponUI weaponUI;
        public Image equippedSpellUI;
        public PCWeaponUI throwableUI;


        [Space(10)]
        [Header("Status")]
        public bool weaponDrawn = false;
        public bool isDrawingWeapon = false;
        public bool isUndrawingWeapon = false;

        public GameObject playerBlockingArea;
        public WeaponOnHand currentWeaponOnHand;
        public Projectile currentProjectile;

        // Combat System
        [Space(10)]
        [Header("Melee Combat System")]
        public AudioSource combatAudioSource;
        public AudioSource spellsAudioSource;

        public bool wantsToAttack;
        public bool canAttack = true;
        public bool isAttacking = false; // Is playing attack animation
        public bool isCastingSpell = false;

        public bool isBlocking = false;
        public bool isUnbalanced = false;

        public bool lastAttackWasCharged;
        public int curAttackType = 0;
        public int curChargedAttackType = 0;
        [HideInInspector] public int lastAttackIndex = 0;
        [HideInInspector] public int lastChargedAttackIndex = 0;


        [Space(10)]
        [Header("Prefabs")]
        public GameObject bloodParticlePrefab;

        bool inputShootArrow = false;

        // Firearms update
        public SwayEffect weaponSway;
        public float defaultSway = 0.055f;

        public bool isAiming = false; // always same as isBlocking
        public bool holdingFire = false;
        public float curFireRate = 0;
        public GameObject aimHolderT;

        public bool isThrowingGrenade = false;
        public bool grenadeInput;
        public bool grenadeThrown = false;
        public Throwable currentThrowable;

        public void INPUT_ArrowShoot(InputAction.CallbackContext value)
        {
            if (equipment.currentWeapon != null && equipment.currentWeapon.weaponType == WeaponType.Bow)
            {
                if (value.interaction is HoldInteraction)
                {
                    if (value.started)
                        inputShootArrow = false;

                    if (value.canceled)
                        inputShootArrow = true;
                }
                else
                    inputShootArrow = true;
            }
        }

        public bool blockInput;
        public void INPUT_Block(InputAction.CallbackContext value)
        {
            if (value.interaction is HoldInteraction)
            {
                if (value.started)
                    blockInput = true;

                if (value.canceled)
                    blockInput = false;
            }
            else
                blockInput = false;
        }

        public void INPUT_CastSpell(InputAction.CallbackContext value)
        {
            AttackCastSpell();
        }

        public void INPUT_PutWeaponAway(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                if (value.interaction is HoldInteraction)
                    PutWeaponAway();
                else
                    FirearmReload();
            }
        }

        public void INPUT_Attack(InputAction.CallbackContext value)
        {
            holdingFire = value.ReadValueAsButton();

            if (value.performed)
            {
                if (equipment.currentWeapon.weaponClass == WeaponClass.Melee)
                {
                    if (value.interaction is HoldInteraction)
                        AttackHold();
                    else
                        AttackTap();
                }
                else if (equipment.currentWeapon.weaponClass == WeaponClass.Firearm)
                {

                }
            }
        }

        public void INPUT_ThrowGrenade(InputAction.CallbackContext value)
        {
            grenadeInput = value.ReadValueAsButton();

            if (value.performed)
            {
                StartThrowingGrenade();
            }
        }

        // Use this for initialization
        void Start()
        {
            SetWeaponUI();
        }

        public void Update()
        {
            // Check for attack
            if (equipment.currentWeapon != null && fpcAnim != null)
            {
                // Melee (standalone, melee and bow) - Fireamrms (update)
                if (equipment.currentWeapon.weaponClass == WeaponClass.Melee)
                {
                    if (equipment.currentWeapon.weaponType == WeaponType.Bow)
                        BowAttackCheck();

                    if (equipment.currentWeapon.weaponType != WeaponType.Crossbow)
                        BlockCheck();
                }
                else if (equipment.currentWeapon.weaponClass == WeaponClass.Firearm)
                {

                    // Check for automatic fire (semi-weapons are handled in INPUT_ function
                    if (equipment.currentWeapon.fireType == FirearmFireType.Auto && holdingFire)
                        AttackFire();
                    else
                    {
                        if (RckPlayer.instance.input.currentActionMap.FindAction("Attack") != null && RckPlayer.instance.input.currentActionMap.FindAction("Attack").triggered)
                        {
                            if (equipment.currentWeapon.fireType == FirearmFireType.Semi)
                            {
                                AttackFire();
                            }
                        }
                    }

                    FirearmAimCheck();

                    // Adjust AimingT
                    if(isBlocking)
                    {
                        if (!RckPlayer.instance.IsCrouching)
                        {
                            aimHolderT.transform.localPosition = Vector3.Lerp(aimHolderT.transform.localPosition, currentWeaponOnHand.weaponItem.aimingHolderPos, 10 * Time.deltaTime);
                            aimHolderT.transform.localRotation = Quaternion.Lerp(aimHolderT.transform.localRotation, Quaternion.Euler(currentWeaponOnHand.weaponItem.aimingHolderRot), 10 * Time.deltaTime);
                        }
                        else
                        {
                            aimHolderT.transform.localPosition = Vector3.Lerp(aimHolderT.transform.localPosition, currentWeaponOnHand.weaponItem.aimingHolderPosCrouched, 10 * Time.deltaTime);
                            aimHolderT.transform.localRotation = Quaternion.Lerp(aimHolderT.transform.localRotation, Quaternion.Euler(currentWeaponOnHand.weaponItem.aimingHolderRotCrouched), 10 * Time.deltaTime);
                        }

                        // Adjust FOV
                        RckPlayer.instance.mainCamera.fieldOfView = Mathf.Lerp(RckPlayer.instance.mainCamera.fieldOfView, currentWeaponOnHand.weaponItem.sightFov, RCKSettings.FOV_CHANGE_SPEED * Time.deltaTime);
                        RckPlayer.instance.tpsCamera.fieldOfView = Mathf.Lerp(RckPlayer.instance.tpsCamera.fieldOfView, currentWeaponOnHand.weaponItem.sightFov, RCKSettings.FOV_CHANGE_SPEED * Time.deltaTime);
                    }
                    else
                    {
                        aimHolderT.transform.localPosition = Vector3.Lerp(aimHolderT.transform.localPosition, Vector3.zero, 10 * Time.deltaTime);
                        aimHolderT.transform.localRotation = Quaternion.Lerp(aimHolderT.transform.localRotation, Quaternion.identity, 10 * Time.deltaTime);

                        // Adjust FOV
                        RckPlayer.instance.mainCamera.fieldOfView = Mathf.Lerp(RckPlayer.instance.mainCamera.fieldOfView, RCKSettings.NORMAL_FOV, RCKSettings.FOV_CHANGE_SPEED * Time.deltaTime);
                        RckPlayer.instance.tpsCamera.fieldOfView = Mathf.Lerp(RckPlayer.instance.tpsCamera.fieldOfView, RCKSettings.NORMAL_FOV, RCKSettings.FOV_CHANGE_SPEED * Time.deltaTime);
                    }
                }

                if (!RckPlayer.instance.isInCutsceneMode)
                {
                    fpcAnim.SetBool("isMoving", RckPlayer.instance.m_isMoving);
                    fpcAnim.SetBool("isSprinting", RckPlayer.instance.isRunning);
                }
                else
                {
                    fpcAnim.SetBool("isMoving", false);
                    fpcAnim.SetBool("isSprinting", false);
                }

                // Check grenade
                if(weaponDrawn && isThrowingGrenade && !grenadeInput && !grenadeThrown)
                {
                    // Throw
                    ThrowThrowable();
                }
            }
        }

        public void ThrowThrowable()
        {
            fpcAnim.SetTrigger("ThrowGrenade");
            tpsAnim.SetTrigger("ThrowGrenade");
            grenadeThrown = true;

            // Remove 1 throwable is done in ThrowGrenadeEvent
        }

        public void AttackFire()
        {
            if (equipment.currentWeapon != null && fpcAnim != null)
            {
                if (!isAttacking && canAttack && RckPlayer.instance.AreControlsEnabled() &&
                !RckPlayer.instance.isInteracting && !RckPlayer.instance.isInConversation &&
                !QuestManagerUI.instance.isUIopened && !InventoryUI.instance.isOpen && !isUnbalanced && !RckPlayer.instance.isLooting &&
                !BookReaderManager.instance.backgroundUI.activeInHierarchy && !RckPlayer.instance.isInCutsceneMode && !TutorialAlertMessage.instance.messageOpened && !RckPlayer.instance.isInDeathSequence)
                {
                    if (!weaponDrawn)
                        DrawWeapon();
                    else
                    {
                        // Fire the weapon
                        // Check rate of fire
                        if (curFireRate + currentWeaponOnHand.weaponItem.fireRate < Time.time)
                            if (equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 > 0) // todo shortcut to itemequipped
                                FireWeaponEvent();
                            else
                                FireWeaponEmptyEvent();
                    }
                }
            }
        }

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

            if (!isAiming)
                fpcAnim.Play("Hipfire_1", 0, 0.0f);
            else
                fpcAnim.Play("Aimfire_1", 0, 0.0f);

            // FPS weapon shoot anim
            equipment.currentWeaponAnimator.Play("Shoot", 0, 0.0f);


            currentWeaponOnHand.audioSource.clip = currentWeaponOnHand.weaponItem.fireSound;
            currentWeaponOnHand.audioSource.Play();

            cameraAnim.AttackAnimation(equipment.currentWeapon.weaponAttacks[curAttackType].cameraAnim);

            if (ThirdPersonPlayer.instance.tpsWeaponDrawn)
            {
                // Third Person
                if (!isAiming)
                    tpsAnim.Play(currentWeaponOnHand.weaponItem.weaponType.ToString() + "_Hipfire_1", 1, 0.0f);
                else
                    tpsAnim.Play(currentWeaponOnHand.weaponItem.weaponType.ToString() + "_Aimfire_1", 1, 0.0f);

                // TPS weapon shoot anim
                ThirdPersonPlayer.instance.currentWeaponAnimator.Play("Shoot", 0, 0.0f);
                tpsAnim.Update(0);
            }

            // Force immediate update
            fpcAnim.Update(0);

            lastAttackIndex = curAttackType;

            canAttack = true;
            
            // Remove one round from mag
            equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 -= currentWeaponOnHand.weaponItem.ammoPerShot;
            SetWeaponUI();

            for (int i = 0; i < currentWeaponOnHand.weaponItem.projectilesPerShot; i++)
            {
                float randomSpreadX = 0.0f;
                float randomSpreadY = 0.0f;

                if (!isAiming)
                {
                    randomSpreadX = Random.Range(-currentWeaponOnHand.weaponItem.hipSpread, currentWeaponOnHand.weaponItem.hipSpread);
                    randomSpreadY = Random.Range(-currentWeaponOnHand.weaponItem.hipSpread, currentWeaponOnHand.weaponItem.hipSpread);

                    if (RckPlayer.instance.iscrouching)
                    {
                        randomSpreadX = randomSpreadX * RCKSettings.SPREAD_CROUCHED_REDUCTION_HIP;
                        randomSpreadY = randomSpreadY * RCKSettings.SPREAD_CROUCHED_REDUCTION_HIP;
                    }
                }
                else
                {
                    randomSpreadX = Random.Range(-currentWeaponOnHand.weaponItem.aimSpread, currentWeaponOnHand.weaponItem.aimSpread);
                    randomSpreadY = Random.Range(-currentWeaponOnHand.weaponItem.aimSpread, currentWeaponOnHand.weaponItem.aimSpread);

                    if (RckPlayer.instance.iscrouching)
                    {
                        randomSpreadX = randomSpreadX * RCKSettings.SPREAD_CROUCHED_REDUCTION_AIM;
                        randomSpreadY = randomSpreadY * RCKSettings.SPREAD_CROUCHED_REDUCTION_AIM;
                    }
                }

                Ray ray;

                if (!RckPlayer.instance.isInThirdPerson)
                    ray = RckPlayer.instance.mainCamera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
                else
                {
                    // If is not free look, shoot towards the camera
                    if (!ThirdPersonPlayer.instance.isInFreeLook)
                        ray = RckPlayer.instance.tpsCamera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
                    else
                    {
                        // If player is in free look, shot from the gun forward
                        Vector3 rayStart = ThirdPersonPlayer.instance.currentWeaponOnHand.muzzlePos.position;
                        Vector3 rayDirection = (ThirdPersonPlayer.instance.currentWeaponOnHand.muzzlePos.position + ThirdPersonPlayer.instance.currentWeaponOnHand.muzzlePos.forward * 10 - rayStart).normalized + new Vector3(randomSpreadX, randomSpreadY, 0);

                        ray = new Ray(rayStart, rayDirection);
                    }
                }

                ray.direction = ray.direction.normalized + new Vector3(randomSpreadX, randomSpreadY, 0);

                Debug.DrawRay(RckPlayer.instance.mainCamera.transform.position, ray.direction * currentWeaponOnHand.weaponItem.Reach, Color.red, 3);

                if(!RckPlayer.instance.isInThirdPerson)
                    currentWeaponOnHand.FireRaycast(RckPlayer.instance, ray);
                else
                    ThirdPersonPlayer.instance.currentWeaponOnHand.FireRaycast(RckPlayer.instance, ray);
            }

            // Apply recoil
            RckPlayer.instance.mouseLook.recoil += currentWeaponOnHand.weaponItem.recoilHit;

        }

        public void FireWeaponEmptyEvent()
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

            if (!isAiming)
                fpcAnim.Play("Hipfire_1_Empty", 0, 0.0f);
            else
                fpcAnim.Play("Aimfire_1_Empty", 0, 0.0f);

            if (!currentWeaponOnHand.audioSource.isPlaying)
            {
                currentWeaponOnHand.audioSource.clip = currentWeaponOnHand.weaponItem.emptyFireSound;
                currentWeaponOnHand.audioSource.Play();
            }

            cameraAnim.AttackAnimation(equipment.currentWeapon.weaponAttacks[curAttackType].cameraAnim);

            if (ThirdPersonPlayer.instance.tpsWeaponDrawn)
            {
                // Third Person
                if (!isAiming)
                    tpsAnim.Play(currentWeaponOnHand.weaponItem.weaponType.ToString() + "_Hipfire_1", 1, 0.0f);
                else
                    tpsAnim.Play(currentWeaponOnHand.weaponItem.weaponType.ToString() + "_Aimfire_1", 1, 0.0f);

                tpsAnim.Update(0);
            }


            // Force immediate update
            fpcAnim.Update(0);

            holdingFire = false;
        }

        public bool wasAiming = false;
        public void FirearmAimCheck()
        {
            if (!RckPlayer.instance.isAlive)
                return;

            isBlocking = (blockInput && weaponDrawn && canAttack &&
                          !RckPlayer.instance.isInteracting && !RckPlayer.instance.isInConversation &&
                           !QuestManagerUI.instance.isUIopened && !InventoryUI.instance.isOpen && !isUnbalanced && !RckPlayer.instance.isLooting && !RckPlayer.instance.isInDeathSequence &&
                           !BookReaderManager.instance.backgroundUI.activeInHierarchy && !RckPlayer.instance.isInCutsceneMode);

            isAiming = isBlocking;

            fpcAnim.SetBool("isBlocking", isBlocking);
            tpsAnim.SetBool("isBlocking", isBlocking);

            if (currentWeaponOnHand.weaponItem.animateAim)
            {
                fpcAnim.Update(0);
                tpsAnim.Update(0);
            }

            if (isBlocking && !wasBlocking)
            {
                if (currentWeaponOnHand.weaponItem.animateAim)
                {
                    fpcAnim.ResetTrigger("hasBlockedAttack");
                    fpcAnim.SetTrigger("StartBlocking");

                    tpsAnim.ResetTrigger("hasBlockedAttack");
                    tpsAnim.SetTrigger("StartBlocking");
                }

                weaponSway.amount = currentWeaponOnHand.weaponItem.swayWhileAiming;
                RckPlayer.instance.isRunning = false; // stop running

                if(!RckPlayer.instance.isInThirdPerson)
                    RckPlayer.instance.uiManager.crosshairGameObject.SetActive(false);
            }

            if (currentWeaponOnHand.weaponItem.animateAim)
            {
                fpcAnim.Update(0);
                tpsAnim.Update(0);
            }

            if (isBlocking)
            {
                if (!wasBlocking)
                    wasBlocking = true;
            }
            else
            {
                if (wasBlocking)
                {
                    wasBlocking = false;
                    isBlocking = false;

                    weaponSway.amount = defaultSway;

                    RckPlayer.instance.uiManager.crosshairGameObject.SetActive(true);
                }
            }
        }

        public void FirearmReload()
        {
            if(weaponDrawn && canAttack && currentWeaponOnHand.weaponItem.weaponClass == WeaponClass.Firearm && Inventory.PlayerInventory.GetItemCount(equipment.currentWeapon.ammo) > 0 &&
                equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 < currentWeaponOnHand.weaponItem.clipRounds)
            {
                canAttack = false;

                if (currentWeaponOnHand.weaponItem.reloadType == ReloadType.Normal)
                {
                    // Make reload happen
                    if (equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 == 0)
                    {
                        fpcAnim.Play("Reload_Empty", 0, 0.0f);
                        equipment.currentWeaponAnimator.Play("Reload_Empty", 0, 0.0f);
                    }
                    else
                    {
                        fpcAnim.Play("Reload", 0, 0.0f);
                        equipment.currentWeaponAnimator.Play("Reload", 0, 0.0f);
                    }

                    if (ThirdPersonPlayer.instance.tpsWeaponDrawn)
                    {
                        // Make reload happen
                        if (equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 == 0)
                        {
                            tpsAnim.Play(currentWeaponOnHand.weaponItem.ItemID + "_Reload_1_Empty", 1, 0.0f);
                            ThirdPersonPlayer.instance.currentWeaponAnimator.Play("Reload_Empty_TPS", 0, 0.0f);
                        }
                        else
                        {
                            tpsAnim.Play(currentWeaponOnHand.weaponItem.ItemID + "_Reload_1", 1, 0.0f);
                            ThirdPersonPlayer.instance.currentWeaponAnimator.Play("Reload_TPS", 0, 0.0f);
                        }

                        tpsAnim.Update(0);
                    }

                    // Force immediate update
                    fpcAnim.Update(0);

                    equipment.currentWeaponAnimator.Update(0);
                    ThirdPersonPlayer.instance.currentWeaponAnimator.Update(0);

                    // play sounds

                    // rest is handled by anim events
                }
                else if (currentWeaponOnHand.weaponItem.reloadType == ReloadType.Single)
                {
                    // Handle single reload
                    StartCoroutine(SingleReloadTask());
                }
            }
        }

        public bool wantsToCancelRelaod = false;
        IEnumerator SingleReloadTask()
        {
            if (Inventory.PlayerInventory.GetItemCount(equipment.currentWeapon.ammo) > 0)
            {
                // Start going
                fpcAnim.CrossFade("Reload_Start", 0.0f);
                equipment.currentWeaponAnimator.Play("Reload_Start", 0, 0.0f);

                if (ThirdPersonPlayer.instance.tpsWeaponDrawn)
                {
                    tpsAnim.Play(currentWeaponOnHand.weaponItem.ItemID + "_ReloadS_Start", 1, 0.0f);
                    tpsAnim.Update(0);
                }

                fpcAnim.Update(0);
                equipment.currentWeaponAnimator.Update(0);

                yield return new WaitForEndOfFrame();

                // Start going in input
                while (fpcAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                    yield return null;

                wantsToCancelRelaod = false;

                do
                {
                    fpcAnim.CrossFadeInFixedTime("Reload_PutOneBullet", 0.0f);
                    equipment.currentWeaponAnimator.Play("Reload_PutOneBullet", 0, 0.0f);

                    if (ThirdPersonPlayer.instance.tpsWeaponDrawn)
                    {
                        tpsAnim.Play(currentWeaponOnHand.weaponItem.ItemID + "_ReloadS_PutOneBullet", 1, 0.0f);
                        tpsAnim.Update(0);
                    }

                    fpcAnim.Update(0);
                    equipment.currentWeaponAnimator.Update(0);

                    yield return new WaitForEndOfFrame();

                    while (fpcAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                    {
                        if (holdingFire || (Inventory.PlayerInventory.GetItemCount(equipment.currentWeapon.ammo) <= 0))
                            wantsToCancelRelaod = true;

                        yield return null;
                    }

                    if (equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 >= currentWeaponOnHand.weaponItem.clipRounds)
                        wantsToCancelRelaod = true;
                }
                while (wantsToCancelRelaod == false);

                fpcAnim.CrossFade("Reload_End", 0.0f);
                equipment.currentWeaponAnimator.Play("Reload_End", 0, 0.0f);

                if (ThirdPersonPlayer.instance.tpsWeaponDrawn)
                {
                    tpsAnim.Play(currentWeaponOnHand.weaponItem.ItemID + "_ReloadS_End", 1, 0.0f);
                    tpsAnim.Update(0);
                }

                fpcAnim.Update(0);
                equipment.currentWeaponAnimator.Update(0);
            }
            yield return null;
        }

        public void FirearmReloadEvent()
        {
            int remainder = currentWeaponOnHand.weaponItem.clipRounds - equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1;
            int totalAmmo = Inventory.PlayerInventory.GetItemCount(equipment.currentWeapon.ammo);

            if(totalAmmo >= remainder)
            {
                Inventory.PlayerInventory.RemoveItem(equipment.currentWeapon.ammo, remainder, false);
                equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 += remainder;
                SetWeaponUI();
            }
            else
            {
                // Not enough ammo, partial reload
                Inventory.PlayerInventory.RemoveItem(equipment.currentWeapon.ammo, totalAmmo, false);
                equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1 += totalAmmo;
                SetWeaponUI();
            }
        }

        public void AttackTap()
        {
            if (equipment.currentWeapon != null && fpcAnim != null)
            {
                if (!isAttacking && canAttack && RckPlayer.instance.AreControlsEnabled() &&
                !RckPlayer.instance.isInteracting && !RckPlayer.instance.isInConversation &&
                !QuestManagerUI.instance.isUIopened && !InventoryUI.instance.isOpen && !isUnbalanced && !RckPlayer.instance.isLooting &&
                !BookReaderManager.instance.backgroundUI.activeInHierarchy && !RckPlayer.instance.isInCutsceneMode && !TutorialAlertMessage.instance.messageOpened && !RckPlayer.instance.isInDeathSequence)
                {
                    if (!weaponDrawn)
                        DrawWeapon();
                    else
                    {
                        if (equipment.currentWeapon.weaponType == WeaponType.Crossbow)
                            CrossbowAttack();
                        else if (equipment.currentWeapon.weaponType == WeaponType.Bow)
                        {
                            BowAttack();
                            inputShootArrow = true;
                        }
                        else
                            MeleeAttackEvent();
                    }
                }
            }
        }

        public void AttackCastSpell()
        {
            if(spellKnowledge.spellInUse != null)
            {
                if (!isAttacking && canAttack && !isDrawingWeapon && !isUndrawingWeapon &&
                !RckPlayer.instance.isInteracting && !RckPlayer.instance.isInConversation &&
                !QuestManagerUI.instance.isUIopened && !InventoryUI.instance.isOpen && !isUnbalanced && !RckPlayer.instance.isLooting && !RckPlayer.instance.isInDeathSequence &&
                !BookReaderManager.instance.backgroundUI.activeInHierarchy && !RckPlayer.instance.isInCutsceneMode && !TutorialAlertMessage.instance.messageOpened && fpcAnim != null)
                {
                    // Check if the player has enough mana
                    if(RckPlayer.instance.playerAttributes.CurMana >= spellKnowledge.spellInUse.manaCost)
                        AttackCastSpellEvent();
                    else
                        AlertMessage.instance.InitAlertMessage("You don't have enough mana.", 2f, false);
                }
            }
            else
            {
                AlertMessage.instance.InitAlertMessage("You don't have any spell equipped.", 2f, false);
            }
        }

        public void AttackHold()
        {
            if (equipment.currentWeapon != null && fpcAnim != null)
                if (!isAttacking && canAttack && RckPlayer.instance.AreControlsEnabled() &&
                !RckPlayer.instance.isInteracting && !RckPlayer.instance.isInConversation &&
                !QuestManagerUI.instance.isUIopened && !InventoryUI.instance.isOpen && !isUnbalanced && !RckPlayer.instance.isLooting && !RckPlayer.instance.isInDeathSequence &&
                !BookReaderManager.instance.backgroundUI.activeInHierarchy && !RckPlayer.instance.isInCutsceneMode)
            {
                if (!weaponDrawn)
                    DrawWeapon();
                else
                {
                    if (equipment.currentWeapon.weaponType == WeaponType.Crossbow)
                        CrossbowAttack();
                    else if (equipment.currentWeapon.weaponType == WeaponType.Bow)
                            BowAttack();
                    else
                        MeleeChargedAttack();
                }
            }
        }

        public void PutWeaponAway()
        {
            if (equipment.currentWeapon != null && fpcAnim != null)
                if (weaponDrawn && canAttack && 
                !RckPlayer.instance.isInteracting && !RckPlayer.instance.isInConversation &&
                !QuestManagerUI.instance.isUIopened && !InventoryUI.instance.isOpen && !isUnbalanced && !RckPlayer.instance.isLooting && !RckPlayer.instance.isInDeathSequence &&
                !BookReaderManager.instance.backgroundUI.activeInHierarchy)
                UndrawWeapon();
        }

        public void OnEquipmentChanges()
        {
            // If there is a weapon equipped
            if (equipment.itemsEquipped[(int)EquipmentSlots.RHand] != null && equipment.itemsEquipped[(int)EquipmentSlots.RHand].item != null)
            {
                // Destroy previous weapon equipped if there was a swap
                if (equipment.currentWeapon != null)
                {
                    Destroy(equipment.currentWeaponObject);
                    equipment.currentWeapon = null;
                }

                // Instantiate Weapon on hand, assign references
                equipment.currentWeapon = (WeaponItem)equipment.itemsEquipped[(int)EquipmentSlots.RHand].item;
                if (equipment.currentWeapon.weaponType == WeaponType.Bow || equipment.currentWeapon.weaponType == WeaponType.Crossbow)
                {
                    equipment.currentWeaponObject = Instantiate(equipment.currentWeapon.WeaponOnHand, fpsArms.lHand);
                    // Bow make use of weaponAnimator, set it
                    equipment.currentWeaponAnimator = equipment.currentWeaponObject.GetComponent<Animator>();
                }
                else
                    equipment.currentWeaponObject = Instantiate(equipment.currentWeapon.WeaponOnHand, fpsArms.rHand);

                currentWeaponOnHand = equipment.currentWeaponObject.GetComponent<WeaponOnHand>();

                equipment.currentWeaponObject.SetLayer(RCKLayers.FirstPerson, true);
                defaultWeaponOnHand.gameObject.SetActive(false);

                equipment.currentWeaponObject.SetActive(false);

                fpcAnim.GetComponent<Animator>().runtimeAnimatorController = equipment.currentWeapon.fpsAnimatorController;
                fpcAnim.Rebind();

                curAttackType = 0;
                curChargedAttackType = 0;

                //cameraAnim.m_Animator.runtimeAnimatorController = equipment.currentWeapon.CameraAnimator;

                isDrawingWeapon = false;
                isUndrawingWeapon = false;
                isAttacking = false;
                isBlocking = false;
                isUnbalanced = false;
                canAttack = true;

                equipment.currentWeaponAnimator = equipment.currentWeaponObject.GetComponent<Animator>();

                ThirdPersonPlayer.instance.UpdateAnimator();
                if (weaponDrawn)
                {
                    ThirdPersonPlayer.instance.tpsWeaponDrawn = false;
                    weaponDrawn = false;
                    DrawWeapon();
                }

            }
            else if (equipment.currentWeapon == null)
            {
                // Unequip = destroy previous equipped weapon
                Destroy(equipment.currentWeaponObject);
                equipment.currentWeapon = null;

                cameraAnim.m_Animator.runtimeAnimatorController = defaultCameraAnimator;

                isDrawingWeapon = false;
                isUndrawingWeapon = false;
                isAttacking = false;
                isBlocking = false;
                isUnbalanced = false;

                equipment.currentWeapon = defaultWeapon;

                equipment.currentWeaponObject = null;
                currentWeaponOnHand = defaultWeaponOnHand;
                defaultWeaponOnHand.gameObject.SetActive(true);

                ThirdPersonPlayer.instance.currentWeaponOnHand = ThirdPersonPlayer.instance.defaultWeaponOnHand;

                fpcAnim.GetComponent<Animator>().runtimeAnimatorController = equipment.currentWeapon.fpsAnimatorController;
                fpcAnim = fpsArmsObject.GetComponent<Animator>();

                ThirdPersonPlayer.instance.UpdateAnimator();
                if (weaponDrawn)
                {
                    ThirdPersonPlayer.instance.tpsWeaponDrawn = false;
                    weaponDrawn = false;
                    DrawWeapon();
                }

                canAttack = true;
            }

            if (!projectileHolder && equipment.currentWeapon != null && equipment.currentWeapon.weaponType == WeaponType.Crossbow)
                projectileHolder = equipment.currentWeaponObject.GetComponent<CrossbowReferences>().projectileHolder;

            Equipment.PlayerEquipment.OnEquipmentChanges();
            SetWeaponUI();
            
        }

        public void SetWeaponUI()
        {
            if (equipment.currentWeapon != null)
            {
                weaponUI.weaponIcon.sprite = equipment.currentWeapon.ItemIcon;
                if (equipment.currentWeapon.weaponType == WeaponType.Bow || equipment.currentWeapon.weaponType == WeaponType.Crossbow)
                {
                    if (equipment.itemsEquipped[(int)EquipmentSlots.Ammo] != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item != null)
                    {
                        weaponUI.SetAmmoAmount(true, equipment.itemsEquipped[(int)EquipmentSlots.Ammo].Amount.ToString());
                    }
                    else
                        weaponUI.SetAmmoAmount(false);
                }
                else if(equipment.currentWeapon.weaponClass == WeaponClass.Firearm)
                {
                    int totalAmmo = Inventory.PlayerInventory.GetItemCount(equipment.currentWeapon.ammo);

                    weaponUI.SetAmmoAmountFirearm(true, equipment.itemsEquipped[(int)EquipmentSlots.RHand].metadata.intProperty1, totalAmmo);
                }
                else
                    weaponUI.SetAmmoAmount(false);
            }

            if(spellKnowledge.spellInUse != null)
            {
                equippedSpellUI.gameObject.SetActive(true);
                equippedSpellUI.sprite = spellKnowledge.spellInUse.spellIcon;
            }
            else
            {
                equippedSpellUI.gameObject.SetActive(false);
                equippedSpellUI.sprite = equippedSpellUI.transform.parent.GetComponent<Image>().sprite;
            }

            if(equipment.itemsEquipped[(int)EquipmentSlots.Throwable] != null && equipment.itemsEquipped[(int)EquipmentSlots.Throwable].item != null)
            {
                throwableUI.gameObject.SetActive(true);
                throwableUI.weaponIcon.sprite = equipment.itemsEquipped[(int)EquipmentSlots.Throwable].item.ItemIcon;
                throwableUI.SetAmmoAmount(true, equipment.itemsEquipped[(int)EquipmentSlots.Throwable].Amount.ToString());
            }
            else
            {
                throwableUI.gameObject.SetActive(false);
            }
        }

        public void RemoveCurrentWeapon()
        {
            if (equipment.currentWeaponObject != null)
            {
                Destroy(equipment.currentWeaponObject);
                equipment.currentWeapon = defaultWeapon;
                cameraAnim.m_Animator.runtimeAnimatorController = defaultCameraAnimator;
                isAttacking = false;
                isBlocking = false;
                isUnbalanced = false;
                canAttack = false;

                FullComboChainReset();

                OnEquipmentChanges();
                fpcAnim.SetTrigger("ImmediateReturnToEmpty");
                ThirdPersonPlayer.instance.UpdateAnimator();
                // If weapon was draw, draw the default as well
                if (weaponDrawn)
                {
                    ThirdPersonPlayer.instance.tpsWeaponDrawn = false;
                    weaponDrawn = false;
                    DrawWeapon();
                }

            }
        }

        public void BowAttack()
        {
            // Check if there is ammo (and if it's of correct type
            if (equipment.itemsEquipped[(int)EquipmentSlots.Ammo] != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].Amount >= 1)
            {
                // Check if the ammo is compatible with the equipped weapon
                if (!((AmmoItem)equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item).type.HasFlag(AmmoItem.AmmoType.Arrow))
                {
                    AlertMessage.instance.InitAlertMessage("The equipped ammo cannot be used with the current weapon.", AlertMessage.DEFAULT_MESSAGE_DURATION_SHORT);
                    return;
                }

                // Player wants to nock an arrow
                wantsToAttack = true;

                isAttacking = true;

                curAttackType = 0; // you may choose different shot types (ex. for the crouch)

                fpcAnim.ResetTrigger("Attack");
                equipment.currentWeaponAnimator.ResetTrigger("Load");
                fpcAnim.Play("Load");
                equipment.currentWeaponAnimator.Play("Load");

                ThirdPersonPlayer.instance.m_Animator.ResetTrigger("Attack");
                ThirdPersonPlayer.instance.currentWeaponAnimator.ResetTrigger("Load");
                ThirdPersonPlayer.instance.currentWeaponAnimator.Play("Load");
                ThirdPersonPlayer.instance.m_Animator.Play("Load (Bow)");
                ThirdPersonPlayer.instance.m_Animator.SetTrigger("CancelJump");

                fpcAnim.Update(0);
                ThirdPersonPlayer.instance.m_Animator.Update(0);

                lastAttackIndex = curAttackType;
                RckPlayer.instance.playerAttributes.DamageStamina(equipment.currentWeapon.weaponAttacks[lastAttackIndex].StaminaAmount, true, RCKSettings.DRAIN_STAMINA_ON_ATTACK_SPEEDAMOUNT);
                RckPlayer.instance.StopRecoveringStamina();

                canAttack = false;
                lastAttackWasCharged = false;
            }
        }

        public void ShootArrow()
        {
            // Set the animation from the AnimatorController
            fpcAnim.SetTrigger("Attack");
            fpcAnim.SetInteger("AttackType", curAttackType + 1);

            equipment.currentWeaponAnimator.ResetTrigger("Shoot");
            equipment.currentWeaponAnimator.SetTrigger("Shoot");

            if (ThirdPersonPlayer.instance.m_Animator)
            {
                // Set the animation from the AnimatorController
                ThirdPersonPlayer.instance.m_Animator.SetTrigger("Attack");
                ThirdPersonPlayer.instance.m_Animator.SetInteger("AttackType", curAttackType + 1);
                ThirdPersonPlayer.instance.m_Animator.SetTrigger("CancelJump");
            }

            if (ThirdPersonPlayer.instance.currentWeaponAnimator)
            {
                ThirdPersonPlayer.instance.currentWeaponAnimator.ResetTrigger("Shoot");
                ThirdPersonPlayer.instance.currentWeaponAnimator.SetTrigger("Shoot");
            }

            projectileNocked = false;
            wantsToAttack = false;
            isAttacking = false;

            ThirdPersonPlayer.instance.DestroyAmmoProjectile();

            // Shoot the arrow
            var ray = RckPlayer.instance.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            currentProjectile.Shoot(equipment.currentWeapon, (AmmoItem)equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item, Entity.GetPlayerEntity(), ray.direction, true);

            Inventory.PlayerInventory.RemoveItem(equipment.itemsEquipped[(int)EquipmentSlots.Ammo], 1, false);
            SetWeaponUI();
            RckPlayer.instance.InvokeResetRecover();

            if(equipment.currentWeapon.weaponAttacks[0] != null)
                PlayerCombat.instance.currentWeaponOnHand.PlayOneShot(equipment.currentWeapon.weaponAttacks[0].attackSound);
        }

        public bool projectileNocked = false;
        public void BowAttackCheck()
        {
            if(equipment.currentWeapon != null)
            // Release bow
            if (equipment.currentWeapon.weaponType == WeaponType.Bow &&
                projectileNocked && currentProjectile != null && (RckPlayer.instance.input.currentActionMap.FindAction("Attack").triggered || inputShootArrow))
            {
                ShootArrow();
                inputShootArrow = false;
            }
        }

        public Transform projectileHolder;

        public void CrossbowAttack()
        {
            // Check if there is ammo (and if it's of correct type
            if (equipment.itemsEquipped[(int)EquipmentSlots.Ammo] != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].Amount >= 1)
            {
                // Check if the ammo is compatible with the equipped weapon
                if (!((AmmoItem)equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item).type.HasFlag(AmmoItem.AmmoType.Dart))
                {
                    AlertMessage.instance.InitAlertMessage("The equipped ammo cannot be used with the current weapon.", AlertMessage.DEFAULT_MESSAGE_DURATION_SHORT);
                    return;
                }

                // Player wants to nock an arrow
                wantsToAttack = true;

                isAttacking = true;

                curAttackType = 0; // you may choose different shot types (ex. for the crouch)

                // Check if the fatigue is enough for the current attack
                //if (playerVitals.playerAttributes.CurStamina < equipment.currentWeapon.weaponAttacks[curAttackType].StaminaAmount)
                //    curAttackType = 0;

                fpcAnim.ResetTrigger("Attack");
                equipment.currentWeaponAnimator.ResetTrigger("Load");

                // Set the animation from the AnimatorController
                //fpcAnim.SetTrigger("Attack");
                //fpcAnim.SetInteger("AttackType", 0);

                fpcAnim.Play("Reload");
                equipment.currentWeaponAnimator.Play("Reload");

                //equipment.currentWeaponAnimator.SetTrigger("Load");

                //cameraAnim.AttackAnimation(equipment.currentWeapon.weaponAttacks[curAttackType].cameraAnim);

                lastAttackIndex = curAttackType;
                //playerVitals.AttackFatigue(equipment.currentWeapon.weaponAttacks[curAttackType].StaminaAmount);

                // To reset the combo if the attack is not done fast
                //if (curAttackType + 1 < equipment.currentWeapon.AttackTypes)
                //    curAttackType++;
                //else
                //    curAttackType = 0;

                //CancelInvoke("ResetComboChain");

                //if (curAttackType != 0)
                //    Invoke("ResetComboChain", equipment.currentWeapon.attackChainTime);

                // audio
                if (equipment.currentWeapon.weaponAttacks[curAttackType].attackSound != null)
                {
                    combatAudioSource.clip = equipment.currentWeapon.weaponAttacks[curAttackType].attackSound;
                    combatAudioSource.Play();
                }

                canAttack = false;
                lastAttackWasCharged = false;
            }

            if (weaponDrawn && projectileNocked && currentProjectile != null)
            {
                // Set the animation from the AnimatorController
                fpcAnim.SetTrigger("Attack");
                fpcAnim.SetInteger("AttackType", 1);

                equipment.currentWeaponAnimator.ResetTrigger("Shoot");
                equipment.currentWeaponAnimator.SetTrigger("Shoot");

                projectileNocked = false;
                wantsToAttack = false;
                isAttacking = false;

                // Shoot the arrow
                var ray = RckPlayer.instance.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                currentProjectile.Shoot(equipment.currentWeapon, (AmmoItem)equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item, Entity.GetPlayerEntity(), ray.direction, true);

                Inventory.PlayerInventory.RemoveItem(equipment.itemsEquipped[(int)EquipmentSlots.Ammo], 1);
                SetWeaponUI();
            }
        }

        public void DrawWeapon()
        {
            fpcAnim.SetTrigger("Draw");
            fpcAnim.SetBool("isDrawn", true);
            fpcAnim.ResetTrigger("Undraw");

            // Third Person
            ThirdPersonPlayer.instance.combatType = (int)equipment.currentWeapon.weaponType;
            tpsAnim.SetInteger("CombatType", ThirdPersonPlayer.instance.combatType);

            tpsAnim.SetTrigger("Draw");
            tpsAnim.SetBool("isDrawn", true);
            tpsAnim.ResetTrigger("Undraw");

            // Force immediate update
            fpcAnim.Update(0);

            if(tpsAnim != null)
                tpsAnim.Update(0);

            FullComboChainReset();
            wantsToAttack = false;

            isDrawingWeapon = true;
            isUndrawingWeapon = false;
            

            //weaponDrawn = true; // Handled by PCFPSArms via Animation Event
        }

        public void UndrawWeapon()
        {
            ThirdPersonPlayer.instance.tpsWeaponDrawn = false;
            weaponDrawn = false;
            fpcAnim.SetBool("isDrawn", false);
            fpcAnim.ResetTrigger("Draw");
            fpcAnim.SetTrigger("Undraw");

            // Third Person
            tpsAnim.SetBool("isDrawn", false);
            tpsAnim.ResetTrigger("Draw");
            tpsAnim.SetTrigger("Undraw");

            // Force immediate update
            fpcAnim.Update(0);
            tpsAnim.Update(0);

            isUndrawingWeapon = true;

            if (currentWeaponOnHand.weaponItem.sOnDraw)
                currentWeaponOnHand.PlayOneShot(currentWeaponOnHand.weaponItem.sOnSheathe);

            FullComboChainReset();
        }

        public bool wasBlocking = false;
        public void BlockCheck()
        {
            if (!RckPlayer.instance.isAlive)
                return;

            isBlocking = (blockInput && weaponDrawn && canAttack &&
                          !RckPlayer.instance.isInteracting && !RckPlayer.instance.isInConversation &&
                           !QuestManagerUI.instance.isUIopened && !InventoryUI.instance.isOpen && !isUnbalanced && !RckPlayer.instance.isLooting && !RckPlayer.instance.isInDeathSequence &&
                           !BookReaderManager.instance.backgroundUI.activeInHierarchy && !RckPlayer.instance.isInCutsceneMode);

            fpcAnim.SetBool("isBlocking", isBlocking);
            tpsAnim.SetBool("isBlocking", isBlocking);

            fpcAnim.Update(0);
            tpsAnim.Update(0);

            if (isBlocking && !wasBlocking)
            {
                fpcAnim.ResetTrigger("hasBlockedAttack");
                fpcAnim.SetTrigger("StartBlocking");

                tpsAnim.ResetTrigger("hasBlockedAttack");
                tpsAnim.SetTrigger("StartBlocking");
                RckPlayer.instance.StopRecoveringStamina();
            }

            fpcAnim.Update(0);
            tpsAnim.Update(0);

            if (isBlocking)
            {
                if (!playerBlockingArea.activeSelf)
                    playerBlockingArea.SetActive(true);

                if (!wasBlocking)
                {
                    //playerVitals.BlockingFatigue();
                    wasBlocking = true;
                }
            }
            else
            {
                if (playerBlockingArea.activeSelf)
                    playerBlockingArea.SetActive(false);

                if (wasBlocking)
                {
                    //playerVitals.StopBlockingFatigue();
                    wasBlocking = false;
                    isBlocking = false;
                    RckPlayer.instance.InvokeResetRecover(true, 1);
                }
            }
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

        /// <summary>
        /// Handled by events on Animations, reset the attack
        /// </summary>
        public void ResetAttackStateEvent()
        {
            canAttack = true;
        }

        public void MeleeAttackEvent()
        {
            isAttacking = true;

            // Check if the fatigue is enough for the current attack
            //if (RckPlayer.instance.playerAttributes.CurStamina < equipment.currentWeapon.weaponAttacks[curAttackType].StaminaAmount)
            //    curAttackType = 0;


            // curAttackType constraints
            if (currentWeaponOnHand.weaponItem.weaponType == WeaponType.Unarmed && equipment.isUsingTorch)
                curAttackType = 0;

            fpcAnim.ResetTrigger("Attack");

            // Set the animation from the AnimatorController, the AnimationsEvents on the 'Swing' animation will do the job
            fpcAnim.SetTrigger("Attack");
            fpcAnim.SetInteger("AttackType", curAttackType);

            cameraAnim.AttackAnimation(equipment.currentWeapon.weaponAttacks[curAttackType].cameraAnim);

            // Third Person
            tpsAnim.ResetTrigger("Attack");
            tpsAnim.SetTrigger("Attack");
            tpsAnim.SetInteger("AttackType", curAttackType);
            tpsAnim.SetTrigger("CancelJump");

            // Force immediate update
            fpcAnim.Update(0);
            tpsAnim.Update(0);

            lastAttackIndex = curAttackType;
            RckPlayer.instance.playerAttributes.DamageStamina(equipment.currentWeapon.weaponAttacks[lastAttackIndex].StaminaAmount, true, RCKSettings.DRAIN_STAMINA_ON_ATTACK_SPEEDAMOUNT);
            RckPlayer.instance.StopRecoveringStamina();
            RckPlayer.instance.InvokeResetRecover();

            //playerVitals.AttackFatigue(equipment.currentWeapon.weaponAttacks[curAttackType].StaminaAmount);

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

        public void AttackCastSpellEvent()
        {
            isAttacking = true;
            isCastingSpell = true;

            // Check if the fatigue is enough for the current attack
            //if (RckPlayer.instance.playerAttributes.CurStamina < equipment.currentWeapon.weaponAttacks[curAttackType].StaminaAmount)
            //    curAttackType = 0;

            int castHand = (Equipment.PlayerEquipment.isUsingShield || Equipment.PlayerEquipment.isUsingTorch || (Equipment.PlayerEquipment.currentWeapon != null && 
                            Equipment.PlayerEquipment.currentWeapon.weaponType == WeaponType.Bow)) ? 1 : 0;

            fpcAnim.ResetTrigger("Attack");
            fpcAnim.ResetTrigger("CastSpell");
            tpsAnim.ResetTrigger("Attack");
            tpsAnim.ResetTrigger("CastSpell");

            // Set the animation from the AnimatorController
            fpcAnim.SetTrigger("CastSpell");
            fpcAnim.SetInteger("CastType", (int)spellKnowledge.spellInUse.mode);
            fpcAnim.SetInteger("CastHand", castHand);

            // Third Person
            tpsAnim.SetTrigger("CastSpell");
            tpsAnim.SetInteger("CastType", (int)spellKnowledge.spellInUse.mode);
            tpsAnim.SetInteger("CastHand", castHand);
            tpsAnim.SetTrigger("CancelJump");
            //cameraAnim.AttackAnimation(equipment.currentWeapon.weaponAttacks[curAttackType].cameraAnim);

            // Third Person
            //tpsAnim.ResetTrigger("Attack");
            //tpsAnim.SetTrigger("Attack");
            //tpsAnim.SetInteger("AttackType", curAttackType);

            // Force immediate update
            fpcAnim.Update(0);
            tpsAnim.Update(0);

            RckPlayer.instance.playerAttributes.DamageMana(spellKnowledge.spellInUse.manaCost, true, RCKSettings.DRAIN_MANA_ON_CAST_SPEEDAMOUNT);

            //playerVitals.AttackFatigue(equipment.currentWeapon.weaponAttacks[curAttackType].StaminaAmount);

            canAttack = false;
            wantsToAttack = false;
            lastAttackWasCharged = false;
        }

        public void BlockEvent()
        {
            throw new System.NotImplementedException();
        }

        public void OnEnterCombat()
        {
            throw new System.NotImplementedException();
        }

        public void UseMelee()
        {
            throw new System.NotImplementedException();
        }

        public void OnAttackIsBlocked()
        {
            canAttack = false;
            wantsToAttack = false;
            isAttacking = true;
            isBlocking = false;
            RckPlayer.instance.controlledByAnim = false;
            PlayerCombat.instance.RestoreLayerWeight();
            RckPlayer.instance.stopPlayerInputByAnim = false;

            FullComboChainReset();

            fpcAnim.SetTrigger("hasBeenBlocked");
            tpsAnim.SetTrigger("hasBeenBlocked");
            
            // Force immediate update
            fpcAnim.Update(0);
            tpsAnim.Update(0);
        }

        public void MeleeChargedAttack()
        {
            // Perform a charged attack
            isAttacking = true;
            // Check if the fatigue is enough for the current attack

            if(RckPlayer.instance.isRunning)
            {
                if(equipment.currentWeapon.weaponChargedAttacks.Length >= 2)
                    curChargedAttackType = 1; // 1 means forward charged
                else
                    curChargedAttackType = 0;
            }
            else
                curChargedAttackType = 0; // 0 means standing

            fpcAnim.ResetTrigger("Attack");
            fpcAnim.ResetTrigger("ChargedAttack");

            // Set the animation from the AnimatorController, the AnimationsEvents on the 'Swing' animation will do the job
            fpcAnim.SetTrigger("ChargedAttack");
            fpcAnim.SetInteger("ChargedAttackType", curChargedAttackType);

            tpsAnim.ResetTrigger("Attack");
            tpsAnim.ResetTrigger("ChargedAttack");

            // Set the animation from the AnimatorController, the AnimationsEvents on the 'Swing' animation will do the job
            tpsAnim.SetTrigger("ChargedAttack");
            tpsAnim.SetInteger("ChargedAttackType", curChargedAttackType);
            tpsAnim.SetTrigger("CancelJump");

            // Force immediate update
            fpcAnim.Update(0);
            tpsAnim.Update(0);

            cameraAnim.AttackAnimation(equipment.currentWeapon.weaponChargedAttacks[curChargedAttackType].cameraAnim);

            lastChargedAttackIndex = curChargedAttackType;
            RckPlayer.instance.playerAttributes.DamageStamina(equipment.currentWeapon.weaponChargedAttacks[lastChargedAttackIndex].StaminaAmount, true, RCKSettings.DRAIN_STAMINA_ON_ATTACK_SPEEDAMOUNT);
            RckPlayer.instance.StopRecoveringStamina();
            RckPlayer.instance.InvokeResetRecover();


            CancelInvoke("ResetComboChainCharged");

            if (curChargedAttackType != 0)
                Invoke("ResetComboChainCharged", equipment.currentWeapon.chargedAttackChainTime);

            canAttack = false;
            wantsToAttack = false;
            lastAttackWasCharged = true;

            if(equipment.currentWeapon.weaponChargedAttacks[curChargedAttackType].isFullBody)
            {
                tpsAnim.SetLayerWeight(1, 0);
                tpsAnim.SetLayerWeight(2, 0);
            }
        }

        public void RestoreLayerWeight()
        {
            tpsAnim.SetLayerWeight(1, 1);
            tpsAnim.SetLayerWeight(2, 1);
        }

        void IMeleeAttacker.MeleeAttackCheck()
        {

        }

        public void AssignNewFPSArms(GameObject _fpsArms)
        {
            GameObject arms = Instantiate(_fpsArms, fpsTransform);

            fpsArms = arms.GetComponent<PCFPSArms>();

            fpsArmsObject = arms;
            fpcAnim = fpsArmsObject.GetComponent<Animator>();
            defaultWeaponOnHand = arms.GetComponentInChildren<WeaponOnHand>();

            weaponSway = fpsArmsObject.GetComponent<SwayEffect>();
            if (weaponSway != null)
                weaponSway.enabled = true;
            weaponSway.amount = defaultSway;


            if(equipment == null)
                equipment = RckPlayer.instance.GetComponent<Equipment>();

            if (!equipment.currentWeapon && defaultWeapon)
            {
                equipment.currentWeapon = defaultWeapon;
                currentWeaponOnHand = defaultWeaponOnHand;

                defaultWeaponOnHand.gameObject.SetActive(true);
                equipment.currentWeaponObject = null;
                fpcAnim = fpsArmsObject.GetComponent<Animator>();
                fpcAnim.GetComponent<Animator>().runtimeAnimatorController = equipment.currentWeapon.fpsAnimatorController;
                canAttack = true;

                if (weaponDrawn)
                    fpcAnim.SetTrigger("Draw");

                fpcAnim.SetBool("isDrawn", weaponDrawn);
            }

            equipment.FP_Arms = fpsArms.hands;
        }

        public void StartThrowingGrenade()
        {
            // Check if throwable is equipped
            if (weaponDrawn && equipment.itemsEquipped[(int)EquipmentSlots.Throwable] != null && equipment.itemsEquipped[(int)EquipmentSlots.Throwable].item != null)
            {
                if (!isAttacking && canAttack &&
                    !RckPlayer.instance.isInteracting && !RckPlayer.instance.isInConversation &&
                    !QuestManagerUI.instance.isUIopened && !InventoryUI.instance.isOpen && !isUnbalanced && !RckPlayer.instance.isLooting && !RckPlayer.instance.isInDeathSequence &&
                    !BookReaderManager.instance.backgroundUI.activeInHierarchy && !RckPlayer.instance.isInCutsceneMode && !TutorialAlertMessage.instance.messageOpened && fpcAnim != null)
                // has grenade equipped
                {
                    canAttack = false;
                    isAttacking = true;
                    isThrowingGrenade = true;
                    grenadeThrown = false;
                    fpcAnim.CrossFade("ThrowGrenade_Start", 0.15f);
                    tpsAnim.SetTrigger("StartThrowingGrenade");

                    fpcAnim.Update(0);
                    tpsAnim.Update(0);
                }
            }
            else
                AlertMessage.instance.InitAlertMessage("You don't have a throwable equipped!", 3f);
        }

        public void ThrowGrenadeEvent()
        {
            if(currentThrowable != null)
                currentThrowable.Throw(RckPlayer.instance, m_Camera.transform.forward, true);

            ThirdPersonPlayer.instance.DestroyThrowable();

            // Remove one throwable
            Inventory.PlayerInventory.RemoveItem(Equipment.PlayerEquipment.itemsEquipped[(int)EquipmentSlots.Throwable], 1);

            if (Equipment.PlayerEquipment.itemsEquipped[(int)EquipmentSlots.Throwable].Amount == 0)
            {
                Equipment.PlayerEquipment.itemsEquipped[(int)EquipmentSlots.Throwable].isEquipped = false;
                Equipment.PlayerEquipment.itemsEquipped[(int)EquipmentSlots.Throwable] = null;
            }
            SetWeaponUI();
        }
    }
}