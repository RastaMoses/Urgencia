using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEditor;
using RPGCreationKit.AI;
using System.IO;

namespace RPGCreationKit
{
    [System.Serializable]
    public class AIDatabaseFileDictionary : SerializableDictionary<string, GameObject> { }

    [CreateAssetMenu(fileName = "New AI Database", menuName = "RPG Creation Kit/Databases/New AI Database", order = 1)]
    public class AIDatabaseFile : RckDatabase
    {
        public AIDatabaseFileDictionary dictionary = new AIDatabaseFileDictionary();



#if UNITY_EDITOR
        [ContextMenu("Fill With All Items")]
        public override void fill()
        {
            dictionary.Clear();
            var allItems = GetAllRckAI();

            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i] != null)
                    dictionary.Add(allItems[i].entityID, allItems[i].gameObject);
            }
        }

        public static List<RckAI> GetAllRckAI()
        {
            string[] allAssets = Directory.GetFiles(RCKSettings.EDITOR_AI_SAVE_LOCATION, "*.prefab", SearchOption.AllDirectories);

            List<RckAI> allAI = new List<RckAI>();

            RckAI thisAI = null;
            for (int i = 0; i < allAssets.Length; i++)
            {
                var assetObject = AssetDatabase.LoadAssetAtPath(allAssets[i], typeof(UnityEngine.Object));

                if (PrefabUtility.GetPrefabAssetType(assetObject) != PrefabAssetType.NotAPrefab)
                {
                    thisAI = (assetObject as GameObject).GetComponent<RckAI>();

                    if (thisAI != null && thisAI.entityID != "DEFAULT_ENTITY_ID")
                        allAI.Add((assetObject as GameObject).GetComponent<RckAI>());
                }

                thisAI = null;
            }

            return allAI;
        }
#endif



    }
}