using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;


namespace RPGCreationKit
{
    /// <summary>
    /// This class manages a single Pools for the Items In List
    /// </summary>
    public class ItemsInListPool : MonoBehaviour
    {
        [SerializeField] ItemsInListPoolManager poolManager;
        public GameObject GameObjectToPool;
        public Transform Content;

        public int AmountToPool = 40;

        [Space(5)]

        public List<GameObject> AbsPooledObjects;
        public List<ItemInInventoryUI> usedObjects;

        public bool manualInit = false;

        public void Start()
        {
            if(!manualInit)
                StartPool();
        }

        /// <summary>
        /// Instantiate the items
        /// </summary>
        public void StartPool()
        {
            AbsPooledObjects.Clear();

            // Instantiate the AmountToPool and add it in the list
            for(int i = 0; i < AmountToPool; i++)
            {
                var obj = Instantiate(GameObjectToPool, Content);
                obj.SetActive(false);

                AbsPooledObjects.Add(obj);
            }
        }

      

        /// <summary>
        /// Instantiate new items in the pool
        /// </summary>
        /// <param name="amount"></param>
        public void AddInPool(int amount)
        {
            for(int i = 0; i < amount; i++)
            {
                var obj = Instantiate(GameObjectToPool, Content);
                obj.SetActive(false);

                AbsPooledObjects.Add(obj);
            }
        }

        public void ResetPool()
        {
            usedObjects.Clear();

            for(int i = 0; i < AbsPooledObjects.Count; i++)
            {
                if(AbsPooledObjects[i].activeSelf)
                {
                    AbsPooledObjects[i].GetComponent<Button>().onClick.RemoveAllListeners();
                    AbsPooledObjects[i].SetActive(false);
                }
            }
        }

        public bool HasFreeSlot()
        {
            return AbsPooledObjects.Count > usedObjects.Count;
        }

    }
}