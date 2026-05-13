using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Player;
using RPGCreationKit.SaveSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.Image;

namespace RPGCreationKit
{
    /// <summary>
    /// Dictionary used to table the instantiated distant terrains with their scene names
    /// </summary>
    [System.Serializable]
    public class DistantTerrainsDictionary : SerializableDictionary<string, GameObject> { }


    /// <summary>
    /// Manages the World Streaming and loading of cells
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        bool isFreshStart = true; // true if it's the first time we load, so we gotta load from the savegame
        bool firstCellAsssigment = false;
        // Distant terrain
        DistantTerrainsDictionary distantTerrainsDictionary = new DistantTerrainsDictionary();
        bool distantTerrainSpawned = false;
        Worldspace currentWorldspaceOfDistantWorld = null;
        [SerializeField] private Transform distantTerrainsT;

        public GameObject ExteriorDirectionalLight;

        [Header("UI")]
        [SerializeField] private GameObject gameCanvas;
        [SerializeField] public GameObject loadingCanvas;

        [Header("Settings")]
        public Worldspace currentWorldspace;

        [SerializeField] private Cell cellToLoad;


        public Vector3 playerPosition;
        public Quaternion playerRotation;
        public bool isLoading { get; private set; }
        public List<string> scenesLoaded = new List<string>();
        public List<string> scenesCached = new List<string>();

        [Header("Runtime")]
        [SerializeField] private float streamingCycleRate = 5f;
        public Cell currentCenterCell;

        public string doorReferenceID;

        bool isStreaming = false;
        bool shouldUnfreeze = false;
        public static WorldManager instance;
        private void Awake()
        {
            instance = this;

        }

        private void Start()
        {
            StartCoroutine(CheckUnfreezeAfterLoad());
        }

        public IEnumerator CheckUnfreezeAfterLoad()
        {
            while(true)
            {
                if (RckPlayer.instance.isFreezed && shouldUnfreeze)
                {
                    if (!isLoading && WorldManager.instance != null && Player.RckPlayer.instance != null &&
                    !WorldManager.instance.isLoading && CellsSystem.CellInformation.AllActiveCellsLoaded())
                    {
                        // Wait a few frames
                        yield return new WaitForEndOfFrame();
                        yield return new WaitForEndOfFrame();
                        yield return new WaitForEndOfFrame();

                        RckPlayer.instance.UnfreezeAndEnableControls();
                        shouldUnfreeze = false;


                        if (RCKSettings.AUTOSAVE_ENABLED)
                        {
                            SaveSystemManager.instance.SaveOnNewFile(true);
                            AlertMessage.instance.InitAlertMessage("Autosaving...", 1.5f, false);
                        }
                    }
                }

                yield return null;
            }
        }

        public IEnumerator WorldStreamingCycle()
        {
            while (true)
            {
                if (ids.Count == 0)
                    isLoading = false;

                while (currentWorldspace.worldSpaceType == Worldspace.WorldSpaceType.Interior ||
                       loadingCellTaskRunning)
                {
                    if (ids.Count == 0)
                        isLoading = false;

                    yield return null;
                }

                float shortestDistance = 99999.999f;
                int shortestDistanceID = 0;
                for (int i = 0; i < currentWorldspace.cells.Length; i++)
                {
                    float curDistance = Vector3.Distance(RckPlayer.instance.transform.position, currentWorldspace.cells[i].cellInWorldCoordinates);
                    if (curDistance < shortestDistance)
                    {
                        shortestDistance = curDistance;
                        shortestDistanceID = i;
                    }
                }

                if((currentCenterCell != currentWorldspace.cells[shortestDistanceID] && !isLoading) || firstCellAsssigment)
                {
                    currentCenterCell = currentWorldspace.cells[shortestDistanceID];
                    SetCenterCellAsActiveScene();

                    // Load neighboring cells
                    for (int i = 0; i < currentCenterCell.neighboringCells.Length; i++)
                    {
                        if (currentCenterCell.neighboringCells[i] == null || scenesLoaded.Contains(currentCenterCell.neighboringCells[i].sceneRef.ScenePath)) // empty cell (end of map)
                            continue;

                        LoadCell(currentCenterCell.neighboringCells[i], Random.Range(int.MinValue, int.MaxValue), true);
                    }


                    List<string> toUnload = new List<string>();
                    // Unload previous cells
                    for (int i = 0; i < scenesLoaded.Count; i++)
                    {
                        bool unload = true;

                        for (int j = 0; j < currentCenterCell.neighboringCells.Length; j++)
                        {
                            if (currentCenterCell.neighboringCells[j] == null)
                                continue;

                            if (currentCenterCell.neighboringCells[j].sceneRef.ScenePath == scenesLoaded[i])
                            {
                                unload = false;
                                break;
                            }
                        }

                        if (unload)
                            toUnload.Add(scenesLoaded[i]);
                    }

                    foreach (string str in toUnload)
                        UnloadCell(str);
                }

                firstCellAsssigment = false;

                yield return new WaitForSeconds(streamingCycleRate);
            }
        }

