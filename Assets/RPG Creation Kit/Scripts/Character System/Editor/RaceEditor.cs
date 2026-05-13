using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Window to create races.
    /// </summary>
    public class RaceEditor : EditorWindow
    {
        bool isReady = false;

        SerializedObject serializedObject;

        // Navigation
        public enum CurrentWindow { General, BodyData, FaceData, TextData, Factions };
        CurrentWindow m_Window;

        // Buttons scroll
        Vector2 indexesScrollView;
        Vector2 contentScrollView;

        Vector2 hairWindowScrollView;
        Vector2 eyeWindowScrollView;

        GUIStyle guiLayoutDarkColor = new GUIStyle();
        GUIStyle guiLayoutLightColor = new GUIStyle();

        List<Race> allRaces = new List<Race>();
        
        UnityEngine.Object malePrefab; // prefabs in the scene that are being edited
        UnityEngine.Object femalePrefab; // prefabs in the scene that are being edited
        string assetPathMale;
        string assetPathFemale;

        Race selectedRace;  // copy of the original race object
        bool hasSelectedRace = false;
        int selectedRaceIndex = -1;
        bool isCreatingNewRace = false;

        Texture refreshButtonIcon;

        public ReorderableList maleHairList;
        public ReorderableList femaleHairList;

        public ReorderableList eyeList;

        GameObject gameObject;
        Editor windowEditor;
        bool gameObjectChanged = false;

        bool maleSelected = true;

        // Customize face
        Vector2 faceBlendshapesContent;
        float sliderValue;
        int selectedBlendshapeIndex;

        struct HairSelection
        {
            public int index;
            public bool isMale;

            public HairSelection(int _index, bool _isMale)
            {
                index = _index;
                isMale = _isMale;
            }
        }

        struct EyeSelection
        {
            public int index;
            public bool isMale;

            public EyeSelection(int _index, bool _isMale)
            {
                index = _index;
                isMale = _isMale;
            }
        }

        [MenuItem("RPG Creation Kit/Race Editor")]
        public static void ShowWindow()
        {
            EditorWindow thisWindow = GetWindow(typeof(RaceEditor));

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RaceEditorIcon);
            GUIContent titleContent = new GUIContent("Race Editor", icon);
            thisWindow.titleContent = titleContent;

            thisWindow.minSize = new Vector2(865, 575);
            thisWindow.maxSize = new Vector2(865, 575);
        }

        void Init()
        {
            guiLayoutDarkColor.normal.background = MakeTex(600, 1, new Color(5.0f, 5.0f, 5.0f, .1f));
            guiLayoutLightColor.normal.background = MakeTex(600, 1, new Color(37.0f, 79.0f, 133.0f, .4f));
            refreshButtonIcon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RefreshButton);
            RefreshRaceList();
            isReady = true;
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

        public void RefreshRaceList()
        {
            allRaces = GetAllRaces<Race>();
            allRaces.Sort(SortByID);
        }


        private void OnSelectedRaceChanges()
        {
            selectedRace = allRaces[selectedRaceIndex];

            // Create a new object of this race
            serializedObject = new SerializedObject(allRaces[selectedRaceIndex]);
            RegenerateMalePrefabs();
            RegenerateFemalePrefabs();

            // Restore the lists
            maleHairList = new ReorderableList(serializedObject, serializedObject.FindProperty("maleHairTypes"), true, true, true, true);
            femaleHairList = new ReorderableList(serializedObject, serializedObject.FindProperty("femaleHairTypes"), true, true, true, true);

            eyeList = new ReorderableList(serializedObject, serializedObject.FindProperty("eyeTypes"), true, true, true, true);
        }

        private void RegenerateFemalePrefabs()
        {
            DestroyImmediate(windowEditor);
            
            if (femalePrefab != null)
                DestroyImmediate(((GameObject)femalePrefab).gameObject);

            if (serializedObject.FindProperty("femaleModel").objectReferenceValue != null)
            {
                assetPathFemale = AssetDatabase.GetAssetPath(((BodyData)serializedObject.FindProperty("femaleModel").objectReferenceValue).gameObject);
                femalePrefab = PrefabUtility.LoadPrefabContents(assetPathFemale);
                femalePrefab.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                EditorUtility.SetDirty(femalePrefab);
            }
            gameObjectChanged = true;
        }

        private void RegenerateMalePrefabs()
        {
            DestroyImmediate(windowEditor);

            // If there is an Editor instance of the body, destroy it
            if (malePrefab != null)
                DestroyImmediate(((GameObject)malePrefab).gameObject);

            // If the Races has models, spawn them as prefabs
            if (serializedObject.FindProperty("maleModel").objectReferenceValue != null)
            {
                assetPathMale = AssetDatabase.GetAssetPath(((BodyData)serializedObject.FindProperty("maleModel").objectReferenceValue).gameObject);

                malePrefab = PrefabUtility.LoadPrefabContents(assetPathMale);
                //malePrefab.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                EditorUtility.SetDirty(malePrefab);
            }

            gameObjectChanged = true;
        }

        private static int SortByID(Race r1, Race r2)
        {
            return r1.ID.CompareTo(r2.ID);
        }

        private void OnGUI()
        {
            if (!isReady)
                Init();


            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            ButtonStyle.border = new RectOffset(2, 2, 2, 2);
            ButtonStyle.fontSize = 16;
            ButtonStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            GUIStyle HeaderStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            HeaderStyle.fontSize = 14;
            HeaderStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(200.0f), GUILayout.ExpandWidth(false));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("RACES", HeaderStyle, GUILayout.MaxWidth(165.0f), GUILayout.ExpandWidth(false));

            if (GUILayout.Button(new GUIContent(refreshButtonIcon, "Refreshes the Race list."), GUILayout.MaxWidth(32)))
                RefreshRaceList();
                    
            EditorGUILayout.EndHorizontal();

            indexesScrollView = EditorGUILayout.BeginScrollView(indexesScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(200.0f), GUILayout.Height(500.0f));
            indexesScrollView.x = (indexesScrollView.x / 2);
            Rect clickArea = EditorGUILayout.BeginVertical(guiLayoutDarkColor, GUILayout.Width(200.0f), GUILayout.Height(500.0f));

            Event current = Event.current;

            GUI.enabled = !isCreatingNewRace;

            Color defaultColor = GUI.color;

            // Display all current quest stages
            for (int i = 0; i < allRaces.Count; i++)
            {
                if (selectedRaceIndex == i && hasSelectedRace)
                    GUI.color = Color.green;

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(allRaces[i].ID, GUILayout.MaxWidth(170.0f)))
                {
                    GUI.FocusControl(null);
                    selectedRaceIndex = i;
                    hasSelectedRace = true;
                    OnSelectedRaceChanges();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUI.color = defaultColor;
            }
            GUI.enabled = true;

            if (!isCreatingNewRace)
            {
                // DROPDOWN TO CREATE NEW STAGES
                if (clickArea.Contains(current.mousePosition) && current.type == EventType.ContextClick)
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddDisabledItem(new GUIContent("Races"));
                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("New"), false, CreateNew);
                    menu.AddItem(new GUIContent("Delete Selected"), false, DeleteSelected);

                    menu.ShowAsContext();

                    current.Use();
                }

                void CreateNew()
                {
                    Debug.Log("Creating new Race");
                }

                void DeleteSelected()
                {
                    if(hasSelectedRace)
                        Debug.Log("Deleting: " + selectedRace.ID);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal("box");

            #region NavigationMenu
            //  Buttons for navigation

            if (m_Window == CurrentWindow.General)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("General", GUILayout.Width(125), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.General;
            }

            if (m_Window == CurrentWindow.BodyData)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Body Data", GUILayout.Width(125), GUILayout.Height(20)))
            {
                if (m_Window != CurrentWindow.BodyData)
                    DestroyImmediate(windowEditor);

                m_Window = CurrentWindow.BodyData;
                gameObjectChanged = true;
            }

            if (m_Window == CurrentWindow.FaceData)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Face Data", GUILayout.Width(125), GUILayout.Height(20)))
            {
                if (m_Window != CurrentWindow.FaceData)
                    DestroyImmediate(windowEditor);

                m_Window = CurrentWindow.FaceData;
                gameObjectChanged = true;
            }

            if (m_Window == CurrentWindow.TextData)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Text Data", GUILayout.Width(125), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.TextData;
            }

            if (m_Window == CurrentWindow.Factions)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Factions", GUILayout.Width(125), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.Factions;
            }

            EditorGUILayout.EndHorizontal();
            GUI.color = defaultColor;
            #endregion

            if (hasSelectedRace && serializedObject != null)
            {
                // here goes the switch window
                switch (m_Window)
                {
                    case CurrentWindow.General:
                        contentScrollView = 
                            EditorGUILayout.BeginScrollView(contentScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(650.0f), GUILayout.Height(500.0f));

                        var raceID = serializedObject.FindProperty("ID");
                        var raceName = serializedObject.FindProperty("raceName");

                        var maleAtt = serializedObject.FindProperty("baseAttributesMale");
                        var femaleAtt = serializedObject.FindProperty("baseAttributesFemale");

                        var maleDerAtt = serializedObject.FindProperty("baseDerivedAttributesMale");
                        var femaleDerAtt = serializedObject.FindProperty("baseDerivedAttributesFemale");

                        EditorGUILayout.LabelField("BASE INFORMATION", HeaderStyle);

                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.PropertyField(raceID, GUILayout.Width(400));
                        EditorGUILayout.PropertyField(raceName, GUILayout.Width(400));
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.Space(5);

                        EditorGUILayout.LabelField("BASE ATTRIBUTES", HeaderStyle);
                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.BeginHorizontal("box");
                        EditorGUILayout.PropertyField(maleAtt, GUILayout.Width(200));
                        EditorGUILayout.PropertyField(femaleAtt, GUILayout.Width(200));
                        maleAtt.isExpanded = true;
                        femaleAtt.isExpanded = true;
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal("box");
                        EditorGUILayout.PropertyField(maleDerAtt, GUILayout.Width(200));
                        EditorGUILayout.PropertyField(femaleDerAtt, GUILayout.Width(200));
                        maleDerAtt.isExpanded = true;
                        femaleDerAtt.isExpanded = true;
                        EditorGUILayout.EndHorizontal();




                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndScrollView();
                        break;

                    case CurrentWindow.BodyData:
                        #region content
                        contentScrollView =
                            EditorGUILayout.BeginScrollView(contentScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(650.0f), GUILayout.Height(500.0f));

                        var maleBody = serializedObject.FindProperty("maleModel");
                        var femaleBody = serializedObject.FindProperty("femaleModel");

                        var maleFPS = serializedObject.FindProperty("maleFPS");
                        var femaleFPS = serializedObject.FindProperty("femaleFPS");

                        EditorGUILayout.LabelField("BODY DATA", HeaderStyle);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical("box");

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(maleBody, GUILayout.MaxWidth(350));
                        if (EditorGUI.EndChangeCheck())
                            RegenerateMalePrefabs();

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(femaleBody, GUILayout.MaxWidth(350));
                        if (EditorGUI.EndChangeCheck())
                            RegenerateFemalePrefabs();


                        EditorGUILayout.PropertyField(maleFPS, GUILayout.MaxWidth(350));
                        EditorGUILayout.PropertyField(femaleFPS, GUILayout.MaxWidth(350));

                        // Draw Model Preview
                        // Draw Model Preview

                        GUIStyle bgColor = new GUIStyle();
                        bgColor.normal.background = Texture2D.blackTexture;

                        if (windowEditor == null)
                        {
                            if (maleSelected)
                            {
                                if (malePrefab != null)
                                {
                                    windowEditor = Editor.CreateEditor(malePrefab);
                                    gameObjectChanged = false;
                                }
                                else
                                    DestroyImmediate(windowEditor);
                            }
                            else if (femalePrefab != null)
                            {
                                windowEditor = Editor.CreateEditor(femalePrefab);
                                gameObjectChanged = false;
                            }else
                                DestroyImmediate(windowEditor);

                        }

                        if (windowEditor != null)
                        {
                            windowEditor.DrawPreview(GUILayoutUtility.GetRect(64, 355));
                            windowEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(1, 1), bgColor);
                            windowEditor.ReloadPreviewInstances();
                            windowEditor.Repaint();
                            windowEditor.ResetTarget();
                            this.Repaint();
                        } else
                        {
                            EditorGUILayout.Space(315);

                            if (maleSelected)
                                EditorGUILayout.HelpBox("There is no Male Model associated with this race, reference a prefab in the \"Body Data\" tab.", MessageType.Warning);
                            else
                                EditorGUILayout.HelpBox("There is no Female Model associated with this race, reference a prefab in the \"Body Data\" tab.", MessageType.Warning);
                        }

                        // Male/Female switch
                        EditorGUILayout.BeginHorizontal();
                        GUI.color = (maleSelected) ? Color.grey : defaultColor;
                        if (GUILayout.Button("Male"))
                        {
                            maleSelected = true;
                            DestroyImmediate(windowEditor);
                        }

                        GUI.color = (!maleSelected) ? Color.grey : defaultColor;
                        if (GUILayout.Button("Female"))
                        {
                            maleSelected = false;
                            DestroyImmediate(windowEditor);
                        }

                        GUI.color = defaultColor;
                        EditorGUILayout.EndHorizontal();

                        this.Repaint();
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical();
                        hairWindowScrollView =
                            EditorGUILayout.BeginScrollView(hairWindowScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.MaxHeight(200.0f));

                        EditorGUILayout.BeginVertical("box", GUILayout.MaxHeight(200.0f));

                        int currentMaleIndex = serializedObject.FindProperty("defaultMaleHair").intValue;
                        int currentFemaleIndex = serializedObject.FindProperty("defaultFemaleHair").intValue;

                        if (maleSelected)
                        {
                            maleHairList.DoLayoutList();
                            maleHairList.drawHeaderCallback = (Rect rect) =>
                            {
                                EditorGUI.LabelField(rect, "(MALE) Hair Types", HeaderStyle);
                            };

                            maleHairList.drawElementCallback =
                            (Rect rect, int index, bool isActive, bool isFocused) =>
                            {
                                var element = maleHairList.serializedProperty.GetArrayElementAtIndex(index);
                                rect.y += 2;

                                EditorGUI.LabelField(rect, "Type " + index.ToString());
                                EditorGUI.PropertyField(
                                    new Rect(rect.x + 60, rect.y, 150, EditorGUIUtility.singleLineHeight),
                                    element, GUIContent.none);
                            };


                            maleHairList.onRemoveCallback = (ReorderableList list) =>
                            {
                                var element = list.serializedProperty.GetArrayElementAtIndex(list.index);
                                element.DeleteCommand();
                                element.DeleteCommand();

                                serializedObject.FindProperty("defaultMaleHair").intValue = -1;
                                currentMaleIndex = -1;

                                SelectHairType(new HairSelection(-1, true));
                            };

                        } else
                        {
                            femaleHairList.DoLayoutList();
                            femaleHairList.drawHeaderCallback = (Rect rect) =>
                            {
                                EditorGUI.LabelField(rect, "(FEMALE) Hair Types", HeaderStyle);
                            };

                            femaleHairList.drawElementCallback =
                            (Rect rect, int index, bool isActive, bool isFocused) =>
                            {
                                var element = femaleHairList.serializedProperty.GetArrayElementAtIndex(index);
                                rect.y += 2;

                                EditorGUI.LabelField(rect, "Type " + index.ToString());
                                EditorGUI.PropertyField(
                                    new Rect(rect.x + 60, rect.y, 150, EditorGUIUtility.singleLineHeight),
                                    element, GUIContent.none);
                            };

                            femaleHairList.onRemoveCallback = (ReorderableList list) =>
                            {
                                var element = list.serializedProperty.GetArrayElementAtIndex(list.index);
                                element.DeleteCommand();
                                element.DeleteCommand();

                                serializedObject.FindProperty("defaultFemaleHair").intValue = -1;
                                currentFemaleIndex = -1;

                                SelectHairType(new HairSelection(-1, false));
                            };
                        }

                        EditorGUILayout.EndScrollView();

                        EditorGUILayout.EndVertical();

                        eyeWindowScrollView =
                            EditorGUILayout.BeginScrollView(eyeWindowScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.MaxHeight(200.0f));

                        EditorGUILayout.BeginVertical("box", GUILayout.MaxHeight(200.0f));

                        eyeList.DoLayoutList();
                        eyeList.drawHeaderCallback = (Rect rect) => {
                            EditorGUI.LabelField(rect, "Eyes Types", HeaderStyle);
                        };

                        eyeList.drawElementCallback =
                        (Rect rect, int index, bool isActive, bool isFocused) => {
                            var element = eyeList.serializedProperty.GetArrayElementAtIndex(index);
                            rect.y += 2;

                            EditorGUI.LabelField(rect, "Type " + index.ToString());
                            EditorGUI.PropertyField(
                                new Rect(rect.x + 60, rect.y, 150, EditorGUIUtility.singleLineHeight),
                                element, GUIContent.none);
                        };

                        EditorGUILayout.EndScrollView();
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(280.0f));
                        EditorGUILayout.LabelField("Default Hair", HeaderStyle);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Male:", GUILayout.MaxWidth(55.0f));
                        GUIContent newContent = new GUIContent();

                        // And update the serializedObject
                        currentMaleIndex = serializedObject.FindProperty("defaultMaleHair").intValue;

                        try
                        {
                            newContent.text = (currentMaleIndex != -1) ? maleHairList.serializedProperty.GetArrayElementAtIndex(currentMaleIndex).objectReferenceValue.name : "NONE";
                        }
                        catch
                        {
                            currentMaleIndex = -1;
                            newContent.text = (currentMaleIndex != -1) ? maleHairList.serializedProperty.GetArrayElementAtIndex(currentMaleIndex).objectReferenceValue.name : "NONE";
                        }
                        if (GUILayout.Button(newContent))
                        {
                            GenericMenu menu = new GenericMenu();

                            menu.AddDisabledItem(new GUIContent("HairTypes"));
                            menu.AddSeparator("");
                            
                            menu.AddItem(new GUIContent("NONE"), (serializedObject.FindProperty("defaultMaleHair").intValue == -1), SelectHairType, new HairSelection(-1, true));

                            for (int i = 0; i < maleHairList.count; i++)
                            {
                                bool isSelected = (serializedObject.FindProperty("defaultMaleHair").intValue == i);
                                if(maleHairList.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue != null)
                                    menu.AddItem(new GUIContent(maleHairList.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue.name), isSelected, SelectHairType, new HairSelection(i, true));
                            }

                            menu.ShowAsContext();

                            current.Use();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Female:", GUILayout.MaxWidth(55.0f));
                        newContent = new GUIContent();

                        currentFemaleIndex = serializedObject.FindProperty("defaultFemaleHair").intValue;

                        try
                        {
                            newContent.text = (currentFemaleIndex != -1) ? femaleHairList.serializedProperty.GetArrayElementAtIndex(currentFemaleIndex).objectReferenceValue.name : "NONE";
                        }
                        catch
                        {
                            currentFemaleIndex = -1;
                            newContent.text = (currentFemaleIndex != -1) ? femaleHairList.serializedProperty.GetArrayElementAtIndex(currentFemaleIndex).objectReferenceValue.name : "NONE";
                        }
                        if (GUILayout.Button(newContent))
                        {
                            GenericMenu menu = new GenericMenu();

                            menu.AddDisabledItem(new GUIContent("HairTypes"));
                            menu.AddSeparator("");


                            menu.AddItem(new GUIContent("NONE"), (serializedObject.FindProperty("defaultFemaleHair").intValue == -1), SelectHairType, new HairSelection(-1, false));

                            for (int i = 0; i < femaleHairList.count; i++)
                            {
                                bool isSelected = (serializedObject.FindProperty("defaultFemaleHair").intValue == i);
                                if(femaleHairList.serializedProperty.GetArrayElementAtIndex(i) != null)
                                    menu.AddItem(new GUIContent(femaleHairList.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue.name), isSelected, SelectHairType, new HairSelection(i, false));
                            }

                            menu.ShowAsContext();

                            current.Use();
                        }
                        EditorGUILayout.EndHorizontal();

                        void SelectHairType(object hairSelection)
                        {
                            HairSelection hSel = (HairSelection)hairSelection;
                            serializedObject.FindProperty((hSel.isMale) ? "defaultMaleHair" : "defaultFemaleHair").intValue = hSel.index;

                            // Instantiate Hair in the selected body
                            BodyData sBodyData;
                            if (hSel.isMale)
                            {
                                sBodyData = (malePrefab as GameObject).GetComponent<BodyData>();

                                if (sBodyData.hair != null)
                                    DestroyImmediate(sBodyData.hair.gameObject);

                                if (hSel.index > -1)
                                {
                                    serializedObject.FindProperty("defaultMaleHair").intValue = hSel.index;

                                    SerializedObject hairElement = new SerializedObject(maleHairList.serializedProperty.GetArrayElementAtIndex(hSel.index).objectReferenceValue); // this is Hair
                                    UnityEngine.Object sHair = hairElement.FindProperty("mesh").objectReferenceValue;
                                    sBodyData.hair = ((GameObject)(Instantiate(sHair, sBodyData.head.transform))).GetComponent<SkinnedMeshRenderer>();
                                    Debug.Log(sBodyData.hair);

                                    // Attach the hair
                                    sBodyData.hair.transform.parent = sBodyData.head.transform;
                                    sBodyData.hair.rootBone = sBodyData.head.rootBone;
                                    sBodyData.hair.bones = sBodyData.head.bones;

                                    this.Repaint();
                                    windowEditor.Repaint();
                                    windowEditor.ReloadPreviewInstances();
                                    windowEditor.Repaint();
                                    this.Repaint();
                                }
                            }
                            else
                            {
                                sBodyData = (femalePrefab as GameObject).GetComponent<BodyData>();

                                if (sBodyData.hair != null)
                                    DestroyImmediate(sBodyData.hair.gameObject);

                                if (hSel.index > -1)
                                {
                                    serializedObject.FindProperty("defaultFemaleHair").intValue = hSel.index;

                                    SerializedObject hairElement = new SerializedObject(femaleHairList.serializedProperty.GetArrayElementAtIndex(hSel.index).objectReferenceValue); // this is Hair
                                    UnityEngine.Object sHair = hairElement.FindProperty("mesh").objectReferenceValue;
                                    sBodyData.hair = ((GameObject)(Instantiate(sHair, sBodyData.head.transform))).GetComponent<SkinnedMeshRenderer>();
                                    Debug.Log(sBodyData.hair);

                                    // Attach the hair
                                    sBodyData.hair.transform.parent = sBodyData.head.transform;
                                    sBodyData.hair.rootBone = sBodyData.head.rootBone;
                                    sBodyData.hair.bones = sBodyData.head.bones;

                                    this.Repaint();
                                    windowEditor.Repaint();
                                    windowEditor.ReloadPreviewInstances();
                                    windowEditor.Repaint();
                                    this.Repaint();
                                }
                            }
                        }

                        EditorGUILayout.LabelField("Default Eyes", HeaderStyle);
                        EditorGUILayout.BeginHorizontal();

                        int currentEyeMaleIndex = serializedObject.FindProperty("defaultMaleEye").intValue;
                        int currentEyeFemaleIndex = serializedObject.FindProperty("defaultFemaleEye").intValue;                

                        EditorGUILayout.LabelField("Male:", GUILayout.MaxWidth(55.0f));
                        var maleEyeContent = new GUIContent();
                        currentEyeMaleIndex = serializedObject.FindProperty("defaultMaleEye").intValue;

                        try
                        {
                            maleEyeContent.text = (currentEyeMaleIndex != 0) ? eyeList.serializedProperty.GetArrayElementAtIndex(currentEyeMaleIndex).objectReferenceValue.name : "Default";
                        }
                        catch
                        {
                            currentEyeMaleIndex = 0;
                            maleEyeContent.text = (currentEyeMaleIndex != 0) ? eyeList.serializedProperty.GetArrayElementAtIndex(currentEyeMaleIndex).objectReferenceValue.name : "Default";
                        }
                        if (GUILayout.Button(maleEyeContent))
                        {
                            GenericMenu menu = new GenericMenu();

                            menu.AddDisabledItem(new GUIContent("Eyes Type"));
                            menu.AddSeparator("");


                            menu.AddItem(new GUIContent("Default"), (serializedObject.FindProperty("defaultMaleEye").intValue == 0), SelectEyeType, new EyeSelection(0, true));

                            for (int i = 0; i < eyeList.count; i++)
                            {
                                if (eyeList.serializedProperty.GetArrayElementAtIndex(i) != null)
                                    menu.AddItem(new GUIContent(eyeList.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue.name), (serializedObject.FindProperty("defaultMaleEye").intValue == i), SelectEyeType, new EyeSelection(i, true));
                            }

                            menu.ShowAsContext();

                            current.Use();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Female:", GUILayout.MaxWidth(55.0f));
                        var femaleEyeContent = new GUIContent();
                        currentEyeFemaleIndex = serializedObject.FindProperty("defaultFemaleEye").intValue;

                        try
                        {
                            femaleEyeContent.text = (currentEyeFemaleIndex != 0) ? eyeList.serializedProperty.GetArrayElementAtIndex(currentEyeFemaleIndex).objectReferenceValue.name : "Default";
                        }
                        catch
                        {
                            currentEyeFemaleIndex = 0;
                            femaleEyeContent.text = (currentEyeFemaleIndex != 0) ? eyeList.serializedProperty.GetArrayElementAtIndex(currentEyeFemaleIndex).objectReferenceValue.name : "Default";
                        }
                        if (GUILayout.Button(femaleEyeContent))
                        {
                            GenericMenu menu = new GenericMenu();

                            menu.AddDisabledItem(new GUIContent("Eyes Type"));
                            menu.AddSeparator("");


                            menu.AddItem(new GUIContent("Default"), (serializedObject.FindProperty("defaultFemaleEye").intValue == 0), SelectEyeType, new EyeSelection(0, false));

                            for (int i = 0; i < eyeList.count; i++)
                            {
                                if (eyeList.serializedProperty.GetArrayElementAtIndex(i) != null)
                                    menu.AddItem(new GUIContent(eyeList.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue.name), (serializedObject.FindProperty("defaultFemaleEye").intValue == i), SelectEyeType, new EyeSelection(i, false));
                            }

                            menu.ShowAsContext();

                            current.Use();
                        }

                        void SelectEyeType(object _eyeSelection)
                        {
                            EyeSelection hSel = (EyeSelection)_eyeSelection;
                            serializedObject.FindProperty((hSel.isMale) ? "defaultMaleEye" : "defaultFemaleEye").intValue = hSel.index;

                            // Instantiate Hair in the selected body
                            BodyData sBodyData;
                            if (hSel.isMale)
                            {
                                sBodyData = (malePrefab as GameObject).GetComponent<BodyData>();

                                serializedObject.FindProperty("defaultMaleEye").intValue = hSel.index;

                                SerializedObject eyeElement = new SerializedObject(eyeList.serializedProperty.GetArrayElementAtIndex(hSel.index).objectReferenceValue); // this is Hair
                                UnityEngine.Object sEye = eyeElement.FindProperty("eyes").objectReferenceValue;

                                sBodyData.eyeIndex = serializedObject.FindProperty("defaultMaleEye").intValue;
                                sBodyData.eyes.sharedMaterial = ((SkinnedMeshRenderer)sEye).sharedMaterial;

                                this.Repaint();
                                windowEditor.Repaint();
                                windowEditor.ReloadPreviewInstances();
                                windowEditor.Repaint();
                                this.Repaint();
                            }
                            else
                            {
                                sBodyData = (femalePrefab as GameObject).GetComponent<BodyData>();

                                serializedObject.FindProperty("defaultFemaleEye").intValue = hSel.index;

                                SerializedObject eyeElement = new SerializedObject(eyeList.serializedProperty.GetArrayElementAtIndex(hSel.index).objectReferenceValue); // this is Hair
                                UnityEngine.Object sEye = eyeElement.FindProperty("eyes").objectReferenceValue;

                                sBodyData.eyeIndex = serializedObject.FindProperty("defaultFemaleEye").intValue;
                                sBodyData.eyes.sharedMaterial = ((SkinnedMeshRenderer)sEye).sharedMaterial;

                                this.Repaint();
                                windowEditor.Repaint();
                                windowEditor.ReloadPreviewInstances();
                                windowEditor.Repaint();
                                this.Repaint();
                            }
                        }

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();


                        EditorGUILayout.EndScrollView();
                        #endregion
                        break;

                    case CurrentWindow.FaceData:
                        contentScrollView =
                            EditorGUILayout.BeginScrollView(contentScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(650.0f), GUILayout.Height(500.0f));

                        EditorGUILayout.BeginHorizontal();

                        faceBlendshapesContent =
                            EditorGUILayout.BeginScrollView(faceBlendshapesContent, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(270.0f), GUILayout.Height(500.0f));
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("FACE BLENDSHAPES", HeaderStyle);


                        // Get Blendshapes of the selected mesh
                        string blendshapeName;
                        GUIContent guicontent = new GUIContent();
                        SkinnedMeshRenderer selectedHead = null;

                        if (malePrefab != null && maleSelected)
                            selectedHead = (malePrefab as GameObject).GetComponent<BodyData>().head;
                        else if(femalePrefab != null && !maleSelected)
                            selectedHead = (femalePrefab as GameObject).GetComponent<BodyData>().head;


                        if (selectedHead != null)
                        {
                            // Display all blendshapes
                            for (int i = 0; i < selectedHead.sharedMesh.blendShapeCount; i++)
                            {
                                blendshapeName = selectedHead.sharedMesh.GetBlendShapeName(i).ToString();

                                guicontent.text = blendshapeName;
                                guicontent.tooltip = blendshapeName;

                                if (selectedBlendshapeIndex == i)
                                    GUI.color = Color.gray;

                                if (GUILayout.Button(guicontent))
                                {
                                    selectedBlendshapeIndex = i;
                                    sliderValue = selectedHead.GetBlendShapeWeight(i);
                                }

                                GUI.color = defaultColor;
                            }
                        }

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndScrollView();

                        GUILayout.FlexibleSpace();

                        EditorGUILayout.BeginVertical();
                        bgColor = new GUIStyle();

                        if (selectedHead != null)
                        {
                            if (windowEditor == null || gameObjectChanged)
                            {
                                windowEditor = Editor.CreateEditor(selectedHead.gameObject);
                                gameObjectChanged = false;
                            }
                        }

                        if (windowEditor != null)
                        {
                            windowEditor.DrawPreview(GUILayoutUtility.GetRect(64, 390));
                            windowEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(1, 1), bgColor);
                            windowEditor.ReloadPreviewInstances();
                            windowEditor.Repaint();
                            windowEditor.ResetTarget();
                            this.Repaint();
                        }
                        else
                        {
                            EditorGUILayout.Space(350);

                            if (maleSelected)
                                EditorGUILayout.HelpBox("There is no Male Model associated with this race, reference a prefab in the \"Body Data\" tab.", MessageType.Warning);
                            else
                                EditorGUILayout.HelpBox("There is no Female Model associated with this race, reference a prefab in the \"Body Data\" tab.", MessageType.Warning);
                        }

                        // Male/Female switch
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        GUI.color = (maleSelected) ? Color.grey : defaultColor;
                        if (GUILayout.Button("Male", GUILayout.MaxWidth(100)))
                        {
                            maleSelected = true;
                            gameObjectChanged = true;
                            DestroyImmediate(windowEditor);
                            selectedBlendshapeIndex = 0;
                        }

                        GUI.color = (!maleSelected) ? Color.grey : defaultColor;
                        if (GUILayout.Button("Female", GUILayout.MaxWidth(100)))
                        {
                            maleSelected = false;
                            gameObjectChanged = true;
                            DestroyImmediate(windowEditor);
                            selectedBlendshapeIndex = 0;
                        }

                        GUI.color = defaultColor;
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (selectedHead != null)
                        {
                            HeadBlendshapesManager bsManager = selectedHead.GetComponent<HeadBlendshapesManager>();
                            if (bsManager != null)
                                bsManager.AdjustChildBlendshapes();

                            if (selectedHead.sharedMesh.blendShapeCount > 0)
                            {
                                sliderValue = EditorGUILayout.Slider(selectedHead.GetBlendShapeWeight(selectedBlendshapeIndex), -100, 100);
                                selectedHead.SetBlendShapeWeight(selectedBlendshapeIndex, sliderValue);

                                EditorGUILayout.BeginHorizontal();

                                if (GUILayout.Button("Random Face"))
                                {
                                    for (int i = 0; i < selectedHead.sharedMesh.blendShapeCount; i++)
                                        selectedHead.SetBlendShapeWeight(i, UnityEngine.Random.Range(-100, 100));
                                }

                                if (GUILayout.Button("Reset Face"))
                                {
                                    for (int i = 0; i < selectedHead.sharedMesh.blendShapeCount; i++)
                                        selectedHead.SetBlendShapeWeight(i, 0);
                                }

                                EditorGUILayout.EndHorizontal();
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("The selected mesh has no blendshapes, therefore you cannot customize this character's face.", MessageType.Warning);
                            }
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();



                        EditorGUILayout.EndScrollView();

                        break;
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("SAVE", ButtonStyle))
            {
                serializedObject.ApplyModifiedProperties();
                // Apply prefab

                if (malePrefab != null)
                {
                    PrefabUtility.SaveAsPrefabAsset((GameObject)malePrefab, assetPathMale);
                    //PrefabUtility.UnloadPrefabContents((GameObject)malePrefab);
                }

                if (femalePrefab != null)
                {
                    PrefabUtility.SaveAsPrefabAsset((GameObject)femalePrefab, assetPathFemale);
                    //PrefabUtility.UnloadPrefabContents((GameObject)femalePrefab);
                }
            }

            if (GUILayout.Button("SAVE & CLOSE", ButtonStyle))
            {
                serializedObject.ApplyModifiedProperties();

                if (malePrefab != null)
                {
                    PrefabUtility.SaveAsPrefabAsset((GameObject)malePrefab, assetPathMale);
                    PrefabUtility.UnloadPrefabContents((GameObject)malePrefab);
                }

                if (femalePrefab != null)
                {
                    PrefabUtility.SaveAsPrefabAsset((GameObject)femalePrefab, assetPathFemale);
                    PrefabUtility.UnloadPrefabContents((GameObject)femalePrefab);
                }

                DestroyImmediate(windowEditor);
                this.Close();
            }


            if (GUILayout.Button("CANCEL", ButtonStyle))
            {
                DestroyImmediate(windowEditor);
                this.Close();
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        private void OnDestroy()
        {
            DestroyImmediate(malePrefab);
            DestroyImmediate(femalePrefab);
        }

        public static List<T> GetAllRaces<T>() where T : Race
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