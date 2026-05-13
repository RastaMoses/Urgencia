using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using System;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using RPGCreationKit.PersistentReferences;

namespace RPGCreationKit.RCKEditor
{
    /// <summary>
    /// Window that allows to create the distance cells for a given worldspace.
    /// </summary>
    public class DistantCellRenderingWindow : EditorWindow
    {
        [SerializeField]
        SerializedObject sObject;

        bool assembleWorldspace = true;

        Worldspace worldspaceToAssemble;
        public GameObject terrainPreset;
        bool autoSetDistantObjectInCell = true;

        [SerializeField]
        ReorderableList scenesToAssembleList;


        [SerializeField]
        List<SceneReferenceLite> scenesToAssemble;

        Vector2 scenesScrollView;
        SerializedProperty property;

        string outpath = "Assets/RPG Creation Kit/Terrain/DistantCells/";


        [MenuItem("RPG Creation Kit/Distant Cell Rendering")]
        private static void OpenWindow()
        {
            DistantCellRenderingWindow window = GetWindow<DistantCellRenderingWindow>();

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RPGCKEditorWindowIcon);

            GUIContent titleContent = new GUIContent("Distant Cell Rendering", icon);
            window.titleContent = titleContent;
            window.minSize.Set(300,600);
        }

        private void Awake()
        {
            scenesToAssemble = new List<SceneReferenceLite>();

            ScriptableObject target = this;
            sObject = new SerializedObject(target);
            property = sObject.FindProperty("scenesToAssemble");

            scenesToAssembleList = new ReorderableList(sObject, sObject.FindProperty("scenesToAssemble"), true, true, true, true);
            scenesToAssembleList.list = scenesToAssemble;
        }


        private void OnGUI()
        {
            GUIStyle TitleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            TitleStyle.fontSize = 20;

            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            ButtonStyle.border = new RectOffset(2, 2, 2, 2);
            ButtonStyle.fontSize = 16;

            if (sObject == null)
            {
                ScriptableObject target = this;
                sObject = new SerializedObject(target);
                property = sObject.FindProperty("scenesToAssemble");
            }

            if (scenesToAssembleList == null)
            {
                scenesToAssembleList = new ReorderableList(sObject, sObject.FindProperty("scenesToAssemble"), true, true, true, true);
                scenesToAssembleList.list = scenesToAssemble;
            }

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("DISTANT CELL RENDERING", TitleStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("This window allows you to quickly setup distant terrains for a given worldspace or a list of cells.", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal("box", GUILayout.MaxWidth(50));
            EditorGUILayout.LabelField("Mode: ", GUILayout.MaxWidth(65));

            if (GUILayout.Button(new GUIContent((assembleWorldspace) ? "Worldspace" : "Scenes"), GUILayout.MaxWidth(120)))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Worldspace"), false, Callback, true);
                menu.AddItem(new GUIContent("Individual Scenes"), false, Callback, false);

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();

            if (assembleWorldspace)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Add from worldspace:");
                worldspaceToAssemble = (Worldspace)EditorGUILayout.ObjectField(worldspaceToAssemble, typeof(Worldspace), false, GUILayout.MaxWidth(250));

                GUI.enabled = (worldspaceToAssemble != null);

                if (GUILayout.Button("Add scenes", GUILayout.ExpandWidth(false)))
                {
                    for (int i = 0; i < worldspaceToAssemble.cells.Length; i++)
                    {
                        scenesToAssembleList.list.Add((SceneReferenceLite)worldspaceToAssemble.cells[i].sceneRef);
                    }

                    sObject.Update();
                }

                EditorGUILayout.EndHorizontal();
            }

            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Terrain Preset:");
            terrainPreset = (GameObject)EditorGUILayout.ObjectField(terrainPreset, typeof(GameObject), false, GUILayout.MaxWidth(250));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Auto-Set object in Cell:");
            autoSetDistantObjectInCell = EditorGUILayout.Toggle(autoSetDistantObjectInCell);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            scenesScrollView =
                            EditorGUILayout.BeginScrollView(scenesScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.MaxHeight(250.0f));

            scenesToAssembleList.DoLayoutList();
            scenesToAssembleList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Scenes To Render");

            };


            scenesToAssembleList.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = property.GetArrayElementAtIndex(index);
                rect.y += 2;

                EditorGUI.LabelField(rect, "Scene: " + index.ToString());

                rect.x += 65;
                rect.xMax -= 65;
                EditorGUI.PropertyField(
                    rect,
                    element, GUIContent.none);
            };
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear All", GUILayout.ExpandWidth(false)))
                ClearScenesToAssemble();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Output path:");
            outpath = EditorGUILayout.TextField(outpath);

            EditorGUILayout.Space(30);

