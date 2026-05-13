using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit
{

    // Type of target, used to choose from where to retrieve the target upon loading
    public enum ITargetableType
    {
        Entity = 0,
        Door = 1,
        PRef = 2,
        Transform = 3,
        AIPath = 4
    };

    /// <summary>
    /// Used on objects that can be hit, knocked out with physics and that generates decal (for blood use IDamageable.GenerateBlood).
    /// </summary>
    public interface ITargetable
    {
        string GetID();
        ITargetableType GetTargetableType();

        /// <summary>
        /// This string is used to get extra data from targetables and varies from Targetable to Targetable.
        /// Doors Targetables for example uses this GetExtraData to return the cell which the door brings to, 
        /// in order to look it up and assign it once the PurposeState gets loaded from a savegame.
        /// </summary>
        /// <returns></returns>
        string GetExtraData();
    }
}
