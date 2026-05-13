using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
using RPGCreationKit.DialogueSystem;
using UnityEditor;

[CustomNodeEditor(typeof(ConditionsNode))]
public class ConditionsNodeEditor : NodeEditor
{
    DialogueSystemConditionWindow myWindow;
    private ConditionsNode conditionNode;

    public override void OnBodyGUI()
    {
        if (conditionNode == null) conditionNode = target as ConditionsNode;

        // Update serialized object's representation
        serializedObject.Update();

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

        UnityEditor.EditorGUILayout.Space(2.5f);

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("comment"), GUIContent.none);
        UnityEditor.EditorGUILayout.Space(2.5f);

        if (UnityEngine.GUILayout.Button("Configure Conditions"))
        {
            var AllWindows = Resources.FindObjectsOfTypeAll<DialogueSystemConditionWindow>();
            bool winFound = false;

            for (int i = 0; i < AllWindows.Length; i++)
            {
                winFound = true;
                AllWindows[i].Focus();
            }


            if (!winFound)
            {
                myWindow = UnityEditor.Editor.CreateInstance<DialogueSystemConditionWindow>();
                myWindow.minSize = new Vector2(970, 520);
                myWindow.maxSize = new Vector2(970, 520);
                UnityEditor.EditorApplication.delayCall += DelayOpening;
            }
        }

        UnityEditor.EditorGUILayout.Space(2.5f);

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("resultTrue"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("resultFalse"));

        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }

    public void DelayOpening()
    {
        myWindow.Init(serializedObject);
    }
}