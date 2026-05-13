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
    [CustomNodeEditor(typeof(DRepeaterNode))]
    public class DRepeaterNodeEditor : BTNodeNodeEditor
    {
        private DRepeaterNode repeaterNode;

        public override void OnBodyGUI()
        {
            if (repeaterNode == null) repeaterNode = target as DRepeaterNode;
            btNode = repeaterNode;

            Color dColor = GUI.color;

            
            if (repeaterNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (repeaterNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (repeaterNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;
            

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 100;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            foldout = EditorGUILayout.Foldout(foldout, "Show/Hide", true);
            EditorGUI.indentLevel++;
            if (foldout)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("repeatForever"));

                if(!serializedObject.FindProperty("repeatForever").boolValue)
                {
                    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("count"));
                }

                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("endOnFail"));

                EditorGUILayout.Space(2.5f);
            }
            EditorGUI.indentLevel--;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("output"));

            GUI.color = dColor;
        }

        public override int GetWidth()
        {
            return 200;
        }
    }
}