        private void LoadingInterface(bool loading)
        {
            if (loading)
            {
                loadingCanvas.SetActive(true);
                gameCanvas.SetActive(false);
                GameStatus.instance.IsPaused = true;
                SceneManager.LoadScene("_LoadingScreen_", LoadSceneMode.Additive);
            }
            else
            {
                loadingCanvas.SetActive(false);
                gameCanvas.SetActive(true);
                GameStatus.instance.IsPaused = false;
            }
        }

        List<int> ids = new List<int>();
        public void LoadCell(Cell _cell, int _id, bool streaming = true, Door _door = null, RCKTransform _transform = null, bool shouldUnfreezeAfter = false)
        {
            ids.Add(_id);
            isLoading = true;

            if (!streaming)
                LoadingInterface(true);

            StartCoroutine(LoadCellTask(_cell, _id, streaming, _door, _transform, shouldUnfreezeAfter));
        }

        bool loadingCellTaskRunning = false;
        public IEnumerator LoadCellTask(Cell _cell, int _id, bool streaming = false, Door _door = null, RCKTransform _transform = null, bool shouldUnfreezeAfter = false)
        {
            loadingCellTaskRunning = true;

            bool cacheNeeded = false;

            List<string> savedCache = new List<string>();

            if (scenesCached.Contains(_cell.sceneRef.ScenePath))
            {
                foreach (string str in scenesCached)
                    savedCache.Add(str);

                cacheNeeded = true;
            }

            if (!streaming)
                EmptyCache(cacheNeeded);

            // Save DoorID if we've used a door
            if (_door != null)
                doorReferenceID = _door.linkedDoorObjRef;

            // If this isn't the first time we load something
            if (currentCenterCell != null)
            {
                //If the worldspace that we're loading wants to empty the cache, do it
                //if (_cell.worldspace != currentWorldspace && _cell.worldspace.forceToUnloadAllCached)
                //    EmptyCache(false);

                // Check if we should recheck the world
                if (currentWorldspace.worldSpaceType == Worldspace.WorldSpaceType.Interior && _cell.worldspace.worldSpaceType == Worldspace.WorldSpaceType.Exterior)
                    firstCellAsssigment = true;

                // When travelign from one exterior worldspace to another, we should recheck the world
                if (currentWorldspace.worldSpaceType == Worldspace.WorldSpaceType.Exterior && _cell.worldspace.worldSpaceType == Worldspace.WorldSpaceType.Exterior &&
                    currentWorldspace.worldSpaceID != _cell.worldspace.worldSpaceID)
                {
                    firstCellAsssigment = true;
                }

                // Check if loading this cell causes the currently loaded worldspace/cells to be unloaded
                if (_cell.worldspace != currentWorldspace && _cell.worldspace.unloadPreviousWorld)
                {
                    List<AsyncOperation> operations = new List<AsyncOperation>();
                    List<string> toUnload = new List<string>();

                    for (int i = 0; i < scenesLoaded.Count; i++)
                        toUnload.Add(scenesLoaded[i]);

                    for (int i = 0; i < toUnload.Count; i++)
                        operations.Add(UnloadCell(toUnload[i]));

                    // Wait for all operations to finish
                    while (operations.Count > 0)
                    {
                        operations.RemoveAll(op => op.isDone);
                        yield return null;
                    }

                    // Destroy distant world as well
                    distantTerrainSpawned = false;
                    currentWorldspaceOfDistantWorld = null;
                    foreach (Transform t in distantTerrainsT.transform)
                        Destroy(t.gameObject);

                    // Set new worldspace
                    currentWorldspace = _cell.worldspace;
                    OnWorldspaceChanges();
                }
                // Otherwise if this is a different worldspace than the one currently loaded, disable the currently loaded worldspace/cells:
                else if (_cell.worldspace != currentCenterCell.worldspace)
                {
                    List<string> toDisable = new List<string>();
                    for (int i = 0; i < scenesLoaded.Count; i++)
                    {
                        toDisable.Add(scenesLoaded[i]);
                        DisableLoadedCell(scenesLoaded[i]);
                    }

                    for (int i = 0; i < toDisable.Count; i++)
                    {
                        scenesLoaded.Remove(toDisable[i]);

                        if(currentCenterCell.CanBeCached)
                            scenesCached.Add(toDisable[i]);
                    }

                    // Disable distant world as well
                    distantTerrainsT.gameObject.SetActive(false);

                    currentWorldspace = _cell.worldspace;
                    OnWorldspaceChanges();
                }
                // If it is an interior worldspace, unload the previous scenes
                else if (_cell.worldspace == currentWorldspace && currentWorldspace.worldSpaceType == Worldspace.WorldSpaceType.Interior)
                {

                    List<string> toDisable = new List<string>();
                    for (int i = 0; i < scenesLoaded.Count; i++)
                    {
                        toDisable.Add(scenesLoaded[i]);
                        DisableLoadedCell(scenesLoaded[i], true);
                    }
                    
                    // Cache
                    for (int i = 0; i < toDisable.Count; i++)
                    {
                        scenesLoaded.Remove(toDisable[i]);
                        scenesCached.Add(toDisable[i]);
                    }

                    currentWorldspace = _cell.worldspace;
                    OnWorldspaceChanges();
                }
            }
            // If it is the first time we load something, initialize state
            else
            {
                currentWorldspace = _cell.worldspace;
                currentCenterCell = GetPlayerCenterCell();
                firstCellAsssigment = true;
                OnWorldspaceChanges();

                // Check if the distant cell should be unloaded
                GameObject dTerrainCenter = null;
                if (distantTerrainsDictionary.TryGetValue(currentCenterCell.ID, out dTerrainCenter))
                {
                    dTerrainCenter.SetActive(false);
                }
            }

            // Load the scene(s) (cached)
            if (savedCache.Contains(_cell.sceneRef.ScenePath))
            {
                for (int i = 0; i < savedCache.Count; i++)
                {
                    EnableDisabledCell(savedCache[i]);
                    scenesLoaded.Add(savedCache[i]);
                }
            }
            else
            {
                AsyncOperation mainOperation = SceneManager.LoadSceneAsync(_cell.sceneRef.ScenePath, LoadSceneMode.Additive);
                scenesLoaded.Add(_cell.sceneRef.ScenePath);

                while (!mainOperation.isDone)
                    yield return null;
            }

            if (_cell.worldspace.worldSpaceType == Worldspace.WorldSpaceType.Interior && !_cell.keepExteriorLight)
            {
                ExteriorDirectionalLight.SetActive(false);
                currentCenterCell = _cell;
                SetActiveScene(_cell.sceneRef.ScenePath);
            }
            else
            {
                currentCenterCell = _cell;
                ExteriorDirectionalLight.SetActive(true);

                if(_cell.worldspace.worldSpaceType == Worldspace.WorldSpaceType.Interior)
                    SetActiveScene(_cell.sceneRef.ScenePath);
            }

            if (!string.IsNullOrEmpty(doorReferenceID))
                MovePlayerToDoorTelemark(doorReferenceID);

            doorReferenceID = null;

            yield return new WaitForEndOfFrame();

            if (!streaming)
                LoadingInterface(false);

            yield return new WaitForEndOfFrame();

            if (shouldUnfreezeAfter)
                shouldUnfreeze = true;

            ids.Remove(_id);

            loadingCellTaskRunning = false;

            // Check if the distant cell should be unloaded
            GameObject dTerrain = null;
            if (distantTerrainsDictionary.TryGetValue(_cell.ID, out dTerrain))
            {
                dTerrain.SetActive(false);
            }
        }

