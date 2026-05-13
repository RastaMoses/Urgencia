using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using UnityEditor.Callbacks;
using RPGCreationKit;

namespace RPGCreationKit
{

    public enum MeshBlendShape
    {
      Head = 0,
      UpperChest = 1,
      LowerChest = 2,
      UpperRarm = 3,
      LowerRarm = 4,
      RHand = 5,
      UpperLarm = 6,
      LowerLarm = 7,
      LHand = 8,
      UpperRleg = 9,
      LowerRleg = 10,
      UpperLleg = 11,
      LowerLleg = 12,
      RightFoot = 13,
      LeftFoot = 14
    };

    [System.Serializable]
    public struct BlendShapes
    {
        public MeshBlendShape thisBlendShape;
        [Range(0, 100)]
        public int weight;
    }

    /// <summary>
    /// Where the ArmorItem is equipped
    /// </summary>
    public enum BipedObject
    {
        Head = 0,
        UpperBody = 1,
        LowerBody = 2,
        Hand = 3,
        Foot = 4,
        RightRing = 5,
        LeftRing = 6,
        Amulet = 7,
        Shield = 8,
        Torch = 9
    }

    public enum StaticObjectSlot
    {
        LeftHand = 0,
        RightHand = 1
    };

    /// <summary>
    /// Weight Type of the Item
    /// </summary>
    public enum WeightType
    {
        Cloth = 0,
        Light = 1,
        Heavy = 2
    }

    /// <summary>
    /// Class for AmmoItems, like Arrows and projectiles
    /// </summary>
    [CreateAssetMenu(fileName = "New ArmorItem", menuName = "RPG Creation Kit/Items/ArmorItem", order = 1)]
    public class ArmorItem : Item
    {
        public override ItemTypes itemType { get { return ItemTypes.ArmorItem; } }


        public SkinnedMeshRenderer MaleBipedModel;
        public SkinnedMeshRenderer FemaleBipedModel;

        public GameObject maleStaticObject;
        public GameObject femaleStaticObject;

        public bool isStaticObject;

        public BlendShapes[] Male_Blendshapes;
        public BlendShapes[] Female_Blendshapes;

        public bool useFirstPersonModel; // Use this to instantiate only the arms of the Chest equipment
        public SkinnedMeshRenderer fpMaleModel;
        public SkinnedMeshRenderer fpFemaleModel;

        public BipedObject[] Bipeds;
        public WeightType WeightType;

        public float Health = 500.0f;      // Health of the Item, reduced when the character recieve damage
        public float ArmorRating = 0.0f;   // Amor Rating, reduce Damage from physical attacks
        [Range(0f, 1f)]
        public float blockingMultiplier = .5f;   // Used for shields (to overwrite the weapon multiplier)

        [Range(0f, 10f)]
        public float staminaDrainMultiplier = .65f;   // Stamina drain multiplier is calculated in this way: damage * staminaDrainMultiplier

        public bool HideHair = false;
        public bool HideAmulet = false;
        public bool HideRings = false;

        public bool hideHead = false;
        public bool hideUpperbody = false;
        public bool hideArms = false;
        public bool hideHands = false;
        public bool hideLegs = false;
        public bool hideFeet = false;

        // For shields
        public AudioClip blockingSound;
    }
}