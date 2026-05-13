using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.BehaviourTree;
using UnityEditor;

namespace RPGCreationKit
{
    [System.Serializable]
    public class BehavioursDatabaseDictionary : SerializableDictionary<string, RPGCK_BT> { }

    [CreateAssetMenu(fileName = "New Behaviours Database", menuName = "RPG Creation Kit/Databases/New Behaviours Database", order = 1)]
    public class BehaviourDatabaseFile : RckDatabase
    {
        [SerializeField] public List<RPGCK_BT> allBehaviours = new List<RPGCK_BT>();

        public BehavioursDatabaseDictionary dictionary;

#if UNITY_EDITOR
        [ContextMenu("Fill With All Items")]
        public override void fill()
        {
            dictionary.Clear();
            allBehaviours = GetAllInstances<RPGCK_BT>();

            for (int i = 0; i < allBehaviours.Count; i++)
            {
                if (allBehaviours[i] != null)
                {
                    Debug.Log(i + " " + allBehaviours[i].name);
                    dictionary.Add(allBehaviours[i].ID, allBehaviours[i]);
                }
            }
        }

        public static List<T> GetAllInstances<T>() where T : RPGCK_BT
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