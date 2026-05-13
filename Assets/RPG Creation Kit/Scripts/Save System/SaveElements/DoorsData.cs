using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.SaveSystem
{
    [System.Serializable]
    public class DoorsDataDictionary : SerializableDictionary<string, DoorSaveData> { }

    [System.Serializable]
    public class DoorSaveData
    {
        public string doorID;
        public bool isLocked;
        public bool isOpened;
        public int doorLockLevel;
        public string currentAnimation;
        public float currentAnimationTime;

        public DoorSaveData(string doorID, bool isLocked, bool isOpened, int doorLockLevel, string currentAnimation, float currentAnimationTime)
        {
            this.doorID = doorID;
            this.isLocked = isLocked;
            this.isOpened = isOpened;
            this.doorLockLevel = doorLockLevel;
            this.currentAnimation = currentAnimation;
            this.currentAnimationTime = currentAnimationTime;
        }

        public DoorSaveData(string doorID, bool isLocked)
        {
            this.doorID = doorID;
            this.isLocked = isLocked;
        }

        public DoorSaveData(string doorID, bool isLocked, int lockLevel)
        {
            this.doorID = doorID;
            this.doorLockLevel = lockLevel;
        }

        public DoorSaveData() { }
    }

    /// <summary>
    /// Contains all the doors and their state (opened/closed/locked/unlocked)
    /// </summary>
    [System.Serializable]
    public class DoorsData
    {
        [SerializeField] public DoorsDataDictionary allDoors = new DoorsDataDictionary();
    }
}