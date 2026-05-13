using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
using RPGCreationKit.DialogueSystem;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit.DialogueSystem
{
    [CustomNodeEditor(typeof(EntryNode))]
    public class EntryNodeEditor : NodeEditor
    {
        private EntryNode lineNode;

        public MonoScript[] allResultScripts;

        public override void OnBodyGUI()
        {
            if (lineNode == null) lineNode = target as EntryNode;
            if (allResultScripts == null) allResultScripts = NodesHelper.GetAllResultScripts<ResultScript>();

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 150;

            EditorGUILayout.LabelField("Dialogue Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isNpcToNpcDialogue"), new GUIContent("is Npc To Npc Dialogue", "Is this a dialogue that doesn't involve the player?"));


            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("firstNode"));

            EditorGUILayout.Space(2.5f);

            float defaultLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = 130;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("events"));
            EditorGUIUtility.labelWidth = defaultLabelWidth;

            EditorGUILayout.Space(2.5f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Result Script:");

            var resultScript = serializedObject.FindProperty("resultScript");

            string resultScriptMenuValue = string.IsNullOrEmpty(resultScript.stringValue) ? "-None-" : resultScript.stringValue;
            if (GUILayout.Button(resultScriptMenuValue))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                menu.AddDisabledItem(new GUIContent("Result Scripts"));

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("None"), false, Callback, new ScriptElementData(resultScript, ""));
                for (int i = 0; i < allResultScripts.Length; i++)
                {
                    menu.AddItem(new GUIContent(allResultScripts[i].name), false, Callback, new ScriptElementData(resultScript, allResultScripts[i].GetClass().Namespace + "." + allResultScripts[i].name));
                }

                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

        }

        public static MonoScript[] GetScriptAssetsOfType<T>()
        {
            MonoScript[] scripts = (MonoScript[])Object.FindObjectsOfTypeIncludingAssets(typeof(MonoScript));

            List<MonoScript> result = new List<MonoScript>();

            foreach (MonoScript m in scripts)
            {
                if (m.GetClass() != null && m.GetClass().IsSubclassOf(typeof(T)) && m.GetType() != typeof(Shader))
                {
                    result.Add(m);
                }
            }
            return result.ToArray();
        }

        void Callback(object obj)
        {
            ScriptElementData questScriptData = (ScriptElementData)obj;
            questScriptData.property.stringValue = questScriptData.value;

            questScriptData.property.serializedObject.ApplyModifiedProperties();
        }

        public override int GetWidth()
        {
            if (serializedObject.FindProperty("events").FindPropertyRelative("consequences").isExpanded)
            {
                return 350;
            }
            else
                return 250;
        }

    }
}