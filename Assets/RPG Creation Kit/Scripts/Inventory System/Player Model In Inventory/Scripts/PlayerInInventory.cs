using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    /// <summary>
    /// Manages the Player In Inventory screen, instantiating items and controlling the Animator
    /// </summary>
    public class PlayerInInventory : MonoBehaviour
    {

        #region Singleton
        public static PlayerInInventory instance;
        private void Awake()
        {
            if (!instance)
                instance = this;
        }
        #endregion
        public RuntimeAnimatorController pcMaleAnimator;
        public RuntimeAnimatorController pcFemaleAnimator;

        public RotateWithMouse rotWithMouse;

        public GameObject PlayerModel;
        public Animator m_Animator;

        public BodyData character;

        private WeaponItem currentWeapon;
        private GameObject currentWeaponObject;

        private AmmoItem currentAmmo;
        private GameObject currentAmmoObject;

        public Equipment equipment;


        // Use this for initialization
        void Start()
        {

        }

        private void Update()
        {
            if (m_Animator && m_Animator.gameObject.activeInHierarchy && Time.timeScale == 0.0f)
                m_Animator.Update(Time.unscaledDeltaTime);
        }

        public void OnEquipmentChangesHands()
        {
            // If there is a weapon equipped
            if (equipment.itemsEquipped[(int)EquipmentSlots.RHand] != null && equipment.itemsEquipped[(int)EquipmentSlots.RHand].item != null)
            {
                // Destroy previous weapon equipped if there was a swap
                if (currentWeapon != null)
                {
                    Destroy(currentWeaponObject);
                    currentWeapon = null;
                    currentWeapon = equipment.currentWeapon;
                }

                // Instantiate Weapon on hand, assign references
                currentWeapon = (WeaponItem)equipment.itemsEquipped[(int)EquipmentSlots.RHand].item;
                currentWeaponObject = Instantiate(currentWeapon.WeaponOnHand, character.rHand);

            }
            else if (currentWeapon != null)
            {
                // Unequip = destroy previous equipped weapon
                Destroy(currentWeaponObject);
                currentWeapon = null;
                currentWeapon = equipment.currentWeapon;
            }


            UpdateAnimator();

            ThirdPersonPlayer.instance.OnEquipmentChangesHands();
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

            UpdateAnimator();

            ThirdPersonPlayer.instance.OnEquipmentChangesAmmo();
        }

        public void ShowHideCharacter(bool show)
        {
            PlayerModel.SetActive(show);

            if(show)
                UpdateAnimator();
        }

        public void UpdateAnimator()
        {
            // Decide what animator to use
            if (currentWeapon != null && m_Animator != null)
            {
                if (currentWeapon.weaponType == WeaponType.BladeOneHand || currentWeapon.weaponType == WeaponType.DaggerOneHand || currentWeapon.weaponType == WeaponType.BluntOneHand)
                    m_Animator.SetInteger("WeaponEquipped", 1);
                if (currentWeapon.weaponType == WeaponType.BladeTwoHands || currentWeapon.weaponType == WeaponType.BluntTwoHands)
                    m_Animator.SetInteger("WeaponEquipped", 2);
                if (currentWeapon.weaponType == WeaponType.Bow)
                    m_Animator.SetInteger("WeaponEquipped", 3);
                if (currentWeapon.weaponType == WeaponType.FIREARM_OneHanded_1)
                    m_Animator.SetInteger("WeaponEquipped", 5);
                if (currentWeapon.weaponType == WeaponType.FIREARM_TwoHanded_1)
                    m_Animator.SetInteger("WeaponEquipped", 6);

            }
            else
            {
                m_Animator.SetInteger("WeaponEquipped", 0);
            }

            m_Animator.SetBool("isUsingTorch", equipment.isUsingTorch);

        }

        public void SpawnNewPlayer(Race _race, GameObject _character, bool sex, RPGCreationKit.SaveSystem.FaceBlendshapesSaveData _faceData, int _hairType, int _eyesType)
        {
            GameObject cc = Instantiate(_character, transform);
            PlayerModel = cc;
            m_Animator = PlayerModel.GetComponent<Animator>();
            character = PlayerModel.GetComponent<BodyData>();

            m_Animator.runtimeAnimatorController = (sex == false) ? pcMaleAnimator : pcFemaleAnimator;
            m_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            m_Animator.enabled = true;
            m_Animator.Update(0f);

            ShowHideCharacter(false);

            rotWithMouse.PlayerInInventory = PlayerModel;
            equipment.characterModel = character;

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

            character.GetComponentInChildren<AIHeadPos>().gameObject.SetActive(false);
        }

        public void ApplyFaceBlenshapes()
        {

        }
    }
}