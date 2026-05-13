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
    [CustomNodeEditor(typeof(DReturnSuccess))]
    public class DReturnSuccessNodeEditor : BTNodeNodeEditor
    {
        private DReturnSuccess returnSuccessNode;

        public override void OnBodyGUI()
        {
            if (returnSuccessNode == null) returnSuccessNode = target as DReturnSuccess;
            btNode = returnSuccessNode;

            Color dColor = GUI.color;

            if (returnSuccessNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (returnSuccessNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (returnSuccessNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 100;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            /*
            foldout = EditorGUILayout.Foldout(foldout, "Show/Hide", true);
            EditorGUI.indentLevel++;
            if (foldout)
            {
                
            }
            EditorGUI.indentLevel--;
            */

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("output"));

            GUI.color = dColor;
        }

        public override int GetWidth()
        {
            return 200;
        }
    }
}
