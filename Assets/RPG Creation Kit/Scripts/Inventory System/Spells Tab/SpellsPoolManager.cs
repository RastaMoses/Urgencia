using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEngine.UI;

namespace RPGCreationKit
{
    public class SpellsPoolManager : MonoBehaviour
    {
        public List<SpellInUI> InUseObjects = new List<SpellInUI>();
        [SerializeField] public ItemsInListPool RestorationPool;
        [SerializeField] public ItemsInListPool DestructionPool;
        [SerializeField] public ItemsInListPool AlterationPool;

        public void OnEnable()
        {
            PopulateSpells();
        }

        public void OnDisable()
        {
            ResetPools();
        }

        public void ResetPools()
        {
            // Restore all pools
            RestorationPool.ResetPool();
            DestructionPool.ResetPool();
            AlterationPool.ResetPool();
        }

        public void PopulateSpells()
        {
            InUseObjects.Clear();
            for(int i = 0; i < SpellsKnowledge.Player.Spells.Count; i++)
            {
                Spell current = SpellsKnowledge.Player.Spells[i];
                SpellInUI pooledObject = null;
                switch (current.type)
                {
                    case SpellTypes.Restoration:
                        // Check if pool has one available item
                        if (!RestorationPool.HasFreeSlot())
                            RestorationPool.AddInPool(1);

                        pooledObject = RestorationPool.AbsPooledObjects[RestorationPool.usedObjects.Count].GetComponent<SpellInUI>();
                        pooledObject.Init(current);

                        RestorationPool.usedObjects.Add(pooledObject);
                        break;

                    case SpellTypes.Destruction:
                        // Check if pool has one available item
                        if (!DestructionPool.HasFreeSlot())
                            DestructionPool.AddInPool(1);

                        pooledObject = DestructionPool.AbsPooledObjects[DestructionPool.usedObjects.Count].GetComponent<SpellInUI>();
                        pooledObject.Init(current);

                        DestructionPool.usedObjects.Add(pooledObject);
                        break;

                    case SpellTypes.Alteration:
                        // Check if pool has one available item
                        if (!AlterationPool.HasFreeSlot())
                            AlterationPool.AddInPool(1);

                        pooledObject = AlterationPool.AbsPooledObjects[AlterationPool.usedObjects.Count].GetComponent<SpellInUI>();
                        pooledObject.Init(current);

                        AlterationPool.usedObjects.Add(pooledObject);
                        break;
                }

                pooledObject.GetComponent<Button>().onClick.AddListener(() => pooledObject.OnClick());
                pooledObject.poolManager = this;
                pooledObject.gameObject.SetActive(true);
                InUseObjects.Add(pooledObject);
            }
        }

        public void UpdateAllSpellsUI()
        {
            for(int i = 0; i < InUseObjects.Count; i++)
            {
                if (InUseObjects[i] != null)
                    InUseObjects[i].UpdateItem();
            }
        }
    }
}
