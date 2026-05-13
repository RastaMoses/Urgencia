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
    [CustomNodeEditor(typeof(EditGraphVariableNode))]
    public class EditGraphVariableNodeEditor : BTNodeNodeEditor
    {
        private EditGraphVariableNode setVariableNode;

        string typeToDisplay = "";
        Type variableType;

        public override void OnCreate()
        {
            base.OnCreate();
        }


        public override void OnBodyGUI()
        {
            if (setVariableNode == null) setVariableNode = target as EditGraphVariableNode;
            btNode = setVariableNode;

            Color dColor = GUI.color;

            if (setVariableNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (setVariableNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (setVariableNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 105;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            foldout = EditorGUILayout.Foldout(foldout, "Show/Hide", true);
            EditorGUI.indentLevel++;

            if (foldout)
            {
                if ((string.IsNullOrEmpty(typeToDisplay) || variableType == null) && setVariableNode.btVariable != null)
                {
                    variableType = setVariableNode.btVariable.GetType();
                    typeToDisplay = variableType.ToString();
                }


                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("btVariable"));

                if (EditorGUI.EndChangeCheck() && setVariableNode.btVariable != null)
                {
                    variableType = setVariableNode.btVariable.GetType();
                    typeToDisplay = variableType.ToString();
                }


                EditorGUILayout.LabelField("(" + typeToDisplay + ")", GUILayout.MaxWidth(80));
                EditorGUILayout.EndHorizontal();

                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("editType"));


                if (variableType == typeof(BT_Int))
                {
                    setVariableNode.instantValue.parameterType = BTParameterType.INT;
                    setVariableNode.instantValue.intValue = EditorGUILayout.IntField("Int Value", setVariableNode.instantValue.intValue);
                }
                else if (variableType == typeof(BT_Float))
                {
                    setVariableNode.instantValue.parameterType = BTParameterType.FLOAT;
                    setVariableNode.instantValue.floatValue = EditorGUILayout.FloatField("Float Value", setVariableNode.instantValue.floatValue);
                }
                else if (variableType == typeof(BT_Bool))
                {
                    setVariableNode.instantValue.parameterType = BTParameterType.BOOL;
                    setVariableNode.instantValue.boolValue = EditorGUILayout.Toggle("Bool Value", setVariableNode.instantValue.boolValue);
                }

                serializedObject.ApplyModifiedProperties();
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