        private void DisableLoadedCell(string _scenePath, bool _checkingSameWorldspace = false)
        {
            Scene scene = SceneManager.GetSceneByPath(_scenePath);

            GameObject[] rootGameObjects = scene.GetRootGameObjects();

            CellInformation cellInformation = null;
            for (int z = 0; z < rootGameObjects.Length; z++)
            {
                if(rootGameObjects[z].name == "AI_Container")
                {
                    for(int i = 0; i < rootGameObjects[z].transform.childCount; i++)
                        rootGameObjects[z].transform.GetChild(i).GetComponent<RckAI>().onlineComponents.OnGoingOffline();

                    continue;
                }

                if (rootGameObjects[z].CompareTag("RPG Creation Kit/CellInfo"))
                {
                    var cellInfo = rootGameObjects[z].GetComponent<CellInformation>();
                    cellInfo.OnCellIsBeingUnloaded();
                    cellInfo.isActiveInScene = false;
                    cellInformation = cellInfo;
                }

                rootGameObjects[z].SetActive(false);
            }

            // If the cell cant be cached, unload it
            if ((cellInformation != null && !cellInformation.cell.CanBeCached &&
                !(cellInformation.cell.CanBeCachedOnlyInSameWorldspace && _checkingSameWorldspace)))
            {
                StartCoroutine(UnloadNonCachableScene(_scenePath));
            }
        }

