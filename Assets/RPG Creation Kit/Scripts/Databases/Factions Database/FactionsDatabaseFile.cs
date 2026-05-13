using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEditor;

namespace RPGCreationKit
{
    [System.Serializable]
    public class FactionsDatabaseDictionary : SerializableDictionary<string, Faction> { }

    [CreateAssetMenu(fileName = "New Factions Database", menuName = "RPG Creation Kit/Databases/New Factions Database", order = 1)]
    public class FactionsDatabaseFile : RckDatabase
    {
        [SerializeField] private List<Faction> allFactions = new List<Faction>();

        public FactionsDatabaseDictionary dictionary;

#if UNITY_EDITOR
        [ContextMenu("Fill With All Items")]
        public override void fill()
        {
            dictionary.Clear();
            allFactions = GetAllInstances<Faction>();

            for (int i = 0; i < allFactions.Count; i++)
            {
                if (allFactions[i] != null)
                {
                    Debug.Log(i + " " + allFactions[i].name);
                    dictionary.Add(allFactions[i].ID, allFactions[i]);
                }
            }
        }

        public static List<T> GetAllInstances<T>() where T : Faction
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