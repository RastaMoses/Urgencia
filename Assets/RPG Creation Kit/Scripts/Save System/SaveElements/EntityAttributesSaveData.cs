using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RPGCreationKit.SaveSystem
{
    [Serializable]
    public class EntityAttributesSaveData
    {
        public BaseAttributes attributes;
        public EntityDerivedAttributes derivedAttributes;
        public List<EffectOnEntity> activeEffects = new List<EffectOnEntity>();

        public uint curLvl;
        public ulong curXP;
        public ulong nextLvlXP;
        public uint curAttributePoints;

        public EntityAttributesSaveData(BaseAttributes attributes, EntityDerivedAttributes derivedAttributes, List<EffectOnEntity> activeEffects, uint cLvl, ulong cXp, ulong nxtLvlXp, uint cAttrPoints)
        {
            this.attributes = attributes;
            this.derivedAttributes = derivedAttributes;
            this.activeEffects = activeEffects;

            this.curLvl = cLvl;
            this.curXP = cXp;
            this.nextLvlXP = nxtLvlXp;
            this.curAttributePoints = cAttrPoints;
        }
    }
}