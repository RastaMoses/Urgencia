using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RPGCreationKit.SaveSystem
{
    [Serializable]
    public class SaveFileData
    {
        public string saveType;
        public string fileVersion;
        public string saveDate;
        public uint saveNumber;
    }
}