using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Allows to have a Transform-like data without GameObjects
    /// </summary>
    [System.Serializable]
    public class RCKTransform
    {
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;

        public RCKTransform(Vector3 _pos, Quaternion _rot)
        {
            position = _pos;
            rotation = _rot;
        }

        public static RCKTransform Zero()
        {
            return new RCKTransform(Vector3.zero, Quaternion.identity);
        }

        public static bool IsChildOfRecursive(Transform origin, Transform child, bool debug = false)
        {
            if(debug)
                Debug.Log(origin.name + " | " + child.name);


            if (origin.name == child.name)
                return true;

            if (child.parent == null)
                return false;
            else
                return IsChildOfRecursive(origin, child.parent, debug);
        }

        public static bool IsChildOfPlayer(Transform t)
        {
            Transform parent = t.parent;

            while (parent != null)
            {
                if (parent.CompareTag("Player"))
                    return true;
                else
                    parent = parent.parent;
            }

            return false;
        }

        public static Vector2 xz(Vector3 vv)
        {
            return new Vector2(vv.x, vv.z);
        }

        public static float HorizontalDistance(Vector3 from, Vector3 unto)
        {
            Vector2 a = xz(from);
            Vector2 b = xz(unto);
            return Vector2.Distance(a, b);
        }
    }
}