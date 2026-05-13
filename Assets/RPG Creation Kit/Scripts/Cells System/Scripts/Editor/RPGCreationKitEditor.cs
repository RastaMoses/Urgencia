using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Reflection;
using System;
using RPGCreationKit.PersistentReferences;

namespace RPGCreationKit
{ 
    public class RPGCreationKitEditor : EditorWindow
    {
        static bool stopWorldStreaming = false;
        static bool startWorldStreaming = false;

        Vector2 scrollPos;

        // -----------------------------------------------------------------------------
        // For Streaming in Editor
        // -----------------------------------------------------------------------------
        bool streamWoldspaceInEditor;

        bool loadAllNpcsInOpenedScenesWhileStreaming = false;
        bool loadAllNpcsInActiveSceneWhileStreaming = false;

        static PersistentReferenceManager editorPRef;

        Vector3 sceneViewPos;
        Quaternion sceneViewRot;

        private bool isStreaming;
        Worldspace curWoldspace;
        Cell currentCenterCell;

        private List<string> scenesLoaded = new List<string>();

        // -----------------------------------------------------------------------------
        // For auto-generate NavMeshes of a Worldspace
        // -----------------------------------------------------------------------------
        Worldspace worldspace;

        public static void StartEditorWorldStreaming()
        {
            GetPersistentReferences();

            startWorldStreaming = true;
        }

        public static void StopEditorWorldStreaming()
        {
            stopWorldStreaming = true;
            toggleStreaming = false;
        }

        public static void GetPersistentReferences()
        {
            if (!IsSceneLoaded("_PersistentReferences_"))
            {
                // Open scene in single mode
                string[] guids = AssetDatabase.FindAssets("t:scene _PersistentReferences_");
                EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guids[0]), OpenSceneMode.Additive);
            }

