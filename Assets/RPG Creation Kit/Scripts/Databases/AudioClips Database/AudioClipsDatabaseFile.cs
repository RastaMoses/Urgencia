using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.DialogueSystem;
using UnityEditor;

namespace RPGCreationKit
{
    [System.Serializable]
    public class AudioClipsDatabaseDictionary : SerializableDictionary<string, AudioClip> { }

    [CreateAssetMenu(fileName = "New Audio Database", menuName = "RPG Creation Kit/Databases/New Audio Database", order = 1)]
    public class AudioClipsDatabaseFile : RckDatabase
    {
        [SerializeField] private List<AudioClip> allAudioClips = new List<AudioClip>();
        public AudioClipsDatabaseDictionary dictionary;


        // This database is manually filled
#if UNITY_EDITOR
        [ContextMenu("Update")]
        public override void fill()
        {
            dictionary.Clear();

            for (int i = 0; i < allAudioClips.Count; i++)
            {
                if (allAudioClips[i] != null)
                {
                    Debug.Log(i + " " + allAudioClips[i].name);
                    dictionary.Add(allAudioClips[i].name, allAudioClips[i]);
                }
            }
        }
#endif
    }
}