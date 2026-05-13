using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;
using System;

namespace RPGCreationKit
{
    /// <summary>
    /// Class for AmmoItems, like Arrows and projectiles
    /// </summary>
    [CreateAssetMenu(fileName = "New AmmoItem", menuName = "RPG Creation Kit/Items/AmmoItem", order = 1)]
    public class AmmoItem : Item
    {
        // Override
        public override ItemTypes itemType { get { return ItemTypes.AmmoItem; } }

        [System.Flags]
        public enum AmmoType
        {
            Arrow = 1,
            Dart = 2,
            Ninemm = 3
        }

        [EnumFlags] public AmmoType type;

        public bool canBeEquipped = true; // True for arrows, false for firearms ammos

        public GameObject projectile;
        public GameObject bagOnBody;

        public float Damage = 8.0f;
        public float Speed = 1.5f;  // Speed of projectile when fired


        /// <summary>
        /// Returns the types of this AmmoItem
        /// </summary>
        /// <returns></returns>
        public List<int> ReturnSelectedElements()
        {
            List<int> selectedElements = new List<int>();
            for (int i = 0; i < System.Enum.GetValues(typeof(AmmoType)).Length; i++)
            {
                int layer = 1 << i;
                if (((int)type & layer) != 0)
                {
                    selectedElements.Add(i);
                }
            }

            return selectedElements;
        }


    }
}
