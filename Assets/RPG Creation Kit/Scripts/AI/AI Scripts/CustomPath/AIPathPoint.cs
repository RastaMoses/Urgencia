using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.AI
{
    /// <summary>
    /// A point of a NPC_Path 
    /// </summary>
    [ExecuteInEditMode]
    public class AIPathPoint : MonoBehaviour
    {
        public bool wait = false;                       // Should the AI wait at this point?
        public float waitTime = 2;    // If so, how much time? (seconds)

        public bool faceDirection = false;
        public Transform DirectionToFace; // Direction to face while waiting

        // To trigger when AI reaches this point
        public Events events;
        public string resultScript;

        /*
        // Draw AICustomPath gizmos
        public void OnDrawGizmosSelected()
        {
            this.transform.parent.SendMessage("OnDrawGizmosSelected");
        }
        */
    }
}
