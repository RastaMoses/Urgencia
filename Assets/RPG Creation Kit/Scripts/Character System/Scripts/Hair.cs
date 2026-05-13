using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    [CreateAssetMenu(fileName = "New Hair", menuName = "RPG Creation Kit/Character/New Hair", order = 1)]
    [System.Serializable]
    public class Hair : ScriptableObject
    {
        public string ID;
        public string hairName;

        public bool playable;
        public bool canBeMale;
        public bool canBeFemale;

        public GameObject mesh;
        public Material[] materials;
    }
}