        private IEnumerator UnloadNonCachableScene(string _scenePath)
        {
            while (WorldManager.instance.isLoading)
                yield return new WaitForEndOfFrame();

            UnloadCell(_scenePath, true);
        }

        private void EnableDisabledCell(string _scenePath)
        {
            Scene scene = SceneManager.GetSceneByPath(_scenePath);

            GameObject[] rootGameObjects = scene.GetRootGameObjects();

            for (int i = 0; i < rootGameObjects.Length; i++)
            {
                if (rootGameObjects[i].name == "AI_Container")
                {
                    for (int z = 0; z < rootGameObjects[i].transform.childCount; z++)
                        if(!rootGameObjects[i].transform.GetChild(z).GetComponent<RckAI>().goToSleepLogicEnabled)
                            rootGameObjects[i].transform.GetChild(z).GetComponent<RckAI>().onlineComponents.OnGoingOnline();

                    continue;
                }

                if (rootGameObjects[i].CompareTag("RPG Creation Kit/CellInfo"))
                {
                    var cellInfo = rootGameObjects[i].GetComponent<CellInformation>();
                    cellInfo.isActiveInScene = true;
                }

                rootGameObjects[i].SetActive(true);
            }
        }

        private AsyncOperation UnloadCell(Cell _cellToUnload)
        {
            Scene scene = SceneManager.GetSceneByPath(_cellToUnload.sceneRef.ScenePath);

            GameObject[] rootGameObjects = scene.GetRootGameObjects();

            for (int z = 0; z < rootGameObjects.Length; z++)
                if (rootGameObjects[z].CompareTag("RPG Creation Kit/CellInfo"))
                {
                    rootGameObjects[z].GetComponent<CellInformation>().OnCellIsBeingUnloaded();
                    break;
                }


            scenesLoaded.Remove(_cellToUnload.sceneRef.ScenePath);
            return SceneManager.UnloadSceneAsync(scene);
        }

