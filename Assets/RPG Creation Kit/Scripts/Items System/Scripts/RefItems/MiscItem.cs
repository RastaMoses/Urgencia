using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Class for MiscItem
    /// </summary>
    [CreateAssetMenu(fileName = "New MiscItem", menuName = "RPG Creation Kit/Items/MiscItem", order = 1)]
    public class MiscItem : Item
    {
        public override ItemTypes itemType { get { return ItemTypes.MiscItem; } }

        public bool showInPlayerInventory = true;
    }
}
