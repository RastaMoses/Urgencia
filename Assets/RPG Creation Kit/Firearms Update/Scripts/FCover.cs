using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit
{
    [System.Serializable]
    public class CoverPoint
    {
        [HideInInspector] public int id;
        public Transform t;
        public bool crouched;
        public bool isInUse;
        public Entity entityUsing;

        public CoverPoint(Transform _t, bool _crouched, bool _inUse, Entity _entityUsing)
        {
            t = _t;
            crouched = _crouched;
            isInUse = _inUse;
            entityUsing = _entityUsing;
        }

        public void FreeCoverPoint()
        {
            isInUse = false;
            entityUsing = null;
        }
    }
    public class FCover : MonoBehaviour
    {
        public List<CoverPoint> points = new List<CoverPoint>();

        private void Start()
        {
            for(int i = 0; i < points.Count; i++)
                points[i].id = i;
        }


        public bool HasSpotAvailable()
        {
            for (int i = 0; i < points.Count; i++)
                if (!points[i].isInUse)
                    return true;

            return false;
        }

        public CoverPoint AssignSpotToAI(Entity ai)
        {
            for (int i = 0; i < points.Count; i++)
                if (!points[i].isInUse)
                {
                    points[i].isInUse = true;
                    points[i].entityUsing = ai;
                    return points[i];
                }

             return null;
        }

        public void FreeSpotFromAI(CoverPoint _point)
        {
            _point.FreeCoverPoint();
        }

#if UNITY_EDITOR

        public void OnDrawGizmos()
        {
            var color = Gizmos.color;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].t == null)
                    continue;

                // Draw a yellow sphere at the transform's position
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(points[i].t.position, .25f);
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                UnityEditor.Handles.Label(points[i].t.position, "CoverPoint", style);
            }
            Gizmos.color = color;
        }

#endif
    }
}