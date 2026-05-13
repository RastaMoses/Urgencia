using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEditor;

namespace RPGCreationKit
{
    [System.Serializable]
    public class ItemsDatabaseDictionary : SerializableDictionary<string, Item> { }

    [CreateAssetMenu(fileName = "New Items Database", menuName = "RPG Creation Kit/Databases/New Items Database", order = 1)]
    public class ItemsDatabaseFile : RckDatabase
    {
        [SerializeField] private List<Item> allItems = new List<Item>();

        public ItemsDatabaseDictionary dictionary;

#if UNITY_EDITOR
        [ContextMenu("Fill With All Items")]
        public override void fill()
        {
            dictionary.Clear();
            allItems = GetAllInstances<Item>();

            for(int i = 0; i < allItems.Count; i++)
            {
                if(allItems[i] != null)
                {
                    Debug.Log(i + " " + allItems[i].name);
                    dictionary.Add(allItems[i].ItemID, allItems[i]);
                }
            }
        }

        public static List<T> GetAllInstances<T>() where T : Item
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            List<T> a = new List<T>(guids.Length);

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a.Add(AssetDatabase.LoadAssetAtPath<T>(path));
            }

            return a;
        }
#endif
    }
}