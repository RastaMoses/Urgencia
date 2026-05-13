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
    [CustomNodeEditor(typeof(WaitNode))]
    public class WaitNodeEditor : BTNodeNodeEditor
    {
        private WaitNode waitNode;

        public override void OnBodyGUI()
        {
            if (waitNode == null) waitNode = target as WaitNode;
            btNode = waitNode;

            Color dColor = GUI.color;

            if (waitNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (waitNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (waitNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 80;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            foldout = EditorGUILayout.Foldout(foldout, "Show/Hide", true);
            EditorGUI.indentLevel++;

            if (foldout)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("did"));

                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("randomizeWaitTime"));

                if(!serializedObject.FindProperty("randomizeWaitTime").boolValue)
                    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("waitTime"));
                else
                {
                    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("minWait"));
                    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("maxWait"));
                }

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