using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class AttachToMesh : MonoBehaviour
    {
        [SerializeField] SkinnedMeshRenderer attachTo;


        [ContextMenu("Attach To Mesh")]
        void Attach()
        {
            SkinnedMeshRenderer newMesh = GetComponent<SkinnedMeshRenderer>();
            newMesh.transform.parent = attachTo.transform.parent;

            newMesh.rootBone = attachTo.rootBone;
            newMesh.bones = attachTo.bones;
        }
    }
}