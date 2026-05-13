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
    [CustomNodeEditor(typeof(AI_SetFieldNode))]
    public class SetFieldNodeEditor : BTNodeNodeEditor
    {
        private AI_SetFieldNode setFieldNode;

        string[] allFields;

        string typeToDisplay = "";
        Type fieldType;

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
        }


        public override void OnBodyGUI()
        {
            if (setFieldNode == null) setFieldNode = target as AI_SetFieldNode;
            btNode = setFieldNode;

            Color dColor = GUI.color;

            if (setFieldNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (setFieldNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (setFieldNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 105;


            // Initalize
            if (allFields.Length > 0 && string.IsNullOrEmpty(serializedObject.FindProperty("FieldToSet").stringValue))
                serializedObject.FindProperty("FieldToSet").stringValue = allFields[0];

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            int index = 0;

            try
            {
                index = allFields
                    .Select((v, i) => new { Name = v, Index = i })
                    .First(x => x.Name == serializedObject.FindProperty("FieldToSet").stringValue)
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

                if (string.IsNullOrEmpty(typeToDisplay) || fieldType == null)
                {
                    fieldType = typeof(RckAI).GetField(serializedObject.FindProperty("FieldToSet").stringValue).FieldType;
                    typeToDisplay = fieldType.Name.ToString();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("FieldToSet").stringValue = allFields[EditorGUILayout.Popup(index, allFields)];

                if (EditorGUI.EndChangeCheck())
                {
                    fieldType = typeof(RckAI).GetField(serializedObject.FindProperty("FieldToSet").stringValue).FieldType;
                    typeToDisplay = fieldType.Name.ToString();
                }


                EditorGUILayout.LabelField("(" + typeToDisplay + ")", GUILayout.MaxWidth(80));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("useVariable"));

                if(serializedObject.FindProperty("useVariable").boolValue)
                    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("storedValue"));
                else
                {
                    if (fieldType == typeof(int))
                    {
                        setFieldNode.instantValue.parameterType = BTParameterType.INT;
                        setFieldNode.instantValue.intValue = EditorGUILayout.IntField("Int Value", setFieldNode.instantValue.intValue);
                    }
                    else if (fieldType == typeof(float))
                    {
                        setFieldNode.instantValue.parameterType = BTParameterType.FLOAT;
                        setFieldNode.instantValue.floatValue = EditorGUILayout.FloatField("Float Value", setFieldNode.instantValue.floatValue);
                    }
                    else if(fieldType == typeof(bool))
                    {
                        setFieldNode.instantValue.parameterType = BTParameterType.BOOL;
                        setFieldNode.instantValue.boolValue = EditorGUILayout.Toggle("Bool Value", setFieldNode.instantValue.boolValue);
                    }
                }

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
