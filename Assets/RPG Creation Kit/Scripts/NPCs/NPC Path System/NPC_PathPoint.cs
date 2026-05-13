using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;


namespace RPGCreationKit
{
    /// <summary>
    /// A point of a NPC_Path 
    /// </summary>
    [ExecuteInEditMode]
    public class NPC_PathPoint : MonoBehaviour
    {
        public bool Wait = false;                       // Should the NPC wait at this point?
        [HideInInspector] public float WaitTime = 2;    // If so, how much time? (seconds)

        [HideInInspector] public Transform DirectionToFace; // Direction to face while waiting


        // Draw NPC_Path gizmos
        public void OnDrawGizmosSelected()
        {
            this.transform.parent.SendMessage("OnDrawGizmosSelected");
        }
    }
}
