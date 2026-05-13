using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RPGCreationKit;
using RPGCreationKit.Player;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UIElements;

namespace RPGCreationKit.Player
{
    /// <summary>
    /// Manages the Player In Inventory screen, instantiating items and controlling the Animator
    /// </summary>
    public class ThirdPersonPlayer : MonoBehaviour
    {

        #region Singleton
        public static ThirdPersonPlayer instance;
        private void Awake()
        {
            if (!instance)
                instance = this;
        }
        #endregion
        public RuntimeAnimatorController pcMaleAnimator;
        public RuntimeAnimatorController pcFemaleAnimator;

        public GameObject PlayerModel;
        public Animator m_Animator;

        public BodyData character;
        public Ragdoll ragdoll;
        private WeaponItem currentWeapon;
        public GameObject currentWeaponObject;
        public WeaponOnHand currentWeaponOnHand;
        public GameObject currentWeaponOnHip;

        private AmmoItem currentAmmo;
        public GameObject currentAmmoObject;


        public Animator currentWeaponAnimator;

        public Equipment equipment;

        public int combatType;
        public WeaponOnHand defaultWeaponOnHand;

        public GameObject TPSCameraHold;
        public bool freeCameraInput = false;
        public bool isInFreeLook = false;
        public bool pauseLookAt = false;
        // Third Person Specific

        public GameObject curThrowable;

        public bool forceFreeLook = false;

        public bool tpsWeaponDrawn = false;

        // RootMotion enabler
        public Vector3 naturalGFXPosition;
        public Vector3 natrualGFXRotation;

        public void INPUT_TPSCameraLook(InputAction.CallbackContext value)
        {
            if (value.interaction is HoldInteraction)
            {
                if (value.started)
                    freeCameraInput = true;

                if (value.canceled)
                    freeCameraInput = false;
            }
            else
                freeCameraInput = false;
        }

        public void INPUT_OnLookChanges(InputAction.CallbackContext value)
        {
            Vector2 dValue = value.ReadValue<Vector2>();
            x = dValue.x;
            y = dValue.y;
        }

        // Use this for initialization
        void Start()
        {
            if (RCKSettings.ROOT_MOTION_CONTROLLER_ENABLED)
            {
                StartCoroutine("ConstantRestore");
            }
        }


        private void LateUpdate()
        {
            if (forceFreeLook)
                freeCameraInput = true;

            FreeCameraLook();
        }

        public void OnEquipmentChangesHands()
        {
            // If there is a weapon equipped
            if (equipment.itemsEquipped[(int)EquipmentSlots.RHand] != null && equipment.itemsEquipped[(int)EquipmentSlots.RHand].item != null)
            {
                // Destroy previous weapon equipped if there was a swap
                if (currentWeaponObject != null)
                {
                    Destroy(currentWeaponObject);
                    currentWeapon = null;
                }

                if (currentWeaponOnHip != null)
                    Destroy(currentWeaponOnHip);

                // Instantiate Weapon on hand, assign references
                currentWeapon = (WeaponItem)equipment.itemsEquipped[(int)EquipmentSlots.RHand].item;

                if (equipment.currentWeapon.weaponType == WeaponType.Bow)
                    currentWeaponObject = Instantiate(equipment.currentWeapon.WeaponOnHand, character.lHand);
                else
                    currentWeaponObject = Instantiate(equipment.currentWeapon.WeaponOnHand, character.rHand);

                currentWeaponOnHand = currentWeaponObject.GetComponent<WeaponOnHand>();
                currentWeaponOnHand.thisAttacker = PlayerCombat.instance.gameObject.GetComponent<IMeleeAttacker>();
                currentWeaponAnimator = currentWeaponObject.GetComponent<Animator>();

                if (currentWeaponOnHand != null)
                    currentWeaponOnHand.thisEntity = RckPlayer.GetPlayerEntity();

                if (equipment.currentWeapon.WeaponSheathed != null)
                {
                    if (equipment.currentWeapon.weaponType == WeaponType.Bow ||
                       equipment.currentWeapon.weaponType == WeaponType.BladeTwoHands ||
                       equipment.currentWeapon.weaponType == WeaponType.BluntTwoHands ||
                       equipment.currentWeapon.weaponType == WeaponType.FIREARM_TwoHanded_1)
                        currentWeaponOnHip = Instantiate(equipment.currentWeapon.WeaponSheathed, character.upperChest);
                    else
                        currentWeaponOnHip = Instantiate(equipment.currentWeapon.WeaponSheathed, character.hips);
                }

                currentWeaponObject.SetActive(false);
                currentWeaponOnHip.SetActive(true);
            }
            else if (currentWeapon != null)
            {
                // Unequip = destroy previous equipped weapon
                Destroy(currentWeaponObject);
                Destroy(currentWeaponOnHip);
                currentWeapon = null;
            }

            if (RckPlayer.instance.isInThirdPerson)
                RckPlayer.instance.SwitchToThirdPerson();
            else
                RckPlayer.instance.SwitchToFirstPerson();
        }

