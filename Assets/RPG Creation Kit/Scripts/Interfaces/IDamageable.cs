using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Contains information about the damage received/gaved
    /// </summary>
    public class DamageContext
    {
        public float amount;
        public Entity sender;
        public bool wasBlocked;

        public WeaponItem weaponItem;
        public AmmoItem ammoItem;
        public Spell spell;

        public DamageContext(float _amount, Entity _sender, WeaponItem _weaponItem, AmmoItem _ammoItem, bool _wasBlocked, Spell _spell = null)
        {
            amount = _amount;
            sender = _sender;
            weaponItem = _weaponItem;
            ammoItem = _ammoItem;
            wasBlocked = _wasBlocked;
            spell = _spell;
        }

        // Used to add further damage, like from stats calculation (strength for melee, intelligence for magic)
        public void AddToDamage(float _addedAmount)
        {
            amount += _addedAmount;
        }

        public void MultiplyDamage(float _multiplier)
        {
            amount *= _multiplier;
        }
    }

    /// <summary>
    /// Used on objects that has EntityAttributes on them an therefore can be damaged.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Damages the Entity if a DamageContext is provided. 
        /// </summary>
        /// <param name="damageContext"></param>
        void Damage(DamageContext damageContext);

        void DamageBlocked(DamageContext damageContext);

        /// <summary>
        /// Generates particle for the damaged object
        /// </summary>
        void GenerateBlood();

        /// <summary>
        /// Makes the Entity die.
        /// </summary>
        void Die();

        /// <summary>
        /// Returns true if the Entity is hostile.
        /// </summary>
        /// <returns></returns>
        bool IsHostile();

        /// <summary>
        /// Returns true if the Entity is hostile against the Player Character.
        /// </summary>
        /// <returns></returns>
        bool IsHostileAgainstPC();

        /// <summary>
        /// Returns true if the the Healthbar on the PlayerUI should be used when this entity is hostile.
        /// </summary>
        /// <returns></returns>
        bool ShouldDisplayHealthIfHostile();

        /// <summary>
        /// Returns true if the target is alive.
        /// </summary>
        bool IsAlive();

        /// <summary>
        /// Returns true if the target is unconscious.
        /// </summary>
        /// <returns></returns>
        bool IsUnconscious();

        /// <summary>
        /// Gets the max HP value of the Entity
        /// </summary>
        /// <returns></returns>
        float GetMaxHP();

        /// <summary>
        /// Gets the current HP value of the Entity
        /// </summary>
        /// <returns></returns>
        float GetCurrentHP();

        /// <summary>
        /// Gets the name of the Entity
        /// </summary>
        /// <returns></returns>
        string GetEntityName();

        /// <summary>
        /// Gets the name of the Entity
        /// </summary>
        /// <returns></returns>
        string GetEntityID();

        Entity GetEntity();

        bool Bleeds();

        void InstantiateBlood(Vector3 pos);

        bool IsRckAI();
    }
}