using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class EffectsOnPlayerUIPoolManager : MonoBehaviour
    {
        public static EffectsOnPlayerUIPoolManager pool;
        [SerializeField] private GameObject pooledObject;

        [SerializeField] private int pooledAmount;
        private List<GameObject> pooledObjects;

        private void Awake()
        {
            if (pool == null)
                pool = this;
            else
                Destroy(this);
        }
        private void Start()
        {
            // Start the pool
            pooledObjects = new List<GameObject>();
            for(int i = 0; i < pooledAmount; i++)
            {
                GameObject obj = Instantiate(pooledObject, transform);
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }
        }

        public EffectOnEntityUI GetPooledObject()
        {
            for(int i = 0; i < pooledObjects.Count; i++)
            {
                if (!pooledObjects[i].activeInHierarchy)
                    return pooledObjects[i].GetComponent<EffectOnEntityUI>();
            }

            // If this code runs, we did not find any available pooled object, so let's make another one.
            GameObject obj = Instantiate(pooledObject, transform);
            obj.SetActive(false);
            pooledObjects.Add(obj);
            return obj.GetComponent<EffectOnEntityUI>();
        }

    }
}