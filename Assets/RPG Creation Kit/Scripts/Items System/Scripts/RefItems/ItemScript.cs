using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class ItemScript : MonoBehaviour
    {
        public virtual void OnAdd(Inventory inventory) { }
        public virtual void OnRemove(Inventory inventory) { }
        public virtual void OnEquip(Inventory inventory) { }
        public virtual void OnUnequip(Inventory inventory) { }
        public virtual void OnDepositInContainer(LootingPoint container) { }
        public virtual void OnTakeFromContainer(LootingPoint container) { }
        public virtual void OnUseConsumable(EntityAttributes entity) { }

        public virtual void OnReadBook(BookItem book) { }
    }
}