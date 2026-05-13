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
    [CustomNodeEditor(typeof(AI_ChangeSteeringBehaviour))]
    public class AI_ChangeSteeringBehaviourNodeEditor : BTNodeNodeEditor
    {
        private AI_ChangeSteeringBehaviour thisNode;

        public override void OnBodyGUI()
        {
            if (thisNode == null) thisNode = target as AI_ChangeSteeringBehaviour;
            btNode = thisNode;

            Color dColor = GUI.color;

            if (thisNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (thisNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (thisNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 150;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            foldout = EditorGUILayout.Foldout(foldout, "Show/Hide", true);
            EditorGUI.indentLevel++;

            if (foldout)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("behaviour"));

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