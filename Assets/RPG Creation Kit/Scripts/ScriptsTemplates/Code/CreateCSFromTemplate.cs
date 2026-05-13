using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEditor;
using System.Text;
using System.IO;

namespace RPGCreationKit
{
#if UNITY_EDITOR
    public static class CreateCSFromTemplate
    {
        private static Texture2D scriptIcon = (EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D);

        /// <summary>Creates a new C# Class.</summary>
        [MenuItem("Assets/Create/RPG Creation Kit/C#/New Item Script.cs", false, 89)]
        private static void CreateNewItemScript()
        {
            string[] guids = AssetDatabase.FindAssets("RPGCreationKit__ItemScript_NewItemScript.cs");
            if (guids.Length == 0)
            {
                Debug.LogWarning("RPGCreationKit__ItemScript_NewItemScript.cs.txt not found in asset database");
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            CreateFromTemplate(
                "NewItemScript.cs",
                path
            );
        }

        /// <summary>Creates a new C# Class.</summary>
        [MenuItem("Assets/Create/RPG Creation Kit/C#/Quests/New Quest Script.cs", false, 89)]
        private static void CreateNewQuestScript()
        {
            string[] guids = AssetDatabase.FindAssets("RPGCreationKit__QuestScript_NewQuestScript.cs");
            if (guids.Length == 0)
            {
                Debug.LogWarning("RPGCreationKit__QuestScript_NewQuestScript.cs.txt not found in asset database");
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            CreateFromTemplate(
                "NewQuestScript.cs",
                path
            );
        }

        /// <summary>Creates a new C# Class.</summary>
        [MenuItem("Assets/Create/RPG Creation Kit/C#/Quests/New QuestStage Script.cs", false, 89)]
        private static void CreateNewQuesStagetScript()
        {
            string[] guids = AssetDatabase.FindAssets("RPGCreationKit__QuestStageScript_NewQuestStageXScript.cs");
            if (guids.Length == 0)
            {
                Debug.LogWarning("RPGCreationKit__QuestStageScript_NewQuestStageXScript.cs.txt not found in asset database");
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            CreateFromTemplate(
                "NewQuestStageScript.cs",
                path
            );
        }

        /// <summary>Creates a new C# Class.</summary>
        [MenuItem("Assets/Create/RPG Creation Kit/C#/New Result Script.cs", false, 89)]
        private static void CreateNewResultScript()
        {
            string[] guids = AssetDatabase.FindAssets("RPGCreationKit__ResultScript_NewResultScript.cs");
            if (guids.Length == 0)
            {
                Debug.LogWarning("RPGCreationKit__ResultScript_NewResultScript.cs.txt not found in asset database");
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            CreateFromTemplate(
                "NewResultScript.cs",
                path
            );
        }

        public static void CreateFromTemplate(string initialName, string templatePath)
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateCodeFile>(),
                initialName,
                scriptIcon,
                templatePath
            );
        }

        /// Inherits from EndNameAction, must override EndNameAction.Action
        public class DoCreateCodeFile : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                Object o = CreateScript(pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(o);
            }
        }

        /// <summary>Creates Script from Template's path.</summary>
        internal static UnityEngine.Object CreateScript(string pathName, string templatePath)
        {
            string className = Path.GetFileNameWithoutExtension(pathName).Replace(" ", string.Empty);
            string templateText = string.Empty;

            UTF8Encoding encoding = new UTF8Encoding(true, false);

            if (File.Exists(templatePath))
            {
                /// Read procedures.
                StreamReader reader = new StreamReader(templatePath);
                templateText = reader.ReadToEnd();
                reader.Close();

                templateText = templateText.Replace("#SCRIPTNAME#", className);
                templateText = templateText.Replace("#NOTRIM#", string.Empty);
                /// You can replace as many tags you make on your templates, just repeat Replace function
                /// e.g.:
                /// templateText = templateText.Replace("#NEWTAG#", "MyText");

                /// Write procedures.

                StreamWriter writer = new StreamWriter(Path.GetFullPath(pathName), false, encoding);
                writer.Write(templateText);
                writer.Close();

                AssetDatabase.ImportAsset(pathName);
                return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
            }
            else
            {
                Debug.LogError(string.Format("The template file was not found: {0}", templatePath));
                return null;
            }
        }
    }
#endif
}
