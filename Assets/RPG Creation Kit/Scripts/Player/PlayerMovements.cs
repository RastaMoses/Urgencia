using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RPGCreationKit;
using System.IO;

namespace RPGCreationKit.Player
{
    public class PlayerMovements : PlayerStatus
    {
        [SerializeField] public CharacterController charController;
        [SerializeField] public MouseLook mouseLook;

        public bool enableGravityWithoutInput = true;
        public bool isFreezed = false;

        protected bool controlsEnabled = true;
        protected bool movementsEnabled = true;

        public bool isInCutsceneMode = false;

        public bool isJumping = false;
        public bool isWalking = false;
        public bool isRunning = false;
        public bool iscrouching = false;
        public bool isMounted = false;
        public bool isMounting = false;
        public bool isDismounting = false;

        // Movement controlled by anim
        // Used for charged attacks that moves the player
        public bool stopPlayerInputByAnim = false;
        public bool controlledByAnim = false; // if the movemenets of the player are controlled by animation like forward charged attack
        public bool runSpeedIfControlledByAnim = false;
        public float controlledByAnimX;
        public float controlledByAnimZ;

        public AudioClip[] maleJumpSounds;
        public AudioClip[] femaleJumpSounds;

        [Space(5)]

        [SerializeField] private GameObject fpHandsContainer;
        [SerializeField] private float fpsHandOffsetStep = 2;
        [SerializeField] private Vector3 FPSHandsOffsetWhileStanding;
        [SerializeField] private Vector3 FPSHandsOffsetWhileCrouching;

        [SerializeField][Range(0f, 1f)] private float m_RunstepLenghten;

        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        private float m_StepCycle;
        private float m_NextStep;

        public float timeAirbone = 0.0f;

        [Header("Dodge")]
        [SerializeField] private float dodgeDuration = 0.3f;
        [SerializeField] private float dodgeCooldown = 0.6f;
        [SerializeField] private bool dodgeRequiresGrounded = true;

        private bool isDodging = false;
        private float nextDodgeTime = 0f;
        private Coroutine dodgeCoroutine;

        public AudioClip dodgeSound;

        public bool IsCrouching
        {
            get { return iscrouching; }
            set
            {
                if (charController.isGrounded && !crouchEditInProgress)
                {
                    iscrouching = value;
                    OnCrouchChanges();
                }
            }
        }

        float x, z;

        Vector3 direction;

        public float curSpeed;

        public float walkSpeed = 1.5f;
        public float jogSpeed = 5.0f;
        public float runSpeed = 7.5f;
        public float crouchSpeed = 2.5f;

        bool isSprintingFat = false;
        [SerializeField] private float sprintCost;

        public float jumpForce = 2.5f;

        public float gravity = -9.81f;
        [SerializeField] private float fallMultiplier = 2.2f;
        [SerializeField] private float lowJumpMultiplier = 2.0f;

        public float m_StickToGroundForce = -7f;
        Vector3 velocity;

        [Header("Movement Feel")]
        [SerializeField] private float accel = 25f;
        [SerializeField] private float decel = 35f;
        [SerializeField] private float airAccel = 10f;
        [SerializeField] private float airDecel = 10f;

        [Header("Jump Forgiveness")]
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBuffer = 0.12f;
        private float coyoteCounter = 0f;
        private float jumpBufferCounter = 0f;
        private bool jumpHeld = false;

        private Vector3 horizVel = Vector3.zero;
        private float inputMag = 0f;

        private bool m_PreviouslyGrounded;
        public bool m_isMoving;
        private bool canStandUp = false;

        [SerializeField] private float cameraStandingY = 0f;
        [SerializeField] private float cameraCrouchingY = -.75f;

        [SerializeField] private float standingHeight = 2.7f;
        [SerializeField] private float crouchHeight = 1.7f;
        [SerializeField] private float crouchEditSpeed = 2f;

        [SerializeField] private float standingHeightHitZone = 2.5f;
        [SerializeField] private float crouchHeightHitZone = 1.5f;

        public bool isInThirdPerson;
        public ThirdPersonPlayer thirdPersonModel;

        public void INPUT_MoveAxis(InputAction.CallbackContext value)
        {
            Vector2 dValue = value.ReadValue<Vector2>();

            if (!inventory.IsOverencumbred())
            {
                x = dValue.x;
                z = dValue.y;
            }
            else
            {
                x = dValue.x;
                z = dValue.y;

                // force walking
                isWalking = true;
                InGameHelpUI.instance.SetWalkToggleUI(isWalking);
                AlertMessage.instance.InitAlertMessage("You are overencumbred.", 5, false);
            }
        }

