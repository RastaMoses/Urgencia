using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;
using UnityEditorInternal;

namespace RPGCreationKit
{
    public class FactionsWindow : EditorWindow
    {
        bool showDemoFiles = true;
        List<Faction> allFactions = new List<Faction>();

        Vector2 indexesScrollView;
        Vector2 contentScrollView;
        Vector2 alliedRListScrollView;
        Vector2 hostileRListScrollView;

        GUIStyle guiLayoutDarkColor = new GUIStyle();
        GUIStyle guiLayoutLightColor = new GUIStyle();

        SerializedObject serializedObject;
        bool hasSelectedFaction = false;
        int selectedFactionIndex = -1;
        bool isCreatingNewFaction = false;

        Texture refreshButtonIcon;

        public ReorderableList alliedFactionsRList;
        public ReorderableList hostileFactionRList;

        public bool isReady = false;

        string searchString = string.Empty;

        [MenuItem("RPG Creation Kit/Factions Editor")]
        public static void ShowWindow()
        {
            EditorWindow thisWindow = GetWindow(typeof(FactionsWindow));

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.FactionEditorIcon);
            GUIContent titleContent = new GUIContent("Factions Editor", icon);
            thisWindow.titleContent = titleContent;

            thisWindow.minSize = new Vector2(865, 615);
            thisWindow.maxSize = new Vector2(865, 615);
        }

        void Init()
        {
            guiLayoutDarkColor.normal.background = MakeTex(600, 1, new Color(5.0f, 5.0f, 5.0f, .1f));
            guiLayoutLightColor.normal.background = MakeTex(600, 1, new Color(37.0f, 79.0f, 133.0f, .4f));
            refreshButtonIcon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RefreshButton);
            showDemoFiles = RckProjectSettings.instance.showDemoFiles;
            RefreshFactionList();
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

        public void RefreshFactionList()
        {
            allFactions = GetAllFactions<Faction>();

            if (!showDemoFiles)
            {
                for (int i = allFactions.Count - 1; i >= 0; i--)
                    if (allFactions[i]._IS_DEMO_FILE)
                        allFactions.RemoveAt(i);
            }

            allFactions.Sort(SortByID);
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

            GUIStyle TitleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            TitleStyle.fontSize = 20;
            TitleStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(200.0f), GUILayout.ExpandWidth(false));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("FACTIONS", HeaderStyle, GUILayout.MaxWidth(165.0f), GUILayout.ExpandWidth(false));

            if (GUILayout.Button(new GUIContent(refreshButtonIcon, "Refreshes the Factions list."), GUILayout.MaxWidth(32)))
                RefreshFactionList();

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            showDemoFiles = EditorGUILayout.Toggle("Show Demo Files?", showDemoFiles, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                RefreshFactionList();
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

            indexesScrollView = 
                EditorGUILayout.BeginScrollView(indexesScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(200.0f), GUILayout.Height(500.0f));
            indexesScrollView.x = (indexesScrollView.x / 2);
            Rect clickArea = EditorGUILayout.BeginVertical(guiLayoutDarkColor, GUILayout.Width(200.0f), GUILayout.Height(500.0f));

            Event current = Event.current;

            GUI.enabled = !isCreatingNewFaction;

            // Display all current quest stages
            for (int i = 0; i < allFactions.Count; i++)
            {
                // Check if we have to skip for search
                if (!string.IsNullOrEmpty(searchString.ToLower()))
                    if (!allFactions[i].ID.ToLower().Contains(searchString.ToLower()))
                        continue;

                if (selectedFactionIndex == i && hasSelectedFaction)
                    GUI.color = Color.green;

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(allFactions[i].ID, GUILayout.MaxWidth(170.0f)))
                {
                    GUI.FocusControl(null);
                    selectedFactionIndex = i;
                    hasSelectedFaction = true;
                    OnSelectedFactionChanges();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUI.color = defaultColor;
            }
            GUI.enabled = true;

            if (!isCreatingNewFaction)
            {
                // DROPDOWN TO CREATE NEW STAGES
                if (clickArea.Contains(current.mousePosition) && current.type == EventType.ContextClick)
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddDisabledItem(new GUIContent("Factions"));
                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("New"), false, CreateNew);
                    menu.AddItem(new GUIContent("Delete Selected"), false, DeleteSelected);

                    menu.ShowAsContext();

                    current.Use();
                }

                void CreateNew()
                {
                    Debug.Log("Creating new Faction");
                }

