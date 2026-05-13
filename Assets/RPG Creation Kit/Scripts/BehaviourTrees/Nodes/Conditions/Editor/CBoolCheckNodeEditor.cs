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
    [CustomNodeEditor(typeof(CBoolCheckNode))]
    public class CBoolCheckNodeEditor : BTNodeNodeEditor
    {
        private CBoolCheckNode boolCheckNode;

        public override void OnBodyGUI()
        {
            if (boolCheckNode == null) boolCheckNode = target as CBoolCheckNode;
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

            EditorGUIUtility.labelWidth = 60;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            foldout = EditorGUILayout.Foldout(foldout, "Show/Hide", true);
            EditorGUI.indentLevel++;
            if (foldout)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("not"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("boolToCheck"));

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