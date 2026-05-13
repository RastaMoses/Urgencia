using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RPGCreationKit.SaveSystem
{
    [Serializable]
    public class FaceBlendshapeSaveData
    {
        public int index;
        public float weight;

        public FaceBlendshapeSaveData(int index, float weight)
        {
            this.index = index;
            this.weight = weight;
        }
    }

    [Serializable]
    public class FaceBlendshapesSaveData
    {
        [SerializeField] public List<FaceBlendshapeSaveData> allShapes = new List<FaceBlendshapeSaveData>();
    }
}