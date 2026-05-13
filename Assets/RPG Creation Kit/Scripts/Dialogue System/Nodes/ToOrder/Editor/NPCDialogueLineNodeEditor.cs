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
    [CustomNodeEditor(typeof(NPCDialogueLineNode))]
    public class NPCDialogueLineNodeEditor : DialogueNodeEditor
    {
        private NPCDialogueLineNode lineNode;

        public MonoScript[] allResultScripts;

        public override void OnBodyGUI()
        {
            base.OnBodyGUI();

            serializedObject.Update();

            if (lineNode == null) lineNode = target as NPCDialogueLineNode;
            if (allResultScripts == null) allResultScripts = NodesHelper.GetAllResultScripts<ResultScript>();


            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            foldout = EditorGUILayout.Foldout(foldout, "Show/Hide", true);

            if (!foldout)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("line"));

            EditorGUI.indentLevel++;

            if (foldout)
            {
                if (((DialogueGraph)target.graph).GetEntryNode().isNpcToNpcDialogue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("speakerID"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lookAtEntityID"));

                }

                EditorGUILayout.Space(2.5f);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("line"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("plainLine"));


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Talking Anim:");

                var dialogueAnimation = serializedObject.FindProperty("dialogueAnimationStr");

                // Draw anim
                string animMenuValue = string.IsNullOrEmpty(dialogueAnimation.stringValue) ? "-None-" : dialogueAnimation.stringValue;
                if (GUILayout.Button(animMenuValue))
                {
                    // create the menu and add items to it
                    GenericMenu menu = new GenericMenu();

                    menu.AddDisabledItem(new GUIContent("Dialogue Anims"));

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("None"), false, Callback, new ScriptElementData(dialogueAnimation, ""));
                    for (int i = 0; i < NPCDialogueLineNode.DIALOGUE_ANIMATIONS.Length; i++)
                    {
                        bool curActive = dialogueAnimation.stringValue == NPCDialogueLineNode.DIALOGUE_ANIMATIONS[i];
                        menu.AddItem(new GUIContent(NPCDialogueLineNode.DIALOGUE_ANIMATIONS[i]), curActive, Callback2, new ScriptElementData(dialogueAnimation, NPCDialogueLineNode.DIALOGUE_ANIMATIONS[i]));
                    }

                    menu.ShowAsContext();
                }

                EditorGUILayout.EndHorizontal();

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

                EditorGUILayout.PropertyField(serializedObject.FindProperty("useLenghtOfClip"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("audioClip"));

                if (!serializedObject.FindProperty("useLenghtOfClip").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lineTime"));

                EditorGUILayout.Space(2.5f);

            }
            EditorGUI.indentLevel--;


            Color defaultColor = GUI.backgroundColor;
            if (serializedObject.FindProperty("afterLine").enumValueIndex == (int)AfterLine.EndDialogue)
                GUI.backgroundColor = Color.red;


            EditorGUILayout.PropertyField(serializedObject.FindProperty("afterLine"));

            GUI.backgroundColor = defaultColor;

            if (serializedObject.FindProperty("afterLine").enumValueIndex == (int)AfterLine.NPC_DialogueLine)
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("nextLine"));

            if (serializedObject.FindProperty("afterLine").enumValueIndex == (int)AfterLine.PlayerQuestions)
            {
                EditorGUILayout.Space(7.5f);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Listening Anim:");

                var dialogueAnimation = serializedObject.FindProperty("dialogueAnimationListeningStr");

                // Draw anim
                string animMenuValue = string.IsNullOrEmpty(dialogueAnimation.stringValue) ? "-None-" : dialogueAnimation.stringValue;
                if (GUILayout.Button(new GUIContent(animMenuValue, animMenuValue)))
                {
                    // create the menu and add items to it
                    GenericMenu menu = new GenericMenu();

                    menu.AddDisabledItem(new GUIContent("Dialogue Anims"));

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("None"), false, Callback, new ScriptElementData(dialogueAnimation, ""));
                    for (int i = 0; i < NPCDialogueLineNode.DIALOGUE_ANIMATIONS.Length; i++)
                    {
                        menu.AddItem(new GUIContent(NPCDialogueLineNode.DIALOGUE_ANIMATIONS[i]), false, Callback2, new ScriptElementData(dialogueAnimation, NPCDialogueLineNode.DIALOGUE_ANIMATIONS[i]));
                    }

                    menu.ShowAsContext();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("removePreviousQuestions"));

                if (!serializedObject.FindProperty("removePreviousQuestions").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("questionsToRemove"));

                EditorGUILayout.Space(7.5f);

                NodeEditorGUILayout.DynamicPortList("playerQuestions", typeof(NPCDialogueLineNode), serializedObject, NodePort.IO.Output, Node.ConnectionType.Override);
            }

            if (serializedObject.FindProperty("afterLine").enumValueIndex == (int)AfterLine.Continue)
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("output"));


            GUI.enabled = true;

            EditorGUILayout.Space(2.5f);

            serializedObject.ApplyModifiedProperties();

        }


        public static MonoScript[] GetScriptAssetsOfType<T>()
        {
            MonoScript[] scripts = (MonoScript[])Object.FindObjectsOfTypeIncludingAssets(typeof(MonoScript));

            List<MonoScript> result = new List<MonoScript>();

            foreach (MonoScript m in scripts)
            {
                if (m.GetClass() != null && m.GetClass().IsSubclassOf(typeof(T)) && m.GetType() != typeof(Shader))
                {
                    result.Add(m);
                }
            }
            return result.ToArray();
        }

        void Callback(object obj)
        {
            ScriptElementData questScriptData = (ScriptElementData)obj;
            questScriptData.property.stringValue = questScriptData.value;

            questScriptData.property.serializedObject.ApplyModifiedProperties();
        }

        void Callback2(object obj)
        {
            ScriptElementData questScriptData = (ScriptElementData)obj;
            questScriptData.property.stringValue = questScriptData.value;

            questScriptData.property.serializedObject.ApplyModifiedProperties();
        }


        public override int GetWidth()
        {
            if (serializedObject.FindProperty("events").isExpanded && foldout)
                return 450;
            else
                return 250;
        }

    }
}