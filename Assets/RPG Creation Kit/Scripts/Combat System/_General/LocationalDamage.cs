using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Settings for the locational damage.
    /// </summary>
    [System.Serializable]
    public class LocationalDamage : MonoBehaviour
    {
        public float HEAD_MULTIPLIER = 1.5f;
        public float UPPERCHEST_MULTIPLIER = 1.0f;
        public float CHEST_MULTIPLIER = 1.0f;
        public float ARMS_MULTIPLIER = .9f;
        public float HANDS_MULTIPLIER = .85f;
        public float LEGS_MULTIPLIER = .9f;
        public float FEET_MULTIPLIER = .85f;

        /// <summary>
        /// Given a base damage and the hit body part, returns the locational damage.
        /// </summary>
        /// <param name="bodyPart"></param>
        /// <param name="_baseDamage"></param>
        /// <returns></returns>
        public float CalculateLocationalDamage(BodyPart bodyPart, float _baseDamage)
        {
            if (bodyPart != null)
            {
                switch (bodyPart.thisBodyPart)
                {
                    case BodyPart.BodyParts.Head:       return (_baseDamage * HEAD_MULTIPLIER);
                    case BodyPart.BodyParts.Upperchest: return (_baseDamage * UPPERCHEST_MULTIPLIER);
                    case BodyPart.BodyParts.Chest:      return (_baseDamage * CHEST_MULTIPLIER);
                    case BodyPart.BodyParts.Arm:        return (_baseDamage * ARMS_MULTIPLIER);
                    case BodyPart.BodyParts.Hand:       return (_baseDamage * HANDS_MULTIPLIER);
                    case BodyPart.BodyParts.Foot:       return (_baseDamage * FEET_MULTIPLIER);

                    default:
                        return _baseDamage;
                }
            }
            else
                return _baseDamage;
        }
    }
}