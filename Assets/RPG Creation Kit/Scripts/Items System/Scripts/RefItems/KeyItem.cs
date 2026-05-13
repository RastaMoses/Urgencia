using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Class for KeyItem
    /// </summary>
    [CreateAssetMenu(fileName = "New Key", menuName = "RPG Creation Kit/Items/KeyItem", order = 1)]
    public class KeyItem : Item
    {
        public override ItemTypes itemType { get { return ItemTypes.KeyItem; } }

    }
}
