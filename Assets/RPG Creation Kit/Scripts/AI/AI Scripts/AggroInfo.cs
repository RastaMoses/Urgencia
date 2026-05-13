using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Determines in terms of % what are the biggest factors to calculate aggro.
    /// </summary>
    public static class AggroSettings
    {
        public enum Modifier
        {
            Distance = 0,
            Damage = 1,
            Weapon = 2
        }

        // All values must sum up to 100
        public static int DISTANCE_MODIFIER = 70;
        public static int DAMAGE_MODIFIER   = 20;
        public static int WEAPON_MODIFIER   = 10;

        // Other
        public static float COMBAT_PULSE_RATE = 2;
        public static float COMBAT_PULSE_REDUCTION = 20;

        public static int SortByAggro(AggroInfo a1, AggroInfo a2)
        {
            return a1.aggroValue.CompareTo(a2.aggroValue);
        }
    }

    /// <summary>
    /// Class that contains information about the aggro of an enemy/target in the RPG Creation Kit.
    /// </summary>
    [System.Serializable]
    public class AggroInfo 
    {
        public AggroInfo(float _initialValue)
        {
            SetAggroValue(_initialValue);
        }

        // The aggro value, which will determine which is the most dangerous target and the one to attack first.
        public float aggroValue = 50;

        /// <summary>
        /// Directly sets the aggro value without any check [Range: 0-100] and returns it.
        /// </summary>
        public float SetAggroValue(float _value)
        {
            aggroValue = _value;
            return (aggroValue = Mathf.Clamp(aggroValue, 0, 100));
        }

        /// <summary>
        /// Alters and returns the aggro value by an event-type call. When this is called the aggro value sums up to the current aggro value the formula: DISTANCE: (MODIFIER_VALUE / _value) | OTHERS: ((MODIFIER_VALUE * _value) / 100)
        /// </summary>
        /// <param name="_modifier"></param>
        /// <returns></returns>
        public float AlterAggroValue(AggroSettings.Modifier _modifier, float _value)
        {
            switch (_modifier)
            {
                case AggroSettings.Modifier.Distance:
                    // If distance is less or equal than 4, pretend it's 1 so we get an high aggro influence since we're very close to the target
                    if (_value <= 4)
                        _value = 1;

                    aggroValue += (AggroSettings.DISTANCE_MODIFIER / _value);
                    break;

                case AggroSettings.Modifier.Damage:
                    aggroValue += (AggroSettings.DAMAGE_MODIFIER * _value) / 100;
                    break;

                case AggroSettings.Modifier.Weapon:
                    aggroValue += (AggroSettings.WEAPON_MODIFIER * _value) / 100;
                    break;

                default:
                    break;
            }

          return (aggroValue = Mathf.Clamp(aggroValue, 0, 100));
        }

        /// <summary>
        /// Reduces the current aggro value of AggroSettings.COMBAT_PULSE_REDUCTION and returns it.
        /// </summary>
        /// <returns></returns>
        public float CombatPulse()
        {
            aggroValue -= AggroSettings.COMBAT_PULSE_REDUCTION;
            return (aggroValue = Mathf.Clamp(aggroValue, 0, 100));
        }
    }
}