                void DeleteSelected()
                {
                    if (hasSelectedFaction)
                        Debug.Log("Deleting: " + allFactions[selectedFactionIndex].ID);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (hasSelectedFaction && serializedObject != null)
            {
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                float previousWidth = EditorGUIUtility.labelWidth;

                EditorGUIUtility.labelWidth = this.position.width - 200;

                EditorGUILayout.LabelField("Faction: " + serializedObject.FindProperty("ID").stringValue, TitleStyle);
                EditorGUIUtility.labelWidth = previousWidth;

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(20);

                contentScrollView =
                            EditorGUILayout.BeginScrollView(contentScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(650.0f), GUILayout.Height(500.0f));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("ID"), GUILayout.MaxWidth(350));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("factionName"), GUILayout.MaxWidth(350));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("friendlyFire"), GUILayout.MaxWidth(350));

                GUILayout.Space(50);

                EditorGUILayout.BeginHorizontal();
                #region close
                alliedFactionsRList.elementHeight = EditorGUIUtility.singleLineHeight * 2f;

                alliedRListScrollView =
                            EditorGUILayout.BeginScrollView(alliedRListScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(300.0f), GUILayout.Height(250.0f));

                alliedFactionsRList.DoLayoutList();
                alliedFactionsRList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Allied Factions", HeaderStyle);
                };

                alliedFactionsRList.drawElementCallback =
                            (Rect rect, int index, bool isActive, bool isFocused) =>
                            {
                                var factionElement = alliedFactionsRList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("faction");
                                var dispositionElement = alliedFactionsRList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("disposition");

                                rect.y -= 12;
                                EditorGUI.LabelField(rect, "Faction:");
                                rect.x += 200;
                                EditorGUI.LabelField(rect, "Disposition:");

                                rect.x -= 200;
                                rect.y += 30;

                                EditorGUI.PropertyField(
                                    new Rect(rect.x, rect.y, 190, EditorGUIUtility.singleLineHeight),
                                    factionElement, GUIContent.none);

                                rect.x += 200;

                                EditorGUI.PropertyField(
                                    new Rect(rect.x, rect.y, 40, EditorGUIUtility.singleLineHeight),
                                    dispositionElement, GUIContent.none);

                            };

                EditorGUILayout.EndScrollView();

                GUILayout.Space(10);


                hostileFactionRList.elementHeight = EditorGUIUtility.singleLineHeight * 2f;

                hostileRListScrollView =
                            EditorGUILayout.BeginScrollView(hostileRListScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(300.0f), GUILayout.Height(250.0f));

                hostileFactionRList.DoLayoutList();
                hostileFactionRList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Hostile Factions", HeaderStyle);
                };

                hostileFactionRList.drawElementCallback =
                            (Rect rect, int index, bool isActive, bool isFocused) =>
                            {
                                var factionElement = hostileFactionRList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("faction");
                                var dispositionElement = hostileFactionRList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("disposition");

                                rect.y -= 12;
                                EditorGUI.LabelField(rect, "Faction:");
                                rect.x += 200;
                                EditorGUI.LabelField(rect, "Disposition:");

                                rect.x -= 200;
                                rect.y += 30;

                                EditorGUI.PropertyField(
                                    new Rect(rect.x, rect.y, 190, EditorGUIUtility.singleLineHeight),
                                    factionElement, GUIContent.none);

                                rect.x += 200;

                                EditorGUI.PropertyField(
                                    new Rect(rect.x, rect.y, 40, EditorGUIUtility.singleLineHeight),
                                    dispositionElement, GUIContent.none);

                            };

                EditorGUILayout.EndScrollView();
                #endregion
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(25);

                EditorGUILayout.HelpBox("The Disposition controls how much the Faction likes/hate this faction. A Disposition of 100 in an Allied Faction means that the said faction will behave at the best, while 0 means they will be neutral. A Disposition of 100 in an Hostile faction means they will be neutral, while they will fight to death if it is 0.", MessageType.Info, true);

                EditorGUILayout.EndScrollView();

                EditorGUILayout.EndVertical();

            }


            EditorGUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SAVE", ButtonStyle))
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("SAVE & CLOSE", ButtonStyle))
            {
                serializedObject.ApplyModifiedProperties();
                this.Close();
            }

            if (GUILayout.Button("CANCEL", ButtonStyle))
            {
                this.Close();
            }
            GUILayout.EndHorizontal();
        }



        void OnSelectedFactionChanges()
        {
            // Create a new object of this race
            serializedObject = new SerializedObject(allFactions[selectedFactionIndex]);

            alliedFactionsRList = new ReorderableList(serializedObject, serializedObject.FindProperty("alliedFactions"), true, true, true, true);
            hostileFactionRList = new ReorderableList(serializedObject, serializedObject.FindProperty("hostileFactions"), true, true, true, true);
        }

        private static int SortByID(Faction f1, Faction f2)
        {
            return f1.ID.CompareTo(f1.ID);
        }

        public static List<T> GetAllFactions<T>() where T : Faction
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