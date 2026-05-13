using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.AI
{
    public enum AIGUIDReferenceType
    {
        MainTarget = 0,
        CurrentPath = 1,
        DoorToReachTarget = 2,
        ActionPoint = 3
    };

    /// <summary>
    /// Contains useful GUID References that can be accessed by other scripts.
    /// </summary>
    public class AIGUIDReferences : MonoBehaviour
    {

        [ContextMenu("Assign PHYS")]
        public void doo()
        {
            GetComponent<AI.RckAI>().physicalCollider = GetComponent<BoxCollider>();
        }

        /// <summary>
        /// Tries to set a reference, providing a GameObject, it will check if the given GameObject has a GUIDReference, if it has, it will be set as the given type.
        /// </summary>
        /// <param name="_gameObject"></param>
        /// <param name="_type"></param>
        /// <returns></returns>
        public bool TryToSetReference(GameObject _gameObject, AIGUIDReferenceType _type)
        {
            return true;
        }

        /// <summary>
        /// Clears the reference of the provided type
        /// </summary>
        /// <param name="_type"></param>
        public void ClearReference(AIGUIDReferenceType _type)
        {
            
        }

        /// <summary>
        /// Clears all the references
        /// </summary>
        public void ClearAllReferences()
        {
            
        }

        /// <summary>
        /// Tries to get the GameObject of the given type of reference
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public GameObject TryToGetReferenceGameObject(AIGUIDReferenceType _type)
        {
            return null;
        }
    }
}