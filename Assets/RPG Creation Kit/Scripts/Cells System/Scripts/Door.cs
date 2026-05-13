using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using UnityEngine.SceneManagement;
using UnityEditor;
using RPGCreationKit.SaveSystem;

namespace RPGCreationKit.CellsSystem
{
    public enum DoorLockLevel { VeryEasy, Easy, Medium, Hard, VeryHard, Impossible};

    public class Door : MonoBehaviour, ITargetable
    {
        CellInformation cellInfo = null;

        [SerializeField] public List<string> allDoors = new List<string>(); // All doors of the connected cell

        [Header("General")]
        public string objReference; // DoorRef, MUST be set

        public bool persistentReference;
        public string persistentReferenceID;
        public Animation anim;
        public Transform groundPivot; // The pivot on a walkable surface (for AI to teleport)


        [Header("Door Data")]
        public bool locked = false;
        public DoorLockLevel lockLevel;

        public KeyItem key;

        [Header("Door Teleport")]
        public bool teleports;
        public Cell toCell;
        public string linkedDoorObjRef;

        [Header("Door Sounds")]
        public AudioClip onDoorOpen;
        public AudioClip onDoorOpenLocked;
        public AudioClip onDoorOpenUnlocked;
        public AudioClip onDoorClose;

        public Transform teleportMarker;

        public bool isOpened = false;


        [ContextMenu("Regenerate objReference")]
        public void RegenerateGUIDStr()
        {
            objReference = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        private void Reset()
        {
            if(!teleportMarker)
            {
                // If the teleport marker already exists in the root
                bool found = false;
                foreach(Transform t in transform)
                {
                    if(t.CompareTag("RPG Creation Kit/DoorTeleportMarker"))
                    {
                        found = true;
                        teleportMarker = t;

                        var tComp = t.gameObject.GetComponent<DoorTeleportMarker>();

                        if (tComp != null)
                            tComp.owner = this;
                         else
                        {
                            tComp = t.gameObject.AddComponent<DoorTeleportMarker>();
                            tComp.owner = this;
                        }

                        break;
                    }
                }

                if (!found) // if there was no teleport marker in the root of the door, create it.
                {
                    GameObject telMarker = new GameObject("DoorTeleportMarker")
                    {
                        tag = "RPG Creation Kit/DoorTeleportMarker"
                    };
                    telMarker.transform.parent = gameObject.transform;
                    telMarker.transform.localPosition = Vector3.zero;
                    telMarker.transform.localRotation = Quaternion.identity;
                    telMarker.transform.localScale = new Vector3(1, 2, 1);
                    var comp = telMarker.AddComponent<DoorTeleportMarker>();
                    comp.owner = this;
                    teleportMarker = telMarker.transform;
                }
            }
        }


        private void OnEnable()
        {
            TryLoadFromSavefile();
        }

        private void Update()
        {
            // Keep the savefile updated with the animation time
            if (anim != null && anim.isPlaying)
            {
                if (SaveSystem.SaveSystemManager.instance.saveFile.DoorData.allDoors.ContainsKey(objReference))
                    SaveSystem.SaveSystemManager.instance.saveFile.DoorData.allDoors[objReference].currentAnimationTime = anim[(isOpened) ? "Open" : "Close"].normalizedTime;
                else
                    SaveSystem.SaveSystemManager.instance.saveFile.DoorData.allDoors.Add(objReference, ToSaveData());

                SaveSystem.SaveSystemManager.instance.saveFile.DoorData.allDoors[objReference].currentAnimationTime = anim[(isOpened) ? "Open" : "Close"].normalizedTime;
            }
        }

        public void OpenCloseDoor()
        {
            if (anim == null)
                return;

            if (!anim.isPlaying)
            {
                if (!isOpened)
                {
                    anim.Play("Open");
                    isOpened = true;
                }
                else
                {
                    anim.Play("Close");
                    isOpened = false;
                }
            }

            SaveOnFile();
        }

        public void OpenCloseDoor(bool open)
        {
            if (anim == null)
                return;

            if (!anim.isPlaying)
            {
                if (open)
                {
                    isOpened = true;
                    anim.Play("Open");
                }
                else
                {
                    isOpened = false;
                    anim.Play("Close");
                }
            }

            SaveOnFile();
        }

        public void ForceOpen()
        {
            isOpened = true;
            anim.Play("Open");
        }

        public void ForceOpenCrossFade()
        {
            isOpened = true;
            anim.CrossFade("Open", .25f);
        }

        public void LockDoor(DoorLockLevel level)
        {
            locked = true;
            lockLevel = level;
            SaveOnFile();
        }

        public void UnlockDoor()
        {
            locked = false;
            SaveOnFile();
        }

        private void OnDestroy()
        {
            if(teleportMarker != null)
                DestroyImmediate(teleportMarker.gameObject);

            //cellInfo.allDoorsRef.Remove(objReference);
        }

        public bool TryLoadFromSavefile()
        {
            DoorSaveData data;
            SaveSystemManager.instance.saveFile.DoorData.allDoors.TryGetValue(objReference, out data);

            if(data != null)
            {
                if (anim != null)
                    anim[data.currentAnimation].normalizedTime = data.currentAnimationTime;

                OpenCloseDoor(data.isOpened);
                locked = data.isLocked;
                objReference = data.doorID;
                lockLevel = (DoorLockLevel)data.doorLockLevel;

                return true;
            }

            return false;
        }

        public void SaveOnFile()
        {
            if (SaveSystemManager.instance.saveFile.DoorData.allDoors.ContainsKey(objReference))
            {
                // Update entry
                SaveSystemManager.instance.saveFile.DoorData.allDoors[objReference] = ToSaveData();
            }
            else // create
                SaveSystemManager.instance.saveFile.DoorData.allDoors.Add(objReference, ToSaveData());
        }

        public DoorSaveData ToSaveData()
        {
            DoorSaveData data = new DoorSaveData()
            {
                doorID = objReference,
                isLocked = locked,
                isOpened = isOpened,
                doorLockLevel = (int)lockLevel,
                currentAnimation = (isOpened) ? "Open" : "Close", 
            };

            if(anim != null)
                data.currentAnimationTime = anim[data.currentAnimation].normalizedTime;

            return data;
        }

        string ITargetable.GetID()
        {
            return objReference;
        }

        ITargetableType ITargetable.GetTargetableType()
        {
            return ITargetableType.Door;
        }

        string ITargetable.GetExtraData()
        {
            return (toCell != null) ? toCell.ID : string.Empty;
        }
    }
}