            if (GUILayout.Button("GENERATE FOR ALL", ButtonStyle))
                AssembleAll();
        }

        private void AssembleAll()
        {
            if(terrainPreset == null)
                EditorUtility.DisplayDialog("Distant Cell Rendering", "You have not selected a preset.", "Close");
            else if (scenesToAssembleList.list.Count <= 0)
                EditorUtility.DisplayDialog("Distant Cell Rendering", "You haven't assigned any scenes to render.", "Close");
            else
            {
                if (EditorUtility.DisplayDialog("Distant Cell Rendering", "Are you sure to render " + scenesToAssembleList.list.Count + " scenes? Generating distant terrain to render may take several minutes, depending on the number of scenes, their complexity and the number of Persistent References in them.", "Generate", "Cancel"))
                {
                    // Check folder path
                    if(!AssetDatabase.IsValidFolder(outpath))
                    {
                        EditorUtility.DisplayDialog("Distant Cell Rendering", "Error! The path you've specified \"" + outpath + "\" is not a valid folder.", "OK");
                        return;
                    }

                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        Terrain presetComponent = terrainPreset.GetComponent<Terrain>();
                        if(terrainPreset == null)
                        {
                            EditorUtility.DisplayDialog("Distant Cell Rendering", "ERROR! The selected terrain preset does not have a 'Terrain' component attached to it, therefore is not valid.", "Abort");
                            return;
                        }

                        for (int i = 0; i < scenesToAssemble.Count; i++)
                        {
                            EditorUtility.DisplayProgressBar("Generating Distant Cells to Render...", "Working on " + scenesToAssemble[i].SceneName, ((float)i) / (float)scenesToAssemble.Count);

                            // If reference is not assigned, skip.
                            if (scenesToAssemble[i] == null)
                                continue;

                            // Load the current scene
                            var loadedScene = EditorSceneManager.OpenScene(scenesToAssemble[i].ScenePath, OpenSceneMode.Single);
                            EditorSceneManager.SetActiveScene(loadedScene);

                            // Find the terrain object
                            GameObject actualTerrainObject = GameObject.FindObjectOfType<Terrain>().gameObject;

                            if(actualTerrainObject != null)
                            {
                                GameObject distantTerrainObject = Instantiate(actualTerrainObject);
                                distantTerrainObject.name = "DISTANT_" + distantTerrainObject.name;

                                // Set distant terrain properties
                                Terrain terrain = distantTerrainObject.GetComponent<Terrain>();

                                terrain.drawInstanced = presetComponent.drawInstanced;
                                terrain.heightmapPixelError = presetComponent.heightmapPixelError;
                                terrain.basemapDistance = presetComponent.basemapDistance;
                                terrain.shadowCastingMode = presetComponent.shadowCastingMode;
                                terrain.reflectionProbeUsage = presetComponent.reflectionProbeUsage;
                                terrain.bakeLightProbesForTrees = presetComponent.bakeLightProbesForTrees;
                                terrain.detailObjectDistance = presetComponent.detailObjectDistance;
                                terrain.detailObjectDensity = presetComponent.detailObjectDensity;
                                terrain.treeDistance = presetComponent.treeDistance;
                                terrain.treeCrossFadeLength = presetComponent.treeCrossFadeLength;
                                terrain.treeMaximumFullLODCount = presetComponent.treeMaximumFullLODCount;

                                // Find and place Distant Object inside the terrain
                                DistantObjectTag[] distantObjects = GameObject.FindObjectsOfType<DistantObjectTag>();

                                foreach (DistantObjectTag distantObject in distantObjects)
                                {
                                    GameObject newObj = Instantiate(distantObject.gameObject, null);
                                    newObj.transform.position = distantObject.transform.position;
                                    newObj.transform.SetParent(distantTerrainObject.transform);
                                }


                                string path = AssetDatabase.GenerateUniqueAssetPath(outpath + "/" + distantTerrainObject.name + ".prefab");
                                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(distantTerrainObject, path);

                                // Auto set
                                if(autoSetDistantObjectInCell)
                                {
                                    // Get CellInformation
                                    CellInformation cellInfo = FindObjectOfType<CellInformation>();

                                    if(cellInfo != null)
                                    {
                                        cellInfo.cell.distantCellObject = savedPrefab;
                                        EditorUtility.SetDirty(cellInfo.cell);
                                    }
                                    else
                                    {
                                        Debug.LogError("DistantCellRenderingWindow: Tried to autoset the distant terrain object of the scene " + loadedScene.name + " but it has no cell information!");
                                    }

                                }

                                DestroyImmediate(distantTerrainObject);

                                Debug.Log("Done in " + loadedScene.name);
                            }
                            


                            EditorSceneManager.MarkSceneDirty(loadedScene);

                            EditorSceneManager.SaveScene(loadedScene);
                            EditorSceneManager.CloseScene(loadedScene, true);
                        }

                        EditorUtility.ClearProgressBar();
                    }
                }
            }
        }

        private void ClearScenesToAssemble()
        {
            scenesToAssembleList.list.Clear();
            sObject.Update();
        }

        private void Callback(object _val)
        {
            assembleWorldspace = (bool)_val;
        }
    }
}