        public void INPUT_JumpCmd(InputAction.CallbackContext value)
        {
            if (!controlsEnabled || isFreezed || isMounted)
                return;

            if (value.started)
            {
                jumpHeld = true;

                if (charController.isGrounded && IsCrouching)
                {
                    INPUT_CrouchCmd(value);
                    return;
                }

                // Buffer jump press
                jumpBufferCounter = jumpBuffer;
            }

            if (value.canceled)
            {
                jumpHeld = false;
            }
        }

        public void INPUT_RunCmd(InputAction.CallbackContext value)
        {
            if (controlsEnabled && !isFreezed && !isMounted)
                if (value.started && playerAttributes.CurStamina >= 5.0f)
                {
                    if (charController.isGrounded)
                    {
                        if (!isRunning && !PlayerCombat.instance.isAiming)
                        {
                            if (IsCrouching && canStandUp)
                            {
                                IsCrouching = false;
                                isRunning = true;
                            }
                            else if (!IsCrouching)
                                isRunning = true;
                        }
                        else
                            isRunning = false;
                    }
                }
        }

        public void INPUT_CrouchCmd(InputAction.CallbackContext value)
        {
            if (controlsEnabled && !isFreezed && !isMounted)
            {
                if (value.started)
                {
                    if (IsCrouching == true)
                    {
                        if (canStandUp)
                            IsCrouching = false;
                    }
                    else
                        IsCrouching = true;
                }
            }
            else if (isMounted && controlsEnabled && !isFreezed)
            {
                if (!isDismounting && !RckPlayer.instance.isMounting)
                {
                    if (value.started)
                        RckPlayer.instance.DismountFromMount();
                }
            }
        }

        public void INPUT_ChangePersonCmd(InputAction.CallbackContext value)
        {
            if (controlsEnabled && !isFreezed && value.started)
            {
                if (!isInThirdPerson)
                {
                    SwitchToThirdPerson();
                }
                else
                {
                    SwitchToFirstPerson();
                }
            }
        }

        public void INPUT_ToggleWalkCmd(InputAction.CallbackContext value)
        {
            if (controlsEnabled && !isFreezed && !isMounted)
                if (value.started)
                {
                    isWalking = !isWalking;

                    InGameHelpUI.instance.SetWalkToggleUI(isWalking);
                }
        }

        public void INPUT_DodgeCmd(InputAction.CallbackContext value)
        {
            if (controlsEnabled && !isFreezed && !isMounted)
                if (value.started)
                {
                    TryDodge();
                }
        }

        public override void Start()
        {
            base.Start();
            m_PreviouslyGrounded = charController.isGrounded;

            thirdPersonModel = ThirdPersonPlayer.instance;
        }

        public override void Update()
        {
            base.Update();

            if (!isFreezed)
            {
                UpdateJumpForgiveness();

                HandleMovements();

                if (IsCrouching)
                    CanStandUpCheck();

                BroadcastAnimatorToThirdPerson();
            }
        }

        public virtual void FixedUpdate()
        {

        }

        private void UpdateJumpForgiveness()
        {
            // Coyote time
            if (charController.isGrounded)
                coyoteCounter = coyoteTime;
            else
                coyoteCounter -= Time.deltaTime;

            // Jump buffer
            if (jumpBufferCounter > 0f)
                jumpBufferCounter -= Time.deltaTime;
        }

        public void ZeroVelocity()
        {
            horizVel = Vector3.zero;
            velocity = Vector3.zero;
        }

