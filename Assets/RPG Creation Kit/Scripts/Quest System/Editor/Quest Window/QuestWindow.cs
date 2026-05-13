using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using RPGCreationKit;
using System.IO;
using System.Reflection;
using System.Linq;

namespace RPGCreationKit
{
    public class QuestWindow : EditorWindow
    {
        public bool isReady = false;
        public SerializedObject questObj;

        // Navigation
        public enum CurrentWindow { Quest, QuestStages, NPCs, Dialogues };
        CurrentWindow m_Window;

        // Buttons scroll
        Vector2 buttonsScrollPos;

        Vector2 indexesScrollView;

        GUIStyle guiLayoutDarkColor = new GUIStyle();
        GUIStyle guiLayoutLightColor = new GUIStyle();
        //  Quest Conditions
        Vector2 questConditionsScrollView = Vector2.zero;
        int selectedQuestConditionIndex = -1;

        //  Quest Stage Conditions 
        Vector2 questStageConditionsScrollView = Vector2.zero;
        int selectedQuestStageConditionIndex = -1;

        bool isCreatingNewStage = false;

        bool hasSelectedStage = false;
        int selectedStage = 0;

        MonoScript[] allQuestScripts;
        MonoScript[] allQuestStagesScripts;


        void Callback(object obj)
        {
            ScriptElementData questScriptData = (ScriptElementData)obj;

            questScriptData.property.stringValue = questScriptData.value;
        }

        
        public void Init(SerializedObject _quest)
        {
            // Windows is created from 'Configure' button of the Inspector of the Quest

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.QuestWindowIcon);
            GUIContent titleContent = new GUIContent("Quest Window", icon);
            this.titleContent = titleContent;

            // We copy the Item SerializedObject to not lose reference.
            SerializedObject itemcopy = new SerializedObject(_quest.targetObject);
            questObj = itemcopy;

            guiLayoutDarkColor.normal.background = MakeTex(600, 1, new Color(5.0f, 5.0f, 5.0f, .1f));
            guiLayoutLightColor.normal.background = MakeTex(600, 1, new Color(37.0f, 79.0f, 133.0f, .4f));

            allQuestScripts = NodesHelper.GetAllResultScripts<QuestScript>();
            allQuestStagesScripts = NodesHelper.GetAllResultScripts<QuestStageScript>();

            isReady = true;
            this.Show();
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

        public void OnGUI()
        {
            if (!questObj.targetObject)
            {
                Debug.LogWarning("ItemObj: NullReferenceException");
                return;
            }


            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            ButtonStyle.border = new RectOffset(2,2,2,2);
            ButtonStyle.fontSize = 16;
            ButtonStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            GUIStyle HeaderStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            HeaderStyle.fontSize = 14;
            HeaderStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Configuring: " + questObj.targetObject.name, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            // Scroll View Of Buttons:
            buttonsScrollPos =
                EditorGUILayout.BeginScrollView(buttonsScrollPos, GUILayout.Width(position.width), GUILayout.Height(75));
            GUILayout.BeginHorizontal();
            GUILayout.Space(212);
            GUILayout.BeginHorizontal("box");

            #region NavigationMenu
            //  Buttons for navigation

            var defaultColor = GUI.color;

            if (m_Window == CurrentWindow.Quest)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Quests", GUILayout.Width(125), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.Quest;
            }

            if (m_Window == CurrentWindow.QuestStages)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Quest Stages", GUILayout.Width(125), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.QuestStages;
            }

            if (m_Window == CurrentWindow.NPCs)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("NPCs", GUILayout.Width(125), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.NPCs;
            }

