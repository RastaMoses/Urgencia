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
    [CustomNodeEditor(typeof(AI_GetFieldNode))]
    public class GetFieldNodeEditor : BTNodeNodeEditor
    {
        private AI_GetFieldNode getFieldNode;

        string[] allFields;

        string typeToDisplay = "";

        public override void OnCreate()
        {
            base.OnCreate();

            // Fill fields
            allFields =
            typeof(RckAI).GetFields()
            .Where(x => x.IsPublic)
            .Where(m => BTVariable.SUPPORTED_TYPES.Contains(m.FieldType))
            .Select(x => x.Name)
            .ToArray();

            if (allFields.Length > 0 && string.IsNullOrEmpty(serializedObject.FindProperty("FieldToGet").stringValue))
                serializedObject.FindProperty("FieldToGet").stringValue = allFields[0];
        }


        public override void OnBodyGUI()
        {
            if (getFieldNode == null) getFieldNode = target as AI_GetFieldNode;
            btNode = getFieldNode;

            Color dColor = GUI.color;

            if (getFieldNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (getFieldNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (getFieldNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 105;

            // Initalize
            if (allFields.Length > 0 && string.IsNullOrEmpty(serializedObject.FindProperty("FieldToGet").stringValue))
                serializedObject.FindProperty("FieldToGet").stringValue = allFields[0];

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            int index = 0;

            try
            {
                index = allFields
                    .Select((v, i) => new { Name = v, Index = i })
                    .First(x => x.Name == serializedObject.FindProperty("FieldToGet").stringValue)
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

                if(string.IsNullOrEmpty(typeToDisplay))
                    typeToDisplay = typeof(RckAI).GetField(serializedObject.FindProperty("FieldToGet").stringValue).FieldType.Name.ToString();

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("FieldToGet").stringValue = allFields[EditorGUILayout.Popup(index, allFields)];

                if (EditorGUI.EndChangeCheck())
                    typeToDisplay = typeof(RckAI).GetField(serializedObject.FindProperty("FieldToGet").stringValue).FieldType.Name.ToString();

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