        private void HandleMovements()
        {
            if (isDodging)
                return;

            if (isMounted)
                return;

            // Ground snap
            if (charController.isGrounded && velocity.y < 0f)
                velocity.y = m_StickToGroundForce;

            // Read raw input direction
            Vector3 raw = (transform.right * x) + (transform.forward * z);
            inputMag = Mathf.Clamp01(raw.magnitude);
            Vector3 moveDir = (inputMag > 0.0001f) ? (raw / raw.magnitude) : Vector3.zero;

            if (isRunning || iscrouching)
            {
                isWalking = false;
                InGameHelpUI.instance.SetWalkToggleUI(isWalking);
            }

            if (inventory.IsOverencumbred())
            {
                if (isRunning)
                    isRunning = false;

                isWalking = true;

                InGameHelpUI.instance.SetWalkToggleUI(isWalking);
            }

            curSpeed = (isRunning) ? runSpeed : (IsCrouching) ? crouchSpeed : jogSpeed;

            if (isWalking)
                curSpeed = walkSpeed;

            if (controlledByAnim)
            {
                isRunning = false;

                curSpeed = runSpeedIfControlledByAnim ? runSpeed : jogSpeed;

                Vector3 animRaw = (transform.right * controlledByAnimX) + (transform.forward * controlledByAnimZ);
                float animMag = Mathf.Clamp01(animRaw.magnitude);
                moveDir = (animMag > 0.0001f) ? (animRaw / animRaw.magnitude) : Vector3.zero;
                inputMag = animMag > 0f ? 1f : 0f;
            }

            if (stopPlayerInputByAnim && !controlledByAnim)
            {
                moveDir = Vector3.zero;
                inputMag = 0f;
            }

            // Buffered jump and coyote time
            if (!IsCrouching && jumpBufferCounter > 0f && (charController.isGrounded || coyoteCounter > 0f))
            {
                DoJump();
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
            }

            // Velocity smoothing
            Vector3 desiredHoriz = moveDir * (curSpeed * inputMag);

            bool grounded = charController.isGrounded;
            float a = grounded ? accel : airAccel;
            float b = grounded ? decel : airDecel;

            float rate = (desiredHoriz.sqrMagnitude > horizVel.sqrMagnitude) ? a : b;
            horizVel = Vector3.MoveTowards(horizVel, desiredHoriz, rate * Time.deltaTime);

            // Gravity
            if (movementsEnabled || (enableGravityWithoutInput && !movementsEnabled))
                ApplyGravity();

            // Animator flags
            if (!isInCutsceneMode)
            {
                m_isMoving = (desiredHoriz.sqrMagnitude > 0.0001f);
                cameraAnim.m_Animator.SetBool("isMoving", m_isMoving);
                cameraAnim.m_Animator.SetBool("isWalking", !isRunning);
            }
            else
            {
                m_isMoving = false;
                cameraAnim.m_Animator.SetBool("isMoving", false);
                cameraAnim.m_Animator.SetBool("isWalking", false);
            }

            if (m_isMoving && isRunning)
                SprintFatigue();

            if (!movementsEnabled && !enableGravityWithoutInput)
            {
                horizVel = Vector3.zero;
                velocity = Vector3.zero;
            }

            // Move
            if (charController.enabled)
            {
                Vector3 motion = horizVel;
                motion.y = velocity.y;
                charController.Move(motion * Time.deltaTime);
            }

            ClampStates();

            if (!charController.isGrounded)
            {
                timeAirbone += 1 * Time.deltaTime;
            }

            if (!m_PreviouslyGrounded && charController.isGrounded)
            {
                Land();
            }

            ProgressStepCycle(curSpeed);

            m_PreviouslyGrounded = charController.isGrounded;
        }

        protected void Land()
        {
            // Landed
            isJumping = false;

            cameraAnim.m_Animator.SetTrigger("Land");
            PlayerCombat.instance.fpcAnim.ResetTrigger("Jump");
            PlayerCombat.instance.fpcAnim.SetTrigger("Land");
            ThirdPersonPlayer.instance.m_Animator.SetTrigger("Land");

            // Fall damage
            if (RCKSettings.FALLDAMAGE_ENABLED)
                if (timeAirbone >= RCKSettings.FALLDAMAGE_AIRBONE_THRESHOLD)
                {
                    float fallDmg = RCKFunctions.CalculateFallDamage(timeAirbone);
                    float reduction = RCKSettings.GetFalldamageAgilityReduction(playerAttributes.attributes.Agility, fallDmg);

                    fallDmg -= reduction;
                    if (fallDmg < 0)
                        fallDmg = 0;

                    GetIDamageable().Damage(new DamageContext(fallDmg, this, null, null, false));
                }

            timeAirbone = 0.0f;
        }

        private void DoJump()
        {
            if (!movementsEnabled && !enableGravityWithoutInput)
                return;

            if (controlledByAnim)
                return;

            AudioClip clip = null;

            if (SaveSystem.SaveSystemManager.instance.saveFile.PlayerData.playerSex == false)
            {
                if (maleJumpSounds != null && maleJumpSounds.Length > 0)
                    clip = maleJumpSounds[Random.Range(0, maleJumpSounds.Length)];
            }
            else
            {
                if (femaleJumpSounds != null && femaleJumpSounds.Length > 0)
                    clip = femaleJumpSounds[Random.Range(0, femaleJumpSounds.Length)];
            }

            if (clip != null)
                GameAudioManager.instance.PlayOneShot(AudioSources.Player, clip);

            // Keep your formula (gravity is negative)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

            ThirdPersonPlayer.instance.m_Animator.ResetTrigger("CancelJump");

            cameraAnim.m_Animator.SetTrigger("Jump");
            if (PlayerCombat.instance.canAttack)
            {
                StartCoroutine(DelayJumpAnim());
            }

            // Use stamina
            if (RCKSettings.USE_STAMINA_WHILE_JUMPING)
            {
                RckPlayer.instance.playerAttributes.DamageStamina(RCKSettings.STAMINA_DRAIN_PER_JUMP, true, RCKSettings.DRAIN_STAMINA_ON_ATTACK_SPEEDAMOUNT);
                RckPlayer.instance.StopRecoveringStamina();
                RckPlayer.instance.InvokeResetRecover();
            }

            isJumping = true;
        }

