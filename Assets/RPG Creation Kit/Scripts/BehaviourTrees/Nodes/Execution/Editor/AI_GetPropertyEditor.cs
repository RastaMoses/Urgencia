using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.BehaviourTree.Data;
using UnityEditor;
using RPGCreationKit;
using System.Linq;
using System.Reflection;
using RPGCreationKit.AI;
using System;

namespace RPGCreationKit.BehaviourTree
{
    [CustomNodeEditor(typeof(AI_GetPropertyNode))]
    public class AI_GetPropertyEditor : BTNodeNodeEditor
    {
        private AI_GetPropertyNode getPropertyNode;

        string[] allProperties;

        string typeToDisplay = "";

        public override void OnCreate()
        {
            base.OnCreate();

            // Fill fields
            allProperties =
            typeof(RckAI).GetProperties()
            .Where(m => BTVariable.SUPPORTED_TYPES.Contains(m.PropertyType))
            .Select(x => x.Name)
            .ToArray();

            if (allProperties.Length > 0 && string.IsNullOrEmpty(serializedObject.FindProperty("PropertyToGet").stringValue))
                serializedObject.FindProperty("PropertyToGet").stringValue = allProperties[0];
        }


        public override void OnBodyGUI()
        {
            if (getPropertyNode == null) getPropertyNode = target as AI_GetPropertyNode;
            btNode = getPropertyNode;

            Color dColor = GUI.color;

            if (getPropertyNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (getPropertyNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (getPropertyNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 105;

            // Initalize
            if (allProperties.Length > 0 && string.IsNullOrEmpty(serializedObject.FindProperty("PropertyToGet").stringValue))
                serializedObject.FindProperty("PropertyToGet").stringValue = allProperties[0];

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            int index = 0;

            try
            {
                index = allProperties
                    .Select((v, i) => new { Name = v, Index = i })
                    .First(x => x.Name == serializedObject.FindProperty("PropertyToGet").stringValue)
                    .Index;
            }
            catch
            {
                index = 0;
            }


            foldout = EditorGUILayout.Foldout(foldout, "Show/Hide", true);
            EditorGUI.indentLevel++;

            if (foldout)
            {
                //NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("ComponentToGet"));

                if (string.IsNullOrEmpty(typeToDisplay))
                    typeToDisplay = typeof(RckAI).GetProperty(serializedObject.FindProperty("PropertyToGet").stringValue).PropertyType.Name.ToString();

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("PropertyToGet").stringValue = allProperties[EditorGUILayout.Popup(index, allProperties)];

                if (EditorGUI.EndChangeCheck())
                    typeToDisplay = typeof(RckAI).GetProperty(serializedObject.FindProperty("PropertyToGet").stringValue).PropertyType.Name.ToString();

                EditorGUILayout.LabelField("(" + typeToDisplay + ")", GUILayout.MaxWidth(80));
                EditorGUILayout.EndHorizontal();

                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("storedValue"));

                EditorGUILayout.Space(2.5f);
            }
            EditorGUI.indentLevel--;

            GUI.color = dColor;
        }

        public override int GetWidth()
        {
            return 285;
        }
    }
}