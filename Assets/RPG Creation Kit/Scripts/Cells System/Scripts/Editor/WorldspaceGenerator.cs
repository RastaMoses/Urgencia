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
using UnityEditor.SceneTemplate;
using System.IO;
using System.Linq;

namespace RPGCreationKit.RCKEditor
{
    /// <summary>
    /// Window that allows to assemble and correctly setup the persistent references among all the scenes you choose to reference.
    /// </summary>
    public class WorldspaceGenerator : EditorWindow
    {
        public SceneTemplateAsset template;

        [SerializeField]
        SerializedObject sObject;

        public GameObject terrainPreset;
        Worldspace worldspaceToAssemble;

        // Generator related
        int startingX = -2;
        int endingX = 2;
        int startingY = -2;
        int endingY = 2;

        string startingCellID = "";
        string outpath = "Assets/RPG Creation Kit/";

        List<Cell> cellsToAdd = new List<Cell>();

        [MenuItem("RPG Creation Kit/Cells System/Worldspace Generator")]
        private static void OpenWindow()
        {
            WorldspaceGenerator window = GetWindow<WorldspaceGenerator>();

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RPGCKEditorWindowIcon);

            GUIContent titleContent = new GUIContent("Worldspace Generator", icon);
            window.titleContent = titleContent;
        }

        private void Awake()
        {
            ScriptableObject target = this;
            sObject = new SerializedObject(target);
        }


