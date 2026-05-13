using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace RPGCreationKit
{
    [System.Serializable]
    public class WeaponAttack
    {
        public int AttackTypeIndex = 0; // The index of the attacktypes inside the weapon 

        public float BaseDamage = 30;
        public float StaminaAmount = 20;
        public int howManyHitsCanSustain = 1; //how many enemies can this swing hit?
        public bool isFullBody = false;

        [HideInInspector] public CameraAttackAnimation cameraAnim;
        public AudioClip attackSound;

        public void Init()
        {

        }
    }
}