        public void OnEquipmentChangesAmmo()
        {
            if (equipment.itemsEquipped[(int)EquipmentSlots.Ammo] != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item != null)
            {
                if (currentAmmoObject != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item != currentAmmoObject)
                {
                    Destroy(currentAmmoObject);
                    currentAmmo = null;
                }

                // Instantiate Weapon on hand, assign references
                currentAmmo = (AmmoItem)equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item;
                currentAmmoObject = Instantiate(currentAmmo.bagOnBody, character.upperChest);
            }
            else if (currentAmmo != null)
            {
                // Unequip = destroy previous equipped weapon
                Destroy(currentAmmoObject);
                currentAmmo = null;
            }

            if (RckPlayer.instance.isInThirdPerson)
                RckPlayer.instance.SwitchToThirdPerson();
            else
                RckPlayer.instance.SwitchToFirstPerson();
        }

        public void ShowHideCharacter(bool show)
        {
            PlayerModel.SetActive(show);

            if (show)
                UpdateAnimator();
        }

        public void UpdateAnimator()
        {
            m_Animator.SetTrigger("ReturnToDefault");
            m_Animator.SetBool("InCombat", false);
            m_Animator.Update(0f);
        }

        public void SpawnNewPlayer(Race _race, GameObject _character, bool sex, RPGCreationKit.SaveSystem.FaceBlendshapesSaveData _faceData, int _hairType, int _eyesType)
        {
            GameObject cc = Instantiate(_character, transform);
            PlayerModel = cc;
            Destroy(PlayerModel.GetComponent<Animator>());

            character = PlayerModel.GetComponent<BodyData>();

            //Equipment.PlayerEquipment.characterModel = character;
            Equipment.PlayerEquipment.characterModel = character;
            // Apply face blendshapes
            BodyData bodyData = character.GetComponent<BodyData>();

            for (int i = 0; i < _faceData.allShapes.Count; i++)
                bodyData.head.SetBlendShapeWeight(_faceData.allShapes[i].index, _faceData.allShapes[i].weight);

            HeadBlendshapesManager headBlendshapes = bodyData.GetComponentInChildren<HeadBlendshapesManager>();
            headBlendshapes.AdjustChildBlendshapes();

            // spawn hair 
            if (_hairType != -1)
            {
                // Spawn new hair
                Hair hair = (!sex) ? _race.maleHairTypes[_hairType] : _race.femaleHairTypes[_hairType];

                bodyData.hair = Instantiate(hair.mesh.gameObject, bodyData.transform).GetComponent<SkinnedMeshRenderer>();

                // Attach
                bodyData.hair.transform.parent = bodyData.head.transform;
                bodyData.hair.rootBone = bodyData.head.rootBone;
                bodyData.hair.bones = bodyData.head.bones;
            }

            // edit eyes
            Eye eyesType = _race.eyeTypes[_eyesType];

            bodyData.eyes.sharedMaterial = eyesType.eyes.sharedMaterial;

            // Disable AI HeadPos on the Model
            character.GetComponentInChildren<AIHeadPos>().gameObject.SetActive(false);

            m_Animator.avatar = bodyData.myAvatar;
            m_Animator.enabled = false;
            m_Animator.Update(Time.deltaTime);
            m_Animator.enabled = true;

            if (!RCKSettings.ROOT_MOTION_CONTROLLER_ENABLED)
                m_Animator.applyRootMotion = false;
            else
                m_Animator.applyRootMotion = true;

            m_Animator.runtimeAnimatorController = (sex == false) ? pcMaleAnimator : pcFemaleAnimator;
            m_Animator.enabled = true;

            PlayerCombat.instance.tpsAnim = m_Animator;
            ThirdPersonPlayer.instance.currentWeaponOnHand = defaultWeaponOnHand;

            ragdoll = character.GetComponent<Ragdoll>();
            ragdoll.animator = m_Animator;

            m_Animator.Rebind();
            m_Animator.Update(0f);
        }

