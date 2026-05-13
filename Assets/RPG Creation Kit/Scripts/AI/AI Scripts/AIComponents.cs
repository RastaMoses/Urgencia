using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEngine.AI;

namespace RPGCreationKit.AI
{
    public class AIComponents
    {
        
    }

    /// <summary>
    /// Defines components that needs to be active only if the AI is fully loaded
    /// </summary>
    [System.Serializable]
    public class AIOnlineComponents
    {
        public RckAI ai;

        public GameObject GFX; // Graphics

        public Rigidbody rigidbody;
        public Animator animator;
        public AIGUIDReferences iGUIDReferences;

        public NavMeshAgent agent;
        public AudioSource audioSource;

        /// <summary>
        /// Called when the AI has to go in Offline mode, disables a variety of components. This call happens when the AI is set to useOfflineMode and needs to be unloaded (like player went far away from it).
        /// </summary>
        public void OnGoingOffline()
        {
            ai.pauseBT = true;
            GFX.SetActive(false);

            rigidbody.isKinematic = true;

            ai.DisablePhysicalCollider();
            ai.DisableInteractableCollider();

            audioSource.enabled = false;
            agent.enabled = false;

            ai.GetCellInfo = false;

            if(ai.movementType != MovementType.InActionPoint)
                ai.movementType = MovementType.Offline;
            ai.isInOfflineMode = true;
        }

        /// <summary>
        /// Called when the AI returns from Offline mode, enables a variety of components. This call happens when the AI is set to useOfflineMode and was unloaded. (like player went near it).
        /// </summary>
        public void OnGoingOnline()
        {
            GFX.SetActive(true);

            if (ai.isPersistentReference && !ai.isAlive)
            {
                ai.isInOfflineMode = false;
                return;
            }

            rigidbody.isKinematic = true;
            ai.EnablePhysicalCollider();
            ai.EnableInteractableCollider();

            audioSource.enabled = true;

            ai.UpdateStateDelayed();
            ai.pauseBT = false;

            ai.GetCellInfo = true;
        }
    }
}