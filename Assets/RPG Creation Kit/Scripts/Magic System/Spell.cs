using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// The types of spells that exist
    /// </summary>
    public enum SpellTypes
    {
        Alteration = 0,     // Alters an attribute
        Restoration,    // Restores an attribute
        Destruction     // Deals damage
    }

    public enum SpellModes
    {
        Self = 0,       // Effect on the caster
        Touch,          // Effect on who is near the caster to the touch
        Projectile      // Effect on who gets hit by the instantiated projectile
    }

    public enum SpellTag
    {
        None = 0,
        RestoreHealth,
        DamageProjectile,
        DamageTouch
    }

    /// <summary>
    /// The base information of the spell.
    /// </summary>
    [CreateAssetMenu(fileName = "[SPELL] New Spell", menuName = "RPG Creation Kit/Spells/New Spell", order = 1)]
    public class Spell : ScriptableObject
    {
        public bool _IS_DEMO_FILE = true;

        public string spellID;
        public string spellName;
        public Sprite spellIcon;

        public SpellTypes type;
        public SpellModes mode;
        public SpellTag tag;

        public float manaCost;
        public float magnitude;
        public float area;
        public float speed;
        public float aggroModifier;

        public GameObject magicProjectile;

        public EffectOnEntity[] onCasterEffects;
        public EffectOnEntity[] onTargetEffects;

        public bool usesTooltip = false;
        [TextArea] public string tooltipValue = "";

        // Script to execute on cast
        public string itemScript;

        // Audio
        public AudioClip sOnEquip;
        public AudioClip sOnCast;
    }
}