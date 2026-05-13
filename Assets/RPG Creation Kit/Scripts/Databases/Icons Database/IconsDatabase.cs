using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class IconsDatabase : MonoBehaviour
    {
        public static IconsDatabase instance;
        private void Awake()
        {
            instance = this;
            sFile = file;
        }

        public IconsDatabaseFile file;
        public static IconsDatabaseFile sFile;

        public static Sprite GetItem(string _itemID)
        {
            Sprite item;
            sFile.dictionary.TryGetValue(_itemID, out item);
            return item;
        }
    }
}
