using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.PersistentReferences
{
    [System.Serializable]
    public class PersistentReferencesDictionary : SerializableDictionary<string, PersistentReference> { }

    [System.Serializable]
    public class CellsDoorsDictionary : SerializableDictionary<string, string> { }

    /// <summary>
    /// Manages each and every persistent reference in the world, making all of them accessible anytime.
    /// </summary>
    public class PersistentReferenceManager : MonoBehaviour
    {
        public static PersistentReferenceManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        private void Start()
        {
            LoadAllMutables();
        }

        [SerializeField] Transform tAI;
        [SerializeField] Transform tLootingPoint;
        [SerializeField] Transform tDoor;
        [SerializeField] Transform tOther;

        [SerializeField] Transform tUnregistered;

        public PersistentReferencesDictionary refs;

        public CellsDoorsDictionary cellsDoorsDictionary;

        public MutablesInWorldDictionary persistentMutableDictionary;

        /// <summary>
        /// Register a PersistentReference to the Dictionary and trasfers its GameObject to the _PersistentReferences_ scene.
        /// </summary>
        /// <param name="_ref">The reference to register.</param>
        /// <returns>True if successfully registered.</returns>
        public bool RegisterPersistentReference(PersistentReference _ref)
        {
            // Conditions of error
            if (_ref == null)
                return false;

            if(string.IsNullOrEmpty(_ref.refID))
            {
                Debug.LogWarning("Tried to register a reference of type: " + _ref.type.ToString() + " from the GameObject " + _ref.gameObject.name + ". But no RefID was provided.");
                return false;
            }
            else // Reference is set, proceed to register
            {
                // Checks if the reference is already registered
                if(refs.ContainsKey(_ref.refID))
                {
                    Debug.LogWarning("Tried to register the RefID: " + _ref.refID + " but it was already registered. Aborting...");
                    return false;
                }

                // Adds it to the dictionary
                refs.Add(_ref.refID, _ref);

                // Moves to its scene
                //SceneManager.MoveGameObjectToScene(_ref.gameObject, this.gameObject.scene);

                // Sets the correct parent
                switch(_ref.type)
                {
                    case PersistentReferenceType.AI:
                        _ref.gameObject.transform.SetParent(tAI);
                        break;

                    case PersistentReferenceType.Door:
                        _ref.gameObject.transform.SetParent(tDoor);
                        break;

                    case PersistentReferenceType.LootingPoint:
                        _ref.gameObject.transform.SetParent(tLootingPoint);
                        break;

                    case PersistentReferenceType.Other:
                        _ref.gameObject.transform.SetParent(tOther);
                        break;

                    default:
                        _ref.gameObject.transform.SetParent(tOther);
                        break;
                }

                //DisablePersistentReference(_ref.refID);
                return true;
            }
        }

        /// <summary>
        /// Unregisters a PersistentReference from the Dictionary and trasfers its GameObject under the Unregistered GameObjects.
        /// </summary>
        /// <param name="_ref">The reference to unregister.</param>
        /// <returns>True if successfully unregistered.</returns>
        public bool UnregisterPersistentReference(PersistentReference _ref, bool destroyGameObject = false)
        {
            // Conditions of error
            if (_ref == null)
                return false;

            if (string.IsNullOrEmpty(_ref.refID))
            {
                Debug.LogWarning("Tried to unregister a persistent reference of type: " + _ref.type.ToString() + " from the GameObject " + _ref.gameObject.name + ". But no RefID was provided.");
                return false;
            }

            // Try to get the reference to see if this PersistentReference was registered.
            PersistentReference pTest = GetPersistentReference(_ref.refID);

            if (pTest == null)
            {
                Debug.LogWarning("Tried to unregister the persistent reference with ID " + _ref.refID + " but it wasn't registered in first place.");
                return false;
            }

            refs.Remove(_ref.refID);

            if (!destroyGameObject)
                _ref.gameObject.transform.SetParent(tUnregistered);
            else
                DestroyImmediate(_ref.gameObject);

            Debug.Log("Persistent Reference with ID: " + _ref.refID + " was successfully unregistered. (Destroyed: " + destroyGameObject + ")");
            return true;
        }

        /// <summary>
        /// Gets a PersistentReference from the Dictionary.
        /// </summary>
        /// <param name="_refID"></param>
        /// <returns></returns>
        public PersistentReference GetPersistentReference(string _refID)
        {
            PersistentReference reference;
            refs.TryGetValue(_refID, out reference);

            if (reference != null)
                return reference;
            else
            {
                Debug.Log("Tried to Get the Persistent Reference with ID: " + _refID + ", but it isn't registered.");
                return null;
            }
        }

        /// <summary>
        /// Activates the GameObject of a Persistent Reference, providing its refID.
        /// </summary>
        /// <param name="_refID"></param>
        public void ActivatePersistentReference(string _refID)
        {
            PersistentReference reference = GetPersistentReference(_refID);

            if (reference != null)
            {
                reference.gameObject.SetActive(true);

                // If the AI was using offline mode
                if (reference.type == PersistentReferenceType.AI)
                {
                    AI.AIPerception ai = reference.GetComponent<AI.AIPerception>();

                    if (ai != null && ai.usesOfflineMode)
                        ai.onlineComponents.OnGoingOnline();
                    else
                        reference.gameObject.SetActive(true);
                }
                else
                    reference.gameObject.SetActive(true);

                reference.isLoaded = true;
            }
        }

        /// <summary>
        /// Activates the GameObject of a Persistent Reference and returns its GameObject, providing its refID.
        /// </summary>
        /// <param name="_refID"></param>
        public GameObject ActivatePersistentReferenceAndGetGameObject(string _refID)
        {
            PersistentReference reference = GetPersistentReference(_refID);

            if (reference != null)
            {
                reference.gameObject.SetActive(true);
                return reference.gameObject;
            }
            else
            {
                Debug.Log("Tried to Activate the Persistent Reference with ID: " + _refID + ", but it isn't registered.");
                return null;
            }
        }

        /// <summary>
        /// Disables the GameObject of a Persistent Reference, providing its refID.
        /// </summary>
        public void DisablePersistentReference(string _refID)
        {
            PersistentReference reference = GetPersistentReference(_refID);

            if (reference != null)
            {
                // If the AI was using offline mode
                if (reference.type == PersistentReferenceType.AI && reference.isLoaded)
                {
                    AI.AIPerception ai = reference.GetComponent<AI.AIPerception>();

                    if (ai != null && ai.usesOfflineMode)
                        ai.onlineComponents.OnGoingOffline();
                    else
                        reference.gameObject.SetActive(false);
                }
                else
                    reference.gameObject.SetActive(false);

                reference.isLoaded = false;
            }
        }

        /// <summary>
        /// Gets the Type of a Persistent Reference, providing its refID
        /// </summary>
        /// <param name="_refID"></param>
        /// <returns></returns>
        public PersistentReferenceType GetPRefType(string _refID)
        {
            PersistentReference pRef = GetPersistentReference(_refID);

            if (pRef != null)
                return pRef.type;
            else
                return default;
        }

        private void LoadAllMutables()
        {
            for (int i = 0; i < persistentMutableDictionary.Count; i++)
                foreach (KeyValuePair<string, Mutable> item in persistentMutableDictionary)
                    if (item.Value != null)
                        item.Value.LoadFromSavefile();
        }

        public void LoadPersistentAI()
        {
            foreach (KeyValuePair<string, PersistentReference> item in refs)
                if (item.Value != null && item.Value.type == PersistentReferenceType.AI)
                {
                    AI.RckAI ai = item.Value.GetComponent<AI.RckAI>();
                    ai.enabled = true;
                    ai.TryLoadFromSaveFileAsPersistentReference();
                    ai.StartCoroutine(ai.PRefCycle());
                }
        }

        public void SaveAllPersistentAI()
        {
            // Save all persistent references
            foreach (KeyValuePair<string, PersistentReference> pRef in refs)
            {
                if (pRef.Value == null)
                    continue;

                // Save AI
                if (pRef.Value.type == PersistentReferenceType.AI)
                    pRef.Value.gameObject.GetComponent<AI.RckAI>().SaveOnFileAsPersistentReference();
            }
        }

        public void CycleAllPRefsOnce()
        {
            foreach (KeyValuePair<string, PersistentReference> pRef in refs)
            {
                if (pRef.Value == null)
                    continue;

                // Save AI
                if (pRef.Value.type == PersistentReferenceType.AI)
                    pRef.Value.gameObject.GetComponent<AI.RckAI>().PRefCycleOnce();
            }
        }
    }
}