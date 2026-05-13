using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    public enum ConsumableEffectType
    {
        DamageAttribute = 0, // On ConsumableEffectOnAttribute
        DamageHealth = 1, // Directly Damages health 
        DamageStamina = 2, // Directly Damages stamina 

        FortifyAttribute = 3, // On ConsumableEffectOnAttribute
        FortifyHealth = 4, // Direcyly fortifies health
        FortifyStamina = 5, // Directly fortifies stamina

        RestoreAttribute = 6, // Directly restores attribute
        RestoreHealth = 7, // Directly restore health
        RestoreStamina = 8, // Directly restore Stamina

        // New 1.3
        FortifyMana = 9,
        RestoreMana = 10,
        DamageMana = 11,
    }

    [System.Serializable]
    public class EffectOnEntity
    {
        public ConsumableEffectType effectType;
        public EntityBaseAttributes onAttribute;

        [Tooltip("The duration of the effect, set it to -1 to make it endless")]
        public float duration = 20;

        [Tooltip("The strength of the effect")]
        public float magnitude = 1;

        public float magnitudeAlreadyApplied = 0.0f;

        public Sprite effectIcon;
        public string effectIconID;

        public bool showInEffectsUI = true;
        public bool isOnDuration = false;

        public bool isFinished = false;

        public EffectOnEntity(EffectOnEntity effect)
        {
            this.effectType = effect.effectType;
            this.onAttribute = effect.onAttribute;
            this.duration = effect.duration;
            this.magnitude = effect.magnitude;
            this.magnitudeAlreadyApplied = effect.magnitudeAlreadyApplied;
            this.effectIcon = effect.effectIcon;
            this.effectIconID = effect.effectIconID;
            this.showInEffectsUI = effect.showInEffectsUI;
            this.isOnDuration = effect.isOnDuration;
            this.isFinished = effect.isFinished;
        }
    }
}