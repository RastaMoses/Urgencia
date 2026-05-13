using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RPGCreationKit.PersistentReferences
{
    public enum PersistentReferenceType
    {
        AI = 0,
        LootingPoint = 1,
        Door = 2,
        Other = 3
    };

    /// <summary>
    /// Component to add on GameObjects that needs to be accessible everywhere, at anytime, despite everything.
    /// </summary>
    public class PersistentReference : MonoBehaviour, ITargetable
    {
        public bool showHandle = true;


        public string refID = "";
        public bool isLoaded = false;
        public PersistentReferenceType type = PersistentReferenceType.Other;

        [ContextMenu("Register this reference")]
        public void RegisterThisReference()
        {
            PersistentReferenceManager.instance.RegisterPersistentReference(this);
        }

        [ContextMenu("Unregister this reference")]
        public void UnregisterThisReference()
        {
            PersistentReferenceManager.instance.UnregisterPersistentReference(this);
        }

        string ITargetable.GetExtraData()
        {
            return string.Empty;
        }

        string ITargetable.GetID()
        {
            return refID;
        }

        ITargetableType ITargetable.GetTargetableType()
        {
            return ITargetableType.PRef;
        }


#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (showHandle)
            {
                Color defaultGColor = GUI.color;

                GUI.color = Color.cyan;
                if (!string.IsNullOrEmpty(refID))
                    Handles.Label(transform.position, refID);
                else
                    Handles.Label(transform.position, "REFERENCE_ID_NOT_ASSIGNED");

                GUI.color = defaultGColor;
            }
        }

#endif
    }
}