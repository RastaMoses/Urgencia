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
    [CustomNodeEditor(typeof(ARandomIntNode))]
    public class ARandomIntNodeEditor : BTNodeNodeEditor
    {
        private ARandomIntNode actionNode;
        bool foldout = true;

        public override void OnBodyGUI()
        {
            if (actionNode == null) actionNode = target as ARandomIntNode;
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
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("min"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("max"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("inclusive"));
            EditorGUILayout.Space(5);


            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("useVariable"));

            if (serializedObject.FindProperty("useVariable").boolValue)
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("storedValue"));
            else
                actionNode.instantValue = EditorGUILayout.DelayedIntField("Instant Value:", actionNode.instantValue);



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