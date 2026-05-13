using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public static class RCKLayers
    {
        public static int Default = 0;
        public static int Player = 8;
        public static int FirstPerson = 9;
        public static int Projectile = 10;
        public static int BodyPart = 13;
        public static int ThirdPersonOnly = 18;
        public static int ExplosionObstacle = 19;
        public static int IgnorePlayer = 21;

        public static void SetLayer(this GameObject parent, int layer, bool includeChildren = false)
        {
            parent.layer = layer;
            if (includeChildren)
            {
                foreach (Transform trans in parent.transform.GetComponentsInChildren<Transform>(true))
                {
                    trans.gameObject.layer = layer;
                }
            }
        }
    }
}