        // NEW: better gravity shaping
        private void ApplyGravity()
        {
            float dt = Time.deltaTime;

            if (charController.isGrounded && velocity.y <= 0f)
                return;

            // Rising
            if (velocity.y > 0f)
            {
                float mult = jumpHeld ? 1f : lowJumpMultiplier;
                velocity.y += gravity * mult * dt;
            }
            else
            {
                velocity.y += gravity * fallMultiplier * dt;
            }
        }

        IEnumerator DelayJumpAnim()
        {
            yield return new WaitForSeconds(0.1f);

            if (PlayerCombat.instance.canAttack)
            {
                PlayerCombat.instance.fpcAnim.ResetTrigger("Land");
                PlayerCombat.instance.fpcAnim.SetTrigger("Jump");
                ThirdPersonPlayer.instance.m_Animator.SetTrigger("Jump");
            }
        }

        /// <summary>
        /// Controls the states - never make Is Crouching and Is Running be both true at the same time etc
        /// </summary>
        private void ClampStates()
        {
            if (z <= .86f || IsCrouching)
                isRunning = false;

            if (playerAttributes.CurStamina <= 1.0f && isRunning)
                isRunning = false;

            if (isRunning && canStandUp)
                IsCrouching = false;
        }

        /// <summary>
        /// Modifies the Player dimensions when he's crouching
        /// </summary>
        private void OnCrouchChanges()
        {
            StartCoroutine(SmoothHeightChange());
            StartCoroutine(SmoothHitZoneChange());
            StartCoroutine(SmoothFPHandsChange());

            thirdPersonModel.m_Animator.SetBool("isCrouched", iscrouching);
        }

        public bool crouchEditInProgress = false;
        IEnumerator SmoothHeightChange()
        {
            crouchEditInProgress = true;
            if (IsCrouching)
            {
                Vector3 camTarget = new Vector3(cameraAnim.transform.localPosition.x, cameraCrouchingY, cameraAnim.transform.localPosition.z);

                // Lerp camera
                cameraAnim.LerpCamera(camTarget, crouchSpeed);

                while (charController.height > crouchHeight)
                {
                    charController.height -= crouchEditSpeed * Time.deltaTime;
                    charController.center = Vector3.down * (standingHeight - charController.height) / 2.0f;
                    yield return null;
                }

                cameraAnim.MoveCamera(camTarget, true);
                charController.height = crouchHeight;
            }
            else
            {
                Vector3 camTarget = new Vector3(cameraAnim.transform.localPosition.x, cameraStandingY, cameraAnim.transform.localPosition.z);

                // Lerp camera
                cameraAnim.LerpCamera(camTarget, crouchEditSpeed);

                while (charController.height < standingHeight)
                {
                    if (charController.height < standingHeight)
                        charController.height += crouchEditSpeed * Time.deltaTime;

                    charController.center = Vector3.down * (standingHeight - charController.height) / 2.0f;
                    yield return null;
                }

                cameraAnim.MoveCamera(camTarget, true);
                charController.height = standingHeight;
            }

            crouchEditInProgress = false;
            yield return null;
        }

        IEnumerator SmoothFPHandsChange()
        {
            if (IsCrouching)
            {
                while (fpHandsContainer.transform.localPosition != FPSHandsOffsetWhileCrouching)
                {
                    fpHandsContainer.transform.localPosition = Vector3.MoveTowards(fpHandsContainer.transform.localPosition, FPSHandsOffsetWhileCrouching, fpsHandOffsetStep * Time.deltaTime);
                    yield return null;
                }

                fpHandsContainer.transform.localPosition = FPSHandsOffsetWhileCrouching;
            }
            else
            {
                while (fpHandsContainer.transform.localPosition != FPSHandsOffsetWhileStanding)
                {
                    fpHandsContainer.transform.localPosition = Vector3.MoveTowards(fpHandsContainer.transform.localPosition, FPSHandsOffsetWhileStanding, fpsHandOffsetStep * Time.deltaTime);
                    yield return null;
                }

                fpHandsContainer.transform.localPosition = FPSHandsOffsetWhileStanding;
            }
        }

