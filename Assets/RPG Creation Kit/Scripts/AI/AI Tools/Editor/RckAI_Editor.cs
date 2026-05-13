using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.PersistentReferences;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGCreationKit.AI
{
    /// <summary>
    /// This window allows the creation of new AI agents in the RPG Creation Kit.
    /// </summary>
    public class RckAI_Editor : EditorWindow
    {
        bool isReady = false;
        bool showDemoFiles = true;

        public enum CurrentWindow { RckAI, Face, Inventory, Behaviour, Factions, SpellsKnowledge };
        private CurrentWindow m_Window;

        Vector2 indexesScrollView;
        Vector2 contentScrollView;

        Texture refreshButtonIcon;
        GUIStyle guiLayoutDarkColor = new GUIStyle();
        GUIStyle guiLayoutLightColor = new GUIStyle();

        List<RckAI> allAI = new List<RckAI>();

        string searchString = string.Empty;

        int selectedRckAIIndex = -1;
        bool hasSelectedRckAI = true;
        bool isCreatingNewRckAI = false;


        // CUSTOM VARIABLES
        bool isCreatingMale = true;
        bool isPersistentReference = true;
        RuntimeAnimatorController runtimeAnimatorController;
        bool lookAtEnabled = true;

        Vector2 faceBlendshapesContent;
        float sliderValue;
        int selectedBlendshapeIndex;

        GameObject gameObject;
        Editor windowEditor;
        bool gameObjectChanged = false;

        public RckAI dummyRckAI;

        SerializedObject serializedPrefabComponent;
        SerializedObject serializedAttributes;
        SerializedObject serializedInventory;
        SerializedObject serializedEquipment;
        SerializedObject serializedSpell;


        public GameObject RckAIAsset;
        public GameObject RckAIPrefab;          // The abs prefab GameObject
        public RckAI RckAIPrefabComponent; // The RckAI script of the RckAIPrefab
        public GameObject RckAIGfx;

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

        [MenuItem("RPG Creation Kit/RckAI Editor")]
        public static void ShowWindow()
        {
            EditorWindow thisWindow = GetWindow(typeof(RckAI_Editor));

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RaceEditorIcon);
            GUIContent titleContent = new GUIContent("RckAI Editor", icon);
            thisWindow.titleContent = titleContent;

            thisWindow.minSize = new Vector2(900, 615);
            thisWindow.maxSize = new Vector2(900, 615);
        }

        void Init()
        {
            guiLayoutDarkColor.normal.background = MakeTex(600, 1, new Color(5.0f, 5.0f, 5.0f, .1f));
            guiLayoutLightColor.normal.background = MakeTex(600, 1, new Color(37.0f, 79.0f, 133.0f, .4f));
            refreshButtonIcon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RefreshButton);
            showDemoFiles = RckProjectSettings.instance.showDemoFiles;
            RefreshRckAIList();
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

        private void OnGUI()
        {
            if (!isReady)
                Init();

            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            ButtonStyle.border = new RectOffset(2, 2, 2, 2);
            ButtonStyle.fontSize = 16;
            ButtonStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/Almendra-Regulars.ttf");

            GUIStyle HeaderStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            HeaderStyle.fontSize = 14;
            HeaderStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/Almendra-Regulasr.ttf");


            GUIStyle TitleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            TitleStyle.fontSize = 18;
            TitleStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/Almendra-Regulasr.ttf");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(200.0f), GUILayout.ExpandWidth(false));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("RckAI", HeaderStyle, GUILayout.MaxWidth(165.0f), GUILayout.ExpandWidth(false));

            if (GUILayout.Button(new GUIContent(refreshButtonIcon, "Refreshes the Race list."), GUILayout.MaxWidth(32)))
                RefreshRckAIList();

            if (GUILayout.Button(new GUIContent(refreshButtonIcon, "Refreshes the Race listxxxx."), GUILayout.MaxWidth(32)))
            {
                foreach(RckAI _ai in allAI)
                {
                    _ai.attributes.attributes.SetToDefault();
                    _ai.attributes.derivedAttributes.CalculateFromAttributes(_ai.attributes.attributes, true);
                    _ai.attributes.derivedAttributes.SetCurValuesToMax();
                    _ai.attributes.DeriveAndSetLevelFromAttributes();
                    EditorUtility.SetDirty(_ai);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2.5f);

            EditorGUI.BeginChangeCheck();
            showDemoFiles = EditorGUILayout.Toggle("Show Demo Files?", showDemoFiles, GUILayout.ExpandWidth(false));
            if(EditorGUI.EndChangeCheck())
            {
                RefreshRckAIList();
            }

            Color defaultColor = GUI.color;

            if (!string.IsNullOrEmpty(searchString))
                GUI.color = Color.yellow;

            EditorGUILayout.BeginHorizontal("box");
            searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                searchString = string.Empty;
            EditorGUILayout.EndHorizontal();
            GUI.color = defaultColor;

            indexesScrollView = EditorGUILayout.BeginScrollView(indexesScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(200.0f), GUILayout.Height(500.0f));
            indexesScrollView.x = (indexesScrollView.x / 2);
            Rect clickArea = EditorGUILayout.BeginVertical(guiLayoutDarkColor, GUILayout.Width(200.0f), GUILayout.Height(500.0f));
            EditorGUILayout.Space(20);

            Event current = Event.current;

            GUI.enabled = !isCreatingNewRckAI;


            // Display all races
            for (int i = 0; i < allAI.Count; i++)
            {
                // Check if we have to skip for search
                if (!string.IsNullOrEmpty(searchString.ToLower()))
                    if (!allAI[i].entityID.ToLower().Contains(searchString.ToLower()))
                        continue;

                if (selectedRckAIIndex == i && hasSelectedRckAI)
                    GUI.color = Color.green;

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent(allAI[i].entityID, allAI[i].entityID), GUILayout.MaxWidth(170.0f)))
                {
                    GUI.FocusControl(null);
                    selectedRckAIIndex = i;
                    hasSelectedRckAI = true;

                    OnSelectedRckAIChanges();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUI.color = defaultColor;
            }
            GUI.enabled = true;

            if (!isCreatingNewRckAI)
            {
                // DROPDOWN TO CREATE NEW STAGES
                if (clickArea.Contains(current.mousePosition) && current.type == EventType.ContextClick)
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddDisabledItem(new GUIContent("RckAI"));
                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("New"), false, CreateNew);
                    menu.AddItem(new GUIContent("Duplicate Selected"), false, DuplicateSelected);
                    menu.AddItem(new GUIContent("Delete Selected"), false, DeleteSelected);

                    menu.ShowAsContext();

                    current.Use();
                }

                void CreateNew()
                {
                    GameObject newAI = Instantiate(dummyRckAI.gameObject);
                    RckAI newAIComponent = newAI.GetComponent<RckAI>();

                    newAI.name = "[AI] NewRckAI";

                    newAIComponent.entityID = "_New_RckAI";
                    newAIComponent.entityName = "New RckAI";

                    string path = AssetDatabase.GenerateUniqueAssetPath(RCKSettings.EDITOR_AI_SAVE_LOCATION + "[RckAI] NewRckAI.prefab");
                    PrefabUtility.SaveAsPrefabAssetAndConnect(newAI, path, InteractionMode.AutomatedAction);

                    DestroyImmediate(newAI);

                    UnloadPrefab();

                    RefreshRckAIList();
                }

                void DuplicateSelected()
                {
                    if (RckAIAsset != null)
                    {
                        GameObject newAI = Instantiate(RckAIAsset);
                        RckAI aiComponent = newAI.GetComponent<RckAI>();

                        aiComponent.entityID += "Copy";

                        PrefabUtility.SaveAsPrefabAsset(newAI, AssetDatabase.GenerateUniqueAssetPath(RCKSettings.EDITOR_AI_SAVE_LOCATION + "[RckAI] " + aiComponent.entityID + ".prefab"));

                        DestroyImmediate(newAI);
                        UnloadPrefab();
                        hasSelectedRckAI = false;
                        selectedRckAIIndex = -1;
                        RefreshRckAIList();
                    }
                }

                void DeleteSelected()
                {
                    if(RckAIAsset != null)
                    {
                        if(EditorUtility.DisplayDialog("Delete", "Are you sure you want to delete the Agent: \n(ID: \"" + RckAIPrefabComponent.entityID + "\") ?", "Yes", "Cancel"))
                        {
                            if(RckAIPrefabComponent.GetComponent<PersistentReference>() != null)
                                RemovePersistentReference();

                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(RckAIAsset));
                            UnloadPrefab();
                            hasSelectedRckAI = false;
                            selectedRckAIIndex = -1;
                            RefreshRckAIList();
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal("box");

            #region Selection
            if (m_Window == CurrentWindow.RckAI)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("RckAI", GUILayout.Width(100), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.RckAI;
            }

            if (m_Window == CurrentWindow.Face)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Face", GUILayout.Width(100), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.Face;
            }

            if (m_Window == CurrentWindow.Inventory)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Inventory", GUILayout.Width(100), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.Inventory;
            }

            if (m_Window == CurrentWindow.SpellsKnowledge)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Spells", GUILayout.Width(100), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.SpellsKnowledge;
            }

            if (m_Window == CurrentWindow.Behaviour)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Behaviour", GUILayout.Width(100), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.Behaviour;
            }

            if (m_Window == CurrentWindow.Factions)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Factions", GUILayout.Width(100), GUILayout.Height(20)))
            {
                m_Window = CurrentWindow.Factions;
            }

            GUI.color = defaultColor;

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Select Prefab", GUILayout.Width(125), GUILayout.Height(20)))
            {
                Selection.objects = new GameObject[] { RckAIAsset };
            }

            #endregion

            contentScrollView =
                           EditorGUILayout.BeginScrollView(contentScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(650.0f), GUILayout.Height(500.0f));

            if(hasSelectedRckAI && RckAIPrefab != null)
            {
                serializedPrefabComponent.Update();

                EditorGUILayout.Space(20);

                switch (m_Window)
                {
                    case CurrentWindow.RckAI:
                        EditorGUIUtility.labelWidth = 150;
                        EditorGUILayout.LabelField("RCK AI", TitleStyle);
                        EditorGUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal("box");

                            // First stage
                            EditorGUILayout.BeginVertical();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Entity ID:");
                            RckAIPrefabComponent.entityID = EditorGUILayout.TextField(RckAIPrefabComponent.entityID);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Entity Name:");
                            RckAIPrefabComponent.entityName = EditorGUILayout.TextField(RckAIPrefabComponent.entityName);
                            EditorGUILayout.EndHorizontal();

                        EditorGUI.BeginChangeCheck();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Race:");
                            RckAIPrefabComponent.race = (Race)EditorGUILayout.ObjectField(RckAIPrefabComponent.race, typeof(Race), false);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Is Male?");
                            isCreatingMale = EditorGUILayout.Toggle(isCreatingMale);
                            EditorGUILayout.EndHorizontal();

                        if(EditorGUI.EndChangeCheck())
                            GenerateNewGFX();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Is Essential?");
                            RckAIPrefabComponent.isEssential = EditorGUILayout.Toggle(RckAIPrefabComponent.isEssential);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Use Ragdoll?");
                            RckAIPrefabComponent.usesRagdoll = EditorGUILayout.Toggle(RckAIPrefabComponent.usesRagdoll);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.EndVertical();
                            // End of First Stage

                            EditorGUILayout.Space(20);
                            
                            // Second stage
                            EditorGUILayout.BeginVertical();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Persistent Reference?");
                            isPersistentReference = EditorGUILayout.Toggle(isPersistentReference);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Animation Controller:");
                            runtimeAnimatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField(runtimeAnimatorController, typeof(RuntimeAnimatorController), false);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Look At Enabled?");
                            lookAtEnabled = EditorGUILayout.Toggle(lookAtEnabled);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Looting Corpse?");
                            RckAIPrefabComponent.allowsLootWhenDead = EditorGUILayout.Toggle(RckAIPrefabComponent.allowsLootWhenDead);
                            EditorGUILayout.EndHorizontal();

                            if (RckAIPrefabComponent.allowsLootWhenDead)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PrefixLabel("Looting Equipment?");
                                RckAIPrefabComponent.allowsLootOfEquipment = EditorGUILayout.Toggle(RckAIPrefabComponent.allowsLootOfEquipment);
                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Helps members of same faction?");
                            RckAIPrefabComponent.helpsMembersOfSameFactions = EditorGUILayout.Toggle(RckAIPrefabComponent.helpsMembersOfSameFactions);
                            EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();
                            // End of second stage

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space(15);

                        EditorGUIUtility.labelWidth = 60;

                        var curLevel = serializedAttributes.FindProperty("curLevel");
                        var xpModfiierOnDeath = serializedAttributes.FindProperty("xpMultiplierOnDeath");
                        ulong xpAwardedOnKill = RCKSettings.CalculateXPGainedOnAIKill(curLevel.uintValue);
                        xpAwardedOnKill = (ulong)(xpAwardedOnKill * xpModfiierOnDeath.floatValue);

                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField("STATS", TitleStyle, GUILayout.Width(60));

                        GUI.enabled = false;
                        EditorGUILayout.PropertyField(curLevel, false, GUILayout.Width(150));
                        EditorGUILayout.LabelField("Xp OnKill:", GUILayout.Width(70));
                        EditorGUILayout.LongField((long)xpAwardedOnKill, GUILayout.Width(90));
                        GUI.enabled = true;

                        EditorGUILayout.PropertyField(xpModfiierOnDeath, new GUIContent("Xp Mult"), false, GUILayout.Width(140));
                        EditorGUILayout.EndHorizontal();

                        EditorGUIUtility.labelWidth = 170;

                        var attributesProp = serializedAttributes.FindProperty("attributes");
                        var derivedProp = serializedAttributes.FindProperty("derivedAttributes");

                        EditorGUI.BeginChangeCheck();

                        EditorGUILayout.BeginHorizontal("box");
                        EditorGUILayout.PropertyField(attributesProp, true);
                        EditorGUILayout.PropertyField(derivedProp, true);
                        EditorGUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck())
                        {
                            int str = attributesProp.FindPropertyRelative("Strength").intValue;
                            int dex = attributesProp.FindPropertyRelative("Dexterity").intValue;
                            int agi = attributesProp.FindPropertyRelative("Agility").intValue;
                            int con = attributesProp.FindPropertyRelative("Constitution").intValue;
                            int spe = attributesProp.FindPropertyRelative("Speed").intValue;
                            int end = attributesProp.FindPropertyRelative("Endurance").intValue;
                            int cha = attributesProp.FindPropertyRelative("Charisma").intValue;
                            int intel = attributesProp.FindPropertyRelative("Intelligence").intValue;
                            int wil = attributesProp.FindPropertyRelative("Willpower").intValue;


                            derivedProp.FindPropertyRelative("maxHealth").floatValue = RCKSettings.GetMaxHealthCalculation(con, str);
                            derivedProp.FindPropertyRelative("maxMana").floatValue = RCKSettings.GetMaxManaCalculation(wil, intel);
                            derivedProp.FindPropertyRelative("maxStamina").floatValue = RCKSettings.GetMaxStaminaCalculation(end);
                            derivedProp.FindPropertyRelative("maxEncumbrance").intValue = RCKSettings.GetMaxEncumbranceCalculation(str);

                            derivedProp.FindPropertyRelative("curHealth").floatValue = RCKSettings.GetMaxHealthCalculation(con, str);
                            derivedProp.FindPropertyRelative("curMana").floatValue = RCKSettings.GetMaxManaCalculation(wil, intel);
                            derivedProp.FindPropertyRelative("curStamina").floatValue = RCKSettings.GetMaxStaminaCalculation(end);

                            curLevel.uintValue = EntityAttributes.CalculateLevelFromAttributes(str, dex, agi, con, spe, end, cha, intel, wil);

                            serializedAttributes.ApplyModifiedProperties();
                        }

                        break;

                    case CurrentWindow.Face:
                        #region code
                        EditorGUIUtility.labelWidth = 150;

                        if (RckAIGfx == null)
                        {
                            if (RckAIPrefabComponent.race == null)
                                EditorGUILayout.HelpBox("There is no Race assigned to this RckAI Agent, assign a Race first.", MessageType.Warning);
                        }
                        else
                        {
                            contentScrollView =
                            EditorGUILayout.BeginScrollView(contentScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(650.0f), GUILayout.Height(485.0f));

                            EditorGUILayout.BeginHorizontal();

                            faceBlendshapesContent =
                                EditorGUILayout.BeginScrollView(faceBlendshapesContent, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(270.0f), GUILayout.Height(455.0f));
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField("FACE BLENDSHAPES", HeaderStyle);


                            // Get Blendshapes of the selected mesh
                            string blendshapeName;
                            GUIContent guicontent = new GUIContent();
                            SkinnedMeshRenderer selectedHead = null;

                            selectedHead = (RckAIGfx).GetComponent<BodyData>().head;


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
                            EditorGUILayout.Space(45);

                            EditorGUILayout.BeginVertical();
                            var bgColor = new GUIStyle();

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
                                windowEditor.DrawPreview(GUILayoutUtility.GetRect(64, 370));
                                windowEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(1, 1), bgColor);
                                windowEditor.ReloadPreviewInstances();
                                windowEditor.Repaint();
                                windowEditor.ResetTarget();
                                this.Repaint();
                            }
                            else
                            {
                                EditorGUILayout.Space(350);

                                EditorGUILayout.HelpBox("There is no Model associated with this race, reference a prefab in the \"Body Data\" tab.", MessageType.Warning);
                            }

                            // Male/Female switch
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();

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

                                    EditorGUILayout.Separator();

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUIUtility.labelWidth = 60;
                                    EditorGUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Hair:", HeaderStyle);

                                    GUIContent hairButtonContent = new GUIContent();

                                    try
                                    {
                                        if (RckAIPrefabComponent.bodyData.hairIndex != -1)
                                        {
                                            if (isCreatingMale)
                                                hairButtonContent.text = RckAIPrefabComponent.race.maleHairTypes[RckAIPrefabComponent.bodyData.hairIndex].hairName;
                                            else
                                                hairButtonContent.text = RckAIPrefabComponent.race.femaleHairTypes[RckAIPrefabComponent.bodyData.hairIndex].hairName;
                                        }
                                        else
                                            hairButtonContent.text = "NONE";
                                    }
                                    catch
                                    {
                                        hairButtonContent.text = "NONE";
                                    }

                                    if (GUILayout.Button(hairButtonContent))
                                    {
                                        GenericMenu menu = new GenericMenu();

                                        menu.AddDisabledItem(new GUIContent("Hair Types:"));
                                        menu.AddSeparator("");

                                        menu.AddItem(new GUIContent("NONE"), (RckAIPrefabComponent.bodyData.hairIndex == -1), SelectHairType, new HairSelection(-1, true));

                                        if (isCreatingMale)
                                        {
                                            for (int i = 0; i < RckAIPrefabComponent.race.maleHairTypes.Count; i++)
                                                menu.AddItem(new GUIContent(RckAIPrefabComponent.race.maleHairTypes[i].hairName), (RckAIPrefabComponent.bodyData.hairIndex == i), SelectHairType, new HairSelection(i, true));
                                        }
                                        else
                                        {
                                            for (int i = 0; i < RckAIPrefabComponent.race.femaleHairTypes.Count; i++)
                                                menu.AddItem(new GUIContent(RckAIPrefabComponent.race.femaleHairTypes[i].hairName), (RckAIPrefabComponent.bodyData.hairIndex == i), SelectHairType, new HairSelection(i, false));
                                        }

                                        menu.ShowAsContext();
                                    }

                                    void SelectHairType(object hairSelection)
                                    {
                                        HairSelection hSel = (HairSelection)hairSelection;
                                        hSel.isMale = isCreatingMale;

                                        BodyData bodyData = RckAIGfx.GetComponent<BodyData>();
                                        bodyData.hairIndex = hSel.index;

                                        if (hSel.isMale)
                                        {
                                            if(bodyData.hair != null)
                                                DestroyImmediate(bodyData.hair.gameObject);

                                            if(hSel.index > -1)
                                            {
                                                bodyData.hairIndex = hSel.index;
                                                bodyData.hair = ((GameObject)Instantiate(RckAIPrefabComponent.race.maleHairTypes[bodyData.hairIndex].mesh, bodyData.head.transform)).GetComponent<SkinnedMeshRenderer>();

                                                // Attach
                                                bodyData.hair.transform.parent = bodyData.head.transform;
                                                bodyData.hair.rootBone = bodyData.head.rootBone;
                                                bodyData.hair.bones = bodyData.head.bones;

                                                this.Repaint();
                                                windowEditor.Repaint();
                                                windowEditor.ReloadPreviewInstances();
                                                windowEditor.Repaint();
                                                this.Repaint();
                                            }
                                        }
                                        else
                                        {
                                            if (bodyData.hair != null)
                                                DestroyImmediate(bodyData.hair.gameObject);

                                            if (hSel.index > -1)
                                            {
                                                bodyData.hairIndex = hSel.index;
                                                bodyData.hair = ((GameObject)Instantiate(RckAIPrefabComponent.race.femaleHairTypes[bodyData.hairIndex].mesh, bodyData.head.transform)).GetComponent<SkinnedMeshRenderer>();

                                                // Attach
                                                bodyData.hair.transform.parent = bodyData.head.transform;
                                                bodyData.hair.rootBone = bodyData.head.rootBone;
                                                bodyData.hair.bones = bodyData.head.bones;

                                                this.Repaint();
                                                windowEditor.Repaint();
                                                windowEditor.ReloadPreviewInstances();
                                                windowEditor.Repaint();
                                                this.Repaint();
                                            }
                                        }
                                    }

                                    EditorGUILayout.EndVertical();

                                    EditorGUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Eyes:", HeaderStyle);
                                    GUIContent eyesButtonContent = new GUIContent();

                                    try
                                    {
                                        if (RckAIPrefabComponent.bodyData.eyeIndex != -1)
                                        {
                                            eyesButtonContent.text = RckAIPrefabComponent.race.eyeTypes[RckAIPrefabComponent.bodyData.eyeIndex].eyesName;
                                        }
                                        else
                                            eyesButtonContent.text = "Default Eyes";
                                    }
                                    catch
                                    {
                                        eyesButtonContent.text = "Default Eyes";
                                    }

                                    if (GUILayout.Button(eyesButtonContent))
                                    {
                                        GenericMenu menu = new GenericMenu();

                                        menu.AddDisabledItem(new GUIContent("Eye Types:"));
                                        menu.AddSeparator("");

                                        menu.AddItem(new GUIContent("Default"), (RckAIPrefabComponent.bodyData.eyeIndex == 0), SelectEyeType, 0);

                                        for (int i = 0; i < RckAIPrefabComponent.race.eyeTypes.Count; i++)
                                            menu.AddItem(new GUIContent(RckAIPrefabComponent.race.eyeTypes[i].eyesName), (RckAIPrefabComponent.bodyData.eyeIndex == i), SelectEyeType, i);

                                        menu.ShowAsContext();
                                    }
                                    EditorGUILayout.EndVertical();

                                    EditorGUILayout.EndHorizontal();

                                    void SelectEyeType(object _selectedIndex)
                                    {
                                        int indx = (int)_selectedIndex;

                                        BodyData bodyData = RckAIGfx.GetComponent<BodyData>();
                                        bodyData.eyeIndex = indx;

                                        bodyData.eyes.sharedMaterial = RckAIPrefabComponent.race.eyeTypes[bodyData.eyeIndex].eyes.sharedMaterial;

                                        this.Repaint();
                                        windowEditor.Repaint();
                                        windowEditor.ReloadPreviewInstances();
                                        windowEditor.Repaint();
                                        this.Repaint();
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.HelpBox("The selected mesh has no blendshapes, therefore you cannot customize this character's face.", MessageType.Warning);
                                }
                            }

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();



                            EditorGUILayout.EndScrollView();
                        }
                        #endregion
                        break;

                    case CurrentWindow.Inventory:
                        #region
                        EditorGUILayout.LabelField("INVENTORY & EQUIPMENT", TitleStyle);
                        EditorGUIUtility.labelWidth = 150;

                        EditorGUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Can be pick-pocketed?");
                        RckAIPrefabComponent.canBePickPocketed = EditorGUILayout.Toggle(RckAIPrefabComponent.canBePickPocketed);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal("box");

                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.PropertyField(serializedInventory.FindProperty("Items"), GUILayout.Width(300));

                        EditorGUILayout.BeginHorizontal();

                        if (GUILayout.Button("Set Ownership"))
                        {
                            // Foreach item
                            for (int i = 0; i < serializedInventory.FindProperty("Items").arraySize; i++)
                            {
                                serializedInventory.FindProperty("Items").GetArrayElementAtIndex(i).FindPropertyRelative("metadata").FindPropertyRelative("isOwned").boolValue = true;
                                serializedInventory.FindProperty("Items").GetArrayElementAtIndex(i).FindPropertyRelative("metadata").FindPropertyRelative("ownerID").stringValue= RckAIPrefabComponent.entityID;
                            }
                        }

                        if (GUILayout.Button("Clear Ownership"))
                        {
                            // Foreach item
                            for (int i = 0; i < serializedInventory.FindProperty("Items").arraySize; i++)
                            {
                                serializedInventory.FindProperty("Items").GetArrayElementAtIndex(i).FindPropertyRelative("metadata").FindPropertyRelative("isOwned").boolValue = false;
                                serializedInventory.FindProperty("Items").GetArrayElementAtIndex(i).FindPropertyRelative("metadata").FindPropertyRelative("ownerID").stringValue = string.Empty;
                            }
                        }

                        EditorGUILayout.EndHorizontal();


                        EditorGUILayout.EndVertical();


                        var equippedItems = serializedPrefabComponent.FindProperty("equippedItems");
                        var equippedItemsSize = equippedItems.arraySize;

                        // Draw the arrows that allows to immediatly equip the inventory
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.Space(75);  // initial space

                        // Foreach item
                        for(int i = 0; i < serializedInventory.FindProperty("Items").arraySize; i++)
                        {
                            // Get the item of the current search
                            Item currentItem = serializedInventory.FindProperty("Items").GetArrayElementAtIndex(i).FindPropertyRelative("item").objectReferenceValue as Item;
                            
                            // Draw the arrow button
                            if (GUILayout.Button("->"))
                            {
                                // If it's already equipped we should not equipt it again
                                bool found = false;
                                for (int j = 0; j < equippedItemsSize; j++)
                                {
                                    var jItem = (equippedItems.GetArrayElementAtIndex(j).objectReferenceValue as Item);
                                    if (jItem != null && jItem.ItemID == currentItem.ItemID)
                                    {
                                        // It already exists
                                        found = true;
                                        break;
                                    }
                                }

                                // If it's not equipped, equip it 
                                if(!found)
                                {
                                    equippedItems.InsertArrayElementAtIndex(equippedItemsSize);
                                    equippedItems.GetArrayElementAtIndex(equippedItemsSize).objectReferenceValue = currentItem;
                                }
                            }

                            EditorGUILayout.Space(75);

                        }

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical("box");
                        EditorGUIUtility.labelWidth = 70;

                      


                        // Draw the size of the array with a DelayedIntField
                        EditorGUIUtility.labelWidth = 100;
                        int newSize = EditorGUILayout.DelayedIntField("Equipped Items:", equippedItemsSize, EditorStyles.boldLabel);

                        EditorGUIUtility.labelWidth = 80;

                        EditorGUI.indentLevel++;
                        // If the size has changed
                        if (newSize != equippedItemsSize)
                        {
                            // Apply in the array
                            equippedItems.arraySize = newSize;
                        }

                        equippedItems.isExpanded = true;

                        // Draw the whole array
                        for (int i = 0; i < newSize; i++)
                        {
                            var prop = equippedItems.GetArrayElementAtIndex(i);
                            if (prop == null)
                                break;

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(prop);

                            if (GUILayout.Button("X", GUILayout.Width(50)))
                            {
                                equippedItems.DeleteArrayElementAtIndex(i);
                                break;
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("+", GUILayout.Width(50)))
                        {
                            equippedItems.InsertArrayElementAtIndex(equippedItemsSize);
                            break;
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();
                        serializedPrefabComponent.ApplyModifiedProperties();
                        #endregion
                        break;

                    case CurrentWindow.SpellsKnowledge:
                        #region
                        EditorGUILayout.LabelField("Spells Knowledge", TitleStyle);
                        EditorGUIUtility.labelWidth = 150;

                        EditorGUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal("box");

                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.PropertyField(serializedSpell.FindProperty("spellInUse"), GUILayout.Width(300));

                        EditorGUILayout.PropertyField(serializedSpell.FindProperty("Spells"), GUILayout.Width(300));

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();
                        serializedPrefabComponent.ApplyModifiedProperties();
                        #endregion
                        break;

                    case CurrentWindow.Behaviour:
                        EditorGUIUtility.labelWidth = 150;

                        EditorGUILayout.LabelField("BEHAVIOUR", TitleStyle);
                        EditorGUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal("box");

                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Use BT?");
                        RckAIPrefabComponent.useBT = EditorGUILayout.Toggle(RckAIPrefabComponent.useBT);
                        EditorGUILayout.EndHorizontal();



                        GUI.enabled = RckAIPrefabComponent.useBT;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Tick Rate:");
                        RckAIPrefabComponent.tickRate = (BehaviourTree.TreeTickRate)EditorGUILayout.EnumPopup(RckAIPrefabComponent.tickRate);
                        EditorGUILayout.EndHorizontal();

                        switch (RckAIPrefabComponent.tickRate)
                        {
                            case BehaviourTree.TreeTickRate.EveryXFrames:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PrefixLabel("X Frames:");
                                RckAIPrefabComponent.xFrames = EditorGUILayout.IntField(RckAIPrefabComponent.xFrames);
                                EditorGUILayout.EndHorizontal();
                                break;

                            case BehaviourTree.TreeTickRate.EveryXSeconds_Realtime:
                            case BehaviourTree.TreeTickRate.EveryXSeconds_GameTime:
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PrefixLabel("X Secodds:");
                                RckAIPrefabComponent.xSeconds = EditorGUILayout.FloatField(RckAIPrefabComponent.xSeconds);
                                EditorGUILayout.EndHorizontal();
                                break;

                            default:
                                break;
                        }

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space(15);

                        // Second stage
                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Purpose Behaviour:");
                        RckAIPrefabComponent.purposeBehaviourTree = (BehaviourTree.RPGCK_BT)EditorGUILayout.ObjectField(RckAIPrefabComponent.purposeBehaviourTree, typeof(BehaviourTree.RPGCK_BT), false);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Combat Behaviour:");
                        RckAIPrefabComponent.combatBehaviourTree = (BehaviourTree.RPGCK_BT)EditorGUILayout.ObjectField(RckAIPrefabComponent.combatBehaviourTree, typeof(BehaviourTree.RPGCK_BT), false);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();

                        GUI.enabled = true;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Player Damage Tolerance Enabled:");
                        RckAIPrefabComponent.pdtEnabled = EditorGUILayout.Toggle(RckAIPrefabComponent.pdtEnabled);
                        EditorGUILayout.EndHorizontal();

                        GUI.enabled = RckAIPrefabComponent.pdtEnabled;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("PDT Hits: ");
                        RckAIPrefabComponent.pdtHitsBeforeAggro = EditorGUILayout.IntField(RckAIPrefabComponent.pdtHitsBeforeAggro);
                        EditorGUILayout.EndHorizontal();


                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("PDT Timeslice: ");
                        RckAIPrefabComponent.pdtTimeslice = EditorGUILayout.FloatField(RckAIPrefabComponent.pdtTimeslice);
                        EditorGUILayout.EndHorizontal();

                        GUI.enabled = true;

                        EditorGUILayout.Space(25);

                        EditorGUILayout.LabelField("DIALOGUE", TitleStyle);
                        EditorGUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal("box");

                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Use Dialogue System?");
                        RckAIPrefabComponent.dialogueSystemEnabled = EditorGUILayout.Toggle(RckAIPrefabComponent.dialogueSystemEnabled);

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space(15);

                        // Second stage
                        EditorGUILayout.BeginVertical();

                            GUI.enabled = RckAIPrefabComponent.dialogueSystemEnabled;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Current Dialogue:");
                        RckAIPrefabComponent.currentDialogueGraph = (DialogueSystem.DialogueGraph)EditorGUILayout.ObjectField(RckAIPrefabComponent.currentDialogueGraph, typeof(DialogueSystem.DialogueGraph), false);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Default Dialogue:");
                        RckAIPrefabComponent.defaultDialogueGraph = (DialogueSystem.DialogueGraph)EditorGUILayout.ObjectField(RckAIPrefabComponent.defaultDialogueGraph, typeof(DialogueSystem.DialogueGraph), false);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Rotate towards?");
                        RckAIPrefabComponent.dialogueLookAt = EditorGUILayout.Toggle(RckAIPrefabComponent.dialogueLookAt);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Follows outside of Cell?");
                        RckAIPrefabComponent.followTargetOutsideOfCell = EditorGUILayout.Toggle(RckAIPrefabComponent.followTargetOutsideOfCell);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();

                            GUI.enabled = true;

                        break;

                    case CurrentWindow.Factions:
                        EditorGUIUtility.labelWidth = 150;

                        EditorGUILayout.LabelField("FACTIONS", TitleStyle);
                        EditorGUILayout.Space(10);

                        EditorGUILayout.PropertyField(serializedPrefabComponent.FindProperty("belongsToFactions"));
                        serializedPrefabComponent.ApplyModifiedProperties();
                        break;

                    default:
                        break;
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("SAVE", ButtonStyle))
            {
                SaveEditorVariablesToPrefab();

                previouEntityID = RckAIPrefabComponent.entityID;

                serializedAttributes.ApplyModifiedProperties();
                serializedPrefabComponent.ApplyModifiedProperties();
                serializedInventory.ApplyModifiedProperties();
                serializedSpell.ApplyModifiedProperties();

                RckAIAsset.name = "[AI] " + RckAIPrefabComponent.entityID;
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(RckAIAsset), "[AI] " + RckAIPrefabComponent.entityID);
                PrefabUtility.SaveAsPrefabAsset(RckAIPrefab, AssetDatabase.GetAssetPath(RckAIAsset));

                if (newPref || haveToReRegister)
                    RegisterPersistentReference();
                    
                newPref = removePref = updatePref = haveToReRegister = false;

                // Refresh database
                var AllAIDatabaseFiles = GetAllInstances<AIDatabaseFile>();

                foreach (AIDatabaseFile file in AllAIDatabaseFiles)
                    file.fill();
            }

            if (GUILayout.Button("SAVE & CLOSE", ButtonStyle))
            {
                if (RckAIAsset != null && RckAIPrefab != null)
                {
                    SaveEditorVariablesToPrefab();

                    previouEntityID = RckAIPrefabComponent.entityID;

                    serializedAttributes.ApplyModifiedProperties();
                    serializedPrefabComponent.ApplyModifiedProperties();
                    serializedInventory.ApplyModifiedProperties();
                    serializedSpell.ApplyModifiedProperties();

                    RckAIAsset.name = "[AI] " + RckAIPrefabComponent.entityID;
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(RckAIAsset), "[AI] " + RckAIPrefabComponent.entityID);
                    PrefabUtility.SaveAsPrefabAsset(RckAIPrefab, AssetDatabase.GetAssetPath(RckAIAsset));

                    if (newPref || haveToReRegister)
                        RegisterPersistentReference();

                    newPref = removePref = updatePref = haveToReRegister = false;
                }


                // Refresh database
                var AllAIDatabaseFiles = GetAllInstances<AIDatabaseFile>();

                foreach (AIDatabaseFile file in AllAIDatabaseFiles)
                    file.fill();

                // Apply prefab
                this.Close();
            }


            if (GUILayout.Button("CANCEL", ButtonStyle))
            {
                this.Close();
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        string previouEntityID = "";
        private void SaveEditorVariablesToPrefab()
        {
            // Update stuff related to editor variables:
            if (runtimeAnimatorController != RckAIPrefab.GetComponent<Animator>().runtimeAnimatorController)
                RckAIPrefab.GetComponent<Animator>().runtimeAnimatorController = runtimeAnimatorController;

            // The user wanted to have a persistent reference
            if (isPersistentReference && RckAIPrefab.GetComponent<PersistentReferences.PersistentReference>() == null)
            {
                var a = RckAIPrefab.AddComponent<PersistentReferences.PersistentReference>();
                a.refID = RckAIPrefabComponent.entityID;
                a.type = PersistentReferences.PersistentReferenceType.AI;
                newPref = true;
            }

            // The user wanted to remove persistent reference form this AI
            else if (!isPersistentReference && RckAIPrefab.GetComponent<PersistentReferences.PersistentReference>() != null)
            {
                RemovePersistentReference();

                DestroyImmediate(RckAIPrefab.GetComponent<PersistentReferences.PersistentReference>());
                removePref = true;
            }

            // The user already had persistent reference, just update the ID
            else if (isPersistentReference && RckAIPrefab.GetComponent<PersistentReferences.PersistentReference>() != null)
            {
                var a = RckAIPrefab.GetComponent<PersistentReferences.PersistentReference>();

                UpdatePersistentReference();

                a.refID = RckAIPrefabComponent.entityID;
                a.type = PersistentReferences.PersistentReferenceType.AI;

                previouEntityID = "";
                updatePref = true;
            }

            RckAIPrefabComponent.isPersistentReference = isPersistentReference;

            RckAIPrefabComponent.aiLookAt.enabled = lookAtEnabled;
        }

        bool newPref = false, removePref = false, updatePref = false;
        private void RegisterPersistentReference()
        {
            // Register the persistent reference
            bool sceneWasLoaded = IsSceneLoaded("_PersistentReferences_");
            string[] guids = AssetDatabase.FindAssets("t:scene _PersistentReferences_");

            // Unregister the persistent reference
            if (!sceneWasLoaded)
            {
                // Open scene
                EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guids[0]), OpenSceneMode.Additive);
            }

            EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByName("_PersistentReferences_"));

            var editorPRef = FindObjectOfType<PersistentReferenceManager>();

            PersistentReference pref = null;
            if(!editorPRef.refs.TryGetValue(RckAIPrefabComponent.entityID, out pref))
            {
                // Instantiate the prefab
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(RckAIAsset, EditorSceneManager.GetSceneByName("_PersistentReferences_"));
                instance.GetComponent<RckAI>().enabled = false;

                editorPRef.RegisterPersistentReference(instance.GetComponent<PersistentReference>());
            }
            
            EditorUtility.SetDirty(editorPRef);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        private void RemovePersistentReference()
        {
            // Register the persistent reference
            bool sceneWasLoaded = IsSceneLoaded("_PersistentReferences_");
            string[] guids = AssetDatabase.FindAssets("t:scene _PersistentReferences_");

            // Unregister the persistent reference
            if (!sceneWasLoaded)
            {
                // Open scene
                EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guids[0]), OpenSceneMode.Additive);
            }

            EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByName("_PersistentReferences_"));

            var editorPRef = FindObjectOfType<PersistentReferenceManager>();

            // Instantiate the prefab
            var get = editorPRef.GetPersistentReference(RckAIPrefabComponent.entityID);
            editorPRef.UnregisterPersistentReference(get, true);
            EditorUtility.SetDirty(editorPRef);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        bool haveToReRegister = false;
        private void UpdatePersistentReference()
        {
            // Register the persistent reference
            bool sceneWasLoaded = IsSceneLoaded("_PersistentReferences_");
            string[] guids = AssetDatabase.FindAssets("t:scene _PersistentReferences_");

            // Unregister the persistent reference
            if (!sceneWasLoaded)
            {
                // Open scene
                EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guids[0]), OpenSceneMode.Additive);
            }

            EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByName("_PersistentReferences_"));

            var editorPRef = FindObjectOfType<PersistentReferenceManager>();

            // Instantiate the prefab
            if (previouEntityID != RckAIPrefabComponent.entityID)
            {
                Debug.Log("entity id changed");

                var get = editorPRef.GetPersistentReference(previouEntityID);
                editorPRef.UnregisterPersistentReference(get, true);
                haveToReRegister = true;
            }

            EditorUtility.SetDirty(editorPRef);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        static bool IsSceneLoaded(string sceneName_no_extention)
        {
            for (int i = 0; i < EditorSceneManager.sceneCount; ++i)
            {
                Scene scene = EditorSceneManager.GetSceneAt(i);
                if (scene.name == sceneName_no_extention)
                {
                    //the scene is already loaded
                    return true;
                }
            }

            return false;//scene not currently loaded in the hierarchy
        }

        private void OnSelectedRckAIChanges()
        {
            DestroyImmediate(windowEditor);

            if (RckAIPrefab != null)
                UnloadPrefab();

            RckAIAsset = allAI[selectedRckAIIndex].gameObject;
            RckAIPrefab = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(RckAIAsset));

            if (RckAIPrefab != null)
            {
                RckAIPrefabComponent = RckAIPrefab.GetComponent<RckAI>();
                serializedPrefabComponent = new SerializedObject(RckAIPrefabComponent);
                serializedAttributes = new SerializedObject(RckAIPrefabComponent.attributes);
                serializedInventory = new SerializedObject(RckAIPrefabComponent.inventory);
                serializedEquipment = new SerializedObject(RckAIPrefabComponent.equipment);

                // To support NPCs created with < 1.3 versions, if serialized spell is null, add the spell to the prefab
                if(RckAIPrefabComponent.spellsKnowledge == null)
                {
                    Debug.Log("Adding spells knowledge to the selected NPC.");
                    RckAIPrefabComponent.spellsKnowledge = RckAIPrefabComponent.gameObject.AddComponent<SpellsKnowledge>();
                    serializedSpell = new SerializedObject(RckAIPrefabComponent.spellsKnowledge);
                }
                else
                    serializedSpell = new SerializedObject(RckAIPrefabComponent.spellsKnowledge);


                // Attempts to find the Gfx
                if (RckAIPrefab.transform.Find("GFX") != null)
                    RckAIGfx = RckAIPrefab.transform.Find("GFX").gameObject;

                // Load Default Editor variables to match the selected RckAI
                if(RckAIGfx != null)
                    isCreatingMale = RckAIGfx.GetComponent<BodyData>().isMale;

                runtimeAnimatorController = RckAIPrefab.GetComponent<Animator>().runtimeAnimatorController;
                isPersistentReference = (RckAIPrefab.GetComponent<PersistentReferences.PersistentReference>() != null ? true : false);
                lookAtEnabled = RckAIPrefabComponent.aiLookAt.enabled;
            }

        }

        private void GenerateNewGFX()
        {
            if (RckAIPrefab.transform.Find("GFX") != null)
                DestroyImmediate(RckAIPrefab.transform.Find("GFX").gameObject);
            
            // Instanitate a new GFX from the selected race
            if(RckAIPrefabComponent.race != null)
            {
                GameObject newGFX = (GameObject)Instantiate(isCreatingMale ?
                                                            RckAIPrefabComponent.race.maleModel.gameObject :
                                                            RckAIPrefabComponent.race.femaleModel.gameObject,
                                                            RckAIPrefab.transform);

                newGFX.name = "GFX";

                // Initialize and update references
                RckAIPrefabComponent.bodyData = newGFX.GetComponent<BodyData>();
                RckAIPrefabComponent.GetComponent<Animator>().avatar = RckAIPrefabComponent.bodyData.myAvatar;
                RckAIPrefabComponent.lootingPoint = newGFX.GetComponentInChildren<LootingPoint>();
                RckAIPrefabComponent.lootingPoint.inventory = RckAIPrefabComponent.inventory;
                RckAIPrefabComponent.lootingPoint.equipment = RckAIPrefabComponent.equipment;
                RckAIPrefabComponent.headPos = newGFX.GetComponentInChildren<AIHeadPos>().transform;
                RckAIPrefabComponent.audioSource = RckAIPrefabComponent.GetComponent<AudioSource>();

                if(newGFX.GetComponent<AISounds>() != null)
                    RckAIPrefabComponent.aiSounds = newGFX.GetComponent<AISounds>();

                if (RckAIPrefabComponent.headPos != null)
                    RckAIPrefabComponent.entityFocusPart = RckAIPrefabComponent.headPos;

                RckAIPrefabComponent.equipment.characterModel = RckAIPrefabComponent.bodyData;
                RckAIPrefabComponent.ragdoll = newGFX.GetComponent<Ragdoll>();

                if (RckAIPrefabComponent.usesRagdoll)
                    RckAIPrefabComponent.ragdoll.animator = RckAIPrefabComponent.m_Anim;

                RckAIGfx = newGFX;

                RckAIPrefabComponent.onlineComponents.GFX = RckAIGfx;

                // Update RckAI_Prefab Animator
            }

            DestroyImmediate(windowEditor);
        }

        private void UnloadPrefab()
        {
            if(RckAIPrefab != null && PrefabUtility.GetPrefabAssetType(RckAIPrefab) != PrefabAssetType.NotAPrefab)
                PrefabUtility.UnloadPrefabContents(RckAIPrefab);

            serializedPrefabComponent = null;
            serializedAttributes = null;
            serializedInventory = null;
            serializedSpell = null;
            serializedEquipment = null;
            RckAIPrefab = null;
            RckAIPrefabComponent = null;
            RckAIGfx = null;
        }

        private void RefreshRckAIList()
        {
            allAI = GetAllRckAI();

            if(!showDemoFiles)
            {
                for(int i = allAI.Count - 1; i >= 0; i--)
                    if(allAI[i]._IS_DEMO_FILE)
                        allAI.RemoveAt(i);
            }

            allAI.Sort(SortByID);
        }

        private static int SortByID(RckAI a1, RckAI a2)
        {
            return a1.entityID.CompareTo(a2.entityID);
        }

        public static List<RckAI> GetAllRckAI()
        {
            string[] allAssets = Directory.GetFiles(RCKSettings.EDITOR_AI_SAVE_LOCATION, "*.prefab", SearchOption.AllDirectories);

            List<RckAI> allAI = new List<RckAI>();

            RckAI thisAI = null;
            for (int i = 0; i < allAssets.Length; i++)
            {
                var assetObject = AssetDatabase.LoadAssetAtPath(allAssets[i], typeof(UnityEngine.Object));

                if (PrefabUtility.GetPrefabAssetType(assetObject) != PrefabAssetType.NotAPrefab)
                {
                    thisAI = (assetObject as GameObject).GetComponent<RckAI>();

                    if (thisAI != null && thisAI.entityID != "DEFAULT_ENTITY_ID" && (thisAI is CreatureAI) == false)
                        allAI.Add((assetObject as GameObject).GetComponent<RckAI>());
                }

                thisAI = null;
            }

            return allAI;
        }

        public static List<T> GetAllInstances<T>() where T : AIDatabaseFile
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

        private void OnDisable()
        {
            UnloadPrefab();
            DestroyImmediate(windowEditor);
        }

        private void OnDestroy()
        {
            UnloadPrefab();
            DestroyImmediate(windowEditor);
        }
    }
}