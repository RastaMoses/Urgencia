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
    [CustomNodeEditor(typeof(PlayerQuestionsNode))]
    public class PlayerQuestionsNodeEditor : NodeEditor
    {
        private PlayerQuestionsNode thisNode;

        public override void OnBodyGUI()
        {
            serializedObject.Update();

            if (thisNode == null) thisNode = target as PlayerQuestionsNode;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));


            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("removePreviousQuestions"));
            if (!serializedObject.FindProperty("removePreviousQuestions").boolValue)
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("questionsToRemove"));

            EditorGUILayout.Space(2.5f);

            NodeEditorGUILayout.DynamicPortList("playerQuestions", typeof(NPCDialogueLineNode), serializedObject, NodePort.IO.Output, Node.ConnectionType.Override);
        }
    }
}