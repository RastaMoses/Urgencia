using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
using RPGCreationKit.BehaviourTree;
using UnityEditor;
using RPGCreationKit;
using RPGCreationKit.BehaviourTree.Data;

namespace RPGCreationKit.BehaviourTree
{
    [CustomNodeEditor(typeof(CIntComparisonNode))]
    public class CIntComparisonNodeEditor : BTNodeNodeEditor
    {
        private CIntComparisonNode comparisonNode;

        public override void OnBodyGUI()
        {
            if (comparisonNode == null) comparisonNode = target as CIntComparisonNode;
            btNode = comparisonNode;

            Color dColor = GUI.color;

            if (comparisonNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (comparisonNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (comparisonNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 150;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            foldout = EditorGUILayout.Foldout(foldout, "Show/Hide", true);
            EditorGUI.indentLevel++;
            if (foldout)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("firstUseVariable"));

                if (serializedObject.FindProperty("firstUseVariable").boolValue)
                    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("firstStoredValue"));
                else
                    comparisonNode.firstInstantValue = EditorGUILayout.DelayedIntField("Instant Value:", comparisonNode.firstInstantValue);


                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("operation"));


                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("secondUseVariable"));

                if (serializedObject.FindProperty("secondUseVariable").boolValue)
                    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("secondStoredValue"));
                else
                    comparisonNode.secondInstantValue = EditorGUILayout.DelayedIntField("Instant Value:", comparisonNode.secondInstantValue);


                EditorGUILayout.Space(2.5f);
            }
            EditorGUI.indentLevel--;

            GUI.color = dColor;
        }

        public override int GetWidth()
        {
            return 250;
        }
    }
}