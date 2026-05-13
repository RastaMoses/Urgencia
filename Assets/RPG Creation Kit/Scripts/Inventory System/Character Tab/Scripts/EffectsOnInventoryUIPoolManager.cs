using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class EffectsOnInventoryUIPoolManager : MonoBehaviour
    {
        public static EffectsOnInventoryUIPoolManager pool;
        [SerializeField] private GameObject pooledObject;

        [SerializeField] private int pooledAmount;
        private List<GameObject> pooledObjects;

        private void Awake()
        {
            if (pool == null)
                pool = this;
            else
                Destroy(this);

            // Start the pool
            pooledObjects = new List<GameObject>();
            for (int i = 0; i < pooledAmount; i++)
            {
                GameObject obj = Instantiate(pooledObject, transform);
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }
        }


        private void OnEnable()
        {
            for(int i = 0; i < EntityAttributes.PlayerAttributes.activeEffects.Count; i++)
            {
                EffectInCharacterTab effectUI = GetPooledObject();
                effectUI.gameObject.SetActive(true);
                effectUI.Init(EntityAttributes.PlayerAttributes.activeEffects[i], false);
            }
        }

        private void OnDisable()
        {
            for(int i = 0; i < pooledObjects.Count; i++)
                pooledObjects[i].SetActive(false);
        }

        public EffectInCharacterTab GetPooledObject()
        {
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                if (!pooledObjects[i].activeInHierarchy)
                    return pooledObjects[i].GetComponent<EffectInCharacterTab>();
            }

            // If this code runs, we did not find any available pooled object, so let's make another one.
            GameObject obj = Instantiate(pooledObject, transform);
            obj.SetActive(false);
            pooledObjects.Add(obj);
            return obj.GetComponent<EffectInCharacterTab>();
        }
    }
}

