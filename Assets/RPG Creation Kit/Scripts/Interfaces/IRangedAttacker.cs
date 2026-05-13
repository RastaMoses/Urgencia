using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Entities that can attack
    /// </summary>
    interface IRangedAttacker
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
        void RangedAttackCheck();

        /// <summary>
        /// Performs the melee attack
        /// </summary>
        void RangedAttackEvent();

        void BlockEvent();

        void OnEnterCombat();

        void UseRanged();
    }
}