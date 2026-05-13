using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
using RPGCreationKit.DialogueSystem;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit.DialogueSystem
{
    [CustomNodeEditor(typeof(CommentNode))]
    public class CommentNodeEditor : NodeEditor
    {
        private CommentNode lineNode;

        public override void OnBodyGUI()
        {
            if (lineNode == null) lineNode = target as CommentNode;

            // Update serialized object's representation
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 150;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("content"), GUIContent.none);

        }

    }
}