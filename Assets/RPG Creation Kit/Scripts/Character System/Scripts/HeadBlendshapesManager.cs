using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Manages the child blendshapes of the Head, like eyes, mouth, etc. I.E. Moves Eye's blendshapes in the same position of the eye socket position of the Head Mesh.
    /// </summary>


    [System.Serializable]
    public struct BlendshapeChildOfHead
    {
        public SkinnedMeshRenderer meshRenderer;
        public int index;
        public int derivesFromIndex;
    }

    public class HeadBlendshapesManager : MonoBehaviour
    {
        [SerializeField] SkinnedMeshRenderer headMesh;
        [SerializeField] public SkinnedMeshRenderer mouthMesh;

        [SerializeField] BlendshapeChildOfHead[] blendshapesChildOfHead;
        public BlendshapeChildOfHead[] blendshapesChildOfMouth;

        public bool useMouthMovements = true; // to allow NPCs to not use mouth while not having to disable it for all in RckSettings

        // IDs of the blendshapes that represents facial anim
        public List<int> mouthMovementsBlendshapesIDs = new List<int>() { 24, 25, 26, 27, 28 };

        public void Update()
        {
            AdjustMouthBlendshapes();
        }

        public void AdjustChildBlendshapes()
        {
            for(int i = 0; i < blendshapesChildOfHead.Length; i++)
            {
                blendshapesChildOfHead[i].meshRenderer.
                    SetBlendShapeWeight(blendshapesChildOfHead[i].index, headMesh.GetBlendShapeWeight(blendshapesChildOfHead[i].derivesFromIndex));
            }
        }

        // Used to sync FANIM_ of the head with FANIM_ of the mouth
        public void AdjustMouthBlendshapes()
        {
            for (int i = 0; i < blendshapesChildOfMouth.Length; i++)
            {
                blendshapesChildOfMouth[i].meshRenderer.
                    SetBlendShapeWeight(blendshapesChildOfMouth[i].index, headMesh.GetBlendShapeWeight(blendshapesChildOfMouth[i].derivesFromIndex));
            }
        }

        public void SetMouthBlendshape(float valu)
        {
            if (!useMouthMovements)
                return;

            ResetMouthBlendshapes();

            // Always set first
            if(headMesh && mouthMovementsBlendshapesIDs.Count > 0)
            {
                headMesh.SetBlendShapeWeight(mouthMovementsBlendshapesIDs[0], valu);

                for(int i = 1; i < mouthMovementsBlendshapesIDs.Count; i++)
                {
                    float cur = i * 10;
                    if(valu < cur)
                        headMesh.SetBlendShapeWeight(mouthMovementsBlendshapesIDs[i], valu);
                }
            }
        }

        public void ResetMouthBlendshapes()
        {
            if (!useMouthMovements)
                return;

            if (headMesh)
            {
                headMesh.SetBlendShapeWeight(24, 0);
                headMesh.SetBlendShapeWeight(25, 0);
                headMesh.SetBlendShapeWeight(26, 0);
                headMesh.SetBlendShapeWeight(27, 0);
                headMesh.SetBlendShapeWeight(28, 0);
            }
        }
    }
}