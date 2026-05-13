using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit.PersistentReferences;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.PersistentReferences
{
    [System.Serializable]
    public class PersistentReferenceInWindow
    {
        public PersistentReference reference;
        public CellInformation cell;

        public PersistentReferenceInWindow(PersistentReference _ref, CellInformation _cell)
        {
            reference = _ref;
            cell = _cell;
        }
    }

    [System.Serializable]
    public class PersistentReferencesWindow : EditorWindow
    {
        [SerializeField] PersistentReferenceManager editorPRef;
        [SerializeField] CellInformation[] cells;
        [SerializeField] List<PersistentReferenceInWindow> references = new List<PersistentReferenceInWindow>();
        [SerializeField] List<bool> foldouts = new List<bool>();

                                                     // AI   Loot  Door  Other
        [SerializeField] bool[] filters = new bool[] { true, true, true, true };
        [SerializeField] string searchPattern = "";

        static CellInformation[] sCells;

        Texture refreshButtonIcon;

        Vector2 windowScrollView;

        static PersistentReferencesWindow window;

        [MenuItem("RPG Creation Kit/Persistent References")]
        private static void OpenWindow()
        {
            window = GetWindow<PersistentReferencesWindow>();

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RPGCKEditorWindowIcon);

            GUIContent titleContent = new GUIContent("Persistent References", icon);
            window.titleContent = titleContent;
        }

        private void Awake()
        {
            RefreshWindow();
            refreshButtonIcon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RefreshButton);

            UpdateOnSceneChanges();
        }


        private void OnGUI()
        {
            if(!IsSceneLoaded("_PersistentReferences_"))
            {
                EditorGUILayout.HelpBox("_PersistentReferences_ scene isn't loaded. Therefore there is no way to display the persistent references in the opened scenes.", MessageType.Warning, true);
                if(GUILayout.Button("Load _PersistentReferences_"))
                {
                    string[] guids = AssetDatabase.FindAssets("t:scene _PersistentReferences_");
                    EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guids[0]), OpenSceneMode.Additive);
                    RefreshWindow();
                }
                return;
            }

            if(editorPRef == null)
                editorPRef = FindObjectOfType<PersistentReferenceManager>();

            if (references == null)
                references = new List<PersistentReferenceInWindow>();

            if (cells == null || sCells == null)
                RefreshWindow();


            if (sCells.Length != cells.Length)
                RefreshWindow();

            if (refreshButtonIcon == null)
                refreshButtonIcon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RefreshButton);

            EditorGUILayout.Space(10);



            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cells Opened : ["+cells.Length+"]", EditorStyles.boldLabel);
            if (GUILayout.Button(new GUIContent(refreshButtonIcon, "Refreshes the cells."), GUILayout.MaxWidth(25), GUILayout.MaxHeight(25)))
                RefreshWindow();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            Color defaultGUIColor = GUI.contentColor;

            EditorGUILayout.BeginHorizontal("box");

            EditorGUIUtility.labelWidth = 40;
            EditorGUILayout.LabelField("Filters:");

            if (filters[0])
                GUI.contentColor = Color.green;
            else
                GUI.contentColor = defaultGUIColor;

            if(GUILayout.Button("AI",EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                filters[0] = !filters[0];

            EditorGUILayout.Space(5);

            if (filters[1])
                GUI.contentColor = Color.green;
            else
                GUI.contentColor = defaultGUIColor;

            if (GUILayout.Button("Looting Points", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                filters[1] = !filters[1];

            EditorGUILayout.Space(3);

            if (filters[2])
                GUI.contentColor = Color.green;
            else
                GUI.contentColor = defaultGUIColor;

            if (GUILayout.Button("Doors", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                filters[2] = !filters[2];

            EditorGUILayout.Space(3);


            if (filters[3])
                GUI.contentColor = Color.green;
            else
                GUI.contentColor = defaultGUIColor;

            EditorGUILayout.Space(3);

            if (GUILayout.Button("Other", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                filters[3] = !filters[3];

            GUI.contentColor = defaultGUIColor;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            Color previousColor = GUI.color;

            if (!string.IsNullOrEmpty(searchPattern))
                GUI.color = Color.yellow;

            searchPattern = GUILayout.TextField(searchPattern, GUI.skin.FindStyle("ToolbarSearchTextField"));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                searchPattern = string.Empty;

            GUI.color = previousColor;

            GUIStyle foldoutStyle = EditorStyles.foldout;
            FontStyle previousFoldoutFont = foldoutStyle.fontStyle;

            GUIStyle buttonStyle = EditorStyles.toolbarButton;
            FontStyle previousButtonStyle = buttonStyle.fontStyle;

            windowScrollView =
                              EditorGUILayout.BeginScrollView(windowScrollView, GUILayout.Width(position.width - 10), GUILayout.Height(position.height));

            // Get all the CellInfo
            string currentCellDisplayed = "";
            int foldoutIndex = -1;
            for(int i = 0; i < references.Count; i++)
            {
                // If there's a new category
                if (references[i].cell.cell.sceneRef.SceneName != currentCellDisplayed)
                {
                    if(foldoutIndex != -1)
                    EditorGUILayout.Space(15);

                    EditorGUI.indentLevel = 0;
                    currentCellDisplayed = references[i].cell.cell.sceneRef.SceneName;

                    foldoutIndex++;

                    foldoutStyle.fontStyle = FontStyle.Bold;
                    // New Foldout
                    EditorGUILayout.BeginHorizontal("box");

                    foldouts[foldoutIndex] = EditorGUILayout.Foldout(foldouts[foldoutIndex], currentCellDisplayed, true, foldoutStyle);

                    GUILayout.FlexibleSpace();

                    buttonStyle.fontStyle = FontStyle.Bold;

                    if (GUILayout.Button("Enable All", GUILayout.ExpandWidth(false)))
                    {
                        /*
                        for (int z = 0; z < references[i].cell.refIDInThisCell.Count; z++)
                        {
                            // Check if we have to skip for Filter:
                            switch (editorPRef.GetPRefType(references[i].cell.refIDInThisCell[z]))
                            {
                                case PersistentReferenceType.AI:
                                    if (!filters[0])
                                        continue;
                                    break;

                                case PersistentReferenceType.LootingPoint:
                                    if (!filters[1])
                                        continue;
                                    break;

                                case PersistentReferenceType.Door:
                                    if (!filters[2])
                                        continue;
                                    break;

                                case PersistentReferenceType.Other:
                                    if (!filters[3])
                                        continue;
                                    break;
                            }

                            // Check if we have to skip for search
                            if (!string.IsNullOrEmpty(searchPattern.ToLower()))
                            {
                                if (!references[i].cell.refIDInThisCell[z].ToLower().Contains(searchPattern.ToLower()))
                                    continue;
                            }

                            editorPRef.ActivatePersistentReference(references[i].cell.refIDInThisCell[z]);
                        }
                        */
                    }
                    if (GUILayout.Button("Disable All", GUILayout.ExpandWidth(false)))
                    {
                        /*
                        for (int z = 0; z < references[i].cell.refIDInThisCell.Count; z++)
                        {
                            // Check if we have to skip for Filter:
                            switch (editorPRef.GetPRefType(references[i].cell.refIDInThisCell[z]))
                            {
                                case PersistentReferenceType.AI:
                                    if (!filters[0])
                                        continue;
                                    break;

                                case PersistentReferenceType.LootingPoint:
                                    if (!filters[1])
                                        continue;
                                    break;

                                case PersistentReferenceType.Door:
                                    if (!filters[2])
                                        continue;
                                    break;

                                case PersistentReferenceType.Other:
                                    if (!filters[3])
                                        continue;
                                    break;
                            }

                            // Check if we have to skip for search
                            if (!string.IsNullOrEmpty(searchPattern.ToLower()))
                            {
                                if (!references[i].cell.refIDInThisCell[z].ToLower().Contains(searchPattern.ToLower()))
                                    continue;
                            }

                            editorPRef.DisablePersistentReference(references[i].cell.refIDInThisCell[z]);
                        }
                        */
                    }

                    buttonStyle.fontStyle = previousButtonStyle;

                    EditorGUILayout.EndHorizontal();

                    foldoutStyle.fontStyle = previousFoldoutFont;
                }

                if (foldouts[foldoutIndex])
                {
                    bool isGameObjectActive = references[i].reference.gameObject.activeInHierarchy;
                    // Check if we have to skip for Filter:
                    switch (references[i].reference.type)
                    {
                        case PersistentReferenceType.AI:
                            if (!filters[0])
                                continue;
                            break;

                        case PersistentReferenceType.LootingPoint:
                            if (!filters[1])
                                continue;
                            break;

                        case PersistentReferenceType.Door:
                            if (!filters[2])
                                continue;
                            break;

                        case PersistentReferenceType.Other:
                            if (!filters[3])
                                continue;
                            break;
                    }

                    // Check if we have to skip for search
                    if(!string.IsNullOrEmpty(searchPattern.ToLower()))
                    {
                        if (!references[i].reference.refID.ToLower().Contains(searchPattern.ToLower()))
                            continue;
                    }

                    EditorGUI.indentLevel = 1;

                    var cellRect = EditorGUILayout.BeginHorizontal();
                    cellRect.x -= 100;

                    if (cellRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUI.DrawRect(cellRect, Color.gray);

                        Event e = Event.current;

                        if (Event.current.clickCount == 2)
                        {
                            Selection.activeGameObject = references[i].reference.gameObject;
                            SceneView.FrameLastActiveSceneView();
                        }
                        else if(Event.current.clickCount == 1)
                        {
                            Selection.activeGameObject = references[i].reference.gameObject;
                        }
                    }

                    EditorGUIUtility.labelWidth = 120;
                    EditorGUILayout.LabelField(references[i].reference.refID, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    
                    GUI.enabled = !isGameObjectActive;
                    if (GUILayout.Button("Enable", EditorStyles.toolbarButton, GUILayout.MaxWidth(50)))
                    {
                        references[i].reference.gameObject.SetActive(true);
                        Selection.activeGameObject = references[i].reference.gameObject;
                    }

                    GUI.enabled = isGameObjectActive;
                    if (GUILayout.Button("Disable", EditorStyles.toolbarButton, GUILayout.MaxWidth(50)))
                        references[i].reference.gameObject.SetActive(false);

                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }
            }


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        void RefreshWindow()
        {
            foldouts.Clear();
            references.Clear();

            cells = FindObjectsOfType<CellInformation>();
            sCells = cells;
            RefreshReferences();
            UpdateOnSceneChanges();
        }

        void RefreshReferences()
        {
            foldouts.Clear();
            references.Clear();

            if (editorPRef == null)
                editorPRef = FindObjectOfType<PersistentReferenceManager>();

            if (editorPRef == null)
                return;

            for (int i = 0; i < cells.Length; i++)
            {
                /*
                for(int j = 0; j < cells[i].refIDInThisCell.Count; j++)
                {
                    PersistentReferenceInWindow newRef = new PersistentReferenceInWindow(editorPRef.GetPersistentReference(cells[i].refIDInThisCell[j]), cells[i]);
                    references.Add(newRef);
                }
                */

                bool newFoldout = true;
                foldouts.Add(newFoldout);
            }
        }

        static void SRefreshCells()
        {
            sCells = FindObjectsOfType<CellInformation>();
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

        public void OnInspectorUpdate()
        {
            Repaint();
        }

        void UpdateOnSceneChanges()
        {
            EditorSceneManager.sceneClosing -= SceneClosing;
            EditorSceneManager.sceneClosed -= SceneClosed;
            EditorSceneManager.sceneOpening -= SceneOpening;
            EditorSceneManager.sceneOpened -= SceneOpened;
            EditorSceneManager.newSceneCreated -= NewSceneCreated;
            EditorApplication.playModeStateChanged -= LogPlayModeState;

            EditorSceneManager.sceneClosing += SceneClosing;
            EditorSceneManager.sceneClosed += SceneClosed;
            EditorSceneManager.sceneOpening += SceneOpening;
            EditorSceneManager.sceneOpened += SceneOpened;
            EditorSceneManager.newSceneCreated += NewSceneCreated;
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        static void SceneClosing(UnityEngine.SceneManagement.Scene scene, bool removingScene)
        {
            SRefreshCells();
        }

        static void SceneClosed(UnityEngine.SceneManagement.Scene scene)
        {
            SRefreshCells();
        }

        static void SceneOpening(string path, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            SRefreshCells();
        }

        static void SceneOpened(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            SRefreshCells();
        }

        static void NewSceneCreated(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.NewSceneSetup setup, UnityEditor.SceneManagement.NewSceneMode mode)
        {
            SRefreshCells();
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            SRefreshCells();
        }

    }
}