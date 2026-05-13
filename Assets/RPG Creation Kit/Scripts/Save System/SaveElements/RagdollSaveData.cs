using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.SaveSystem
{
    [System.Serializable]
    public class RagdollSaveData 
    {
        [System.Serializable]
        public struct RagdollElement
        {
            public Vector3 pos;
            public Quaternion rot;

            public RagdollElement(Vector3 _p, Quaternion _r)
            {
                pos = _p;
                rot = _r;
            }
        }

        [SerializeField] public List<RagdollElement> ragdollElements = new List<RagdollElement>();
    }
}