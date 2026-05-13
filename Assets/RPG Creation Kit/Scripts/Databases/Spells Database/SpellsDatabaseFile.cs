using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using UnityEditor;

namespace RPGCreationKit
{
    [System.Serializable]
    public class SpellsDatabaseDictionary: SerializableDictionary<string, Spell> { }

    [CreateAssetMenu(fileName = "New Spells Database", menuName = "RPG Creation Kit/Databases/New Spells Database", order = 1)]
    public class SpellsDatabaseFile : RckDatabase
    {
        [SerializeField] private List<Spell> allItems = new List<Spell>();

        public SpellsDatabaseDictionary dictionary;

#if UNITY_EDITOR
        [ContextMenu("Fill With All Items")]
        public override void fill()
        {
            dictionary.Clear();
            allItems = GetAllInstances<Spell>();

            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i] != null)
                {
                    Debug.Log(i + " " + allItems[i].name);
                    dictionary.Add(allItems[i].spellID, allItems[i]);
                }
            }
        }

        public static List<T> GetAllInstances<T>() where T : Spell
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