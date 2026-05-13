using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Class for BookItems
    /// </summary>
    [CreateAssetMenu(fileName = "New BookItem", menuName = "RPG Creation Kit/Items/BookItem", order = 1)]
    public class BookItem : Item
    {
        public override ItemTypes itemType { get { return ItemTypes.BookItem; } }

        public Sprite openedCoverSprite;
        [TextArea] public string BookText = "Book Text...";

        public bool CantBeTaken = false;
        public bool isNoteOrScroll = false;

    }
}
