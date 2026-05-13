using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit
{
    public class RckObjectWindow : EditorWindow
    {
        public enum CurrentWindow { Items, Dialogues, Behaviours, Quests, Worldspaces, Factions, Databases, Spells };
        CurrentWindow m_Window;

        Vector2 navScrollView;
        Vector2 contentScrollView;

        bool isReady = false;
        bool showDemoFiles = true;

        string itemsSearchString = string.Empty;
        public List<Item> allItems = new List<Item>();

        string dialoguesSearchString = string.Empty;
        public List<DialogueGraph> allDialogues = new List<DialogueGraph>();

        string behavioursSearchString = string.Empty;
        public List<RPGCK_BT> allBehaviours = new List<RPGCK_BT>();

        string questsSearchString = string.Empty;
        public List<Quest> allQuests = new List<Quest>();

        string worldspacesSearchString = string.Empty;
        public List<Worldspace> allWorldspaces = new List<Worldspace>();

        string factionsSearchString = string.Empty;
        public List<Faction> allFactions = new List<Faction>();

        string databasesSearchString = string.Empty;
        public List<RckDatabase> allDatabases = new List<RckDatabase>();

        string spellsearchString = string.Empty;
        public List<Spell> allSpells = new List<Spell>();

        // ItemFilter
        bool[] itemFilters = new bool[7] {true, true, true, true, true, true, true};


        [MenuItem("RPG Creation Kit/Rck Object Window")]
        public static void ShowWindow()
        {
            EditorWindow thisWindow = GetWindow(typeof(RckObjectWindow));

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RPGCKEditorWindowIcon);
            GUIContent titleContent = new GUIContent("Rck Object Viewer ", icon);
            thisWindow.titleContent = titleContent;

        }

        private void Init()
        {
            showDemoFiles = RckProjectSettings.instance.showDemoFiles;
            isReady = true;
        }

        private void OnGUI()
        {
            if (!isReady)
                Init();

            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            ButtonStyle.border = new RectOffset(2, 2, 2, 2);
            ButtonStyle.fontSize = 16;
            ButtonStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            GUIStyle resultStyle = new GUIStyle(EditorStyles.toolbarButton) { alignment = TextAnchor.MiddleLeft };
            resultStyle.border = new RectOffset(2, 2, 2, 2);
            resultStyle.fontSize = 12;

            GUIStyle HeaderStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            HeaderStyle.fontSize = 14;
            HeaderStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            EditorGUILayout.Space(7.5f);

            // Title
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("   Rck Object Window", HeaderStyle);
            showDemoFiles = EditorGUILayout.Toggle("Show Demo Files?", showDemoFiles);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.BeginVertical("box");
            navScrollView = EditorGUILayout.BeginScrollView(navScrollView, GUILayout.Width(120));
            #region NavMenu
            var defaultColor = GUI.color;

            if (m_Window == CurrentWindow.Items)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Items", EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(20)))
                m_Window = CurrentWindow.Items;

            EditorGUILayout.Space(1f);
            // --

            if (m_Window == CurrentWindow.Dialogues)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Dialogues", EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(20)))
                m_Window = CurrentWindow.Dialogues;

            EditorGUILayout.Space(1f);
            // --

            if (m_Window == CurrentWindow.Behaviours)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Behaviours", EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(20)))
                m_Window = CurrentWindow.Behaviours;

            EditorGUILayout.Space(1f);
            // --

            if (m_Window == CurrentWindow.Quests)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Quests", EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(20)))
                m_Window = CurrentWindow.Quests;

            EditorGUILayout.Space(1f);
            // --

            if (m_Window == CurrentWindow.Worldspaces)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Worldspaces", EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(20)))
                m_Window = CurrentWindow.Worldspaces;

            EditorGUILayout.Space(1f);
            // --

            if (m_Window == CurrentWindow.Factions)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Faction", EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(20)))
                m_Window = CurrentWindow.Factions;

            EditorGUILayout.Space(1f);
            // --

            if (m_Window == CurrentWindow.Spells)
                GUI.color = Color.green;
            else
                GUI.color = defaultColor;

            if (GUILayout.Button("Spells", EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(20)))
                m_Window = CurrentWindow.Spells;

            EditorGUILayout.Space(1f);
            // --


            #endregion

            GUI.color = defaultColor;
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(m_Window.ToString() + ":");


            EditorGUILayout.Space(5);

            Color previousColor = GUI.color;

            switch (m_Window)
            {
                case CurrentWindow.Items:
                    #region code

                    EditorGUILayout.BeginHorizontal();
                    Color defCol = GUI.color;

                    if (itemFilters[0])
                        GUI.color = Color.green;
                    if(GUILayout.Button("Ammo"))
                    {
                        itemFilters[0] = !itemFilters[0];
                    }
                    GUI.color = defCol;

                    if (itemFilters[1])
                        GUI.color = Color.green;
                    if (GUILayout.Button("Armor"))
                    {
                        itemFilters[1] = !itemFilters[1];
                    }
                    GUI.color = defCol;

                    if (itemFilters[2])
                        GUI.color = Color.green;
                    if (GUILayout.Button("Book"))
                    {
                        itemFilters[2] = !itemFilters[2];
                    }
                    GUI.color = defCol;

                    if(itemFilters[3])
                        GUI.color = Color.green;
                    if (GUILayout.Button("Consumable"))
                    {
                        itemFilters[3] = !itemFilters[3];
                    }
                    GUI.color = defCol;

                    if(itemFilters[4]) 
                        GUI.color = Color.green;
                    if (GUILayout.Button("Key"))
                    {
                        itemFilters[4] = !itemFilters[4];
                    }
                    GUI.color = defCol;

                    if(itemFilters[5]) 
                        GUI.color = Color.green;
                    if (GUILayout.Button("Misc"))
                    {
                        itemFilters[5] = !itemFilters[5];
                    }
                    GUI.color = defCol;

                    if(itemFilters[6]) 
                        GUI.color = Color.green;
                    if (GUILayout.Button("Weapon"))
                    {
                        itemFilters[6] = !itemFilters[6];
                    }
                    GUI.color = defCol;

                    EditorGUILayout.EndHorizontal();

                    if (!string.IsNullOrEmpty(itemsSearchString))
                        GUI.color = Color.yellow;


                    EditorGUILayout.BeginHorizontal();
                    itemsSearchString = GUILayout.TextField(itemsSearchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
                    if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                        itemsSearchString = string.Empty;
                    EditorGUILayout.EndHorizontal();
                    GUI.color = previousColor;

                    EditorGUILayout.Space(2.5f);

                    contentScrollView = EditorGUILayout.BeginScrollView(contentScrollView);

                    for (int i = 0; i < allItems.Count; i++)
                    {
                        if (!showDemoFiles && allItems[i]._IS_DEMO_FILE)
                            continue;

                        // Check if we have to skip for search
                        if (!string.IsNullOrEmpty(itemsSearchString.ToLower()))
                            if (!allItems[i].ItemID.ToLower().Contains(itemsSearchString.ToLower()))
                                continue;

                        // Item filter
                        if (!itemFilters[(int)allItems[i].itemType])
                            continue;

                        if (Selection.activeObject == allItems[i])
                            GUI.color = Color.green;

                        if(GUILayout.Button(allItems[i].ItemID, resultStyle))
                            Selection.activeObject = allItems[i];

                        GUI.color = defaultColor;

                        EditorGUILayout.Space(1f);
                    }
                    #endregion
                    break;

                case CurrentWindow.Dialogues:
                    #region code

                    if (!string.IsNullOrEmpty(dialoguesSearchString))
                        GUI.color = Color.yellow;


                    EditorGUILayout.BeginHorizontal();
                    dialoguesSearchString = GUILayout.TextField(dialoguesSearchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
                    if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                        dialoguesSearchString = string.Empty;
                    EditorGUILayout.EndHorizontal();
                    GUI.color = previousColor;

                    EditorGUILayout.Space(2.5f);

                    contentScrollView = EditorGUILayout.BeginScrollView(contentScrollView);

                    for (int i = 0; i < allDialogues.Count; i++)
                    {
                        if (!showDemoFiles && allDialogues[i]._IS_DEMO_FILE)
                            continue;

                        // Check if we have to skip for search
                        if (!string.IsNullOrEmpty(dialoguesSearchString.ToLower()))
                            if (!allDialogues[i].ID.ToLower().Contains(dialoguesSearchString.ToLower()))
                                continue;

                        if (Selection.activeObject == allDialogues[i])
                            GUI.color = Color.green;

                        if (GUILayout.Button(allDialogues[i].ID, resultStyle))
                            Selection.activeObject = allDialogues[i];

                        GUI.color = defaultColor;

                        EditorGUILayout.Space(1f);
                    }
                    #endregion
                    break;

                case CurrentWindow.Behaviours:
                    #region code

                    if (!string.IsNullOrEmpty(behavioursSearchString))
                        GUI.color = Color.yellow;

                    EditorGUILayout.BeginHorizontal();
                    behavioursSearchString = GUILayout.TextField(behavioursSearchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
                    if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                        behavioursSearchString = string.Empty;
                    EditorGUILayout.EndHorizontal();
                    GUI.color = previousColor;

                    EditorGUILayout.Space(2.5f);

                    contentScrollView = EditorGUILayout.BeginScrollView(contentScrollView);

                    for (int i = 0; i < allBehaviours.Count; i++)
                    {
                        if (!showDemoFiles && allBehaviours[i]._IS_DEMO_FILE)
                            continue;

                        // Check if we have to skip for search
                        if (!string.IsNullOrEmpty(behavioursSearchString.ToLower()))
                            if (!allBehaviours[i].ID.ToLower().Contains(behavioursSearchString.ToLower()))
                                continue;

                        if (Selection.activeObject == allBehaviours[i])
                            GUI.color = Color.green;

                        if (GUILayout.Button(allBehaviours[i].ID, resultStyle))
                            Selection.activeObject = allBehaviours[i];

                        GUI.color = defaultColor;

                        EditorGUILayout.Space(1f);
                    }
                    #endregion
                    break;

                case CurrentWindow.Quests:
                    #region code

                    if (!string.IsNullOrEmpty(questsSearchString))
                        GUI.color = Color.yellow;


                    EditorGUILayout.BeginHorizontal();
                    questsSearchString = GUILayout.TextField(questsSearchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
                    if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                        questsSearchString = string.Empty;
                    EditorGUILayout.EndHorizontal();
                    GUI.color = previousColor;

                    EditorGUILayout.Space(2.5f);

                    contentScrollView = EditorGUILayout.BeginScrollView(contentScrollView);

                    for (int i = 0; i < allQuests.Count; i++)
                    {
                        if (!showDemoFiles && allQuests[i]._IS_DEMO_FILE)
                            continue;

                        // Check if we have to skip for search
                        if (!string.IsNullOrEmpty(questsSearchString.ToLower()))
                            if (!allQuests[i].questID.ToLower().Contains(questsSearchString.ToLower()))
                                continue;

                        if (Selection.activeObject == allQuests[i])
                            GUI.color = Color.green;

                        if (GUILayout.Button(allQuests[i].questID, resultStyle))
                            Selection.activeObject = allQuests[i];

                        GUI.color = defaultColor;

                        EditorGUILayout.Space(1f);
                    }
                    #endregion
                    break;

                case CurrentWindow.Worldspaces:
                    #region code

                    if (!string.IsNullOrEmpty(worldspacesSearchString))
                        GUI.color = Color.yellow;


                    EditorGUILayout.BeginHorizontal();
                    worldspacesSearchString = GUILayout.TextField(worldspacesSearchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
                    if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                        worldspacesSearchString = string.Empty;
                    EditorGUILayout.EndHorizontal();
                    GUI.color = previousColor;

                    EditorGUILayout.Space(2.5f);

                    contentScrollView = EditorGUILayout.BeginScrollView(contentScrollView);

                    for (int i = 0; i < allWorldspaces.Count; i++)
                    {
                        if (!showDemoFiles && allWorldspaces[i]._IS_DEMO_FILE)
                            continue;

                        // Check if we have to skip for search
                        if (!string.IsNullOrEmpty(worldspacesSearchString.ToLower()))
                            if (!allWorldspaces[i].worldSpaceID.ToLower().Contains(worldspacesSearchString.ToLower()))
                                continue;

                        if (Selection.activeObject == allWorldspaces[i])
                            GUI.color = Color.green;

                        if (GUILayout.Button(allWorldspaces[i].worldSpaceID, resultStyle))
                            Selection.activeObject = allWorldspaces[i];

                        GUI.color = defaultColor;

                        EditorGUILayout.Space(1f);
                    }
                    #endregion
                    break;

                case CurrentWindow.Factions:
                    #region code

                    if (!string.IsNullOrEmpty(factionsSearchString))
                        GUI.color = Color.yellow;


                    EditorGUILayout.BeginHorizontal();
                    factionsSearchString = GUILayout.TextField(factionsSearchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
                    if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                        factionsSearchString = string.Empty;
                    EditorGUILayout.EndHorizontal();
                    GUI.color = previousColor;

                    EditorGUILayout.Space(2.5f);

                    contentScrollView = EditorGUILayout.BeginScrollView(contentScrollView);

                    for (int i = 0; i < allFactions.Count; i++)
                    {
                        if (!showDemoFiles && allFactions[i]._IS_DEMO_FILE)
                            continue;

                        // Check if we have to skip for search
                        if (!string.IsNullOrEmpty(factionsSearchString.ToLower()))
                            if (!allFactions[i].ID.ToLower().Contains(factionsSearchString.ToLower()))
                                continue;

                        if (Selection.activeObject == allFactions[i])
                            GUI.color = Color.green;

                        if (GUILayout.Button(allFactions[i].ID, resultStyle))
                            Selection.activeObject = allFactions[i];

                        GUI.color = defaultColor;

                        EditorGUILayout.Space(1f);
                    }
                    #endregion
                    break;

                case CurrentWindow.Spells:
                    #region code

                    if (!string.IsNullOrEmpty(spellsearchString))
                        GUI.color = Color.yellow;


                    EditorGUILayout.BeginHorizontal();
                    spellsearchString = GUILayout.TextField(factionsSearchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
                    if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                        spellsearchString = string.Empty;
                    EditorGUILayout.EndHorizontal();
                    GUI.color = previousColor;

                    EditorGUILayout.Space(2.5f);

                    contentScrollView = EditorGUILayout.BeginScrollView(contentScrollView);

                    for (int i = 0; i < allSpells.Count; i++)
                    {
                        if (!showDemoFiles && allSpells[i]._IS_DEMO_FILE)
                            continue;

                        // Check if we have to skip for search
                        if (!string.IsNullOrEmpty(spellsearchString.ToLower()))
                            if (!allSpells[i].spellID.ToLower().Contains(spellsearchString.ToLower()))
                                continue;

                        if (Selection.activeObject == allSpells[i])
                            GUI.color = Color.green;

                        if (GUILayout.Button(allSpells[i].spellID, resultStyle))
                            Selection.activeObject = allSpells[i];

                        GUI.color = defaultColor;

                        EditorGUILayout.Space(1f);
                    }
                    #endregion
                    break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndVertical();

        }

        private void OnFocus()
        {
            FillAllLists();
        }

        private void FillAllLists()
        {
            allItems = GetAllInstances<Item>();
            allDialogues = GetAllInstances<DialogueGraph>();
            allBehaviours = GetAllInstances<RPGCK_BT>();
            allQuests = GetAllInstances<Quest>();
            allWorldspaces = GetAllInstances<Worldspace>();
            allFactions = GetAllInstances<Faction>();
            allDatabases = GetAllInstances<RckDatabase>();
            allSpells = GetAllInstances<Spell>();
        }

        public static List<T> GetAllInstances<T>() where T : ScriptableObject
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