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
    [CustomNodeEditor(typeof(ParallelNode))]
    public class ParallelNodeEditor : BTNodeNodeEditor
    {
        private ParallelNode parallelNode;

        public override void OnBodyGUI()
        {

            if (parallelNode == null) parallelNode = target as ParallelNode;
            btNode = parallelNode;

            Color dColor = GUI.color;

            if (parallelNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (parallelNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (parallelNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 150;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("stopIfChildFails"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("stopIfChildSucceed"));

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("outputs"));

            EditorGUILayout.Space(2.5f);

            GUI.color = dColor;
        }


        public override int GetWidth()
        {
            return 250;
        }


    }
}