        public CapsuleCollider hitzoneCollider;
        public bool hitzoneCrouchEditInProgress = false;
        IEnumerator SmoothHitZoneChange()
        {
            hitzoneCrouchEditInProgress = true;
            if (IsCrouching)
            {
                while (hitzoneCollider.height > crouchHeightHitZone)
                {
                    hitzoneCollider.height -= crouchEditSpeed * Time.deltaTime;
                    hitzoneCollider.center = Vector3.down * (standingHeightHitZone - hitzoneCollider.height) / 2.0f;
                    yield return null;
                }

                hitzoneCollider.height = crouchHeightHitZone;
            }
            else
            {
                while (hitzoneCollider.height < standingHeightHitZone)
                {
                    if (hitzoneCollider.height < standingHeightHitZone)
                        hitzoneCollider.height += crouchEditSpeed * Time.deltaTime;

                    hitzoneCollider.center = Vector3.down * (standingHeightHitZone - hitzoneCollider.height) / 2.0f;
                    yield return null;
                }

                hitzoneCollider.height = standingHeightHitZone;
            }

            hitzoneCrouchEditInProgress = false;
            yield return null;
        }

        public void SprintFatigue()
        {
            if (isSprintingFat)
                return;

            StopRecoveringStamina();
            StopCoroutine("ReduceFatigue");
            StartCoroutine("ReduceFatigue");
        }

        public IEnumerator ReduceFatigue()
        {
            while (m_isMoving && isRunning)
            {
                CancelInvoke("ResetRecover");

                isSprintingFat = true;

                playerAttributes.CurStamina -= sprintCost * Time.deltaTime;

                if (playerAttributes.CurStamina <= 0)
                    playerAttributes.CurStamina = 0;

                yield return null;
            }

            isSprintingFat = false;
            Invoke("ResetRecover", recoverAfterActionDelay);
        }

        public void EnableDisableControls(bool _enable)
        {
            if (_enable)
            {
                mouseLook.LockCursor();
                // Enable them
                controlsEnabled = true;
                movementsEnabled = true;
                mouseLook.lookEnabled = true;
            }
            else
            {
                mouseLook.UnlockCursor();

                // Disable them
                controlsEnabled = false;
                movementsEnabled = false;
                mouseLook.lookEnabled = false;

                direction = Vector3.zero;
            }
        }

        public void FreezeOnly()
        {
            charController.enabled = false;
            isFreezed = true;

            direction = Vector3.zero;
            velocity = Vector3.zero;
            horizVel = Vector3.zero;

            if (charController.enabled)
                charController.Move(Vector3.zero);
        }

        public void Unfreeze()
        {
            isFreezed = false;
            direction = Vector3.zero;
            velocity = Vector3.zero;
            horizVel = Vector3.zero;

            if (charController.enabled)
                charController.Move(Vector3.zero);

            charController.enabled = true;
        }

        public float crouchRayDistance = 1.5f;
        public float crouchSphereRadius = 1f;
        public LayerMask crouchLayermask;