        private AsyncOperation UnloadCell(string _scenePath, bool _skipRoot = false)
        {
            Scene scene = SceneManager.GetSceneByPath(_scenePath);

            CellInformation cellInfo = null;
            if (!_skipRoot)
            {
                GameObject[] rootGameObjects = scene.GetRootGameObjects();

                for (int z = 0; z < rootGameObjects.Length; z++)
                    if (rootGameObjects[z].CompareTag("RPG Creation Kit/CellInfo"))
                    {
                        cellInfo = rootGameObjects[z].GetComponent<CellInformation>();
                        cellInfo.OnCellIsBeingUnloaded();
                        break;
                    }
            }

            // Check if the distant cell should be unloaded
            GameObject dTerrain = null;
            if (cellInfo != null && distantTerrainsDictionary.TryGetValue(cellInfo.cell.ID, out dTerrain))
            {
                dTerrain.SetActive(true);
            }

            scenesLoaded.Remove(_scenePath);
            return SceneManager.UnloadSceneAsync(_scenePath);
        }

        public void EmptyCache(bool virtually)
        {
            if (!virtually)
            {
                // If the exterior cached scenes were unloaded by the cache system, the distant world needs to be reinstantiated
                // This is because if the door out of city for some reasons should teleport you far away from the actual cell (for example, another door on the other side of the city)
                // Unloading from the cache does not unload/load the distant world
                // Code is now disabled
                bool somethingUnloaded = false;

                for (int i = 0; i < scenesCached.Count; i++)
                {
                    somethingUnloaded = true;
                    SceneManager.UnloadSceneAsync(scenesCached[i]);
                }

                /*
                if(somethingUnloaded)
                {
                    // Destroy distant world as well
                    distantTerrainSpawned = false;
                    currentWorldspaceOfDistantWorld = null;
                    foreach (Transform t in distantTerrainsT.transform)
                        Destroy(t.gameObject);
                }
                */
            }
            scenesCached.Clear();
        }

