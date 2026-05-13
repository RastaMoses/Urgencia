using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using RPGCreationKit.CellsSystem;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;

namespace RPGCreationKit.CellsSystem
{
    public class CellMapView : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField]
        private StyleSheet m_StyleSheet = default;

        // UI elements
        private Label worldspaceNameLabel;
        private DropdownField worldspaceDropdown;
        private VisualElement gridContainer;
        private ScrollView scrollView;
        private Button refreshButton;
        private TextField screenshotPathField;
        private Button browsePathButton;
        private Button regenerateButton;
        private Button selectModeButton;
        private Label selectionCountLabel;

        // Data
        private List<Worldspace> worldspaces = new List<Worldspace>();
        private Worldspace selectedWorldspace;
        private Dictionary<string, Texture2D> cellScreenshots = new Dictionary<string, Texture2D>();
        private Texture2D defaultCellTexture;
        private bool ctrlKeyPressed = false;
        private bool isSelectMode = false;
        private HashSet<string> selectedCellIds = new HashSet<string>();

        // Grid settings
        private const float CELL_SIZE = 150f;
        private const float GRID_SPACING = 5f;
        private Vector2Int minCoords = new Vector2Int(0, 0);
        private Vector2Int maxCoords = new Vector2Int(0, 0);

        [MenuItem("RPG Creation Kit/Cells System/CellMapView")]
        public static void ShowExample()
        {
            CellMapView wnd = GetWindow<CellMapView>();
            wnd.titleContent = new GUIContent("Cell Map View");
        }

        public void CreateGUI()
        {
            // Load default textures
            defaultCellTexture = EditorGUIUtility.IconContent("d_GridLayoutGroup Icon").image as Texture2D;
            if (defaultCellTexture == null)
                defaultCellTexture = Texture2D.grayTexture;

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            if (m_VisualTreeAsset != null)
            {
                VisualElement uxml = m_VisualTreeAsset.Instantiate();
                if (m_StyleSheet != null)
                    uxml.styleSheets.Add(m_StyleSheet);
                root.Add(uxml);

                // Setup UI references
                worldspaceNameLabel = root.Q<Label>("worldspace-name-label");
                worldspaceDropdown = root.Q<DropdownField>("worldspace-dropdown");
                gridContainer = root.Q<VisualElement>("grid-container");
                scrollView = root.Q<ScrollView>("scroll-view");
                refreshButton = root.Q<Button>("refresh-button");
                screenshotPathField = root.Q<TextField>("screenshot-path");
                browsePathButton = root.Q<Button>("browse-path-button");
                regenerateButton = root.Q<Button>("regenerate-button");
                selectModeButton = root.Q<Button>("select-mode-button");
                selectionCountLabel = root.Q<Label>("selection-count-label");

                // Setup UI events
                if (refreshButton != null)
                {
                    refreshButton.clicked += () =>
                    {
                        RefreshWorldspaces();
                        UpdateCellGrid();
                    };
                }

                if (selectModeButton != null)
                {
                    selectModeButton.clicked += ToggleSelectMode;
                }

                if (screenshotPathField != null)
                {
                    screenshotPathField.value = EditorPrefs.GetString("CellMapView_ScreenshotPath", "Assets/_CellScreenshots");
                    screenshotPathField.RegisterValueChangedCallback(evt =>
                    {
                        EditorPrefs.SetString("CellMapView_ScreenshotPath", evt.newValue);
                        LoadCellScreenshots();
                        UpdateCellGrid();
                    });
                }

                if (regenerateButton != null)
                {
                    regenerateButton.clicked += RegenerateAllScreenshots;
                }

                if (browsePathButton != null)
                {
                    browsePathButton.clicked += BrowseForPath;
                }

                if (worldspaceDropdown != null)
                {
                    worldspaceDropdown.RegisterValueChangedCallback(evt =>
                    {
                        selectedWorldspace = worldspaces.Find(w => w.worldSpaceName == evt.newValue);
                        EditorPrefs.SetString("CellMapView_LastWorldspace", evt.newValue);
                        UpdateWorldspaceNameLabel();
                        UpdateCellGrid();
                    });
                }

                // Register keyboard events
                root.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.LeftControl || evt.keyCode == KeyCode.RightControl)
                        ctrlKeyPressed = true;
                });

                root.RegisterCallback<KeyUpEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.LeftControl || evt.keyCode == KeyCode.RightControl)
                        ctrlKeyPressed = false;
                });

                // Initialize data and UI
                RefreshWorldspaces();
                LoadCellScreenshots();
                UpdateDropdown();
            }
            else
            {
                root.Add(new Label("UXML asset not assigned in inspector."));
            }
        }

        private void RefreshWorldspaces()
        {
            worldspaces = GetAllInstances<Worldspace>()
                .Where(w => w.worldSpaceType == Worldspace.WorldSpaceType.Exterior)
                .ToList();
        }

        private void UpdateDropdown()
        {
            if (worldspaceDropdown == null) return;

            var choices = worldspaces.Select(w => w.worldSpaceName).ToList();
            if (choices.Count == 0)
                choices.Add("No exterior worldspaces found");

            // Update the choices list
            worldspaceDropdown.choices = choices;

            // Try to restore last selected worldspace
            string lastWorldspace = EditorPrefs.GetString("CellMapView_LastWorldspace", "");
            if (!string.IsNullOrEmpty(lastWorldspace) && choices.Contains(lastWorldspace))
            {
                worldspaceDropdown.value = lastWorldspace;
                selectedWorldspace = worldspaces.Find(w => w.worldSpaceName == lastWorldspace);
            }
            else if (choices.Count > 0)
            {
                worldspaceDropdown.value = choices[0];
                selectedWorldspace = worldspaces.Find(w => w.worldSpaceName == choices[0]);
            }

            UpdateWorldspaceNameLabel();
        }

        private void UpdateWorldspaceNameLabel()
        {
            if (worldspaceNameLabel == null) return;

            if (selectedWorldspace != null)
                worldspaceNameLabel.text = selectedWorldspace.worldSpaceName;
            else
                worldspaceNameLabel.text = "No Worldspace Selected";
        }

        private void BrowseForPath()
        {
            string currentPath = screenshotPathField != null ? screenshotPathField.value : "Assets/_CellScreenshots";
            string selectedPath = EditorUtility.OpenFolderPanel("Select Screenshot Folder", currentPath, "");
            
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // Convert absolute path to relative if it's within the project
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                
                if (screenshotPathField != null)
                {
                    screenshotPathField.value = selectedPath;
                    EditorPrefs.SetString("CellMapView_ScreenshotPath", selectedPath);
                    LoadCellScreenshots();
                    UpdateCellGrid();
                }
            }
        }

        private void LoadCellScreenshots()
        {
            cellScreenshots.Clear();

            string screenshotPath = screenshotPathField != null ? screenshotPathField.value : EditorPrefs.GetString("CellMapView_ScreenshotPath", "Assets/_CellScreenshots");
            if (!Directory.Exists(screenshotPath))
                return;

            string[] pngFiles = Directory.GetFiles(screenshotPath, "*.png");
            foreach (string file in pngFiles)
            {
                string cellId = Path.GetFileNameWithoutExtension(file);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
                if (texture != null)
                    cellScreenshots[cellId] = texture;
            }
        }

        private void RegenerateAllScreenshots()
        {
            if (selectedWorldspace == null || selectedWorldspace.cells == null) return;

            string path = screenshotPathField != null ? screenshotPathField.value : EditorPrefs.GetString("CellMapView_ScreenshotPath", "Assets/_CellScreenshots");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var cellsToProcess = isSelectMode && selectedCellIds.Count > 0
                ? selectedWorldspace.cells.Where(c => selectedCellIds.Contains(c.ID)).ToArray()
                : selectedWorldspace.cells;

            if (cellsToProcess.Length == 0)
            {
                EditorUtility.DisplayDialog("Regenerate Screenshots", "No cells selected to regenerate.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Regenerate Screenshots",
                $"This will open {cellsToProcess.Length} scenes and take screenshots. It might take a while. Continue?", "Yes", "No"))
            {
                return;
            }

            int count = 0;
            int total = cellsToProcess.Length;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string originalScenePath = EditorSceneManager.GetActiveScene().path;

            foreach (var cell in cellsToProcess)
            {
                if (cell == null || string.IsNullOrEmpty(cell.sceneRef.ScenePath))
                {
                    count++;
                    continue;
                }

                EditorUtility.DisplayProgressBar("Generating Screenshots", $"Processing {cell.cellName} ({count}/{total})", (float)count / total);

                try
                {
                    var scene = EditorSceneManager.OpenScene(cell.sceneRef.ScenePath, OpenSceneMode.Single);
                    RckCustomToolbar.OnSliderValueChanged(RckCustomToolbar.curSliderValue);

                    GameObject target = null;
                    GameObject[] gos = scene.GetRootGameObjects();
                    foreach (GameObject go in gos)
                    {
                        if (go.CompareTag("RPG Creation Kit/CellInfo"))
                        {
                            target = go;
                            break;
                        }
                    }

                    if (target != null)
                    {
                        Vector3 lookAtPos = target.transform.position;

                        var sceneView = SceneView.lastActiveSceneView;
                        if (sceneView == null)
                            sceneView = SceneView.GetWindow<SceneView>();

                        if (sceneView != null)
                        {
                            // Force scene view to show and update
                            sceneView.Show();
                            sceneView.Focus();

                            // Save settings
                            bool oldFog = sceneView.sceneViewState.showFog;
                            bool oldSkybox = sceneView.sceneViewState.showSkybox;
                            bool oldFlares = sceneView.sceneViewState.showFlares;
                            bool oldImageEffects = sceneView.sceneViewState.showImageEffects;

                            // Apply settings for screenshot
                            sceneView.sceneViewState.showFog = false;
                            sceneView.sceneViewState.showSkybox = false;
                            sceneView.sceneViewState.showFlares = false;
                            sceneView.sceneViewState.showImageEffects = false;

                            sceneView.orthographic = true;
                            float size = selectedWorldspace.cellSize.x > 0 ? selectedWorldspace.cellSize.x / 2f : 75f;
                            
                            Vector3 camPos = new Vector3(lookAtPos.x, size, lookAtPos.z);
                            sceneView.LookAtDirect(camPos, Quaternion.Euler(90, 0, 0), size);
                            sceneView.Repaint();

                            // Force update and wait for scene to be ready
                            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                            System.Threading.Thread.Sleep(100);

                            Camera cam = sceneView.camera;
                            if (cam != null)
                            {
                                GameObject tempLight = new GameObject("TempScreenshotLight");
                                Light lightComp = tempLight.AddComponent<Light>();
                                lightComp.type = LightType.Directional;
                                lightComp.transform.rotation = Quaternion.Euler(90, 0, 0);

                                // Manually position the camera
                                cam.transform.position = camPos;
                                cam.transform.rotation = Quaternion.Euler(90, 0, 0);
                                cam.orthographic = true;
                                cam.orthographicSize = size;
                                cam.nearClipPlane = 0.3f;
                                cam.farClipPlane = 2000f;

                                int res = 512;
                                RenderTexture rt = new RenderTexture(res, res, 24);
                                RenderTexture prev = cam.targetTexture;
                                cam.targetTexture = rt;
                                cam.Render();
                                cam.targetTexture = prev;

                                RenderTexture.active = rt;
                                Texture2D screenShot = new Texture2D(res, res, TextureFormat.RGB24, false);
                                screenShot.ReadPixels(new Rect(0, 0, res, res), 0, 0);
                                screenShot.Apply();
                                RenderTexture.active = null;
                                rt.Release();

                                byte[] bytes = screenShot.EncodeToPNG();
                                string filename = Path.Combine(path, cell.ID + ".png");
                                File.WriteAllBytes(filename, bytes);

                                DestroyImmediate(screenShot);
                                DestroyImmediate(tempLight);
                            }

                            // Restore settings
                            sceneView.sceneViewState.showFog = oldFog;
                            sceneView.sceneViewState.showSkybox = oldSkybox;
                            sceneView.sceneViewState.showFlares = oldFlares;
                            sceneView.sceneViewState.showImageEffects = oldImageEffects;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to generate screenshot for cell {cell.cellName}: {e.Message}");
                }

                count++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            LoadCellScreenshots();
            UpdateCellGrid();

            if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath);
                RckCustomToolbar.OnSliderValueChanged(RckCustomToolbar.curSliderValue);
            }
        }

        private void UpdateCellGrid()
        {
            if (gridContainer == null || selectedWorldspace == null) return;

            gridContainer.Clear();
            LoadCellScreenshots(); // Refresh screenshots

            if (selectedWorldspace.cells == null || selectedWorldspace.cells.Length == 0)
            {
                gridContainer.Add(new Label("No cells found in this worldspace."));
                return;
            }

            // Calculate grid bounds
            CalculateGridBounds();

            // Create container with the right size
            int width = maxCoords.x - minCoords.x + 1;
            int height = maxCoords.y - minCoords.y + 1;

            float gridWidth = width * (CELL_SIZE + GRID_SPACING) + GRID_SPACING;
            float gridHeight = height * (CELL_SIZE + GRID_SPACING) + GRID_SPACING;

            gridContainer.style.width = gridWidth;
            gridContainer.style.height = gridHeight;

            // Create a cell element for each coordinate in the grid
            for (int x = minCoords.x; x <= maxCoords.x; x++)
            {
                for (int y = minCoords.y; y <= maxCoords.y; y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    Cell cell = FindCellAtCoordinate(coord);

                    var cellElement = CreateCellElement(cell, coord);

                    // Position the cell in the grid
                    float posX = (x - minCoords.x) * (CELL_SIZE + GRID_SPACING) + GRID_SPACING;
                    float posY = (maxCoords.y - y) * (CELL_SIZE + GRID_SPACING) + GRID_SPACING;

                    cellElement.style.position = Position.Absolute;
                    cellElement.style.left = posX;
                    cellElement.style.top = posY;

                    gridContainer.Add(cellElement);
                }
            }
        }

        private VisualElement CreateCellElement(Cell cell, Vector2Int coord)
        {
            var cellElement = new VisualElement();
            cellElement.AddToClassList("cell");
            cellElement.style.width = CELL_SIZE;
            cellElement.style.height = CELL_SIZE;

            // Add image
            var image = new Image();
            image.AddToClassList("cell__image");

            if (cell != null && cellScreenshots.TryGetValue(cell.ID, out Texture2D screenshot))
            {
                image.image = screenshot;
            }
            else
            {
                image.image = defaultCellTexture;
            }

            cellElement.Add(image);

            // Add checkbox
            var checkbox = new VisualElement();
            checkbox.AddToClassList("cell__checkbox");
            if (isSelectMode)
            {
                checkbox.AddToClassList("cell__checkbox--visible");
                if (cell != null && selectedCellIds.Contains(cell.ID))
                {
                    checkbox.AddToClassList("cell__checkbox--checked");
                    cellElement.AddToClassList("cell--selected");
                }
            }
            cellElement.Add(checkbox);

            // Add coordinate label
            var coordLabel = new Label($"{coord.x},{coord.y}");
            coordLabel.AddToClassList("cell__coord-label");
            cellElement.Add(coordLabel);

            // Add cell name if available
            if (cell != null)
            {
                /* var nameLabel = new Label(cell.cellName);
                nameLabel.AddToClassList("cell__name-label");
                cellElement.Add(nameLabel); */

                // Store cell reference and add click handler
                cellElement.userData = cell;
                cellElement.RegisterCallback<MouseUpEvent>(evt => 
                {
                    if (isSelectMode)
                    {
                        if (evt.ctrlKey)
                        {
                            OpenSelectedCells(cell);
                        }
                        else
                        {
                            ToggleCellSelection(cell);
                        }
                    }
                    else
                    {
                        OpenCellScene(cell, evt.ctrlKey);
                    }
                });
            }
            else
            {
                cellElement.AddToClassList("cell--empty");
            }

            return cellElement;
        }

        private void CalculateGridBounds()
        {
            minCoords = new Vector2Int(int.MaxValue, int.MaxValue);
            maxCoords = new Vector2Int(int.MinValue, int.MinValue);

            foreach (var cell in selectedWorldspace.cells)
            {
                if (cell.cellCoordinates.x < minCoords.x) minCoords.x = (int)cell.cellCoordinates.x;
                if (cell.cellCoordinates.y < minCoords.y) minCoords.y = (int)cell.cellCoordinates.y;
                if (cell.cellCoordinates.x > maxCoords.x) maxCoords.x = (int)cell.cellCoordinates.x;
                if (cell.cellCoordinates.y > maxCoords.y) maxCoords.y = (int)cell.cellCoordinates.y;
            }

            // Add some padding
            /*  minCoords -= new Vector2Int(1, 1);
             maxCoords += new Vector2Int(1, 1); */
        }

        private Cell FindCellAtCoordinate(Vector2Int coord)
        {
            return selectedWorldspace.cells.FirstOrDefault(c =>
                c.cellCoordinates.x == coord.x && c.cellCoordinates.y == coord.y);
        }

        private string GetWorldEditorScenePath()
        {
            if (selectedWorldspace == null || string.IsNullOrEmpty(selectedWorldspace.worldSpaceID))
                return null;

            string sceneName = $"_WorldEditor_{selectedWorldspace.worldSpaceID}";
            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
            Debug.Log($"Searching for World Editor scene. Found {guids.Length} scenes with name {sceneName}.");
            if(guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Debug.Log($"World Editor scene path: {path}");
                return path;
            }
            return null;
        }

        private void OpenCellScene(Cell cell, bool additive)
        {
            if (cell == null || string.IsNullOrEmpty(cell.sceneRef.ScenePath))
                return;

            if (additive || ctrlKeyPressed)
            {
                EditorSceneManager.OpenScene(cell.sceneRef.ScenePath, OpenSceneMode.Additive);
                RckCustomToolbar.OnSliderValueChanged(RckCustomToolbar.curSliderValue);
            }
            else
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    Scene cellScene;
                    string worldEditorPath = GetWorldEditorScenePath();
                    Debug.Log($"Opening cell scene {cell.sceneRef.ScenePath} with world editor path: {worldEditorPath}");
                    if (!string.IsNullOrEmpty(worldEditorPath))
                    {
                        EditorSceneManager.OpenScene(worldEditorPath, OpenSceneMode.Single);
                        cellScene = EditorSceneManager.OpenScene(cell.sceneRef.ScenePath, OpenSceneMode.Additive);
                    }
                    else
                    {
                        cellScene = EditorSceneManager.OpenScene(cell.sceneRef.ScenePath, OpenSceneMode.Single);
                        RckCustomToolbar.OnSliderValueChanged(RckCustomToolbar.curSliderValue);

                        // If possible, update CellView last opened scene, so toolbar button will work with the CellMapView
                        CellView cv = EditorWindow.GetWindow<CellView>();
                        if (cv)
                            cv.SetLastCellString(cell.sceneRef.ScenePath);
                    }

                    // Center view on cell info
                    if (cellScene.IsValid() && cellScene.isLoaded)
                    {
                        GameObject[] gos = cellScene.GetRootGameObjects();
                        foreach (GameObject go in gos)
                        {
                            if (go.CompareTag("RPG Creation Kit/CellInfo"))
                            {
                                Selection.activeObject = go;
                                SceneView.FrameLastActiveSceneView();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void ToggleSelectMode()
        {
            isSelectMode = !isSelectMode;
            
            if (selectModeButton != null)
            {
                selectModeButton.text = isSelectMode ? "Exit Select Mode" : "Select Mode";
                if (isSelectMode)
                    selectModeButton.AddToClassList("cell-map-view__mode-button--active");
                else
                    selectModeButton.RemoveFromClassList("cell-map-view__mode-button--active");
            }

            if (selectionCountLabel != null)
            {
                selectionCountLabel.style.display = isSelectMode ? DisplayStyle.Flex : DisplayStyle.None;
            }

            UpdateRegenerateButtonText();
            UpdateCellGrid();
        }

        private void ToggleCellSelection(Cell cell)
        {
            if (cell == null) return;

            if (selectedCellIds.Contains(cell.ID))
            {
                selectedCellIds.Remove(cell.ID);
            }
            else
            {
                selectedCellIds.Add(cell.ID);
            }

            UpdateSelectionCount();
            UpdateRegenerateButtonText();
            UpdateCellGrid();
        }

        private void UpdateSelectionCount()
        {
            if (selectionCountLabel != null)
            {
                selectionCountLabel.text = $"{selectedCellIds.Count} Selected";
            }
        }

        private void UpdateRegenerateButtonText()
        {
            if (regenerateButton != null)
            {
                if (isSelectMode && selectedCellIds.Count > 0)
                {
                    regenerateButton.text = "Regenerate Selected";
                }
                else
                {
                    regenerateButton.text = "Regenerate All";
                }
            }
        }

        private void OpenSelectedCells(Cell centerCell)
        {
            if (selectedCellIds.Count == 0) return;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string worldEditorPath = GetWorldEditorScenePath();
                bool first = true;

                if (!string.IsNullOrEmpty(worldEditorPath))
                {
                    EditorSceneManager.OpenScene(worldEditorPath, OpenSceneMode.Single);
                    RckCustomToolbar.OnSliderValueChanged(RckCustomToolbar.curSliderValue);
                    // All cells are additive now
                    foreach (var cellId in selectedCellIds)
                    {
                        var cell = selectedWorldspace.cells.FirstOrDefault(c => c.ID == cellId);
                        if (cell != null && !string.IsNullOrEmpty(cell.sceneRef.ScenePath))
                        {
                            EditorSceneManager.OpenScene(cell.sceneRef.ScenePath, OpenSceneMode.Additive);
                        }
                    }
                    RckCustomToolbar.OnSliderValueChanged(RckCustomToolbar.curSliderValue);
                }
                else
                {
                    foreach (var cellId in selectedCellIds)
                    {
                        var cell = selectedWorldspace.cells.FirstOrDefault(c => c.ID == cellId);
                        if (cell != null && !string.IsNullOrEmpty(cell.sceneRef.ScenePath))
                        {
                            EditorSceneManager.OpenScene(cell.sceneRef.ScenePath, first ? OpenSceneMode.Single : OpenSceneMode.Additive);
                            first = false;
                        }
                    }
                    RckCustomToolbar.OnSliderValueChanged(RckCustomToolbar.curSliderValue);
                }

                // Center on the clicked cell
                if (centerCell != null)
                {
                    
                    var scene = SceneManager.GetSceneByPath(centerCell.sceneRef.ScenePath);
                    if (scene.IsValid() && scene.isLoaded)
                    {
                        GameObject[] gos = scene.GetRootGameObjects();
                        foreach (GameObject go in gos)
                        {
                            if (go.CompareTag("RPG Creation Kit/CellInfo"))
                            {
                                Selection.activeObject = go;
                                SceneView.FrameLastActiveSceneView();
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static List<T> GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            List<T> results = new List<T>(guids.Length);

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                results.Add(AssetDatabase.LoadAssetAtPath<T>(path));
            }

            return results;
        }

        private void OnFocus()
        {
            // Refresh data when window gains focus
            RefreshWorldspaces();
            LoadCellScreenshots();
            UpdateDropdown();
            UpdateCellGrid();
        }

        private void OnLostFocus()
        {
            ctrlKeyPressed = false;
        }
    }
}