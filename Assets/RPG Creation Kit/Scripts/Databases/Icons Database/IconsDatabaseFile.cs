using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEditor;

namespace RPGCreationKit
{
    [System.Serializable]
    public class IconsDatabaseDictionary : SerializableDictionary<string, Sprite> { }

    [CreateAssetMenu(fileName = "New Icons Database", menuName = "RPG Creation Kit/Databases/New Icons Database", order = 1)]
    public class IconsDatabaseFile : RckDatabase
    {
        [SerializeField]
        public IconsDatabaseDictionary dictionary = new IconsDatabaseDictionary();
    }
}