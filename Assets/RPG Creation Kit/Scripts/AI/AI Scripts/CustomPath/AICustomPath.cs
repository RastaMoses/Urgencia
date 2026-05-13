using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.AI
{

    public enum AICustomPathType
    {
        OneWay = 0,
        Looped = 1,
        BackAndForth = 2,
        BackAndForthLooped = 3,
    }

    /// <summary>
    /// Manage Paths for NPCs and draw Gizmos for them
    /// </summary>
    [ExecuteInEditMode]
    public class AICustomPath : MonoBehaviour, ITargetable
    {
        public string ID;
        public AICustomPathType type;

        public AIPathPoint[] points;

        public int startsFromIndex = 0;

        [SerializeField] bool debugPath = true;

        bool error = false;
        bool warned = false;

        public void OnDrawGizmos()
        {
            if (!debugPath)
                return;

            if (points != null && points.Length > 0)
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
                        Gizmos.color = Color.yellow;

                        switch (type)
                        {
                            case AICustomPathType.OneWay:
                                // First path
                                if (i == 0 && points.Length > 1)
                                {
                                    Gizmos.color = Color.red;
                                    Gizmos.DrawLine(points[i].transform.position, points[i + 1].transform.position);
                                    Gizmos.color = Color.yellow;
                                }
                                // Second, third, etc
                                else if (i + 1 < points.Length)
                                {
                                    Gizmos.DrawLine(points[i].transform.position, points[i + 1].transform.position);
                                }
                                break;

                            case AICustomPathType.Looped:
                                // First path
                                if (i == 0 && points.Length > 1)
                                {
                                    Gizmos.color = Color.red;
                                    Gizmos.DrawLine(points[i].transform.position, points[i + 1].transform.position);
                                    Gizmos.color = Color.yellow;
                                }
                                // Second, third, etc
                                else if (i + 1 < points.Length)
                                {
                                    Gizmos.DrawLine(points[i].transform.position, points[i + 1].transform.position);
                                }
                                // Last line
                                else
                                {
                                    Gizmos.DrawLine(points[i].transform.position, points[0].transform.position);
                                }
                                break;

                            case AICustomPathType.BackAndForth:
                            case AICustomPathType.BackAndForthLooped:
                                // First path
                                if (i == 0 && points.Length > 1 || i == points.Length -1)
                                {
                                    int temp = i;
                                    if (i == points.Length - 1)
                                        temp = i-1;


                                    Gizmos.color = Color.red;
                                    Gizmos.DrawLine(points[temp].transform.position, points[temp + 1].transform.position);
                                    Gizmos.color = Color.yellow;
                                }
                                // Second, third, etc
                                else if (i + 1 < points.Length)
                                {
                                    Gizmos.DrawLine(points[i].transform.position, points[i + 1].transform.position);
                                }
                                break;

                            default:
                                break;
                        }

                        Gizmos.color = Color.white;

                        if (i == 0)
                            Gizmos.DrawSphere(points[i].transform.position, .5f);
                        else
                            Gizmos.DrawSphere(points[i].transform.position, .3f);
                    }

                }
                else if (!warned)
                {
                    Debug.LogWarning("The AICustomPath '" + gameObject.name + "' contains an unassigned point.");
                    warned = true;
                }
            }

        }

        string ITargetable.GetExtraData()
        {
            return string.Empty;
        }

        string ITargetable.GetID()
        {
            return ID;
        }

        ITargetableType ITargetable.GetTargetableType()
        {
            return ITargetableType.AIPath;
        }
    }
}