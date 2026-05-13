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
    [CustomNodeEditor(typeof(BTNode))]
    public class BTNodeNodeEditor : NodeEditor
    {
        public BTNode btNode;
        protected bool foldout = false;

        public MonoScript[] allResultScripts;

        public override void OnBodyGUI()
        {
            if (btNode == null) btNode = target as BTNode;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 150;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            EditorGUILayout.Space(2.5f);
        }


        public override int GetWidth()
        {
            return 250;
        }

        public override void OnHeaderGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Color defaultColor = GUI.color;
            GUI.color = Color.yellow;
            if (btNode != null && btNode.indexInSequence != -1)
                GUILayout.Label("->[" + btNode.indexInSequence + "] ", NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
            GUI.color = defaultColor;
            GUILayout.Label(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

    }
}
