using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
using UnityEditor;
using RPGCreationKit;
using RPGCreationKit.DialogueSystem;

namespace RPGCreationKit.DialogueSystem
{
    [CustomNodeEditor(typeof(DialogueNode))]
    public class DialogueNodeEditor : NodeEditor
    {
        public DialogueNode dNode;
        protected bool foldout = false;

        public MonoScript[] allResultScripts;

        public override void OnBodyGUI()
        {
            if (dNode == null) dNode = target as DialogueNode;

            EditorGUIUtility.labelWidth = 150;

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
            GUILayout.Label(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
