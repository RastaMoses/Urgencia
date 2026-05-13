using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using UnityEditor;

namespace RPGCreationKit
{
    [System.Serializable]
    public class WorldspacesDatabaseDictionary : SerializableDictionary<string, Worldspace> { }

    [CreateAssetMenu(fileName = "New Worldspace Database", menuName = "RPG Creation Kit/Databases/New Worldspace Database", order = 1)]
    public class WorldspaceDatabaseFile : RckDatabase
    {
        [SerializeField] private List<Worldspace> allItems = new List<Worldspace>();

        public WorldspacesDatabaseDictionary dictionary;

#if UNITY_EDITOR
        [ContextMenu("Fill With All Items")]
        public override void fill()
        {
            dictionary.Clear();
            allItems = GetAllInstances<Worldspace>();

            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i] != null)
                {
                    Debug.Log(i + " " + allItems[i].name);
                    dictionary.Add(allItems[i].worldSpaceID, allItems[i]);
                }
            }
        }

        public static List<T> GetAllInstances<T>() where T : Worldspace
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