            editorPRef = FindObjectOfType<PersistentReferenceManager>();
        }

        [MenuItem("RPG Creation Kit/RPG Creation Kit Editor")]
        private static void OpenWindow()
        {
            RPGCreationKitEditor window = GetWindow<RPGCreationKitEditor>();

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RPGCKEditorWindowIcon);

            GUIContent titleContent = new GUIContent("RPG Creation Kit Editor", icon);
            window.titleContent = titleContent;
        }

        bool streamingError = false;
        string streamingErrorType = "";
        static bool toggleStreaming = false;
        private void OnGUI()
        {
            if (stopWorldStreaming)
            {
                if (streamWoldspaceInEditor)
                {
                    foreach (SceneView scene in SceneView.sceneViews)
                    {
                        scene.ShowNotification(new GUIContent("RPG Creation Kit:\nStopping Streaming the World."), 1);
                        scene.Repaint();
                    }
                }

                streamWoldspaceInEditor = false;
                stopWorldStreaming = false;
            }

            if (startWorldStreaming)
            {
                if (!streamWoldspaceInEditor)
                {
                    foreach (SceneView scene in SceneView.sceneViews)
                    {
                        scene.ShowNotification(new GUIContent("RPG Creation Kit:\nStarting Streaming the World."), 1);
                        scene.Repaint();
                    }
                }

                streamWoldspaceInEditor = true;
                startWorldStreaming = false;
            }


            EditorGUIUtility.labelWidth = 165;

            scrollPos =
                EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor World Streaming", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            Color dColor = GUI.color;

            if(streamingError)
            {
                switch (streamingErrorType)
                {
                    case "E001":
                        EditorGUILayout.HelpBox("Could not stream from the current cell. The CellInformation is not present in the current scene or it is disabled, is this a Worldspace Cell?", MessageType.Error);
                        break;
                    case "E002":
                        EditorGUILayout.HelpBox("Could not stream from the current cell. The current Cell is not correctly setup - missing references to CellInfo or Worldspace.", MessageType.Error);
                        break;
                }

                StopEditorWorldStreaming();
            }

            if (streamWoldspaceInEditor && !streamingError)
            {
                GUI.color = Color.green;
                EditorGUILayout.HelpBox("Currently streaming in the editor!", MessageType.Info);
                GUI.color = dColor;

                sceneViewPos = SceneView.lastActiveSceneView.camera.transform.position;
                sceneViewRot = SceneView.lastActiveSceneView.camera.transform.rotation;

                EditorWorldStreaming();

                SceneView.lastActiveSceneView.Repaint();
                Repaint();
            }
            else if(!streamingError)
            {
                EditorGUILayout.HelpBox("Allows you to stream the Worldspace in the editor from the current opened scene", MessageType.Info);
            }

            // Toggle
            EditorGUI.BeginChangeCheck();

            toggleStreaming = EditorGUILayout.Toggle("Stream Worldspace in Editor", streamWoldspaceInEditor);

            if (EditorGUI.EndChangeCheck())
            {
                streamingError = false;
                currentCenterCell = null;
                scenesLoaded.Clear();

                if (toggleStreaming)
                {
                    CellInformation cellInfo = GameObject.FindObjectOfType<CellInformation>();
                    if (cellInfo == null)
                    {
                        streamingError = true;
                        streamingErrorType = "E001";
                        return;
                    }

                    try
                    {
                        curWoldspace = cellInfo.cell.worldspace;
                        StartEditorWorldStreaming();
                    }
                    catch
                    {
                        streamingError = true;
                        streamingErrorType = "E002";
                        return;
                    }
                } else
                {
                    StopEditorWorldStreaming();
                }
            }

            EditorGUIUtility.labelWidth = 195;

            /*
            GUI.enabled = (streamWoldspaceInEditor);

            GUI.enabled = (!loadAllNpcsInActiveSceneWhileStreaming);
            loadAllNpcsInOpenedScenesWhileStreaming = EditorGUILayout.Toggle(new GUIContent("Load All NPCs in opened scenes", "Loads all the NPCs in the currently open scenes and keeps loading while streaming."), loadAllNpcsInOpenedScenesWhileStreaming);

            GUI.enabled = (!loadAllNpcsInOpenedScenesWhileStreaming);
            loadAllNpcsInActiveSceneWhileStreaming = EditorGUILayout.Toggle(new GUIContent("Load All NPCs in the active scene", "Loads all the NPCs in the currently active scene and keeps loading while streaming."), loadAllNpcsInActiveSceneWhileStreaming);
            GUI.enabled = true;

            EditorGUIUtility.labelWidth = 165;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("NPCs Management", EditorStyles.boldLabel);
            //GUIShowLoadNPCOptions();
            //GUIShowUnloadNPCOptions();

            if (GUILayout.Button("Load Persistent Reference"))
            {
                UnityEditor.EditorApplication.delayCall += LoadPersistentReference;
            }
            */

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.EndScrollView();
        }

        Vector3 lastUpdatedSceneViewPos;
        Quaternion lastUpdatedSceneViewRot;

        /// <summary>
        /// Starting from a cell, streams the world whitin the editor
        /// </summary>
        void EditorWorldStreaming()
        {
            // To avoid keep calling this function when the scene view isn't moving
            if (lastUpdatedSceneViewPos == sceneViewPos && // TODO IF THERE'S A DIFFERENCE OF LIKE 20 UNITS CHECK OTHERWISE DONT
                lastUpdatedSceneViewRot == sceneViewRot)
                return;

            // Check player position to select adeguate cells
            // Determine where the player is, center cell
            Cell checkedCenterCell;
            float shortestDistance = 99999.999f;
            int shortestDistanceID = 0;
            for (int i = 0; i < curWoldspace.cells.Length; i++)
            {
                float curDistance = Vector3.Distance(sceneViewPos, curWoldspace.cells[i].cellInWorldCoordinates);
                if (curDistance < shortestDistance)
                {
                    shortestDistance = curDistance;
                    shortestDistanceID = i;
                }
            }

            checkedCenterCell = curWoldspace.cells[shortestDistanceID];

            if (currentCenterCell == null || currentCenterCell.ID != checkedCenterCell.ID)
            {
                isStreaming = true;

                List<string> alreadyLoadedCells = new List<string>();
                for (int i = 0; i < checkedCenterCell.neighboringCells.Length; i++)
                {
                    if (checkedCenterCell.neighboringCells[i] == null) continue;

                    if (IsSceneLoaded(checkedCenterCell.neighboringCells[i].sceneRef.SceneName))
                        alreadyLoadedCells.Add(checkedCenterCell.neighboringCells[i].sceneRef.ScenePath);
                }

                // Set center cell as currently active
                EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByPath(checkedCenterCell.sceneRef.ScenePath));
                RckCustomToolbar.OnSliderValueChanged(RckCustomToolbar.curSliderValue);


                // Load neighboring cells
                for (int i = 0; i < checkedCenterCell.neighboringCells.Length; i++)
                {
                    if (checkedCenterCell.neighboringCells[i] == null || alreadyLoadedCells.Contains(checkedCenterCell.neighboringCells[i].sceneRef.ScenePath)) // empty cell (end of map)
                        continue;

                    var openedScene = EditorSceneManager.OpenScene(checkedCenterCell.neighboringCells[i].sceneRef.ScenePath, OpenSceneMode.Additive);

                    scenesLoaded.Add(checkedCenterCell.neighboringCells[i].sceneRef.ScenePath);
                }

            }

            currentCenterCell = checkedCenterCell;
            UnloadUnecessaryScenes();

            isStreaming = false;

            lastUpdatedSceneViewPos = sceneViewPos;
            lastUpdatedSceneViewRot = sceneViewRot;
        }


        public void UnloadUnecessaryScenes()
        {
            // Unload unecessary scenes
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                Scene scene = EditorSceneManager.GetSceneAt(i);

                if (scene.name.Contains("WorldLoader"))
                    continue;

                bool shouldUnload = true;

                for (int j = 0; j < currentCenterCell.neighboringCells.Length; j++)
                {

                    if (currentCenterCell.neighboringCells[j] == null)
                        continue;

                    if (currentCenterCell.neighboringCells[j].sceneRef.SceneName == (scene.name))
                    {
                        shouldUnload = false;
                        break;
                    }
                }


                if (shouldUnload)
                {
                    #region disablenpc
                    /*
                    var openedSceneRootObjects = scene.GetRootGameObjects();

                    // Find CellInfo
                    for (int j = 0; j < openedSceneRootObjects.Length; j++)
                    {
                        // Unload NPCs in this cell
                        if (openedSceneRootObjects[j].CompareTag("RPG Creation Kit/CellInfo"))
                        {
                            // Unload NPCs
                            CellInformation cellInfo = openedSceneRootObjects[j].GetComponent<CellInformation>();
                            cellInfo.UnloadNPCsInThisCell();
                        }
                    }
                    */
                    #endregion

                    scenesLoaded.RemoveAll(u => u.Contains(scene.name));
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        bool HasFinishedLoading(Cell _centerCell)
        {
            for(int i = 0; i < _centerCell.neighboringCells.Length; i++)
            {
                if(_centerCell.neighboringCells[i] != null)
                {
                    if (!IsSceneLoaded(_centerCell.neighboringCells[i].sceneRef.SceneName))
                        return false;
                }
            }

            return true;
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

        void OpenRefIDWindowDelay()
        {
            // Open the window and wait for input
            
        }

        void LoadPersistentReference()
        {
            if (!editorPRef)
                GetPersistentReferences();

            InsertRefIDWindowPopup.OpenWindow(new InsertRefIDWindowSettings(true));

            // Check the given RefID
            if (!string.IsNullOrEmpty(InsertRefIDWindowPopup.lastInsertedRefID))
            {
                try
                {
                    if (InsertRefIDWindowPopup.settingsValues.usesFocusOnRefID)
                    {
                        GameObject refLoaded = editorPRef.ActivatePersistentReferenceAndGetGameObject(InsertRefIDWindowPopup.lastInsertedRefID);
                        Selection.activeGameObject = refLoaded;
                        SceneView.FrameLastActiveSceneView();
                    }
                    else
                    {
                        editorPRef.ActivatePersistentReference(InsertRefIDWindowPopup.lastInsertedRefID);
                    }
                }
                catch
                {
                    EditorUtility.DisplayDialog("Warning!", "There is no NPC with the RefID: " + InsertRefIDWindowPopup.lastInsertedRefID + " in the Persistent References", "Close");
                }
            }
            else
                return;
        }
    }

    /*
    void GUIShowLoadNPCOptions()
    {
        if (GUILayout.Button("Load NPCs"))
        {
            GenericMenu loadNpcMenu = new GenericMenu();

            loadNpcMenu.AddDisabledItem(new GUIContent("Load NPCs"));
            loadNpcMenu.AddSeparator("");
            loadNpcMenu.AddItem(new GUIContent("Load all NPCs in open scenes"), false, LoadAllNPCsInOpenScenes);
            loadNpcMenu.AddItem(new GUIContent("Load all NPCs in the active scene"), false, LoadAllNPCsInActiveScene);
            loadNpcMenu.AddItem(new GUIContent("Load NPC with RefID"), false, LoadNPCWithRefID);
            loadNpcMenu.AddSeparator("");
            loadNpcMenu.AddItem(new GUIContent("Cancel"), false, Cancel);

            loadNpcMenu.ShowAsContext();
        }

        /// Loads all the NPCs in the opened scenes
        void LoadAllNPCsInOpenScenes()
        {
            if (!editorPRef)
                GetPersistentReferences();

            GameObject[] cellInfos = GameObject.FindGameObjectsWithTag("RPG Creation Kit/CellInfo");

            CellInformation cellInformation;
            for(int i = 0; i < cellInfos.Length; i++)
            {
                cellInformation = cellInfos[i].GetComponent<CellInformation>();

                for(int j = 0; j < cellInformation.npcsInThisCell.Count; j++)
                    editorPRef.LoadNPC(cellInformation.npcsInThisCell[j]);
            }

        }

        /// Loads all the NPCs in the active scene
        void LoadAllNPCsInActiveScene()
        {
            if (!editorPRef)
                GetPersistentReferences();

            GameObject[] activeRootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            CellInformation cellInformation = null;

            // Find the CellInfo of the active scene
            foreach(GameObject g in activeRootObjects)
            {
                if(g.CompareTag("RPG Creation Kit/CellInfo"))
                {
                    cellInformation = g.GetComponent<CellInformation>();
                    break;
                }
            }

            // If CellInfo is found load every NPC in this cell
            if(cellInformation)
            {
                for (int i = 0; i < cellInformation.npcsInThisCell.Count; i++)
                    editorPRef.LoadNPC(cellInformation.npcsInThisCell[i]);
            }
        }

        /// Loads the NPC with the provided RefID


    void GUIShowUnloadNPCOptions()
    {
        if (GUILayout.Button("Unload NPCs"))
        {
            GenericMenu unloadNpcMenu = new GenericMenu();

            unloadNpcMenu.AddDisabledItem(new GUIContent("Load NPCs"));
            unloadNpcMenu.AddSeparator("");
            unloadNpcMenu.AddItem(new GUIContent("Unload all NPCs in open scenes"), false, UnloadAllNPCsInOpenScenes);
            unloadNpcMenu.AddItem(new GUIContent("Unload all NPCs in the active scene"), false, UnloadAllNPCsInActiveScene);
            unloadNpcMenu.AddItem(new GUIContent("Unload NPC with RefID"), false, UnloadNPCWithRefID);
            unloadNpcMenu.AddSeparator("");
            unloadNpcMenu.AddItem(new GUIContent("Cancel"), false, Cancel);

            unloadNpcMenu.ShowAsContext();
        }

        /// Unloads all the NPCs in all opened scenes
        void UnloadAllNPCsInOpenScenes()
        {
            if (!editorPRef)
                GetPersistentReferences();

            GameObject[] cellInfos = GameObject.FindGameObjectsWithTag("RPG Creation Kit/CellInfo");

            CellInformation cellInformation;
            for (int i = 0; i < cellInfos.Length; i++)
            {
                cellInformation = cellInfos[i].GetComponent<CellInformation>();

                for (int j = 0; j < cellInformation.npcsInThisCell.Count; j++)
                    editorPRef.UnloadNPC(cellInformation.npcsInThisCell[j]);
            }
        }

        /// Unload all NPCs in the active scene
        void UnloadAllNPCsInActiveScene()
        {
            if (!editorPRef)
                GetPersistentReferences();

            GameObject[] activeRootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            CellInformation cellInformation = null;

            // Find the CellInfo of the active scene
            foreach (GameObject g in activeRootObjects)
            {
                if (g.CompareTag("RPG Creation Kit/CellInfo"))
                {
                    cellInformation = g.GetComponent<CellInformation>();
                    break;
                }
            }

            // If CellInfo is found load every NPC in this cell
            if (cellInformation)
            {
                for (int i = 0; i < cellInformation.npcsInThisCell.Count; i++)
                    editorPRef.UnloadNPC(cellInformation.npcsInThisCell[i]);
            }
        }

        /// Unload the NPC with the provided RefID
        void UnloadNPCWithRefID()
        {
            if (!editorPRef)
                GetPersistentReferences();

            InsertRefIDWindowPopup.OpenWindow();

            if (!string.IsNullOrEmpty(InsertRefIDWindowPopup.lastInsertedRefID))
            {
                try
                {
                    editorPRef.UnloadNPC(InsertRefIDWindowPopup.lastInsertedRefID);
                }
                catch
                {
                    EditorUtility.DisplayDialog("Warning!", "There is no NPC with the RefID: " + InsertRefIDWindowPopup.lastInsertedRefID + " in the Persistent References", "Close");
                }
            }
            else
                return;
        }

        void Cancel()
        {

        }
    }
    */
}
