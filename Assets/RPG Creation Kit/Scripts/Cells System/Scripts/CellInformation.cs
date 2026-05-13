using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit.PersistentReferences;
using RPGCreationKit.CellsSystem;
using UnityEngine.SceneManagement;
using RPGCreationKit.AI;
using RPGCreationKit.SaveSystem;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace RPGCreationKit.CellsSystem
{
    [System.Serializable]
    public class AIInWorldDictionary : SerializableDictionary<string, RckAI> { }


    [System.Serializable]
    public class AllDoorsDictionary : SerializableDictionary<string, Door> { }

    [System.Serializable]
    public class AllActionPointsDictionary : SerializableDictionary<string, NPCActionPoint> { }

    [System.Serializable]
    public class MutablesInWorldDictionary : SerializableDictionary<string, Mutable> { }

    [System.Serializable]
    public class TargetableTransformInWorldDictionary : SerializableDictionary<string, TargetableTransform> { }

    [System.Serializable]
    public class AIPathInWorldDictionary : SerializableDictionary<string, AICustomPath> { }

    [System.Serializable]
    public class CellInformation : MonoBehaviour
    {
        public static Dictionary<string, CellInformation> activeCells = new Dictionary<string, CellInformation>();

        public Cell cell;
        public CellEntryTransform cellEntry; // The default entry location for this cell (used when teleported)

        public Transform createdItemsT;
        public Transform aiContainer;

        //public List<NPCActionPoint> actionPoints = new List<NPCActionPoint>();
        public AllActionPointsDictionary allActionPoints = new AllActionPointsDictionary();
        public AllDoorsDictionary allDoors = new AllDoorsDictionary();

        private List<string> AIToInstantiateIfCached = new List<string>();

        public MutablesInWorldDictionary allMutables = new MutablesInWorldDictionary();

        public TargetableTransformInWorldDictionary allTargetables = new TargetableTransformInWorldDictionary();

        public AIPathInWorldDictionary allAIPaths = new AIPathInWorldDictionary();

        public int touchedItemsInWorldCount
        {
            get { return touchedItemsInWorld.Count; }
        }
        [SerializeField] private List<ItemInWorld> touchedItemsInWorld;
        [HideInInspector] [SerializeField] public AIInWorldDictionary aiInWorld = new AIInWorldDictionary();


        public bool isCompletlyLoaded = false;
        public bool isActiveInScene = true;

        public string cellOwner = string.Empty;

        public static void OnAnyCellGetsUnloaded()
        {
            foreach (KeyValuePair<string, CellInformation> entry in activeCells)
                entry.Value.UpdateCell();
        }

        public static bool AllActiveCellsLoaded()
        {
            foreach (KeyValuePair<string, CellInformation> cell in activeCells)
            {
                if (!cell.Value.isCompletlyLoaded)
                    return false;
            }

            return true;
        }

        private void Awake()
        {
            activeCells.Add(cell.ID, this);
        }

        private void OnDestroy()
        {
            activeCells.Remove(cell.ID);
        }

        [ContextMenu("DEBUG ACTIVE CELLS")]
        public void DebugActiveCells()
        {
            foreach(var item in activeCells)
                Debug.Log(item.Key);
        }

        public static bool IsAIInCombatWithPlayer(string _id)
        {
            foreach (KeyValuePair<string, CellInformation> cell in activeCells)
            {
                if (cell.Value.aiInWorld.ContainsKey(_id))
                {
                    RckAI ai = cell.Value.aiInWorld[_id];

                    foreach(VisibleEnemy e in ai.enemyTargets)
                        if(e.m_entity.transform.CompareTag("Player"))
                            return true;
                }
            }

            return false;
        }

        // optimize this by checking in the player's cell first
        public static bool IsAnyAIInCombatWithPlayer()
        {
            foreach (KeyValuePair<string, CellInformation> cell in activeCells)
            {
                foreach (KeyValuePair<string, RckAI> ai in cell.Value.aiInWorld)
                {
                    if(!ai.Value.isInOfflineMode && ai.Value.isAlive)
                        foreach (VisibleEnemy e in ai.Value.enemyTargets)
                            if (e.m_entity.transform.CompareTag("Player"))
                                return true;
                }
            }

            return false;
        }

        public static bool TryToGetAI(string _id, out RckAI ai)
        {
            foreach (KeyValuePair<string, CellInformation> cell in activeCells)
            {
                if (cell.Value.aiInWorld.ContainsKey(_id))
                {
                    ai = cell.Value.aiInWorld[_id];
                    return true;
                }
            }

            ai = null;
            return false;
        }

        public static bool TryToGetPath(string _id, out AICustomPath _path)
        {
            foreach (KeyValuePair<string, CellInformation> cell in activeCells)
            {
                if (cell.Value.allAIPaths.ContainsKey(_id))
                {
                    _path = cell.Value.allAIPaths[_id];
                    return true;
                }
            }

            _path = null;
            return false;
        }

        public static bool TryToGetMutable(string _id, out Mutable mutable)
        {
            foreach (KeyValuePair<string, CellInformation> cell in activeCells)
            {
                if (cell.Value.allMutables.ContainsKey(_id))
                {
                    mutable = cell.Value.allMutables[_id];
                    return true;
                }
            }

            mutable = null;
            return false;
        }

        public static bool TryToGetDoor(string _id, out Door door)
        {
            foreach (KeyValuePair<string, CellInformation> cell in activeCells)
            {
                if (cell.Value.allDoors.ContainsKey(_id))
                {
                    door = cell.Value.allDoors[_id];
                    return true;
                }
            }

            door = null;
            return false;
        }


        public void OnEnable()
        {
            //ActivateAllPRefInThisCell();

            for(int i = 0; i < AIToInstantiateIfCached.Count; i++)
            {
                var aiData = SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiDictionary[AIToInstantiateIfCached[i]];

                var aiToInstantiate = AIDatabase.GetAI(AIToInstantiateIfCached[i]);
                // Spawn the item in the world
                RckAI ai = Instantiate(aiToInstantiate, aiData.position, aiData.rotation, aiContainer).GetComponent<RckAI>();
                ai.hasBeenInstantiated = true;
                ai.runtimeStartingCell = cell.ID;

                Debug.Log("Adding and instantiated: " + ai.entityID);

                aiInWorld.Add(ai.entityID, ai);

                ai.DelayedStart();
                ai.TryLoadFromSavefile();
            }

            AIToInstantiateIfCached.Clear();
        }

        public void AddToAIToInstantiateIfCached(string _toadd)
        {
            AIToInstantiateIfCached.Add(_toadd);

            if(gameObject.activeInHierarchy)
            {
                for (int i = 0; i < AIToInstantiateIfCached.Count; i++)
                {
                    var aiData = SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiDictionary[AIToInstantiateIfCached[i]];

                    var aiToInstantiate = AIDatabase.GetAI(AIToInstantiateIfCached[i]);
                    // Spawn the item in the world
                    RckAI ai = Instantiate(aiToInstantiate, aiData.position, aiData.rotation, aiContainer).GetComponent<RckAI>();
                    ai.hasBeenInstantiated = true;
                    ai.runtimeStartingCell = cell.ID;

                    Debug.Log("Adding and instantiated: " + ai.entityID);

                    aiInWorld.Add(ai.entityID, ai);

                    ai.DelayedStart();
                    ai.TryLoadFromSavefile();
                }

                AIToInstantiateIfCached.Clear();
            }
        }

        private void Start()
        {
            isCompletlyLoaded = false;
            FinishLoadCell();
        }



        public void FinishLoadCell()
        {
            SpawnAllCreatedItemInThisCell();
            LoadAllAIInCell();
            SpawnAllAIInThisCell();
            LoadAIFromSave();
            LoadAllMutables();



            isCompletlyLoaded = true;
        }

        /// <summary>
        /// From the savefile picks every created item inside this cell and spawns them
        /// </summary>
        public void SpawnAllCreatedItemInThisCell()
        {
            if (SaveSystem.SaveSystemManager.instance.saveFile.CreatedItemsInWorldData.allCreatedItemsInWorld.ContainsKey(cell.ID))
            {
                var allItems = SaveSystem.SaveSystemManager.instance.saveFile.CreatedItemsInWorldData.allCreatedItemsInWorld[cell.ID].itemsInThis;
                for(int i = 0; i < allItems.Count; i++)
                {
                    var itemToInstantate = ItemsDatabase.GetItem(allItems[i].itemID);
                    // Spawn the item in the world
                    ItemInWorld itemInWorld = Instantiate(itemToInstantate.itemInWorld, allItems[i].position, allItems[i].rotation, createdItemsT).GetComponent<ItemInWorld>();
                    itemInWorld.metadata = allItems[i].metadata;
                    itemInWorld.isCreatedItem = true;
                    itemInWorld.Amount = allItems[i].amount;
                    itemInWorld.rb.isKinematic = allItems[i].isKinematic;
                    itemInWorld.gameObject.transform.position = allItems[i].position;
                    itemInWorld.gameObject.transform.rotation = allItems[i].rotation;
                    itemInWorld.worldspaceID = allItems[i].WorldspaceID;
                    itemInWorld.createdData = allItems[i];
                }
            }
        }

        public void SpawnAllAIInThisCell()
        {
            if (SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiCellDictionary.ContainsKey(cell.ID))
            {
                var allAI = SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiCellDictionary[cell.ID].allIDs;

                for (int i = 0; i < allAI.Count; i++)
                {
                    var aiData = SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiDictionary[allAI[i]];

                    RckAI aicheck = null;
                    CellInformation.TryToGetAI(allAI[i], out aicheck);

                    if (aiData.startingCellID == aiData.saveCellID || aicheck != null)
                        continue;

                    var aiToInstantiate = AIDatabase.GetAI(allAI[i]);
                    // Spawn the item in the world
                    RckAI ai = Instantiate(aiToInstantiate, aiData.position, aiData.rotation, aiContainer).GetComponent<RckAI>();
                    ai.hasBeenInstantiated = true;
                    ai.runtimeStartingCell = cell.ID;

                    aiInWorld.Add(ai.entityID, ai);
                }
            }
        }

        public RckAI SpawnNewAI(string _ID, Vector3 _pos, Quaternion _rot)
        {
            var aiToInstantiate = AIDatabase.GetAI(_ID);

            if (aiToInstantiate != null)
            {
                // Spawn the item in the world
                RckAI ai = Instantiate(aiToInstantiate, _pos, _rot, aiContainer).GetComponent<RckAI>();
                ai.hasBeenInstantiated = true;
                ai.startingCellID = "";
                ai.runtimeStartingCell = cell.ID;
                ai.cellIDOfLastSaved = cell.ID;
                ai.DelayedStart();
                ai.TryLoadFromSavefile();
                ai.SaveOnFile_JustInstantaited();

                aiInWorld.Add(ai.entityID, ai);
                return ai;
            }

            return null;
        }

        public RckAI SpawnNewAIWhileCellIsCached(string _ID, Vector3 _pos, Quaternion _rot)
        {
            var aiToInstantiate = AIDatabase.GetAI(_ID);

            if (aiToInstantiate != null)
            {
                // Spawn the item in the world
                RckAI ai = Instantiate(aiToInstantiate, _pos, _rot, aiContainer).GetComponent<RckAI>();
                ai.hasBeenInstantiated = true;
                ai.startingCellID = "";
                ai.runtimeStartingCell = cell.ID;
                ai.cellIDOfLastSaved = cell.ID;
                ai.DelayedStart();
                ai.TryLoadFromSavefile();
                ai.SaveOnFile_JustInstantaited();

                aiInWorld.Add(ai.entityID, ai);

                ai.onlineComponents.OnGoingOffline();

                return ai;
            }

            return null;
        }


        public void SpawnAIInDistantCell(string _ID, string _cellID, Vector3 _pos, Quaternion _rot)
        {
            var aiToInstantiate = AIDatabase.GetAI(_ID);

            if (aiToInstantiate != null)
            {
                // Spawn the item in the world
                RckAI ai = Instantiate(aiToInstantiate, _pos, _rot, null).GetComponent<RckAI>();
                ai.hasBeenInstantiated = true;

                ai.runtimeStartingCell = _cellID;
                ai.startingCellID = "";
                ai.cellIDOfLastSaved = _cellID;

                ai.transform.position = _pos;
                ai.transform.rotation = _rot;
                ai.DelayedStart();
                ai.TryLoadFromSavefile();
                ai.SaveOnFile_JustInstantaited();
                Destroy(ai.gameObject);
            }
        }

        public void LoadAllAIInCell()
        {
            foreach(Transform go in aiContainer)
            {
                RckAI ai = go.GetComponent<RckAI>();
                AISaveData aiData = null;

                if (ai == null)
                    continue;

                SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiDictionary.TryGetValue(ai.entityID, out aiData);

                if (aiData == null || aiData.startingCellID == aiData.saveCellID)
                {
                    aiInWorld.Add(ai.entityID, ai);
                }
                else
                    Destroy(ai.gameObject); // Destroy the instance and wait for it to be instantiated.
            }
        }

        public void LoadAIFromSave()
        {
            foreach (KeyValuePair<string, RckAI> ai in aiInWorld)
            {
                ai.Value.DelayedStart();
                ai.Value.TryLoadFromSavefile();
            }
        }

        private void LoadAllMutables()
        {
            for (int i = 0; i < allMutables.Count; i++)
                foreach (KeyValuePair<string, Mutable> item in allMutables)
                    if (item.Value != null)
                        item.Value.LoadFromSavefile();
        }

        void ActivateAllPRefInThisCell()
        {
            /*
            if(refIDInThisCell != null && refIDInThisCell.Count > 0)
            {
                for (int i = 0; i < refIDInThisCell.Count; i++)
                    PersistentReferenceManager.instance.ActivatePersistentReference(refIDInThisCell[i]);
            }
            */
        }

        void DisableAllPRefInThisCell()
        {
            /*
            if (refIDInThisCell != null && refIDInThisCell.Count > 0)
            {
                for (int i = 0; i < refIDInThisCell.Count; i++)
                    PersistentReferenceManager.instance.DisablePersistentReference(refIDInThisCell[i]);
            }
            */
        }

        public void OnCellIsLoaded()
        {

        }

        public void OnCellIsBeingUnloaded()
        {
            SaveTouchedItems();
            OnAnyCellGetsUnloaded();

            // Update cell gets called by the line above
            //SaveTouchedItems();
            //SaveRckAI();

            List<RckAI> modifiableAI = new List<RckAI>();

            foreach (KeyValuePair<string, RckAI> item in aiInWorld)
            {
                if (item.Value != null && item.Value.gameObject.activeInHierarchy)
                    modifiableAI.Add(item.Value);
            }

            for (int i = 0; i < modifiableAI.Count; i++)
                modifiableAI[i].OnBeingUnloaded(this);

            touchedItemsInWorld.Clear();

            DisableAllPRefInThisCell();
        }

        public void UpdateCell()
        {
            SaveRckAI();
        }

        public void AddTouchedItem(ItemInWorld _item)
        {
            if(_item != null)
                touchedItemsInWorld.Add(_item);
        }

        public void SaveTouchedItems()
        {
            try
            {
                for (int i = 0; i < touchedItemsInWorld.Count; i++)
                    if (touchedItemsInWorld[i] != null)
                        touchedItemsInWorld[i].SaveOnFile(false);
            }
            catch { }
        }

        public void SaveRckAI()
        {
            foreach(KeyValuePair<string, RckAI> item in aiInWorld)
            {
                if (item.Value != null && item.Value.gameObject.activeInHierarchy)
                {
                    item.Value.SaveOnFile();
                }
            }
        }

        public void AssignEveryItemCellID()
        {
            ItemInWorld[] allItems = FindObjectsOfType<ItemInWorld>();

            for (int i = 0; i < allItems.Length; i++)
            {
                allItems[i].worldspaceID = cell.ID;

#if UNITY_EDITOR
                EditorUtility.SetDirty(allItems[i]);
#endif
            }
        }

        public NPCActionPoint GetAnUnusedActionPoint(bool useTags = false, bool excludeTags = false, params TypeActionPoint[] tags)
        {
            if(useTags == false)
            {
                // Stores a list of possibilities to avoid picking the same point if it was occupied before
                List<string> possibilities = new List<string>();

                foreach(KeyValuePair<string, NPCActionPoint> ap in allActionPoints)
                {
                    if(ap.Value == null)
                    {
                        Debug.Log("There is a Not Assigned ActionPoint assigned to the CellInfo of the Cell: " + cell.ID);
                        continue;
                    }

                    possibilities.Add(ap.Key);
                }

                NPCActionPoint pointGot = null;
                while (possibilities.Count >= 1)
                {
                    int generatedIndex = 0;

                    if(possibilities.Count > 1)
                        generatedIndex = Random.Range(0, possibilities.Count);

                    // Get a random point
                    pointGot = allActionPoints[possibilities[generatedIndex]];

                    // If it's not occupied return it
                    if (!pointGot.isOccupied)
                        return pointGot;

                    possibilities.RemoveAt(generatedIndex);
                }

                // All points in this cell were occupied
                return null;
            }
            else // We want a research with tags
            {
                // Stores a list of possibilities to avoid picking the same point if it was occupied before
                List<string> possibilities = new List<string>();

                foreach (KeyValuePair<string, NPCActionPoint> ap in allActionPoints)
                {
                    if (ap.Value == null)
                    {
                        Debug.Log("There is a Not Assigned ActionPoint assigned to the CellInfo of the Cell: " + cell.ID);
                        continue;
                    }

                    possibilities.Add(ap.Key);
                }

                NPCActionPoint pointGot = null;
                while (possibilities.Count >= 1)
                {
                    int generatedIndex = Random.Range(0, possibilities.Count);

                    // Get a random point
                    pointGot = allActionPoints[possibilities[generatedIndex]];

                    if (!pointGot.isOccupied)
                    {
                        if(!excludeTags &&  System.Array.Exists(tags, tag => tag == pointGot.actionType) || // If it has the tag we want it to be included
                            excludeTags && !System.Array.Exists(tags, tag => tag == pointGot.actionType))  // Or if it doesn't have the tag we want it to be included
                        {
                            return pointGot;
                        }
                    }

                    possibilities.RemoveAt(generatedIndex);
                }

                // All points in this cell were occupied
                return null;
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if(cell)
                Handles.Label(transform.position, cell.cellCoordinates.ToString());
            else
                Handles.Label(transform.position, "CELL_INFO_NOT_ASSIGNED");
        }


        public void UpdateCellFromInspector()
        {
            CellInformation cellInfo = this;

            // Update every single thing.

            // Update ActionPoints
            try
            {
                cellInfo.allActionPoints.Clear();
                RPGCreationKit.AI.NPCActionPoint[] allActionPoints = FindObjectsOfType<RPGCreationKit.AI.NPCActionPoint>();

                for (int i = 0; i < allActionPoints.Length; i++)
                {
                    if (string.IsNullOrEmpty(allActionPoints[i].actionPointID))
                        Debug.LogWarning("Some NPC Action Point doesn't have an ID assigned. Assign them immediatly to avoid problems with IDs.");
                    else
                        // Check if this is not a mount action point
                        if (!(allActionPoints[i] is MountupPoint))
                            cellInfo.allActionPoints.Add(allActionPoints[i].actionPointID, allActionPoints[i]);
                }
            }
            catch (System.Exception e)
            {

                Debug.Log(e);
            }

            // Update Doors
            cellInfo.allDoors.Clear();
            RPGCreationKit.CellsSystem.Door[] allDoors = FindObjectsOfType<RPGCreationKit.CellsSystem.Door>();

            for (int i = 0; i < allDoors.Length; i++)
            {
                if (string.IsNullOrEmpty(allDoors[i].objReference))
                    Debug.LogWarning("Some Door doesn't have an ID assigned. Assign them immediatly to avoid problems with IDs.");
                else
                    cellInfo.allDoors.Add(allDoors[i].objReference, allDoors[i]);
            }

            // Update mutables
            cellInfo.allMutables.Clear();
            RPGCreationKit.Mutable[] allMutables = FindObjectsOfType<RPGCreationKit.Mutable>();

            for (int i = 0; i < allMutables.Length; i++)
            {
                if (string.IsNullOrEmpty(allMutables[i].GUIDStr))
                    Debug.LogWarning("Some Door doesn't have an ID assigned. Assign them immediatly to avoid problems with IDs.");
                else
                    cellInfo.allMutables.Add(allMutables[i].GUIDStr, allMutables[i]);
            }


            // Put all AI in AI_Container
            RPGCreationKit.AI.RckAI[] allAI = FindObjectsOfType<RPGCreationKit.AI.RckAI>();

            for (int i = 0; i < allAI.Length; i++)
            {
                allAI[i].transform.SetParent(cellInfo.aiContainer);
                allAI[i].startingCellID = cellInfo.cell.ID;
                EditorUtility.SetDirty(allAI[i]);
            }

            // Update targetables
            cellInfo.allTargetables.Clear();
            RPGCreationKit.TargetableTransform[] allTargetables = FindObjectsOfType<RPGCreationKit.TargetableTransform>();

            for (int i = 0; i < allTargetables.Length; i++)
            {
                if (string.IsNullOrEmpty(allTargetables[i].ID))
                    Debug.LogWarning("Some TargetableTransform doesn't have an ID assigned. Assign them immediatly to avoid problems with IDs.");
                else
                    cellInfo.allTargetables.Add(allTargetables[i].ID, allTargetables[i]);
            }

            // Update paths
            cellInfo.allAIPaths.Clear();
            RPGCreationKit.AI.AICustomPath[] allPaths = FindObjectsOfType<RPGCreationKit.AI.AICustomPath>();

            for (int i = 0; i < allPaths.Length; i++)
            {
                if (string.IsNullOrEmpty(allPaths[i].ID))
                    Debug.LogWarning("Some TargetableTransform doesn't have an ID assigned. Assign them immediatly to avoid problems with IDs.");
                else
                    cellInfo.allAIPaths.Add(allPaths[i].ID, allPaths[i]);
            }

            // Assign whatever else
            AssignEveryItemCellID();
        }

#endif

    }
}