        public void ApplyFaceBlenshapes()
        {

        }


        public void DrawWeaponAnimationEvent()
        {
            if (currentWeaponOnHip == null || currentWeaponOnHand == null)
            {
                //UpdateAnimator();
                return;
            }
            currentWeaponOnHip.SetActive(false);
            currentWeaponOnHand.gameObject.SetActive(true);
        }

        public void UndrawWeaponAnimationEvent()
        {
            if (currentWeaponOnHand == null || currentWeaponOnHip == null)
            {
                //UpdateAnimator();
                return;
            }

            currentWeaponOnHand.gameObject.SetActive(false);
            currentWeaponOnHip.SetActive(true);
        }

        public void UndrawWeaponEvent()
        {
            tpsWeaponDrawn = false;
            //m_Anim.runtimeAnimatorController = defaultAnimatorController;
            m_Animator.SetBool("InCombat", false);
            m_Animator.SetInteger("CombatType", (int)equipment.currentWeapon.weaponType);
        }

        /// <summary>
        /// Called by animation event, resets the animator runtime controller to default
        /// </summary>
        public void DrawWeaponEvent()
        {
            tpsWeaponDrawn = true;
            //m_Anim.runtimeAnimatorController = defaultAnimatorController;
            m_Animator.SetBool("InCombat", true);
            m_Animator.SetInteger("CombatType", (int)equipment.currentWeapon.weaponType);
        }

        [Header("Clamp")]
        [SerializeField] float maxLookAngle = 80f;     // cone half-angle
        [SerializeField] float minLookDistance = 0.5f;
        [SerializeField] bool fadeOutWhenBehind = true;
        void OnAnimatorIK(int layerIndex)
        {
            if (!RckPlayer.instance.cameraAnim.tpsLookAtTarget || pauseLookAt)
                return;

            Vector3 rawTargetPos =
                (isInFreeLook && fixedTpsLookAt != null)
                    ? fixedTpsLookAt.transform.position
                    : RckPlayer.instance.cameraAnim.tpsLookAtTarget.position;

            Transform head = m_Animator.GetBoneTransform(HumanBodyBones.Head);
            Vector3 origin = head ? head.position : transform.position + Vector3.up * 1.6f;
            Vector3 forward = head ? head.forward : transform.forward;
            Vector3 up = transform.up;

            Vector3 toTarget = rawTargetPos - origin;
            float dist = Mathf.Max(toTarget.magnitude, minLookDistance);
            Vector3 dir = (dist > 1e-5f) ? (toTarget / dist) : forward;

            float angle = Vector3.Angle(forward, dir);
            float dot = Vector3.Dot(forward, dir);

            Vector3 clampedDir = dir;
            if (angle > maxLookAngle)
            {
                Quaternion from = Quaternion.LookRotation(forward, up);
                Quaternion to = Quaternion.LookRotation(dir, up);
                Quaternion clampedRot = Quaternion.RotateTowards(from, to, maxLookAngle);
                clampedDir = clampedRot * Vector3.forward;
            }

            Vector3 clampedTargetPos = origin + clampedDir * dist;

            float w = 1f;
            if (fadeOutWhenBehind)
                w = Mathf.Clamp01((dot + 0.75f) / 1.2f);

            m_Animator.SetLookAtWeight(w, 0.4f * w, 0.4f * w, 1f * w, 0.5f * w);
            m_Animator.SetLookAtPosition(clampedTargetPos);
        }

        public void AttackAnimationEvent()
        {
            currentWeaponOnHand.StartCasting(false, PlayerCombat.instance.lastAttackIndex);
        }

        public void ChargedAttackAnimationEvent()
        {
            currentWeaponOnHand.StartCasting(true, PlayerCombat.instance.lastChargedAttackIndex);
        }

        public void EndAttackAnimationEvent()
        {
            currentWeaponOnHand.StopCasting();
        }

        public void PlayAttackSound()
        {
            if (RckPlayer.instance.isInThirdPerson)
                currentWeaponOnHand.PlayOneShot(currentWeaponOnHand.weaponItem.weaponAttacks[PlayerCombat.instance.curAttackType].attackSound);
        }

        public void PlayChargedAttackSound()
        {
            if (RckPlayer.instance.isInThirdPerson)
                currentWeaponOnHand.PlayOneShot(currentWeaponOnHand.weaponItem.weaponChargedAttacks[PlayerCombat.instance.curChargedAttackType].attackSound);
        }

