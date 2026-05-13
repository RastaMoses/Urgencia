using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Entities that can attack
    /// </summary>
    public interface IMeleeAttacker
    {
        /// <summary>
        /// Draws the Weapon
        /// </summary>
        void DrawWeapon();

        /// <summary>
        /// Undraws the weapon
        /// </summary>
        void UndrawWeapon();

        /// <summary>
        /// Method that runs when the entity has to perfrom a melee attack, usually used to check distance.
        /// </summary>
        void MeleeAttackCheck();

        /// <summary>
        /// Performs the melee attack
        /// </summary>
        void MeleeAttackEvent();

        void MeleeChargedAttack();

        void BlockEvent();

        void OnEnterCombat();

        void UseMelee();

        /// <summary>
        /// Called when this entity attacks and its attack gets blocked.
        /// </summary>
        void OnAttackIsBlocked();
    }
}