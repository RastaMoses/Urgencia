using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEditor;

namespace RPGCreationKit
{
    [System.Serializable]
    public class RacesDatabaseDictionary : SerializableDictionary<string, Race> { }

    [CreateAssetMenu(fileName = "New Race Database", menuName = "RPG Creation Kit/Databases/New Race Database", order = 1)]
    public class RaceDatabaseFile : RckDatabase
    {
        [SerializeField] private List<Race> allRaces = new List<Race>();

        public RacesDatabaseDictionary dictionary;

#if UNITY_EDITOR
        [ContextMenu("Fill With All Items")]
        public override void fill()
        {
            dictionary.Clear();
            allRaces = GetAllInstances<Race>();

            for (int i = 0; i < allRaces.Count; i++)
            {
                if (allRaces[i] != null)
                {
                    Debug.Log(i + " " + allRaces[i].name);
                    dictionary.Add(allRaces[i].ID, allRaces[i]);
                }
            }
        }

        public static List<T> GetAllInstances<T>() where T : Race
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