        private void OnGUI()
        {
            GUIStyle TitleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            TitleStyle.fontSize = 20;
            TitleStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            ButtonStyle.border = new RectOffset(2, 2, 2, 2);
            ButtonStyle.fontSize = 16;
            ButtonStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Worldspace Generator", TitleStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("This window allows you to quickly create a new empty Exterior Worldspace.", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Worldspace to generate:", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            worldspaceToAssemble = (Worldspace)EditorGUILayout.ObjectField(worldspaceToAssemble, typeof(Worldspace), false, GUILayout.MaxWidth(250));
            if(EditorGUI.EndChangeCheck())
            {
                outpath = "Assets/RPG Creation Kit/" + worldspaceToAssemble.worldSpaceID + "/";
            }

            if(worldspaceToAssemble)
            {
                // Check if the worldspace ID is assigned
                if(string.IsNullOrEmpty(worldspaceToAssemble.worldSpaceID))
                {
                    EditorGUILayout.HelpBox("The selected worldspace have no ID.", MessageType.Error);
                }
                else if(worldspaceToAssemble.worldSpaceType == Worldspace.WorldSpaceType.Interior)
                {
                    EditorGUILayout.HelpBox("The selected worldspace is an Interior Worldspace, this generator is for Exteriors.", MessageType.Error);
                }
                else if(worldspaceToAssemble.cellSize.x == 0 || worldspaceToAssemble.cellSize.y == 0)
                {
                    EditorGUILayout.HelpBox("The selected worldspace has an invalid Cell Size.", MessageType.Error);
                }
                else if (worldspaceToAssemble.cells.Length > 0)
                {
                    EditorGUILayout.HelpBox("The selected worldspace has already cells in it, therefore cannot be auto-generated. Empty the worldspace before proceeding.", MessageType.Error);
                }
                else
                {
                    // Should be good from there
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("The selected worldspace can be auto generated!", MessageType.Info);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Select Worldspace Num of Cells:");

                    EditorGUILayout.BeginHorizontal();
                    startingX = EditorGUILayout.IntField("X (start, end)", startingX, GUILayout.MaxWidth(250));
                    endingX = EditorGUILayout.IntField(endingX, GUILayout.MaxWidth(250));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    startingY = EditorGUILayout.IntField("Y (start, end)", startingY, GUILayout.MaxWidth(250));
                    endingY = EditorGUILayout.IntField(endingY, GUILayout.MaxWidth(250));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField("The resulting worldspace will be composed of " + GetNumberOfCellsToGenerate() + " cells.", EditorStyles.miniLabel);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Starting terrain:");
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("The starting terrain should always be flat. Make sure its size is the same as your Worldspace's \"Cell Size\" (" + worldspaceToAssemble.cellSize.x + ", " + worldspaceToAssemble.cellSize.y + ")", MessageType.Info);
                    EditorGUILayout.Space();


                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Terrain Preset:");
                    terrainPreset = (GameObject)EditorGUILayout.ObjectField(terrainPreset, typeof(GameObject), false, GUILayout.MaxWidth(250));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(40);

                    startingCellID = EditorGUILayout.TextField("Starting Cell ID:", startingCellID);

                    EditorGUILayout.Space(5);

                    EditorGUILayout.LabelField("Output path:", EditorStyles.boldLabel);
                    outpath = EditorGUILayout.TextField(outpath);

                    EditorGUILayout.Space(25);

                    GUI.enabled = terrainPreset != null && !string.IsNullOrEmpty(startingCellID);
                    if(GUILayout.Button("Autogenerate"))
                    {
                        if (EditorUtility.DisplayDialog("Worldspace Generator", "Are you sure you want to auto-generate " + GetNumberOfCellsToGenerate() + " cells? Generating cells may take several minutes.", "Generate", "Cancel"))
                        {
                            if (!Directory.Exists(outpath))
                            {
                                Directory.CreateDirectory(outpath);
                            }

                            // reset array
                            cellsToAdd = new List<Cell>();

                            // Flag the worldspace as autogenerated
                            worldspaceToAssemble.isAutoGenerated = true;

                            int loopsToDo = (endingY - startingY + 1) * (endingX - startingX + 1);
                            int curLoops = 0;
                            for (int y = startingY; y <= endingY; y++)
                            {
                                for(int x = startingX; x <= endingX; x++)
                                {

                                    EditorUtility.DisplayProgressBar("Generating Worldspace...", "Working on (" + x + "," + y + ")", (float)curLoops / (float)loopsToDo);
                                    curLoops++;
                                    // Generate the cell

                                    // Step 1 Create the scene ref from template
                                    string cellID = startingCellID + "(" + x + "," + y + ")";
                                    string scenePath = outpath + cellID + ".unity";

                                    InstantiationResult res = SceneTemplateService.Instantiate(template, false, scenePath);
                                    var loadedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                                    // Step 2, create the Cell object
                                    Cell cell = ScriptableObject.CreateInstance<Cell>();

                                    cell.ID = cellID;
                                    cell.cellName = worldspaceToAssemble.worldSpaceName;
                                    cell.worldspace = worldspaceToAssemble;
                                    cell.cellCoordinates.x = x;
                                    cell.cellCoordinates.y = y;

                                    // Set cell's scene
                                    cell.sceneRef = new SceneReference();
                                    cell.sceneRef.ScenePath = scenePath;

                                    cell.cellInWorldCoordinates = new Vector3((worldspaceToAssemble.cellSize.x) * cell.cellCoordinates.x, 0, (worldspaceToAssemble.cellSize.y) * cell.cellCoordinates.y);

                                    // Default settings
                                    cell.CanBeCached = true;

                                    AssetDatabase.CreateAsset(cell, outpath + "- [CELL] " + cellID + ".asset");
                                    AssetDatabase.SaveAssets();

                                    // Add the cell to the worldspace
                                    cellsToAdd.Add(cell);

                                    // Do things on scene
                                    CellInformation cellInfo = FindObjectOfType<CellInformation>();
                                    cellInfo.cell = cell;

                                    // Set initial positions
                                    cellInfo.transform.position = new Vector3(cell.cellInWorldCoordinates.x, cell.cellInWorldCoordinates.y, cell.cellInWorldCoordinates.z);
                                    
                                    // Spawn and place the terrain
                                    GameObject terrain = Instantiate(terrainPreset, null);
                                    Terrain terrainComp = terrain.GetComponent<Terrain>();
                                    terrain.transform.position = new Vector3(cell.cellInWorldCoordinates.x - (worldspaceToAssemble.cellSize.x / 2), cell.cellInWorldCoordinates.y, cell.cellInWorldCoordinates.z - (worldspaceToAssemble.cellSize.x / 2));
                                    TerrainData newTerrainData = Instantiate(terrainComp.terrainData);
                                    terrainComp.terrainData = newTerrainData;
                                    terrain.GetComponent<TerrainCollider>().terrainData = terrainComp.terrainData;

                                    string terrainDataPath = outpath + "TerrainData/";
                                    if (!Directory.Exists(terrainDataPath))
                                    {
                                        Directory.CreateDirectory(terrainDataPath);
                                    }

                                    AssetDatabase.CreateAsset(newTerrainData, terrainDataPath + "TerrainData" + cellID + ".asset");
                                    AssetDatabase.SaveAssets();

                                    cellInfo.UpdateCell();

                                    EditorSceneManager.MarkSceneDirty(loadedScene);
                                    EditorSceneManager.SaveScene(loadedScene);

                                    // Done with this scene, add it to build settings
                                    List<EditorBuildSettingsScene> tempScenes = EditorBuildSettings.scenes.ToList();
                                    tempScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                                    EditorBuildSettings.scenes = tempScenes.ToArray();
                                }
                            }

                            worldspaceToAssemble.cells = new Cell[cellsToAdd.Count];
                            for(int i = 0; i < cellsToAdd.Count; i++)
                            {
                                worldspaceToAssemble.cells[i] = cellsToAdd[i];
                            }

                            EditorUtility.SetDirty(worldspaceToAssemble);


                            worldspaceToAssemble.CalculateCellsCoordinates();
                            worldspaceToAssemble.CalculateCellsNeighboring();


                            EditorUtility.ClearProgressBar();
                            EditorUtility.DisplayDialog("Worldspace Generator", "Done generating " + GetNumberOfCellsToGenerate() + " cells in: \"" + outpath + "\"", "OK");
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        private int GetNumberOfCellsToGenerate()
        {
            return (Mathf.Abs(endingX - startingX) + 1) * (Mathf.Abs(endingY - startingY) + 1);
        }

    }
}