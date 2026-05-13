using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Contains global variables (or calls to member variables) that can be useful through many situations.
    /// </summary>
    public static class GlobalVariables
    {
        public static int PlayerStrength
        {
            get { if (EntityAttributes.PlayerAttributes != null) return EntityAttributes.PlayerAttributes.attributes.Strength; else return 0; }
        }

        public static int PlayerDexterity
        {
            get { if (EntityAttributes.PlayerAttributes != null) return EntityAttributes.PlayerAttributes.attributes.Dexterity; else return 0; }
        }

        public static int PlayerAgility
        {
            get { if (EntityAttributes.PlayerAttributes != null) return EntityAttributes.PlayerAttributes.attributes.Agility; else return 0; }
        }

        public static int PlayerConstitution
        {
            get { if (EntityAttributes.PlayerAttributes != null) return EntityAttributes.PlayerAttributes.attributes.Constitution; else return 0; }
        }

        public static int PlayerSpeed
        {
            get { if (EntityAttributes.PlayerAttributes != null) return EntityAttributes.PlayerAttributes.attributes.Speed; else return 0; }
        }

        public static int PlayerEndurance
        {
            get { if (EntityAttributes.PlayerAttributes != null) return EntityAttributes.PlayerAttributes.attributes.Endurance; else return 0; }
        }

        public static int PlayerCharisma
        {
            get { if (EntityAttributes.PlayerAttributes != null) return EntityAttributes.PlayerAttributes.attributes.Charisma; else return 0; }
        }

        public static float PlayerCurHealth
        {
            get { if (EntityAttributes.PlayerAttributes != null) return EntityAttributes.PlayerAttributes.CurHealth; else return 0; }
        }

        public static float PlayerMaxHealth
        {
            get { if (EntityAttributes.PlayerAttributes != null) return EntityAttributes.PlayerAttributes.MaxHealth; else return 0; }
        }

        public static float PlayerCurStamina
        {
            get { if (EntityAttributes.PlayerAttributes != null) return EntityAttributes.PlayerAttributes.CurStamina; else return 0; }
        }

        public static float PlayerMaxStamina
        {
            get { if (EntityAttributes.PlayerAttributes != null) return EntityAttributes.PlayerAttributes.MaxStamina; else return 0; }
        }

        public static float PLAYER_HEARS_NPC_DIALOGUES_DISTANCE
        {
            get { return RCKSettings.PLAYER_HEARS_NPC_DIALOGUES_DISTANCE; }
        }
    }
}