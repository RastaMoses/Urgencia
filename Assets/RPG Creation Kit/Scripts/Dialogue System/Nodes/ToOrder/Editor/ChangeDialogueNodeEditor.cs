using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
using RPGCreationKit.DialogueSystem;
using UnityEditor;
using RPGCreationKit;

[CustomNodeEditor(typeof(ChangeDialogueNode))]
public class ChangeDialogueNodeEditor : NodeEditor
{
    DialogueSystemConditionWindow myWindow;
    private ChangeDialogueNode conditionNode;
    public MonoScript[] allResultScripts;

    public override void OnBodyGUI()
    {
        serializedObject.Update();

        if (conditionNode == null) conditionNode = target as ChangeDialogueNode;
        if (allResultScripts == null) allResultScripts = NodesHelper.GetAllResultScripts<ResultScript>();


        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

        UnityEditor.EditorGUILayout.Space(2.5f);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Result Script:");

        var resultScript = serializedObject.FindProperty("resultScript");

        string resultScriptMenuValue = string.IsNullOrEmpty(resultScript.stringValue) ? "-None-" : resultScript.stringValue;
        if (GUILayout.Button(new GUIContent(resultScriptMenuValue, resultScriptMenuValue)))
        {
            // create the menu and add items to it
            GenericMenu menu = new GenericMenu();

            menu.AddDisabledItem(new GUIContent("Result Scripts"));

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("None"), false, Callback, new ScriptElementData(resultScript, ""));
            for (int i = 0; i < allResultScripts.Length; i++)
            {
                menu.AddItem(new GUIContent(allResultScripts[i].name), (allResultScripts[i].GetClass().Namespace + "." + allResultScripts[i].name) == resultScriptMenuValue, Callback, new ScriptElementData(resultScript, allResultScripts[i].GetClass().Namespace + "." + allResultScripts[i].name));
            }

            menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2.5f);

        float defaultLabelWidth = EditorGUIUtility.labelWidth;

        EditorGUIUtility.labelWidth = 130;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("events"));
        EditorGUIUtility.labelWidth = defaultLabelWidth;

        EditorGUILayout.Space(2.5f);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("newDialogue"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onNPCWhosSpeaking"));

        EditorGUILayout.Space(2.5f);

        if (!serializedObject.FindProperty("onNPCWhosSpeaking").boolValue)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("npcRef"));

        EditorGUILayout.Space(2.5f);

        if (serializedObject.FindProperty("onNPCWhosSpeaking").boolValue)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startDialogueImmediatly"), new GUIContent("Start Dialogue", "Starts the new dialogue as soon as this node gets processed."));
        

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("output"));


        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }

    void Callback(object obj)
    {
        ScriptElementData questScriptData = (ScriptElementData)obj;
        questScriptData.property.stringValue = questScriptData.value;

        questScriptData.property.serializedObject.ApplyModifiedProperties();
    }

    public override int GetWidth()
    {
        if (serializedObject.FindProperty("events").isExpanded &&
            serializedObject.FindProperty("events").FindPropertyRelative("consequences").isExpanded)
            return 350;
        else
            return 250;
    }
}