        private void CanStandUpCheck()
        {
            var colliders = Physics.OverlapSphere(transform.position + (transform.up.normalized), crouchSphereRadius, crouchLayermask);

            canStandUp = true; 

            for (int i = 0; i < colliders.Length; i++)
            {
                var c = colliders[i];
                if (!c) continue;

                // Ignore self
                if (c.transform == transform || c.transform.IsChildOf(transform))
                    continue;

                // If the collider is (or belongs to) an allowed tagged object, ignore it
                if (HasAllowedStandUpTag(c.transform))
                    continue;

                // Anything else blocks standing up
                canStandUp = false;
                return;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if(iscrouching)
                Gizmos.DrawSphere(transform.position + (transform.up.normalized), crouchSphereRadius);
        }

        private static bool HasAllowedStandUpTag(Transform t)
        {
            // Check the collider's object and parents
            while (t != null)
            {
                if (t.CompareTag("RPG Creation Kit/AI") || t.CompareTag("RPG Creation Kit/BodyPart"))
                    return true;
                t = t.parent;
            }
            return false;
        }

        public void FreezeAndDisableControl()
        {
            charController.enabled = false;
            isFreezed = true;

            direction = Vector3.zero;
            velocity = Vector3.zero;
            horizVel = Vector3.zero;

            if (charController.enabled)
                charController.Move(Vector3.zero);

            EnableDisableControls(false);
        }

        public void UnfreezeAndEnableControls()
        {
            isFreezed = false;
            direction = Vector3.zero;
            velocity = Vector3.zero;
            horizVel = Vector3.zero;

            if (charController.enabled)
                charController.Move(Vector3.zero);

            EnableDisableControls(true);

            charController.enabled = true;
        }

        public bool IsControlledByPlayer()
        {
            return (!isFreezed && charController.enabled && movementsEnabled && controlsEnabled);
        }

        public void ForceCrouch()
        {
            iscrouching = true;
            OnCrouchChanges();
        }

        public void ForceStandupFromCrouch(bool immediate)
        {
            if (!immediate)
            {
                iscrouching = false;
                OnCrouchChanges();
            }
            else
            {
                iscrouching = false;
            }
        }

        private void PlayFootStepAudio()
        {
            if (!charController.isGrounded)
                return;

            if (m_FootstepSounds == null || m_FootstepSounds.Length <= 1)
                return;

            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            AudioClip swap = m_FootstepSounds[n];

            GameAudioManager.instance.PlayOneShot(AudioSources.Player, m_FootstepSounds[n]);

            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = swap;
        }

        private void ProgressStepCycle(float speed)
        {
            if (charController.velocity.sqrMagnitude > 0f && (x != 0f || z != 0f))
            {
                m_StepCycle += (charController.velocity.magnitude + (speed * (!isRunning ? 1f : m_RunstepLenghten))) *
                               Time.deltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
                return;

            m_NextStep = m_StepCycle + m_StepInterval;
            PlayFootStepAudio();
        }

        public virtual void SwitchToThirdPerson()
        {
            thirdPersonModel.character.ShowAll();
            cameraAnim.SwitchToTPSCamera();
            isInThirdPerson = true;

            // Enable TPS weapon
            if (thirdPersonModel.currentWeaponOnHip != null)
            {
                MeshRenderer[] renderers;
                renderers = thirdPersonModel.currentWeaponOnHip.GetComponents<MeshRenderer>();
                if (renderers.Length > 0)
                    foreach (MeshRenderer r in renderers)
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

                SkinnedMeshRenderer[] skinnedRenderers;
                skinnedRenderers = thirdPersonModel.currentWeaponOnHip.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skinnedRenderers.Length > 0)
                    foreach (SkinnedMeshRenderer r in skinnedRenderers)
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            if (thirdPersonModel.currentWeaponObject != null)
            {
                MeshRenderer[] renderers;
                renderers = thirdPersonModel.currentWeaponObject.GetComponents<MeshRenderer>();
                if (renderers.Length > 0)
                    foreach (MeshRenderer r in renderers)
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

                SkinnedMeshRenderer[] skinnedRenderers;
                skinnedRenderers = thirdPersonModel.currentWeaponObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skinnedRenderers.Length > 0)
                    foreach (SkinnedMeshRenderer r in skinnedRenderers)
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            if (thirdPersonModel.currentWeaponOnHand != null)
                thirdPersonModel.currentWeaponOnHand.enabled = true;

            if (thirdPersonModel.currentAmmoObject != null)
                thirdPersonModel.currentAmmoObject.SetActive(true);

            if (Equipment.PlayerEquipment.currentShieldObject != null)
            {
                if (Equipment.PlayerEquipment.currentShieldObject.GetComponent<MeshRenderer>() != null)
                    Equipment.PlayerEquipment.currentShieldObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                if (Equipment.PlayerEquipment.currentShieldObject.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                    Equipment.PlayerEquipment.currentShieldObject.GetComponentInChildren<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            if (Equipment.PlayerEquipment.currentTorchInHand != null)
            {
                if (Equipment.PlayerEquipment.currentTorchInHand.GetComponent<MeshRenderer>() != null)
                    Equipment.PlayerEquipment.currentTorchInHand.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

                if (Equipment.PlayerEquipment.currentTorchInHand.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                    Equipment.PlayerEquipment.currentTorchInHand.GetComponentInChildren<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            if (thirdPersonModel.projectile != null)
            {
                if (thirdPersonModel.projectile.GetComponent<MeshRenderer>() != null)
                    thirdPersonModel.projectile.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            if (thirdPersonModel.curThrowable != null)
            {
                if (thirdPersonModel.curThrowable.GetComponent<MeshRenderer>() != null)
                    thirdPersonModel.curThrowable.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            // Disable FPS weapon
            if (PlayerCombat.instance.currentWeaponOnHand != null)
            {
                PlayerCombat.instance.currentWeaponOnHand.enabled = false;

                MeshRenderer[] renderers;
                renderers = PlayerCombat.instance.currentWeaponOnHand.gameObject.GetComponents<MeshRenderer>();
                if (renderers.Length > 0)
                    foreach (MeshRenderer r in renderers)
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

                SkinnedMeshRenderer[] skinnedRenderers;
                skinnedRenderers = PlayerCombat.instance.currentWeaponOnHand.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skinnedRenderers.Length > 0)
                    foreach (SkinnedMeshRenderer r in skinnedRenderers)
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            // Account for composed armor item mesh
            ComposedArmorItemMesh[] composedTPS = RckPlayer.instance.thirdPersonModel.GetComponentsInChildren<ComposedArmorItemMesh>();

            for (int i = 0; i < composedTPS.Length; i++)
            {
                composedTPS[i].EnableObjects();
            }

            ComposedArmorItemMesh[] composedFPS = RckPlayer.instance.fpHandsContainer.GetComponentsInChildren<ComposedArmorItemMesh>();
            for (int i = 0; i < composedFPS.Length; i++)
            {
                composedFPS[i].DisableObjects();
            }
        }

        public virtual void SwitchToFirstPerson()
        {
            if (thirdPersonModel.isInFreeLook)
                thirdPersonModel.OnLeaveCameraLook();

            thirdPersonModel.character.HideAll();
            cameraAnim.SwitchToFPSCamera();
            isInThirdPerson = false;

            // Disable TPS weapon
            if (thirdPersonModel.currentWeaponOnHip != null)
            {
                MeshRenderer[] renderers;
                renderers = thirdPersonModel.currentWeaponOnHip.GetComponents<MeshRenderer>();
                if (renderers.Length > 0)
                    foreach (MeshRenderer r in renderers)
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

                SkinnedMeshRenderer[] skinnedRenderers;
                skinnedRenderers = thirdPersonModel.currentWeaponOnHip.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skinnedRenderers.Length > 0)
                    foreach (SkinnedMeshRenderer r in skinnedRenderers)
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            if (thirdPersonModel.currentWeaponObject != null)
            {
                MeshRenderer[] renderers;
                renderers = thirdPersonModel.currentWeaponObject.GetComponents<MeshRenderer>();
                if (renderers.Length > 0)
                    foreach (MeshRenderer r in renderers)
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

                SkinnedMeshRenderer[] skinnedRenderers;
                skinnedRenderers = thirdPersonModel.currentWeaponObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skinnedRenderers.Length > 0)
                    foreach (SkinnedMeshRenderer r in skinnedRenderers)
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            if (thirdPersonModel.currentWeaponOnHand != null)
                thirdPersonModel.currentWeaponOnHand.enabled = false;

            if (thirdPersonModel.currentAmmoObject != null)
                thirdPersonModel.currentAmmoObject.SetActive(false);

            if (Equipment.PlayerEquipment.currentShieldObject != null)
            {
                if (Equipment.PlayerEquipment.currentShieldObject.GetComponent<MeshRenderer>() != null)
                    Equipment.PlayerEquipment.currentShieldObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

                if (Equipment.PlayerEquipment.currentShieldObject.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                    Equipment.PlayerEquipment.currentShieldObject.GetComponentInChildren<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            if (Equipment.PlayerEquipment.currentTorchInHand != null)
            {
                if (Equipment.PlayerEquipment.currentTorchInHand.GetComponent<MeshRenderer>() != null)
                    Equipment.PlayerEquipment.currentTorchInHand.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

                if (Equipment.PlayerEquipment.currentTorchInHand.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                    Equipment.PlayerEquipment.currentTorchInHand.GetComponentInChildren<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            if (thirdPersonModel.projectile != null)
            {
                if (thirdPersonModel.projectile.GetComponent<MeshRenderer>() != null)
                    thirdPersonModel.projectile.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            if (thirdPersonModel.curThrowable != null)
            {
                if (thirdPersonModel.curThrowable.GetComponent<MeshRenderer>() != null)
                    thirdPersonModel.curThrowable.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            // Enable FPS weapon
            if (PlayerCombat.instance.currentWeaponOnHand != null)
            {
                PlayerCombat.instance.currentWeaponOnHand.enabled = true;

                MeshRenderer[] renderers;
                renderers = PlayerCombat.instance.currentWeaponOnHand.gameObject.GetComponents<MeshRenderer>();
                if (renderers.Length > 0)
                    foreach (MeshRenderer r in renderers)
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

                SkinnedMeshRenderer[] skinnedRenderers;
                skinnedRenderers = PlayerCombat.instance.currentWeaponOnHand.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skinnedRenderers.Length > 0)
                    foreach (SkinnedMeshRenderer r in skinnedRenderers)
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            // Account for composed armor item mesh
            ComposedArmorItemMesh[] composedTPS = RckPlayer.instance.thirdPersonModel.GetComponentsInChildren<ComposedArmorItemMesh>();
            for (int i = 0; i < composedTPS.Length; i++)
            {
                composedTPS[i].DisableObjects();
            }

            ComposedArmorItemMesh[] composedFPS = RckPlayer.instance.fpHandsContainer.GetComponentsInChildren<ComposedArmorItemMesh>();
            for (int i = 0; i < composedFPS.Length; i++)
            {
                composedFPS[i].EnableObjects();
            }
        }

        /// <summary>
        /// Syncs the player's input to the third person
        /// </summary>
        public virtual void BroadcastAnimatorToThirdPerson()
        {
            if (movementsEnabled)
            {
                float animX = x;
                float animZ = z;

                if (controlledByAnim || stopPlayerInputByAnim)
                {
                    animX = animZ = 0;
                }

                float speed = Mathf.Clamp(animZ, -1f, 1f);

                if (isWalking)
                {
                    speed *= 0.5f;
                    animX *= 0.5f;
                }

                if(isRunning)
                {
                    speed *= 2;
                }

                thirdPersonModel.m_Animator.SetFloat("Speed", speed, 0.25f, Time.deltaTime);

                thirdPersonModel.m_Animator.SetFloat("Sideways", animX, 0.25f, Time.deltaTime);
            }
            else
            {
                thirdPersonModel.m_Animator.SetFloat("Speed", 0, 0.25f, Time.deltaTime);
                thirdPersonModel.m_Animator.SetFloat("Sideways", 0, 0.25f, Time.deltaTime);
            }

            // is Rotating
            if (!thirdPersonModel.isInFreeLook)
                thirdPersonModel.m_Animator.SetBool("isRotating", Mathf.Abs(mouseLook.x) > 20.0f && movementsEnabled && !isMounted ? true : false);
            else
                thirdPersonModel.m_Animator.SetBool("isRotating", false);

            if(hasBeenDamaged)
            {
                hasBeenDamaged = false;
                thirdPersonModel.m_Animator.SetTrigger("HasBeenHit");
            }

            thirdPersonModel.m_Animator.SetBool("isGrounded", charController.isGrounded);
        }

        public override void PlayerDeath()
        {
            cameraAnim.m_Animator.SetTrigger("Dead");
            base.PlayerDeath();
        }

        public bool AreControlsEnabled()
        {
            return controlsEnabled;
        }

        private void TryDodge()
        {
            if (!IsControlledByPlayer() || Time.time < nextDodgeTime || (dodgeRequiresGrounded && !charController.isGrounded) || RckPlayer.instance.controlledByAnim
                || playerAttributes.CurStamina < RCKSettings.DODGE_STAMINA_DRAIN_ON_DODGE || inventory.IsOverencumbred())
                return;

            // Read input
            Vector2 input = new Vector2(x, z);

            // Don't dodge without input
            if (input.sqrMagnitude <= 0.01f)
                return;

            input.Normalize();

            // Convert to world direction relative to character facing
            Vector3 dir = (transform.right * input.x + transform.forward * input.y).normalized;

            // spend stamina + cooldown
            RckPlayer.instance.playerAttributes.DamageStamina(RCKSettings.DODGE_STAMINA_DRAIN_ON_DODGE, true, RCKSettings.DRAIN_STAMINA_ON_DODGE_SPEEDAMOUNT);
            RckPlayer.instance.StopRecoveringStamina();
            RckPlayer.instance.InvokeResetRecover();

            nextDodgeTime = Time.time + dodgeCooldown;

            if (dodgeCoroutine != null)
                StopCoroutine(dodgeCoroutine);

            dodgeCoroutine = StartCoroutine(DodgeRoutine(dir));
        }

        private IEnumerator DodgeRoutine(Vector3 worldDir)
        {
            isDodging = true;

            isRunning = false;
            isWalking = false;
            InGameHelpUI.instance.SetWalkToggleUI(isWalking);

            float elapsed = 0f;

            // Get dodgedistance


            float speed = RCKSettings.GetDodgeDistance(playerAttributes.attributes.Agility) / dodgeDuration;

            // Keep vertical speed consistent
            float yVel = velocity.y;

            // Play anim
            thirdPersonModel.m_Animator.Play("DodgeTree");

            if(dodgeSound)
                GameAudioManager.instance.PlayOneShot(AudioSources.Player, dodgeSound);

            while (elapsed < dodgeDuration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;

                Vector3 displacement = worldDir * speed * dt;

                // Apply gravity while dodging
                yVel += gravity * dt;
                displacement.y = yVel * dt;

                charController.Move(displacement);

                yield return null;
            }

            // Restore y velocity
            velocity.y = yVel;
            isDodging = false;
        }
    }
}
