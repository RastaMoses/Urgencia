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
    [CustomNodeEditor(typeof(RandomSelectorNode))]
    public class RandomSelectorNodeEditor : BTNodeNodeEditor
    {
        private RandomSelectorNode selectorNode;

        public override void OnBodyGUI()
        {
            if (selectorNode == null) selectorNode = target as RandomSelectorNode;
            btNode = selectorNode;

            Color dColor = GUI.color;

            if (selectorNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (selectorNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (selectorNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 150;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));
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