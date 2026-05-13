using RPGCreationKit.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RPGCreationKit.AI
{
    public class MountAI : RckAI
    {
        public GameObject assignedModel;
        public bool validAssignedModel = false;

        public enum MountMovementType
        {
            Horse = 0
        };

        public MountMovementType mountType;
        public bool isOccupied = false;

        public bool playerMounting = false;     // if true, the player used this point to mount and is currently mounted
        public RckAI aiMounting;                // if not null, the ai used this point to mount and is currently mounted

        public Animator mountingEntityAnim;

        public MountupPoint[] mountPoints;

        public GameObject playerCameraRig;

        public Transform mountedCharacterLooKAt;

        public Transform fpsCameraAnimPos;
        public Transform tpsCameraRootPos;

        public float mountRotationSpeed = 100;

        public List<Collider> extraColliders = new List<Collider>();

        public float movMult = 10;

        bool isManualMountWalking = false;
        public float curMountMaxSpeed = 10;
        public float mountMaxSpeed = 10;
        public float mountMaxSpeedReverse = 1;
        public float mountMaxSpeedWhileWalking = 3;


        public float headRotSpeed = 0.2f;
        public float animSpeed = .5f;

        public float negativeZMax = -0.5f;

        public AudioClip walkSound;
        public AudioClip runSound;

        public Transform interactZoneOffsetRckAI;

        // For Manual Control
        float x, z;
        Vector3 direction;

        [Header("Movement Feel")]
        [SerializeField] private float mountAccel = 7.5f;    // how fast it speeds up
        [SerializeField] private float mountDecel = 18f;     // how fast it slows down when releasing input
        [SerializeField] private float mountBrake = 28f;     // how fast it slows when reversing direction
        [SerializeField] private float mountCoastDrag = 0.12f; // drag to reduce ice skating
        private float mountTargetSpeed = 0f;

        public void INPUT_MoveAxis(InputAction.CallbackContext value)
        {
            Vector2 dValue = value.ReadValue<Vector2>();
            x = dValue.x;
            z = dValue.y;
        }

        public void INPUT_MoveAxisCancel(InputAction.CallbackContext value)
        {
            x = 0;
            z = 0;
        }

        public void INPUT_ToggleWalkCmd(InputAction.CallbackContext value)
        {
            if (value.started)
            {
                isManualMountWalking = !isManualMountWalking;
                InGameHelpUI.instance.SetWalkToggleUI(isManualMountWalking);
            }
        }


        // Called when the player uses the mount
        public void ManualControlSetup()
        {
            pauseBT = true;
            agent.enabled = false;
            playerMounting = true;
            m_Rigidbody.isKinematic = false;

            if (isTraversingLink)
                StopCoroutine("TraverseNormalSpeed");

            // Bind to RckInput
            RckInput.input.actions["Move"].performed += INPUT_MoveAxis;
            RckInput.input.actions["Move"].canceled += INPUT_MoveAxisCancel;

            RckInput.input.actions["ToggleWalk"].started += INPUT_ToggleWalkCmd;

            // Stop the sounds
            playingSound = false;
            audioSource.loop = false;
            audioSource.Stop();

            mountingEntityAnim = RckPlayer.instance.thirdPersonModel.m_Animator;
            InGameHelpUI.instance.SetWalkToggleUI(isManualMountWalking);
        }

        bool immediateStop = false;
        public void PlayerStartsDismounting()
        {
            x = 0;
            z = 0;

            m_Rigidbody.linearVelocity = Vector3.zero;
            immediateStop = true;

            RckInput.input.actions["Move"].performed -= INPUT_MoveAxis;
            RckInput.input.actions["Move"].canceled -= INPUT_MoveAxisCancel;

            RckInput.input.actions["ToggleWalk"].started -= INPUT_ToggleWalkCmd;

            // If this is true, the mount died and the player is dismounting because of it
            if (!isAlive)
                m_Rigidbody.isKinematic = true;

            mountingEntityAnim = null;
        }

        public void PlayerFinishDismounting()
        {
            if (isAlive)
            {
                pauseBT = false;
                agent.enabled = true;
                playerMounting = false;
                m_Rigidbody.isKinematic = true;

                isOccupied = false;
                playerMounting = false;
                aiMounting = null;

                immediateStop = false; 
            }
        }

        public override void Start()
        {
            base.Start();

            for (int i = 0; i < extraColliders.Count; i++)
            {
                Physics.IgnoreCollision(transform.GetComponent<Collider>(), extraColliders[i], true);
            }
        }

        public override void Update()
        {
            HandleMountSounds();

            if (!playerMounting)
            {
                if (aiMounting != null && aiMounting.isInConversation)
                    pauseBT2 = true;
                else
                    pauseBT2 = false;

                base.Update();
            }
            else
            {
                if (enemyTargets.Count > 0) // if the mount is being used by the player and it receives damage, we should not let the horse attack the attacker as soon as we dismount (friendly damage)
                    enemyTargets.Clear(); 
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (playerMounting)
                HandleManualMovements();

            if(immediateStop)
                m_Rigidbody.linearVelocity = new Vector3(0, m_Rigidbody.linearVelocity.y, 0); // stop movement but preserv gravity
        }

        public void HandleManualMovements()
        {
            if (!RckPlayer.instance.isAlive)
                return;

            switch(mountType)
            {
                case MountMovementType.Horse:
                    HORSE_MountMovements();
                    break;
            }
        }

        RaycastHit slopeHit;
        public float mountHeight = 3;
        // Returns true if the mount is on a slope
        private bool OnSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, mountHeight / 2 + 0.5f))
            {
                if (slopeHit.normal != Vector3.up)
                    return true;
                else
                    return false;
            }

            return false;
        }


        bool isReversing = false;
        [SerializeField] protected float mountVelocity = 0;
        [SerializeField] protected float mountVelocity2 = 0;
        public float mountStickToGroundForce = 1000f;   

        public void HORSE_MountMovements()
        {
            z = Mathf.Clamp(z, negativeZMax, 1f);

            float yaw = x * mountRotationSpeed * Time.fixedDeltaTime;
            Quaternion delta = Quaternion.Euler(0f, yaw, 0f);
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * delta);

            Vector3 planeNormal = OnSlope() ? slopeHit.normal : Vector3.up;

            Vector3 vel = m_Rigidbody.linearVelocity;
            Vector3 planarVel = Vector3.ProjectOnPlane(vel, planeNormal);
            Vector3 driveDir = Vector3.ProjectOnPlane(transform.forward, planeNormal).normalized;

            float currentForwardSpeed = Vector3.Dot(planarVel, driveDir);

            curMountMaxSpeed = (z < 0f) ? mountMaxSpeedReverse : (isManualMountWalking) ? mountMaxSpeedWhileWalking : mountMaxSpeed;
            if (currentForwardSpeed < -0.1f)
                isReversing = true;
            else
                isReversing = false;   

                float desiredSpeed = z * curMountMaxSpeed;

            bool hasInput = Mathf.Abs(z) > 0.01f;
            bool reversing = hasInput && Mathf.Abs(currentForwardSpeed) > 0.5f && Mathf.Sign(desiredSpeed) != Mathf.Sign(currentForwardSpeed);

            float rate = !hasInput ? mountDecel : (reversing ? mountBrake : mountAccel);

            mountTargetSpeed = Mathf.MoveTowards(mountTargetSpeed, desiredSpeed, rate * Time.fixedDeltaTime);

            // Coast drag when no input
            if (!hasInput && !immediateStop)
                m_Rigidbody.AddForce(-planarVel * mountCoastDrag, ForceMode.Acceleration);

            if (immediateStop)
            {
                mountTargetSpeed = 0f;
                z = 0f;
                x = 0f;
            }

            if (!immediateStop)
            {
                Vector3 targetPlanarVel = driveDir * mountTargetSpeed;
                Vector3 planarError = targetPlanarVel - planarVel;

                m_Rigidbody.AddForce(planarError * movMult, ForceMode.Acceleration);
            }

            Vector3 verticalVel = vel - planarVel;
            Vector3 clampedPlanarVel = Vector3.ClampMagnitude(planarVel, curMountMaxSpeed);
            m_Rigidbody.linearVelocity = clampedPlanarVel + verticalVel;

            m_Rigidbody.AddForce(-Vector3.up * mountStickToGroundForce, ForceMode.Force);

            // Anim 
            float animForward = Mathf.Clamp(currentForwardSpeed / Mathf.Max(0.01f, curMountMaxSpeed), -1f, 1f);

            if (isManualMountWalking)
                animForward = Mathf.Clamp(animForward, -0.5f, 0.5f);

            m_Anim.SetFloat("Speed", Mathf.SmoothDamp(m_Anim.GetFloat("Speed"), animForward, ref mountVelocity, animSpeed));
            m_Anim.SetFloat("Sideways", Mathf.SmoothDamp(m_Anim.GetFloat("Sideways"), x, ref mountVelocity2, headRotSpeed));

            if (mountingEntityAnim)
                mountingEntityAnim.SetFloat("MountMovement", m_Anim.GetFloat("Speed"));
        }

        public override void OnTriggerStay(Collider other)
        {
            if (!playerMounting)
                base.OnTriggerStay(other);
        }

        public override void OnTriggerExit(Collider other)
        {
            if(!playerMounting)
                base.OnTriggerExit(other);
        }

        public override void Damage(DamageContext context)
        {
            if (playerMounting && context.sender == RckPlayer.instance)
                return;

            base.Damage(context);

            if (playerMounting)
            {
                isHostile = false;
                isHostileAgainstPC = false;
                enemyTargets.Clear();
            }

            if(aiMounting && aiMounting != null)
            {
                aiMounting.EnterInCombatAgainst(context.sender);
            }
        }

        public bool playingSound = false;
        bool playingWalking = false;
        public void HandleMountSounds()
        {
            if (!audioSource) return;

            float speed = m_Rigidbody ? m_Rigidbody.linearVelocity.magnitude : 0f;

            // Stop conditions
            if (!isAlive || (playerMounting && speed <= 0.1f) || (!playerMounting && isStopped) || Time.timeScale < 0.1f)
            {
                playingSound = false;
                playingWalking = false;
                audioSource.loop = false;
                audioSource.Stop();
                return;
            }

            AudioClip targetClip = null;

            if (playerMounting)
            {
                targetClip = isManualMountWalking ? walkSound : runSound;
                playingWalking = isManualMountWalking;
            }
            else
            {
                targetClip = walkSound; 
                playingWalking = true;
            }

            if (!targetClip) return;

            if (isReversing)
                targetClip = walkSound;

            if (audioSource.isPlaying && audioSource.clip == targetClip)
            {
                playingSound = true;
                return;
            }

            // Switch / start
            audioSource.loop = true;
            audioSource.clip = targetClip;
            if(!isInOfflineMode)
                audioSource.Play();
            playingSound = true;
        }

    }
}
