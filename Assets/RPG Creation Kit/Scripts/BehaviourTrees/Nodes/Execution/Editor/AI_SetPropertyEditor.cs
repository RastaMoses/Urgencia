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
    [CustomNodeEditor(typeof(AI_SetPropertyNode))]
    public class AI_SetPropertyEditor : BTNodeNodeEditor
    {
        private AI_SetPropertyNode setProperty;

        string[] allProperties;

        string typeToDisplay = "";
        Type propertyType;

        public override void OnCreate()
        {
            base.OnCreate();

            // Fill fields
            allProperties =
            typeof(RckAI).GetProperties()
            .Where(m => BTVariable.SUPPORTED_TYPES.Contains(m.PropertyType))
            .Select(x => x.Name)
            .ToArray();
        }


        public override void OnBodyGUI()
        {
            if (setProperty == null) setProperty = target as AI_SetPropertyNode;
            btNode = setProperty;

            Color dColor = GUI.color;

            if (setProperty.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (setProperty.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (setProperty.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 105;


            // Initalize
            if (allProperties.Length > 0 && string.IsNullOrEmpty(serializedObject.FindProperty("PropertyToSet").stringValue))
                serializedObject.FindProperty("PropertyToSet").stringValue = allProperties[0];

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            int index = 0;

            try
            {
                index = allProperties
                    .Select((v, i) => new { Name = v, Index = i })
                    .First(x => x.Name == serializedObject.FindProperty("PropertyToSet").stringValue)
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

                if (string.IsNullOrEmpty(typeToDisplay) || propertyType == null)
                {
                    propertyType = typeof(RckAI).GetProperty(serializedObject.FindProperty("PropertyToSet").stringValue).PropertyType;
                    typeToDisplay = propertyType.Name.ToString();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("PropertyToSet").stringValue = allProperties[EditorGUILayout.Popup(index, allProperties)];

                if (EditorGUI.EndChangeCheck())
                {
                    propertyType = typeof(RckAI).GetProperty(serializedObject.FindProperty("PropertyToSet").stringValue).PropertyType;
                    typeToDisplay = propertyType.Name.ToString();
                }


                EditorGUILayout.LabelField("(" + typeToDisplay + ")", GUILayout.MaxWidth(80));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("useVariable"));

                if (serializedObject.FindProperty("useVariable").boolValue)
                    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("storedValue"));
                else
                {
                    if(propertyType == typeof(int))
                    {
                        setProperty.instantValue.parameterType = BTParameterType.INT;
                        setProperty.instantValue.intValue = EditorGUILayout.IntField("Int Value", setProperty.instantValue.intValue);
                    }
                    else if(propertyType == typeof(float))
                    {
                        setProperty.instantValue.parameterType = BTParameterType.FLOAT;
                        setProperty.instantValue.floatValue = EditorGUILayout.FloatField("Float Value", setProperty.instantValue.floatValue);
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