            if (m_Window == CurrentWindow.Dialogues)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Dialogues", GUILayout.Width(125), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.Dialogues;
            }
            GUILayout.EndHorizontal();

            GUI.color = defaultColor;

            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            #endregion


            switch (m_Window)
            {
                case CurrentWindow.Quest:
                    #region code
                    EditorGUILayout.BeginVertical("Box");

                    EditorGUILayout.LabelField("QUEST SETTINGS:", HeaderStyle);

                    EditorGUILayout.PropertyField(questObj.FindProperty("questName"));
                    EditorGUILayout.PropertyField(questObj.FindProperty("questID"));
                    EditorGUILayout.PropertyField(questObj.FindProperty("questXpReward"));

                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(questObj.FindProperty("questDescription"));


                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(questObj.FindProperty("questPriority"));

                    //EditorGUILayout.PropertyField(questObj.FindProperty("questScript"));

                    // display the GenericMenu when pressing a button

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Quest Script:");

                    string menuDisplayValue = string.IsNullOrEmpty(questObj.FindProperty("questScript").stringValue) ? "-None-" : questObj.FindProperty("questScript").stringValue;
                    if (GUILayout.Button(menuDisplayValue))
                    {
                        // create the menu and add items to it
                        GenericMenu menu = new GenericMenu();

                        menu.AddDisabledItem(new GUIContent("Quest Scripts"));

                        menu.AddSeparator("");

                        menu.AddItem(new GUIContent("None"), false, Callback, new ScriptElementData(questObj.FindProperty("questScript"), ""));
                        for (int i = 0; i < allQuestScripts.Length; i++)
                        {
                            menu.AddItem(new GUIContent(allQuestScripts[i].name), false, Callback, new ScriptElementData(questObj.FindProperty("questScript"), allQuestScripts[i].GetClass().Namespace+"."+allQuestScripts[i].name));
                        }

                        menu.ShowAsContext();
                    }

                    EditorGUILayout.EndHorizontal();


                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(questObj.FindProperty("firstStageIsDefaultStage"));
                    EditorGUILayout.PropertyField(questObj.FindProperty("allowRepeatedStages"));

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("Box");
                    Event current = Event.current;

                    EditorGUILayout.LabelField("QUEST CONDITIONS:", HeaderStyle);

                    questConditionsScrollView =
                        EditorGUILayout.BeginScrollView(questConditionsScrollView, true, true, GUILayout.Width(position.width), GUILayout.Height(340));

                    Rect questConditionRectArea = new Rect(questConditionsScrollView.x, questConditionsScrollView.y, position.width, 340);


                    for (int i = 0; i < questObj.FindProperty("questConditions").arraySize; i++)
                    {
                        Rect currentConditionArea = EditorGUILayout.BeginHorizontal((selectedQuestConditionIndex == i) ? guiLayoutLightColor : guiLayoutDarkColor);
                        EditorGUILayout.PropertyField(questObj.FindProperty("questConditions").GetArrayElementAtIndex(i), GUILayout.Width(1500));
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
                        questObj.FindProperty("questConditions").InsertArrayElementAtIndex(questObj.FindProperty("questConditions").arraySize);
                        selectedQuestConditionIndex = questObj.FindProperty("questConditions").arraySize - 1;
                        Repaint();
                    }

                    void DeleteSelectedQuestCondition()
                    {
                        if (selectedQuestConditionIndex != -1)
                        {
                            questObj.FindProperty("questConditions").DeleteArrayElementAtIndex(selectedQuestConditionIndex);
                            selectedQuestConditionIndex = -1;
                            Repaint();
                        }
                    }

                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.BeginHorizontal("box", GUILayout.MaxWidth(150));

                    GUI.enabled = (selectedQuestConditionIndex != -1) ? true : false;

                    GUI.enabled = (selectedQuestConditionIndex != -1 && selectedQuestConditionIndex != 0);
                    if(GUILayout.Button("↑"))
                    {
                        GUI.FocusControl(null);
                        questObj.FindProperty("questConditions").MoveArrayElement(selectedQuestConditionIndex, selectedQuestConditionIndex - 1);
                        selectedQuestConditionIndex = selectedQuestConditionIndex - 1;
                        Repaint();
                    }

                    GUI.enabled = (selectedQuestConditionIndex != -1 && selectedQuestConditionIndex != questObj.FindProperty("questConditions").arraySize - 1);
                    if (GUILayout.Button("↓"))
                    {
                        GUI.FocusControl(null);
                        questObj.FindProperty("questConditions").MoveArrayElement(selectedQuestConditionIndex, selectedQuestConditionIndex + 1);
                        selectedQuestConditionIndex = selectedQuestConditionIndex + 1;
                        Repaint();
                    }

                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();


                    EditorGUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("SAVE", ButtonStyle))
                    {
                        questObj.ApplyModifiedProperties();

                        // Refresh database
                        var AllQuestDatabaseFiles = GetAllInstances<QuestsDatabaseFile>();

                        foreach (QuestsDatabaseFile file in AllQuestDatabaseFiles)
                            file.fill();

                        this.Close();
                    }

                    if (GUILayout.Button("CANCEL", ButtonStyle))
                    {
                        this.Close();
                    }

                    EditorGUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();

                    #endregion
                    break;

                case CurrentWindow.QuestStages:
                    #region code
                    EditorGUILayout.LabelField("      [QUEST STAGES]", HeaderStyle, GUILayout.MaxWidth(200.0f));

                    EditorGUILayout.BeginHorizontal();
                    // --- LEFT PANEL

                    indexesScrollView = EditorGUILayout.BeginScrollView(indexesScrollView, GUILayout.Width(210.0f), GUILayout.Height(550.0f));

                    Rect clickArea = EditorGUILayout.BeginVertical(guiLayoutDarkColor, GUILayout.Width(200.0f), GUILayout.Height(550.0f));

                    current = Event.current;

                    GUI.enabled = !isCreatingNewStage;
                    // Display all current quest stages
                    EditorGUILayout.BeginVertical();
                    for (int i = 0; i < questObj.FindProperty("questStages").arraySize; i++)
                    {
                        if (selectedStage == i && hasSelectedStage)
                            GUI.color = Color.green;

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Stage " + questObj.FindProperty("questStages").GetArrayElementAtIndex(i).FindPropertyRelative("index").intValue.ToString(), GUILayout.Width(150.0f)))
                        {
                            GUI.FocusControl(null);
                            selectedStage = i;
                            selectedQuestStageConditionIndex = -1;
                            hasSelectedStage = true;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUI.color = defaultColor;
                    }
                    GUI.enabled = true;


                    if (!isCreatingNewStage)
                    {
                        // DROPDOWN TO CREATE NEW STAGES
                        if (clickArea.Contains(current.mousePosition) && current.type == EventType.ContextClick)
                        {
                            GenericMenu menu = new GenericMenu();

                            menu.AddDisabledItem(new GUIContent("Quest Stages"));
                            menu.AddSeparator("");

                            menu.AddItem(new GUIContent("New"), false, CreateNew);
                            menu.AddItem(new GUIContent("Delete Selected"), false, DeleteSelected);

                            menu.ShowAsContext();

                            current.Use();
                        }
                    } else
                    {
                        int newStageIndex = 0;

                        EditorGUI.BeginChangeCheck();
                        GUI.SetNextControlName("NewStageField");
                        newStageIndex = EditorGUILayout.DelayedIntField(newStageIndex);
                        GUI.FocusControl("NewStageField");

                        Event e = Event.current;
                        switch (e.type)
                        {
                            case EventType.KeyDown:
                                if (current.keyCode == KeyCode.Escape)
                                {
                                    isCreatingNewStage = false;
                                    Repaint();
                                }
                                break;
                        }

                        // IF CREATE NEW ELEMENT
                        if (EditorGUI.EndChangeCheck())
                        {
                            //check if the selected stage index doesn't already exist
                            bool newStageAlreadyExist = false;
                            for(int i = 0; i < questObj.FindProperty("questStages").arraySize; i++)
                            {
                                if (questObj.FindProperty("questStages").GetArrayElementAtIndex(i).FindPropertyRelative("index").intValue == newStageIndex)
                                {
                                    newStageAlreadyExist = true;
                                    break;
                                }
                            }

                            // If the new stage already exist set it to a non-used stage (the last one +1)
                            if (newStageAlreadyExist)
                                newStageIndex = questObj.FindProperty("questStages").GetArrayElementAtIndex(questObj.FindProperty("questStages").arraySize - 1).FindPropertyRelative("index").intValue +1;

                            questObj.FindProperty("questStages").InsertArrayElementAtIndex(questObj.FindProperty("questStages").arraySize);
                            questObj.FindProperty("questStages").GetArrayElementAtIndex(questObj.FindProperty("questStages").arraySize - 1).FindPropertyRelative("index").intValue = newStageIndex;

                            // Order Array
                            bool triggered = false; // for selecting the corrent stage 
                            for(int i = 0; i < questObj.FindProperty("questStages").arraySize; i++)
                            {
                                if(questObj.FindProperty("questStages").GetArrayElementAtIndex(questObj.FindProperty("questStages").arraySize - 1).FindPropertyRelative("index").intValue <
                                    questObj.FindProperty("questStages").GetArrayElementAtIndex(i).FindPropertyRelative("index").intValue)
                                {
                                    questObj.FindProperty("questStages").MoveArrayElement(questObj.FindProperty("questStages").arraySize - 1, i);

                                    selectedStage = i;
                                    triggered = true;
                                }
                                else if(!triggered)
                                    selectedStage = questObj.FindProperty("questStages").arraySize - 1;
                            }

                            isCreatingNewStage = false;
                        }
                    }

                    void CreateNew()
                    {
                        isCreatingNewStage = true;
                        selectedQuestStageConditionIndex = -1;
                    }

                    void DeleteSelected()
                    {
                        if (hasSelectedStage)
                        {
                            questObj.FindProperty("questStages").DeleteArrayElementAtIndex(selectedStage);
                            hasSelectedStage = false;
                        }
                    }

                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginHorizontal();


                    if (hasSelectedStage)
                    {
                        EditorGUILayout.BeginVertical("box", GUILayout.MaxHeight(360));

                        EditorGUILayout.BeginVertical(guiLayoutDarkColor);
                        EditorGUILayout.LabelField("[QUEST STAGE : " + questObj.FindProperty("questStages").GetArrayElementAtIndex(selectedStage).FindPropertyRelative("index").intValue + "]", HeaderStyle, GUILayout.MaxWidth(400.0f));

                        EditorGUILayout.PropertyField(questObj.FindProperty("questStages").GetArrayElementAtIndex(selectedStage).FindPropertyRelative("description"));

                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 100;

                        EditorGUILayout.PropertyField(questObj.FindProperty("questStages").GetArrayElementAtIndex(selectedStage).FindPropertyRelative("completeQuest"));

                        EditorGUIUtility.labelWidth = 60;
                        EditorGUILayout.PropertyField(questObj.FindProperty("questStages").GetArrayElementAtIndex(selectedStage).FindPropertyRelative("failQuest"));

                        EditorGUIUtility.labelWidth = 110;
                        EditorGUILayout.PropertyField(questObj.FindProperty("questStages").GetArrayElementAtIndex(selectedStage).FindPropertyRelative("displayLogEntry"));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();

                        GUILayout.Space(15);
                        var curResultScript = questObj.FindProperty("questStages").GetArrayElementAtIndex(selectedStage).FindPropertyRelative("resultScript");

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Stage Script:");

                        string resultScriptMenuValue = string.IsNullOrEmpty(curResultScript.stringValue) ? "-None-" : curResultScript.stringValue;
                        if (GUILayout.Button(resultScriptMenuValue))
                        {
                            // create the menu and add items to it
                            GenericMenu menu = new GenericMenu();

                            menu.AddDisabledItem(new GUIContent("Quest Stages Scripts"));

                            menu.AddSeparator("");

                            menu.AddItem(new GUIContent("None"), false, Callback, new ScriptElementData(curResultScript, ""));
                            for (int i = 0; i < allQuestStagesScripts.Length; i++)
                            {
                                menu.AddItem(new GUIContent(allQuestStagesScripts[i].name), false, Callback, new ScriptElementData(curResultScript, allQuestStagesScripts[i].GetClass().Namespace + "." + allQuestStagesScripts[i].name));
                            }

                            menu.ShowAsContext();
                        }
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(15);

                        EditorGUILayout.BeginVertical(guiLayoutDarkColor);
                        current = Event.current;

                        EditorGUILayout.LabelField("QUEST STAGE CONDITIONS:", HeaderStyle);

                        questStageConditionsScrollView =
                            EditorGUILayout.BeginScrollView(questStageConditionsScrollView, true, true, GUILayout.Width(position.width - 230), GUILayout.Height(360));

                        Rect questStageConditionRectArea = new Rect(questStageConditionsScrollView.x, questStageConditionsScrollView.y, position.width, 400);

                        var questStages = questObj.FindProperty("questStages").GetArrayElementAtIndex(selectedStage);

                        for (int i = 0; i < questStages.FindPropertyRelative("stageConditions").arraySize; i++)
                        {
                            Rect currentConditionArea = EditorGUILayout.BeginHorizontal((selectedQuestStageConditionIndex == i) ? guiLayoutLightColor : guiLayoutDarkColor);
                            EditorGUILayout.PropertyField(questStages.FindPropertyRelative("stageConditions").GetArrayElementAtIndex(i), GUILayout.Width(1500));
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.Space(10);

                            if (currentConditionArea.Contains(current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                            {
                                selectedQuestStageConditionIndex = i;
                                Repaint();
                            }
                        }

                        // DROPDOWN TO CREATE NEW CONDITIONS
                        if (questStageConditionRectArea.Contains(current.mousePosition) && current.type == EventType.ContextClick)
                        {
                            GenericMenu menu = new GenericMenu();

                            menu.AddDisabledItem(new GUIContent("Quest Stage Conditions"));
                            menu.AddItem(new GUIContent("New"), false, CreateNewQuestStageCondition);
                            menu.AddItem(new GUIContent("Delete Selected"), false, DeleteSelectedQuestStageCondition);

                            menu.ShowAsContext();

                            current.Use();
                        }

                        void CreateNewQuestStageCondition()
                        {
                            questStages.FindPropertyRelative("stageConditions").InsertArrayElementAtIndex(questStages.FindPropertyRelative("stageConditions").arraySize);
                            selectedQuestStageConditionIndex = questStages.FindPropertyRelative("stageConditions").arraySize - 1;
                            Repaint();
                        }

                        void DeleteSelectedQuestStageCondition()
                        {
                            if (selectedQuestStageConditionIndex != -1)
                            {
                                questStages.FindPropertyRelative("stageConditions").DeleteArrayElementAtIndex(selectedQuestStageConditionIndex);
                                selectedQuestStageConditionIndex = -1;
                                Repaint();
                            }
                        }

                        EditorGUILayout.EndScrollView();

                        EditorGUILayout.BeginHorizontal("box", GUILayout.MaxWidth(150));

                        GUI.enabled = (selectedQuestStageConditionIndex != -1) ? true : false;

                        GUI.enabled = (selectedQuestStageConditionIndex != -1 && selectedQuestStageConditionIndex != 0);
                        if (GUILayout.Button("↑"))
                        {
                            GUI.FocusControl(null);
                            questStages.FindPropertyRelative("stageConditions").MoveArrayElement(selectedQuestStageConditionIndex, selectedQuestStageConditionIndex - 1);
                            selectedQuestStageConditionIndex = selectedQuestStageConditionIndex - 1;
                            Repaint();
                        }

                        GUI.enabled = (selectedQuestStageConditionIndex != -1 && selectedQuestStageConditionIndex != questStages.FindPropertyRelative("stageConditions").arraySize - 1);
                        if (GUILayout.Button("↓"))
                        {
                            GUI.FocusControl(null);
                            questStages.FindPropertyRelative("stageConditions").MoveArrayElement(selectedQuestStageConditionIndex, selectedQuestStageConditionIndex + 1);
                            selectedQuestStageConditionIndex = selectedQuestStageConditionIndex + 1;
                            Repaint();
                        }

                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        GUILayout.FlexibleSpace();

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndVertical();
                    }


                    GUILayout.FlexibleSpace();

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(30);

                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("SAVE", ButtonStyle))
                    {
                        questObj.ApplyModifiedProperties();

                        // Refresh database
                        var AllQuestDatabaseFiles = GetAllInstances<QuestsDatabaseFile>();

                        foreach (QuestsDatabaseFile file in AllQuestDatabaseFiles)
                            file.fill();

                        this.Close();
                    }

                    if (GUILayout.Button("CANCEL", ButtonStyle))
                    {
                        this.Close();
                    }

                    EditorGUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();


                    #endregion
                    break;

                case CurrentWindow.NPCs:                                    
                    break;

                case CurrentWindow.Dialogues:
                    break;

                default:
                    break;
            }
        }


        public static MonoScript[] GetScriptAssetsOfType<T>()
        {
            MonoScript[] scripts = (MonoScript[])FindObjectsOfTypeIncludingAssets(typeof(MonoScript));

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

        public static List<T> GetAllInstances<T>() where T : QuestsDatabaseFile
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            List<T> a = new List<T>(guids.Length);

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a.Add(AssetDatabase.LoadAssetAtPath<T>(path));
            }

            return a;
        }
    }
}