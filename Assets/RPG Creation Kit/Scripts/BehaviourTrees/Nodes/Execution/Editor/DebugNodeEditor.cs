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
    [CustomNodeEditor(typeof(DebugNode))]
    public class DebugNodeEditor : BTNodeNodeEditor
    {
        private DebugNode debugNode;

        public override void OnBodyGUI()
        {
            if (debugNode == null) debugNode = target as DebugNode;
            btNode = debugNode;

            Color dColor = GUI.color;

            if (debugNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (debugNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (debugNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 50;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            foldout = EditorGUILayout.Foldout(foldout, "Show/Hide", true);
            EditorGUI.indentLevel++;

            if (foldout)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("log"));

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