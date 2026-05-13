using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;



namespace RPGCreationKit
{

    /// <summary>
    /// Manage Paths for NPCs and draw Gizmos for them
    /// </summary>
    [ExecuteInEditMode]
    public class NPC_Path : MonoBehaviour
    {

        public Transform[] points = new Transform[0];

        [Space(5)]

        public int StartsFromIndex = 1;

        bool error = false;
        bool warned = false;

        public void OnDrawGizmosSelected()
        {

            if (points.Length > 0)
            {

                // Check for validity of array
                for (int i = 0; i < points.Length; i++)
                {
                    if (points[i] == null)
                    {
                        error = true;
                        break;
                    }
                    else
                        error = false;
                }

                if (!error)
                {
                    for (int i = 0; i < points.Length; i++)
                    {
                        // First path
                        if (i == 0 && points.Length > 1)
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(points[i].position, points[i + 1].position);
                            Gizmos.color = Color.yellow;
                        }
                        // Second, third, etc
                        else if (i + 1 < points.Length)
                        {
                            Gizmos.DrawLine(points[i].position, points[i + 1].position);
                        }
                        // Last line
                        else
                        {
                            Gizmos.DrawLine(points[i].position, points[0].position);
                        }

                    }

                    Gizmos.color = Color.white;
                    for (int i = 0; i < points.Length; i++)
                    {
                        if (i == 0)
                            Gizmos.DrawSphere(points[i].position, .5f);
                        else
                            Gizmos.DrawSphere(points[i].position, .3f);
                    }
                }
                else if (!warned)
                {
                    Debug.LogWarning("The NPC_Path '" + gameObject.name + "' contains an unassigned point.");
                    warned = true;
                }
            }

        }
    }
}