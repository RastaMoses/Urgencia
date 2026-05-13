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
    [CustomNodeEditor(typeof(EventsNode))]
    public class EventsNodeEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            // Update serialized object's representation
            serializedObject.Update();

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            EditorGUILayout.Space(2.5f);

            float defaultLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = 130;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("events"));
            EditorGUIUtility.labelWidth = defaultLabelWidth;

            EditorGUILayout.Space(2.5f);

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("output"));

            serializedObject.ApplyModifiedProperties();
        }

        void Callback(object obj)
        {
            ScriptElementData questScriptData = (ScriptElementData)obj;
            questScriptData.property.stringValue = questScriptData.value;

            questScriptData.property.serializedObject.ApplyModifiedProperties();
        }

        public override int GetWidth()
        {
            if (serializedObject.FindProperty("events").isExpanded)
                return 350;
            else
                return 250;
        }

    }
}