using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.DialogueSystem;
using UnityEditor;

namespace RPGCreationKit
{
    [System.Serializable]
    public class DialoguesDatabaseDictionary : SerializableDictionary<string, DialogueGraph> { }

    [CreateAssetMenu(fileName = "New Dialogues Database", menuName = "RPG Creation Kit/Databases/New Dialogues Database", order = 1)]
    public class DialoguesDatabaseFile : RckDatabase
    {
        [SerializeField] private List<DialogueGraph> allDialogueGraphs = new List<DialogueGraph>();

        public DialoguesDatabaseDictionary dictionary;

        [ContextMenu("Check Empty Voices")]
        public void CheckEmptyVoices()
        {
            for (int i = 0; i < allDialogueGraphs.Count; i++)
            {
                for (int j = 0; j < allDialogueGraphs[i].nodes.Count; j++)
                {
                    if (allDialogueGraphs[i].nodes[j] is NPCDialogueLineNode)
                    {
                        if (((NPCDialogueLineNode)allDialogueGraphs[i].nodes[j]).audioClip == null)
                        {
                            Debug.Log("MISSING! " + allDialogueGraphs[i].ID + "Line: " + ((NPCDialogueLineNode)allDialogueGraphs[i].nodes[j]).line);
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Fill With All Items")]
        public override void fill()
        {
            dictionary.Clear();
            allDialogueGraphs = GetAllInstances<DialogueGraph>();

            for (int i = 0; i < allDialogueGraphs.Count; i++)
            {
                if (allDialogueGraphs[i] != null)
                {
                    Debug.Log(i + " " + allDialogueGraphs[i].name);
                    dictionary.Add(allDialogueGraphs[i].ID, allDialogueGraphs[i]);
                }
            }
        }

        public static List<T> GetAllInstances<T>() where T : DialogueGraph
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