        // Anim events are driven by FP only
        public void ResetAttackStateEvent()
        {
            /*
            if(Player.RckPlayer.instance.isInThirdPerson)
            {
                PlayerCombat.instance.isAttacking = false;
                PlayerCombat.instance.canAttack = true;
            }
            */
        }

        public void BlockAnimationEvent()
        {
            /*
            if (Player.RckPlayer.instance.isInThirdPerson)
            {
                PlayerCombat.instance.isBlocking = true;
            }
            */
        }

        [HideInInspector] public GameObject projectile;
        public void SpawnAmmoOnRHand()
        {
            projectile = Instantiate(((AmmoItem)equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item).projectile, character.rHand);

            if (!RckPlayer.instance.isInThirdPerson)
            {
                if (projectile.GetComponent<MeshRenderer>() != null)
                    projectile.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }

        public void SpawnThowableOnLHand()
        {
            if (curThrowable != null)
                Destroy(curThrowable);

            curThrowable = Instantiate(((WeaponItem)equipment.itemsEquipped[(int)EquipmentSlots.Throwable].item).WeaponOnHand, character.lHand);
            curThrowable.GetComponent<Throwable>().dummy = true;

            if (!RckPlayer.instance.isInThirdPerson)
            {
                if (curThrowable.GetComponent<MeshRenderer>() != null)
                    curThrowable.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }

        public void ThrowGrenadeEvent()
        {
            // handled in first person
        }

        public void ResetThrowGrenadeEvent()
        {
            // Not used for player, only ai
        }

        public void OnAmmoIsReady()
        {

        }

        public void DestroyAmmoProjectile()
        {
            if (projectile != null)
                Destroy(projectile);

            projectile = null;
        }

        public void DestroyThrowable()
        {
            if (curThrowable != null)
                Destroy(curThrowable);

            curThrowable = null;
        }

        public void PlayCastSpellSound()
        {
            /*
            if (!RPGCreationKit.Player.RckPlayer.instance.isInThirdPerson && SpellsKnowledge.Player.spellInUse.sOnCast)
                playerCombat.spellsAudioSource.PlayOneShot(SpellsKnowledge.Player.spellInUse.sOnCast);
            */
        }

        GameObject spellProjectile;
        SpellProjectile currentSpellProjectile;

        public void CastSpellInit()
        {
            Spell curSpell = SpellsKnowledge.Player.spellInUse;

            Transform handT = (Equipment.PlayerEquipment.isUsingShield || (Equipment.PlayerEquipment.currentWeapon != null && Equipment.PlayerEquipment.currentWeapon.weaponType == WeaponType.Bow)) ? character.rHand : character.lHand;

            switch (curSpell.mode)
            {
                case SpellModes.Projectile:
                    spellProjectile = Instantiate(curSpell.magicProjectile, handT);
                    spellProjectile.SetLayer(RCKLayers.ThirdPersonOnly, true);
                    currentSpellProjectile = spellProjectile.GetComponent<SpellProjectile>();
                    currentSpellProjectile.Init(Entity.GetPlayerEntity(), true);
                    break;

                case SpellModes.Touch:
                    spellProjectile = Instantiate(curSpell.magicProjectile, handT);
                    spellProjectile.SetLayer(RCKLayers.ThirdPersonOnly, true);
                    currentSpellProjectile = spellProjectile.GetComponent<SpellProjectile>();
                    currentSpellProjectile.Init(Entity.GetPlayerEntity(), true);
                    break;

                case SpellModes.Self:
                    spellProjectile = Instantiate(curSpell.magicProjectile, handT);
                    spellProjectile.SetLayer(RCKLayers.ThirdPersonOnly, true);
                    currentSpellProjectile = spellProjectile.GetComponent<SpellProjectile>();
                    currentSpellProjectile.Init(Entity.GetPlayerEntity(), true);
                    break;
            }
        }

        public void CastSpellAttackEvent()
        {
            // Instantiate/Do Stuff
            Spell curSpell = SpellsKnowledge.Player.spellInUse;
            Ray ray;

            // From the shooting everything else is managed by he FP shot projectile
            Destroy(currentSpellProjectile.gameObject);
        }

        // Empty because handled in first person
        public void CastSpellEndAndDestroy()
        {

        }

        public void CastSpellResetEvent()
        {
            // Empty because handled in first person
        }

        public void FirearmReloadEvent()
        {
            // Empty because handled in first person
        }

        public void PutOneBulletAnim()
        {
            // Empty because handled in first person
        }

        public void EndSingleReloadTypeAnimEvent()
        {
            // Empty because handled in first person
        }


        private float curXSensitivy;
        private float curYSensitivy;
        public float x, y;
        float xRotation = 0f;

        bool updated = false;
        Vector3 savedRot;

        Vector3 savedRotRoot;

        public Quaternion savedMouseLook;
        public Vector3 savedLookAtTargetLocalPos;
        GameObject fixedTpsLookAt;
        public void FreeCameraLook()
        {
            if (RckPlayer.instance.isInThirdPerson)
            {
                if (freeCameraInput)
                {
                    isInFreeLook = true;

                    if (!updated)
                    {
                        fixedTpsLookAt = new GameObject("FixedTpsLookAt");
                        fixedTpsLookAt.transform.SetParent(RckPlayer.instance.transform);
                        fixedTpsLookAt.transform.position = RckPlayer.instance.cameraAnim.tpsLookAtTarget.position;
                        savedLookAtTargetLocalPos = RckPlayer.instance.cameraAnim.tpsLookAtTarget.localPosition;
                    }

                    savedMouseLook = RckPlayer.instance.mouseLook.transform.rotation;

                    RckPlayer.instance.mouseLook.transform.rotation = Quaternion.Euler(0, RckPlayer.instance.mouseLook.transform.rotation.eulerAngles.y, RckPlayer.instance.mouseLook.transform.rotation.eulerAngles.z);

                    RckPlayer.instance.mouseLook.enabled = false;
                    if (!updated)
                    {
                        savedRot = TPSCameraHold.transform.localRotation.eulerAngles;
                        savedRotRoot = TPSCameraHold.transform.parent.localRotation.eulerAngles;
                        updated = true;
                    }

                    // Take mouse input and rotate TPSHolder
                    if (RckInput.isUsingGamepad)
                    {
                        curXSensitivy = RCKSettings.MOUSE_HORIZONTAL_SPEED * 10;
                        curYSensitivy = RCKSettings.MOUSE_VERTICAL_SPEED * 10;
                    }
                    else
                    {
                        curXSensitivy = RCKSettings.MOUSE_HORIZONTAL_SPEED;
                        curYSensitivy = RCKSettings.MOUSE_VERTICAL_SPEED;
                    }

                    // Calculate the new camera rotation
                    xRotation -= y * curYSensitivy * Time.deltaTime;
                    xRotation = Mathf.Clamp(xRotation, RckPlayer.instance.mouseLook.xLimitDownTPS, RckPlayer.instance.mouseLook.xLimitUpTPS);

                    // Apply the x-axis rotation to the camera
                    TPSCameraHold.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

                    // Rotate the TPSHolderRoot
                    TPSCameraHold.transform.parent.Rotate(Vector3.up, x * curXSensitivy * Time.deltaTime);
                }
                else if (isInFreeLook)
                {
                    OnLeaveCameraLook();
                }
            }
        }

        public void OnLeaveCameraLook()
        {
            isInFreeLook = false;

            RckPlayer.instance.mouseLook.enabled = true;
            RckPlayer.instance.mouseLook.transform.rotation = savedMouseLook;
            RckPlayer.instance.mouseLook.recoil = 0f;
            TPSCameraHold.transform.localRotation = Quaternion.Euler(savedRot);
            TPSCameraHold.transform.parent.localRotation = Quaternion.Euler(savedRotRoot);

            if (fixedTpsLookAt)
                Destroy(fixedTpsLookAt);

            RckPlayer.instance.cameraAnim.tpsLookAtTarget.localPosition = savedLookAtTargetLocalPos;
            updated = false;

            curXSensitivy = 0;
            curYSensitivy = 0;
            x = y = 0;
            xRotation = 0f;
        }

        public float restoreSpeed = 5f;    // adjust to taste
        public float threshold = 0.15f;    // minimum difference before restoring

        private IEnumerator ConstantRestore()
        {
            while (true)
            {
                // Position
                if (Vector3.Distance(transform.localPosition, naturalGFXPosition) > threshold)
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, naturalGFXPosition, Time.deltaTime * restoreSpeed);
                }

                // Rotation
                Quaternion targetRot = Quaternion.Euler(natrualGFXRotation);
                if (Quaternion.Angle(transform.localRotation, targetRot) > threshold)
                {
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot,Time.deltaTime * restoreSpeed);
                }

                yield return null;
            }
        }

        public void HardRestorePositionAndRotationToNatural()
        {
            this.transform.localPosition = naturalGFXPosition;
            this.transform.localEulerAngles = natrualGFXRotation;
        }
    }
}