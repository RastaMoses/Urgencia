using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
using RPGCreationKit.BehaviourTree;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit.BehaviourTree
{
    [CustomNodeEditor(typeof(ActionNode))]
    public class ActionNodeEditor : BTNodeNodeEditor
    {
        private ActionNode actionNode;

        public override void OnBodyGUI()
        {
            if (actionNode == null) actionNode = target as ActionNode;
            btNode = actionNode;

            Color dColor = GUI.color;

            if (actionNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (actionNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (actionNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 150;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            foldout = EditorGUILayout.Foldout(foldout, "Show/Hide", true);
            EditorGUI.indentLevel++;

            if (foldout)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("executionGoesWell"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("debugs"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("timer"));

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