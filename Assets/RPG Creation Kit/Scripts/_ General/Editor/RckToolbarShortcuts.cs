using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEditor;

namespace RPGCreationKit
{
    public class RckToolbarShortcuts : EditorWindow
    {
        static List<RckDatabase> allDatabases;

        [MenuItem("RPG Creation Kit/Update All Databases")]
        public static void UpdateAllDatabases()
        {
            allDatabases = GetAllInstances<RckDatabase>();

            for (int i = 0; i < allDatabases.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Updating Databases...", "Working on " + allDatabases[i].name, ((float)i) / (float)allDatabases.Count);
                allDatabases[i].fill();
                EditorUtility.SetDirty(allDatabases[i]);
                AssetDatabase.SaveAssets();
            }
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("RPG Creation Kit/Display RCK Version")]
        public static void DisplayRckVersion()
        {
            EditorUtility.DisplayDialog("RPG Creation Kit", "This version of the RPG Creation Kit is: V" + RCKSettings.GAME_VERSION, "Ok");
        }

        public static List<T> GetAllInstances<T>() where T : RckDatabase
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


    }
}