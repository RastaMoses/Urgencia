using RPGCreationKit.CellsSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEngine.GridBrushBase;

namespace RPGCreationKit.CellsSystem
{
    public class CellView : EditorWindow
    {
        bool lctrlHeld = false;
        Texture refreshButtonIcon;
        Texture worldLoaderButtonIcon;
        Texture mainMenuLoaderButtonIcon;

        bool init = false;
        public List<Worldspace> worldspaces;
        public Worldspace selectedWorldspace;

        Vector2 cellsScrollPos;

        string searchString = string.Empty;

        string lastLoadedCell = string.Empty;

        bool showDemoFiles = true;


        [MenuItem("RPG Creation Kit/Cells System/Cell View")]
        private static void OpenWindow()
        {
            CellView window = GetWindow<CellView>();

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.CellViewWindowIcon);

            GUIContent titleContent = new GUIContent("Cell View", icon);
            window.titleContent = titleContent;

            window.Show();
        }

        private void OnGUI()
        {
            // Initialize the window
            if (!init)
            {
                showDemoFiles = RckProjectSettings.instance.showDemoFiles;

                RefreshWorldspaces();
                refreshButtonIcon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RefreshButton);
                worldLoaderButtonIcon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.WorldLoaderIcon);
                mainMenuLoaderButtonIcon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.MainMenuIcon);

                if(selectedWorldspace == null)
                    selectedWorldspace = null;


                init = true;
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Worldspace:", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            showDemoFiles = EditorGUILayout.Toggle("Show Demo Files?", showDemoFiles);
            if(EditorGUI.EndChangeCheck())
            {
                RefreshWorldspaces();
            }


            if (GUILayout.Button(new GUIContent(mainMenuLoaderButtonIcon, "Loads the scene _MainMenu_."), GUILayout.MaxWidth(32)))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    // Open scene in single mode
                    string[] guidsWL = AssetDatabase.FindAssets("t:scene _MainMenu_");
                    EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guidsWL[0]), OpenSceneMode.Single);
                }
            }

            if (GUILayout.Button(new GUIContent(worldLoaderButtonIcon, "Loads the scene _WorldLoader_."), GUILayout.MaxWidth(32)))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    // Open scene in single mode
                    string[] guidsWL = AssetDatabase.FindAssets("t:scene _WorldLoader_");
                    EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guidsWL[0]), OpenSceneMode.Single);

                    string[] guidsPR = AssetDatabase.FindAssets("t:scene _PersistentReferences_");
                    EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guidsPR[0]), OpenSceneMode.Additive);
                }
            }

            if (GUILayout.Button(new GUIContent(refreshButtonIcon, "Refreshes the list. Use it when you create new cells or worldspaces."), GUILayout.MaxWidth(32)))
            {
                init = false;
            }
            EditorGUILayout.EndHorizontal();

            string menuDisplayValue = "";
            if (selectedWorldspace != null)
                menuDisplayValue = string.IsNullOrEmpty(selectedWorldspace.worldSpaceName) ? "-Select Worldspace-" : selectedWorldspace.worldSpaceName;
            else
                menuDisplayValue = "-Select Worldspace-";

            if (GUILayout.Button(menuDisplayValue))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                menu.AddDisabledItem(new GUIContent("Worldspaces"));

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("None"), false, Callback, null);
                for (int i = 0; i < worldspaces.Count; i++)
                {
                    menu.AddItem(new GUIContent(worldspaces[i].worldSpaceName), false, Callback, worldspaces[i]);
                }

                menu.ShowAsContext();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            Color previousColor = GUI.color;
            if (!string.IsNullOrEmpty(searchString))
                GUI.color = Color.yellow;

            EditorGUILayout.BeginHorizontal();
            searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                searchString = string.Empty;
            EditorGUILayout.EndHorizontal();
            GUI.color = previousColor;

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cells:", EditorStyles.boldLabel);
            
            if(selectedWorldspace.worldSpaceType == Worldspace.WorldSpaceType.Exterior)
                if(GUILayout.Button("Load All"))
                {
                    bool choice = EditorUtility.DisplayDialog("Warning!",
                            "Are you sure you want to load the whole worldspace in the editor?",
                            "Yes",
                            "Cancel");

                    if (choice)
                    {
                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                        var scene = EditorSceneManager.OpenScene(selectedWorldspace.cells[0].sceneRef.ScenePath, OpenSceneMode.Single);

                        for(int i = 1; i < selectedWorldspace.cells.Length; i++)
                        {
                            EditorSceneManager.OpenScene(selectedWorldspace.cells[i].sceneRef.ScenePath, OpenSceneMode.Additive);
                        }
                    }
                }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical("box");
            EditorGUIUtility.labelWidth = 45;

            EditorGUILayout.BeginScrollView(cellsScrollPos, GUIStyle.none, GUIStyle.none, GUILayout.Width(position.width - 10), GUILayout.Height(35));
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("ID:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Name:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Coordinates:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Scene Name:", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            GUI.enabled = selectedWorldspace;
            cellsScrollPos = 
                EditorGUILayout.BeginScrollView(cellsScrollPos, GUILayout.Width(position.width - 10), GUILayout.Height(position.height - 165));

            // Open scenes
            if (selectedWorldspace)
            {
                for (int i = 0; i < selectedWorldspace.cells.Length; i++)
                {
                    EditorGUIUtility.labelWidth = 45;

                    // Check if we have to skip for search
                    if (!string.IsNullOrEmpty(searchString.ToLower()))
                        if (!selectedWorldspace.cells[i].ID.ToLower().Contains(searchString.ToLower()))
                            continue;

                    var cellRect = EditorGUILayout.BeginHorizontal("box");

                    if (cellRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUI.DrawRect(cellRect, Color.gray);

                        Event e = Event.current;
                        if (e.type == EventType.KeyDown)
                        {
                            if (e.keyCode == KeyCode.LeftControl)
                                lctrlHeld= (true);
                        }
                        else if (e.type == EventType.KeyUp)
                        {
                            if (e.keyCode == KeyCode.LeftControl)
                                lctrlHeld = (false);
                        }

                        if (Event.current.clickCount == 2)
                        {
                            lastLoadedCell = selectedWorldspace.cells[i].sceneRef.ScenePath;

                            if (lctrlHeld) // open scene in additive mode
                            {
                                EditorSceneManager.OpenScene(selectedWorldspace.cells[i].sceneRef.ScenePath, OpenSceneMode.Additive);
                                RckCustomToolbar.OnSliderValueChanged(RckCustomToolbar.curSliderValue);
                            }
                            else
                            {
                                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                {
                                    // Open scene in single mode
                                    var scene = EditorSceneManager.OpenScene(selectedWorldspace.cells[i].sceneRef.ScenePath, OpenSceneMode.Single);

                                    RckCustomToolbar.OnSliderValueChanged(RckCustomToolbar.curSliderValue);

                                    // get the CellInfo GameObject to center the view as soon as we load
                                    GameObject[] gos = scene.GetRootGameObjects();
                                    for (int z = 0; z < gos.Length; z++)
                                        if (gos[z].CompareTag("RPG Creation Kit/CellInfo"))
                                        {
                                            Selection.activeObject = gos[z];
                                            SceneView.FrameLastActiveSceneView();
                                            break;
                                        }
                                }
                            }
                        }
                    }

                    EditorGUILayout.LabelField(selectedWorldspace.cells[i].ID);
                    EditorGUILayout.LabelField(selectedWorldspace.cells[i].cellName);
                    EditorGUILayout.LabelField(selectedWorldspace.cells[i].cellCoordinates.ToString());
                    EditorGUILayout.LabelField(selectedWorldspace.cells[i].sceneRef.SceneName);
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void Callback(object _worldspace)
        {
            selectedWorldspace = (Worldspace)_worldspace;
        }

        private void RefreshWorldspaces()
        {
            worldspaces = GetAllInstances<Worldspace>();

            if(!showDemoFiles)
            {
                for (int i = worldspaces.Count - 1; i >= 0; i--)
                    if (worldspaces[i]._IS_DEMO_FILE)
                        worldspaces.RemoveAt(i);
            }
        }

        public static List<T> GetAllInstances<T>() where T : Worldspace
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

        void OnInspectorUpdate()
        {
            if (EditorWindow.focusedWindow == this &&
                EditorWindow.mouseOverWindow == this)
            {
                this.Repaint();
            }
            else
            {
                if(lctrlHeld)
                    lctrlHeld = false;
            }
        }

        void OnFocus()
        {
            RefreshWorldspaces();
        }

        public void LoadLastLoadedCell()
        {
            if(!string.IsNullOrEmpty(lastLoadedCell))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    // Open scene in single mode
                    var scene = EditorSceneManager.OpenScene(lastLoadedCell, OpenSceneMode.Single);
                    RckCustomToolbar.OnSliderValueChanged(RckCustomToolbar.curSliderValue);
                }
            }
        }

        public void SetLastCellString(string cellString)
        {
            lastLoadedCell = cellString;
        }

        public void LoadMainMenu()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // Open scene in single mode
                string[] guidsWL = AssetDatabase.FindAssets("t:scene _MainMenu_");
                EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guidsWL[0]), OpenSceneMode.Single);
            }
        }
    }
}