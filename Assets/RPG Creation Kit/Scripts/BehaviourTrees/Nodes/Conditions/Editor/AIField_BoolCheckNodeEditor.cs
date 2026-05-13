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
    [CustomNodeEditor(typeof(AIField_BoolCheckNode))]
    public class AIField_BoolCheckNodeEditor : BTNodeNodeEditor
    {
        private AIField_BoolCheckNode boolCheckNode;

        string[] allFields;

        string typeToDisplay = "";

        public override void OnCreate()
        {
            base.OnCreate();

            // Fill fields
            allFields =
            typeof(RckAI).GetFields()
            .Where(x => x.IsPublic)
            .Where(m => m.FieldType == typeof(bool))
            .Select(x => x.Name)
            .ToArray();

            if (allFields.Length > 0 && string.IsNullOrEmpty(serializedObject.FindProperty("FieldToGet").stringValue))
                serializedObject.FindProperty("FieldToGet").stringValue = allFields[0];
        }


        public override void OnBodyGUI()
        {
            if (boolCheckNode == null) boolCheckNode = target as AIField_BoolCheckNode;
            btNode = boolCheckNode;

            Color dColor = GUI.color;

            if (boolCheckNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (boolCheckNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (boolCheckNode.m_NodeDebugState == NodeState.Failure)
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
               NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("not"));

                if (string.IsNullOrEmpty(typeToDisplay))
                    typeToDisplay = typeof(RckAI).GetField(serializedObject.FindProperty("FieldToGet").stringValue).FieldType.Name.ToString();

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("FieldToGet").stringValue = allFields[EditorGUILayout.Popup(index, allFields)];

                if (EditorGUI.EndChangeCheck())
                    typeToDisplay = typeof(RckAI).GetField(serializedObject.FindProperty("FieldToGet").stringValue).FieldType.Name.ToString();

                EditorGUILayout.LabelField("(" + typeToDisplay + ")", GUILayout.MaxWidth(80));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(2.5f);
            }
            EditorGUI.indentLevel--;

            GUI.color = dColor;
        }

        public override int GetWidth()
        {
            return 200;
        }
    }
}