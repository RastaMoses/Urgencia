using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEditor;

namespace RPGCreationKit
{
    [System.Serializable]
    public class QuestsDatabaseDictionary : SerializableDictionary<string, Quest> { }

    [CreateAssetMenu(fileName = "New Quests Database", menuName = "RPG Creation Kit/Databases/New Quests Database", order = 1)]
    public class QuestsDatabaseFile : RckDatabase
    {
        [SerializeField] private List<Quest> allItems = new List<Quest>();

        public QuestsDatabaseDictionary dictionary;

#if UNITY_EDITOR
        [ContextMenu("Fill With All Items")]
        public override void fill()
        {
            dictionary.Clear();
            allItems = GetAllInstances<Quest>();

            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i] != null)
                    dictionary.Add(allItems[i].questID, allItems[i]);
            }
        }

        public static List<T> GetAllInstances<T>() where T : Quest
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