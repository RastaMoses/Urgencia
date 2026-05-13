using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.SaveSystem;
using UnityEditor;
using System;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemInWorld : MonoBehaviour, IHittable, IDraggable
    {
        public string GUIDStr; // and ID for the item autoamtically generated
        public Rigidbody rb;
        private bool attemptedToLoad = false;

        // Created Item
        public Item Item;
        public ItemMetadata metadata;
        public int Amount = 1;
        public bool canBeDragged = true;

        // Save Data
        public bool intentionalDestroy = false;         // True if this item is intentionally destroyed (picked up)
        public bool hasBeenTouched = false;             // Ture if this item is not in its original position (hence will need to be saved)
        public bool isCreatedItem = false;              // True if this item has been created by the player or the game at runtime
        public CreatedItemInWorldSaveData createdData;  // The createData assigned with this item if isCreatedItem == true

        [ContextMenu("Regenerate GUIDStr")]
        public void RegenerateGUIDStr()
        {
            GUIDStr = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        private void Reset()
        {
            rb = GetComponent<Rigidbody>();
        }

        [Space(5)]

        [Header("On Player Take")]
        public Events events;

        [HideInInspector] public bool hasBeenTaken = false;

        void Start()
        {
            TryLoadFromSavefile();
        }

        public void Hit(Vector3 hitDir, float forceAmount = 25, DamageContext damageContext = null)
        {
            if (!rb)
                rb = GetComponent<Rigidbody>();

            rb.AddForce(hitDir * forceAmount, ForceMode.Impulse);
        }

        public void GenerateDecal()
        {

        }

        void FixedUpdate()
        {
            if(attemptedToLoad)
                CheckForTouched();
        }

        public bool TryLoadFromSavefile()
        {
            if (!isCreatedItem)
            {
                ItemInWorldSaveData data;
                SaveSystemManager.instance.saveFile.ItemsInWorldData.allItemsInWorld.TryGetValue(GUIDStr, out data);

                if (data != null)
                {
                    if (data.isPickedUp)
                    {
                        Destroy(gameObject);
                        return true;
                    }
                    else
                    {
                        rb.isKinematic = data.isKinematic;

                        transform.position = data.position;
                        transform.rotation = data.rotation;

                        metadata = data.metadata;
                    }

                    attemptedToLoad = true;
                    return true;
                }
                attemptedToLoad = true;
                return false;
            }

            attemptedToLoad = true;
            return false;
        }


        public string worldspaceID = string.Empty;
        public void SaveOnFile(bool hasBeenPickedUp)
        {
            if (!isCreatedItem)
            {
                var allItems = SaveSystemManager.instance.saveFile.ItemsInWorldData.allItemsInWorld;

                // Check if we have to remove this because it was in older worldspace
                string curWorldspaceID = GetCenterCell().ID;
                if (curWorldspaceID != worldspaceID)
                {
                    // That means the item was moved from the original location to another cell, could happen by the player that drags the item

                    // Set it as picked up and make it a created item
                    if (SaveSystemManager.instance.saveFile.ItemsInWorldData.allItemsInWorld.ContainsKey(GUIDStr))
                        SaveSystemManager.instance.saveFile.ItemsInWorldData.allItemsInWorld[GUIDStr] = ToSaveData(true);
                    else
                        SaveSystemManager.instance.saveFile.ItemsInWorldData.allItemsInWorld.Add(GUIDStr, ToSaveData(true));

                    isCreatedItem = true;
                    transform.SetParent(CellInformation.activeCells[curWorldspaceID].createdItemsT);
                    SaveOnFile(false); // recall thsi function, the item will be saved as a created item
                    return;
                }

                if (SaveSystemManager.instance.saveFile.ItemsInWorldData.allItemsInWorld.ContainsKey(GUIDStr))
                {
                    // Update entry
                    SaveSystemManager.instance.saveFile.ItemsInWorldData.allItemsInWorld[GUIDStr] = ToSaveData(hasBeenPickedUp);
                }
                else // create
                    SaveSystemManager.instance.saveFile.ItemsInWorldData.allItemsInWorld.Add(GUIDStr, ToSaveData(hasBeenPickedUp));
            }
            else
            {
                var allItems = SaveSystem.SaveSystemManager.instance.saveFile.CreatedItemsInWorldData.allCreatedItemsInWorld;
                // Add this created item

                // Check if we have to remove this because it was in older worldspace
                string curWorldspaceID = GetCenterCell().ID;
                if (curWorldspaceID != worldspaceID)
                {
                    // That means the item was moved from the original location to another cell, could happen by the player that drags the item
                    if(allItems.ContainsKey(worldspaceID))
                        allItems[worldspaceID].itemsInThis.Remove(createdData);

                    if (CellInformation.activeCells.ContainsKey(curWorldspaceID))
                    {
                        transform.SetParent(CellInformation.activeCells[curWorldspaceID].createdItemsT);
                        CellInformation.activeCells[curWorldspaceID].AddTouchedItem(this);
                    }
                }


                worldspaceID = curWorldspaceID;
                
                if (allItems.ContainsKey(worldspaceID))
                {
                    if (allItems[worldspaceID].itemsInThis.Contains(createdData))
                    {
                        // Update
                        int index = allItems[worldspaceID].itemsInThis.IndexOf(createdData);
                        allItems[worldspaceID].itemsInThis[index] = ToCreatedItemSaveData();
                    }
                    else // add anew
                        allItems[worldspaceID].itemsInThis.Add(ToCreatedItemSaveData());
                }
                else
                {
                    // Create a new category of worldspace and a new item in it
                    allItems.Add(worldspaceID, new SaveSystem.CreatedItemInWorldCollection());
                    allItems[worldspaceID].itemsInThis.Add(ToCreatedItemSaveData());
                }
            }
        }

        public ItemInWorldSaveData ToSaveData(bool hasBeenPickedUp = false)
        {
            ItemInWorldSaveData data = new ItemInWorldSaveData()
            {
                worldspaceID = worldspaceID,
                metadata = metadata,
                position = transform.position,
                rotation = transform.rotation,
                isPickedUp = hasBeenPickedUp,
                isKinematic = rb.isKinematic
            };

            return data;
        }

        public CreatedItemInWorldSaveData ToCreatedItemSaveData(bool hasBeenPickedUp = false)
        {
            CreatedItemInWorldSaveData data = new CreatedItemInWorldSaveData()
            {
                itemID = Item.ItemID,
                metadata = metadata,
                WorldspaceID = worldspaceID,
                amount = Amount,
                position = transform.position,
                rotation = transform.rotation,
                isKinematic = rb.isKinematic,
            };

            createdData = data;
            return data;
        }

        /// <summary>
        /// Called when a created item has been picked up - used to save the state
        /// </summary>
        public void DeleteCreatedRecord()
        {
            intentionalDestroy = true;
            SaveSystemManager.instance.saveFile.CreatedItemsInWorldData.allCreatedItemsInWorld[WorldManager.instance.currentCenterCell.ID].itemsInThis.Remove(createdData);
        }

        public Cell GetCenterCell()
        {
            if (WorldManager.instance.currentWorldspace.worldSpaceType == Worldspace.WorldSpaceType.Exterior)
            {
                float shortestDistance = 99999.999f;
                int shortestDistanceID = 0;
                for (int i = 0; i < WorldManager.instance.currentWorldspace.cells.Length; i++)
                {
                    float curDistance = Vector3.Distance(transform.position, WorldManager.instance.currentWorldspace.cells[i].cellInWorldCoordinates);
                    if (curDistance < shortestDistance)
                    {
                        shortestDistance = curDistance;
                        shortestDistanceID = i;
                    }
                }

                return WorldManager.instance.currentWorldspace.cells[shortestDistanceID];
            }
            else
            {
                return WorldManager.instance.currentCenterCell;
            }
        }

        [ContextMenu("SetTouched")]
        public void SetTouched()
        {
            string curWorldspace = GetCenterCell().ID;

            /*
            if (string.IsNullOrEmpty(worldspaceID))
                worldspaceID = GetCenterCell().ID;
            */

            if (CellInformation.activeCells.ContainsKey(curWorldspace))
            {
                hasBeenTouched = true;
                CellInformation.activeCells[curWorldspace].AddTouchedItem(this);
            }
        }

        bool delayedSetTouchedCalled = false;
        public void CheckForTouched()
        {
            if(!hasBeenTouched)
            {
                if (rb.linearVelocity.magnitude > 2f)
                {
                    // If there is any loading in progress, delay the SetTouched (this is used to prevent wrong cell pick from SetTouched call)
                    if (WorldManager.instance.isLoading || !CellsSystem.CellInformation.AllActiveCellsLoaded() || GameStatus.instance.IsPaused)
                    {
                        if(!delayedSetTouchedCalled)
                            StartCoroutine(DelayedSetTouched());
                    }
                    else
                        SetTouched();

                }
            }
        }

        public IEnumerator DelayedSetTouched()
        {
            delayedSetTouchedCalled = true;

            while (WorldManager.instance.isLoading || !CellsSystem.CellInformation.AllActiveCellsLoaded() || GameStatus.instance.IsPaused)
                yield return new WaitForEndOfFrame();

            SetTouched();

            delayedSetTouchedCalled = false;
        }

        void IDraggable.OnBeingDragged()
        {
            SetTouched();
        }

        void IDraggable.OnDragEnds()
        {
            
        }

        GameObject IDraggable.GetGameObject()
        {
            return gameObject;
        }
    }
}