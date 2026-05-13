using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEditor;
using RPGCreationKit.SaveSystem;

namespace RPGCreationKit
{
    /// <summary>
    /// A mutable is a GameObject that holds itself plus another GameObject. A mutable can decide which one of either has to be activated.
    /// An example can be a statue, you two versions of a statue, one that is intact and another with a broken piece.
    /// 
    /// At runtime you can decide to break that statue, so the intact one will mutate into the one broken.
    /// </summary>
    public class Mutable : MonoBehaviour
    {
        public string GUIDStr; // and ID for the item autoamtically generated
        
        public GameObject normalObject;
        public GameObject mutatedObject;

        public bool isMutated = false;

        [ContextMenu("Regenerate GUIDStr")]
        public void RegenerateGUIDStr()
        {
            GUIDStr = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }


        private void Start()
        {
            if (isMutated)
                Mutate();
        }

        public void Mutate()
        {
            isMutated = true;

            if(normalObject != null)
            normalObject.SetActive(false);

            if(mutatedObject != null)
                mutatedObject.SetActive(true);

            SaveOnSavefile();
        }

        public void RestoreToNormal()
        {
            isMutated = false;

            if (mutatedObject != null)
                mutatedObject.SetActive(false);

            if(normalObject != null)
                normalObject.SetActive(true);

            SaveOnSavefile();
        }

        public MutableData ToSaveData()
        {
            MutableData newData = new MutableData(isMutated);

            return newData;
        }

        public void LoadFromSavefile()
        {
            MutableData data;
            SaveSystemManager.instance.saveFile.MutablesData.allMutables.TryGetValue(GUIDStr, out data);

            if(data != null)
            {
                if (data.isMutated)
                    Mutate();
                else
                    RestoreToNormal();
            }
        }

        public void SaveOnSavefile()
        {
            var allMutables = SaveSystemManager.instance.saveFile.MutablesData.allMutables;

            if (allMutables.ContainsKey(GUIDStr))
                allMutables[GUIDStr] = ToSaveData();
            else
                allMutables.Add(GUIDStr, ToSaveData());
        }
    }
}