        private void MovePlayerToDoorTelemark(string doorReferenceID)
        {
            // Get door and place Player on it
            if (!string.IsNullOrEmpty(doorReferenceID))
            {
                GameObject[] allDoorsGameObject = GameObject.FindGameObjectsWithTag("RPG Creation Kit/Door");
                Door[] allDoors = new Door[allDoorsGameObject.Length];

                for (int i = 0; i < allDoorsGameObject.Length; i++)
                    allDoors[i] = allDoorsGameObject[i].GetComponent<Door>();


                bool found = false;
                for (int i = 0; i < allDoors.Length; i++)
                {
                    if (allDoors[i].objReference == doorReferenceID)
                    {
                        playerPosition = allDoors[i].teleportMarker.position;
                        playerRotation = allDoors[i].teleportMarker.rotation;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    Debug.LogWarning("Could not find the Door with ReferenceID: " + doorReferenceID + ". Make sure its GameObject is tagged as 'RPG Creation Kit/Door'.");
            }

            MovePlayerToPosition(true);
        }

        public Cell GetPlayerCenterCell()
        {
            float shortestDistance = 99999.999f;
            int shortestDistanceID = 0;
            for (int i = 0; i < currentWorldspace.cells.Length; i++)
            {
                float curDistance = Vector3.Distance(RckPlayer.instance.transform.position, currentWorldspace.cells[i].cellInWorldCoordinates);
                if (curDistance < shortestDistance)
                {
                    shortestDistance = curDistance;
                    shortestDistanceID = i;
                }
            }

            return currentWorldspace.cells[shortestDistanceID];
        }

        public void SetCenterCellAsActiveScene()
        {
            try
            {
                Scene scene = SceneManager.GetSceneByPath(currentCenterCell.sceneRef.ScenePath);
                SceneManager.SetActiveScene(scene);

                // Refresh Lighting immediately
                TimeOfDayManager.instance.RefreshLightingImmediate();
            }
            catch
            {

            }
        }

        public void SetActiveScene(string scenePath)
        {
            try
            {
                Scene scene = SceneManager.GetSceneByPath(currentCenterCell.sceneRef.ScenePath);
                SceneManager.SetActiveScene(scene);

                // Refresh Lighting immediately
                TimeOfDayManager.instance.RefreshLightingImmediate();
            }
            catch
            {

            }
        }

        public void MovePlayerToPosition(bool killXRot)
        {
            RckPlayer.instance.transform.position = playerPosition;

            var toSaveInCamera = playerRotation;
            toSaveInCamera.x = 0;

            // Save everything in the mouse look
            RckPlayer.instance.transform.rotation = playerRotation;

            if(killXRot)
                RckPlayer.instance.mouseLook.xRotation = 0;
        }

        public void StartLoadingFromSavegame()
        {
            StartCoroutine(LoadFromSavegame());
        }

        public IEnumerator LoadFromSavegame()
        {
            PlayerData f = SaveSystemManager.instance.saveFile.PlayerData;
            Cell cellToLoad = WorldspaceDatabase.GetWorldspace(f.playerWorldspaceID).GetCellByID(f.playerCellID);

            LoadCell(cellToLoad, Random.Range(int.MinValue, int.MaxValue));

            while (ids.Count != 0)
                yield return new WaitForEndOfFrame();

            yield return new WaitForEndOfFrame();

            StartCoroutine(WorldStreamingCycle());

            while(LoadingScreenManager.instance != null && (!LoadingScreenManager.instance.isFinished || LoadingScreenManager.instance.loadingText.activeInHierarchy))
                yield return new WaitForEndOfFrame();

            while (WorldManager.instance.isLoading || !CellsSystem.CellInformation.AllActiveCellsLoaded() || GameStatus.instance.IsPaused || loadingCanvas.activeInHierarchy)
                yield return new WaitForEndOfFrame();

            // Load all the persistent references
            PersistentReferences.PersistentReferenceManager.instance.LoadPersistentAI();

            RckPlayer.instance.UnfreezeAndEnableControls();

            RckPlayer.instance.isLoaded = true;

            RckPlayer.instance.FadeScreen(true);
        }


        public void OnWorldspaceChanges()
        {
            if(currentWorldspace.worldSpaceType == Worldspace.WorldSpaceType.Exterior)
            {
                // If there was no exterior distant world represented
                if(currentWorldspaceOfDistantWorld == null && distantTerrainSpawned == false)
                {
                    distantTerrainsDictionary.Clear();

                    // Spawn the distant world
                    foreach(Cell c in currentWorldspace.cells)
                        if(c.distantCellObject != null)
                        {
                            GameObject dTerrain = Instantiate(c.distantCellObject, distantTerrainsT);
                            distantTerrainsDictionary.Add(c.ID, dTerrain);
                        }

                    currentWorldspaceOfDistantWorld = currentWorldspace;
                    distantTerrainSpawned = true;
                    distantTerrainsT.gameObject.SetActive(true);
                }
                // If the same worldspace of the instantiated world was recalled here, we need to re-enable all the distant world
                else if(currentWorldspaceOfDistantWorld == currentWorldspace)
                {
                    // Enable distant world
                    distantTerrainsT.gameObject.SetActive(true);
                }
            }

            // Don't do anything if the worldspace is an interior, at most, the distant world will be disabled
            PersistentReferences.PersistentReferenceManager.instance.CycleAllPRefsOnce();
        }
    }
}