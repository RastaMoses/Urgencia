using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class BodyData : MonoBehaviour
    {
        public bool isMale = true;
        public Avatar myAvatar;

        public SkinnedMeshRenderer head;
        public SkinnedMeshRenderer eyes;
        public SkinnedMeshRenderer upperbody;
        public SkinnedMeshRenderer arms;
        public SkinnedMeshRenderer hands;
        public SkinnedMeshRenderer lowerbody;
        public SkinnedMeshRenderer feet;

        [Space(10)]

        public int hairIndex = -1;
        public int eyeIndex = 0;
        public SkinnedMeshRenderer hair;

        [Space(10)]

        [Header("Bones")]
        public Transform rHand;
        public Transform lHand;
        public Transform upperChest;
        public Transform hips;

        [Space(10)]

        [Header("Default Equipment")]
        [Tooltip("The gameobject used to cover the Upperbody of a character when he is naked")]
        public SkinnedMeshRenderer defaultUpperbody;

        [Tooltip("The gameobject used to cover the Lowerbody of a character when he is naked")]
        public SkinnedMeshRenderer defaultLowerbody;

        public void HideAll()
        {
            SkinnedMeshRenderer[] renders = GetComponentsInChildren<SkinnedMeshRenderer>();
            
            for(int i = 0; i < renders.Length; i++)
            {
                // Skip weapons
                if (renders[i].GetComponentInParent<WeaponOnHand>() == null)
                    renders[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }

        public void ShowAll()
        {
            SkinnedMeshRenderer[] renders = GetComponentsInChildren<SkinnedMeshRenderer>();

            for (int i = 0; i < renders.Length; i++)
            {
                // Skip weapons
                if (renders[i].GetComponentInParent<WeaponOnHand>() == null)
                    renders[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }
    }
}