using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    [CreateAssetMenu(fileName = "New Eye", menuName = "RPG Creation Kit/Character/New Eyes", order = 1)]
    public class Eye : ScriptableObject
    {
        public string ID;
        public string eyesName;

        public SkinnedMeshRenderer eyes;

        public bool playable;
    }
}