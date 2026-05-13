using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "RPG Creation Kit/Items/ConsumableItem", order = 1)]
    public class ConsumableItem : Item
    {
        public override ItemTypes itemType { get { return ItemTypes.ConsumableItem; } }

        public EffectOnEntity[] effects;

        public bool playSound = true;

        public bool isConsumable = true;

        public void Use(EntityAttributes entity)
        {
            entity.ExecuteEffects(effects);

            // Call OnUse of ItemScript
            if (!string.IsNullOrEmpty(base.itemScript))
            {
                ItemScript iScript = (ItemScript)QuestScriptManager.instance.scriptsHolder.AddComponent(System.Type.GetType(base.itemScript));
                iScript.OnUseConsumable(entity);
                Destroy(iScript);
            }
        }
    }
}