using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit
{
    /// <summary>
    /// Used on objects that can be hit, knocked out with physics and that generates decal (for blood use IDamageable.GenerateBlood).
    /// </summary>
    public interface IDraggable
    {
        void OnBeingDragged();
        void OnDragEnds();

        GameObject GetGameObject();
    }
}