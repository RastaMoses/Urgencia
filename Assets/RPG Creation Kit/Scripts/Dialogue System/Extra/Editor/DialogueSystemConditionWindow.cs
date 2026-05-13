using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using RPGCreationKit;
using System.IO;
using System.Reflection;
using System.Linq;
using RPGCreationKit.DialogueSystem;

namespace RPGCreationKit.DialogueSystem
{
    public class DialogueSystemConditionWindow : EditorWindow
    {
        public bool isReady = false;

        public SerializedObject conditionNodeObj;

        GUIStyle guiLayoutDarkColor = new GUIStyle();
        GUIStyle guiLayoutLightColor = new GUIStyle();

        Vector2 questConditionsScrollView = Vector2.zero;
        int selectedQuestConditionIndex = -1;

        public void Init(SerializedObject _item)
        {
            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.QuestWindowIcon);
            GUIContent titleContent = new GUIContent("Node Condition", icon);
            this.titleContent = titleContent;

            // We copy the Item SerializedObject to not lose reference.
            SerializedObject itemcopy = new SerializedObject(_item.targetObject);
            conditionNodeObj = itemcopy;

            guiLayoutDarkColor.normal.background = MakeTex(600, 1, new Color(5.0f, 5.0f, 5.0f, .1f));
            guiLayoutLightColor.normal.background = MakeTex(600, 1, new Color(37.0f, 79.0f, 133.0f, .4f));

            isReady = true;
            this.ShowModal();
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        private void OnGUI()
        {
            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            ButtonStyle.border = new RectOffset(2, 2, 2, 2);
            ButtonStyle.fontSize = 16;
            ButtonStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            EditorGUILayout.BeginVertical("Box");
            Event current = Event.current;

            EditorGUILayout.LabelField("NODE CONDITIONS:", EditorStyles.boldLabel);

            questConditionsScrollView =
                EditorGUILayout.BeginScrollView(questConditionsScrollView, true, true, GUILayout.Width(position.width), GUILayout.Height(400));

            Rect questConditionRectArea = new Rect(questConditionsScrollView.x, questConditionsScrollView.y, position.width, 400);


            for (int i = 0; i < conditionNodeObj.FindProperty("conditions").arraySize; i++)
            {
                Rect currentConditionArea = EditorGUILayout.BeginHorizontal((selectedQuestConditionIndex == i) ? guiLayoutLightColor : guiLayoutDarkColor);
                EditorGUILayout.PropertyField(conditionNodeObj.FindProperty("conditions").GetArrayElementAtIndex(i), GUILayout.Width(1500));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10);

                if (currentConditionArea.Contains(current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    selectedQuestConditionIndex = i;
                    Repaint();
                }
            }

            // DROPDOWN TO CREATE NEW CONDITIONS
            if (questConditionRectArea.Contains(current.mousePosition) && current.type == EventType.ContextClick)
            {
                GenericMenu menu = new GenericMenu();

                menu.AddDisabledItem(new GUIContent("Quests Conditions"));
                menu.AddItem(new GUIContent("New"), false, CreateNewQuestCondition);
                menu.AddItem(new GUIContent("Delete Selected"), false, DeleteSelectedQuestCondition);


                menu.ShowAsContext();

                current.Use();
            }

            void CreateNewQuestCondition()
            {
                conditionNodeObj.FindProperty("conditions").InsertArrayElementAtIndex(conditionNodeObj.FindProperty("conditions").arraySize);
                selectedQuestConditionIndex = conditionNodeObj.FindProperty("conditions").arraySize - 1;
                Repaint();
            }

            void DeleteSelectedQuestCondition()
            {
                if (selectedQuestConditionIndex != -1)
                {
                    conditionNodeObj.FindProperty("conditions").DeleteArrayElementAtIndex(selectedQuestConditionIndex);
                    selectedQuestConditionIndex = -1;
                    Repaint();
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal("box", GUILayout.MaxWidth(150));

            GUI.enabled = (selectedQuestConditionIndex != -1) ? true : false;

            GUI.enabled = (selectedQuestConditionIndex != -1 && selectedQuestConditionIndex != 0);
            if (GUILayout.Button("↑"))
            {
                GUI.FocusControl(null);
                conditionNodeObj.FindProperty("conditions").MoveArrayElement(selectedQuestConditionIndex, selectedQuestConditionIndex - 1);
                selectedQuestConditionIndex = selectedQuestConditionIndex - 1;
                Repaint();
            }

            GUI.enabled = (selectedQuestConditionIndex != -1 && selectedQuestConditionIndex != conditionNodeObj.FindProperty("conditions").arraySize - 1);
            if (GUILayout.Button("↓"))
            {
                GUI.FocusControl(null);
                conditionNodeObj.FindProperty("conditions").MoveArrayElement(selectedQuestConditionIndex, selectedQuestConditionIndex + 1);
                selectedQuestConditionIndex = selectedQuestConditionIndex + 1;
                Repaint();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("SAVE", ButtonStyle))
            {
                conditionNodeObj.ApplyModifiedProperties();
                this.Close();
            }

            if (GUILayout.Button("CANCEL", ButtonStyle))
            {
                this.Close();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}