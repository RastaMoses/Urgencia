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
    [CustomNodeEditor(typeof(ChangeStateNode))]
    public class ChangeStateNodeEditor : NodeEditor
    {
        private ChangeStateNode lineNode;

        public MonoScript[] allResultScripts;

        public override void OnBodyGUI()
        {
            if (lineNode == null) lineNode = target as ChangeStateNode;
            if (allResultScripts == null) allResultScripts = NodesHelper.GetAllResultScripts<ResultScript>();

            // Update serialized object's representation
            serializedObject.Update();

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

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

            EditorGUILayout.Space(2.5f);

            float defaultLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = 115;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("events"));

            EditorGUILayout.Space(2.5f);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("stateChange"));

            switch(serializedObject.FindProperty("stateChange").enumValueIndex)
            {
                case (int)ChangeStateNode.DialogueStateChange.Trade:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("toCurrentNPC"));

                    if (!serializedObject.FindProperty("toCurrentNPC").boolValue)
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("npcRef"));
                    break;

                case (int)ChangeStateNode.DialogueStateChange.Loot:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lootingPointRef"));
                    break;

                case (int)ChangeStateNode.DialogueStateChange.Teleport:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coordinates"));
                    break;

                case (int)ChangeStateNode.DialogueStateChange.ChangeCell:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cell"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coordinates"), true);
                    break;

                case (int)ChangeStateNode.DialogueStateChange.SpeakToNPC:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("npcRef"));
                    break;
            }

            EditorGUILayout.Space(2.5f);
            if (serializedObject.FindProperty("stateChange").enumValueIndex != (int)ChangeStateNode.DialogueStateChange.Trade &&
                serializedObject.FindProperty("stateChange").enumValueIndex != (int)ChangeStateNode.DialogueStateChange.Loot)
            {
                serializedObject.FindProperty("interruptsDialogue").boolValue = true;
                GUI.enabled = false;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("interruptsDialogue"));

            GUI.enabled = true;
            EditorGUILayout.Space(2.5f);

            if (!serializedObject.FindProperty("interruptsDialogue").boolValue)
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("output"));
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
            if (serializedObject.FindProperty("events").isExpanded &&
                serializedObject.FindProperty("events").FindPropertyRelative("consequences").isExpanded)
                return 350;
            else
                return 300;
        }

    }
}
