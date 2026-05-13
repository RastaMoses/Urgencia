using RPGCreationKit;
using RPGCreationKit.BehaviourTree.Data;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RPGCreationKit.AI
{
    /// <summary>
    /// Class that defines something that can be seen by an AI.
    /// </summary>
    [System.Serializable]
    public class VisibleTarget
    {
        public Transform tr;
        public float distance;

        public VisibleTarget(Transform _transform, float _distance)
        {
            tr = _transform;
            distance = _distance;
        }

        public static int SortByDistance(VisibleTarget fVT, VisibleTarget sVT)
        {
            return fVT.distance.CompareTo(sVT.distance);
        }
    }

    /// <summary>
    /// Defines a visible enemy that the AI will attack.
    /// </summary>
    [System.Serializable]
    public class VisibleEnemy
    {
        public VisibleEnemy(Entity _entity, EntityAttributes _attributes, AggroInfo _aggro)
        {
            m_entity = _entity;
            m_entityAttributes = _attributes;
            m_aggro = _aggro;
        }

        public Entity m_entity;
        public EntityAttributes m_entityAttributes;
        public AggroInfo m_aggro;
    }

    [System.Serializable]
    public class VisibleNPCActionPoint
    {
        public NPCActionPoint actionPoint;
        public float distance;

        public VisibleNPCActionPoint(GameObject _gameObject, float _distance)
        {
            actionPoint = _gameObject.GetComponent<NPCActionPoint>();
            distance = _distance;
        }

        public VisibleNPCActionPoint(NPCActionPoint _actionPoint, float _distance)
        {
            actionPoint = _actionPoint;
            distance = _distance;
        }

        public static int SortByDistance(VisibleTarget fAP, VisibleTarget sAP)
        {
            return fAP.distance.CompareTo(sAP.distance);
        }
    }


    [System.Serializable]
    public class VisibleDoor
    {
        public Door door;
        public float distance;

        public VisibleDoor(Door door, float distance)
        {
            this.door = door;
            this.distance = distance;
        }
    }


    /// <summary>
    /// Allows the AI to see and understand the sorroundings.
    /// </summary>
    public class AIPerception : AIInventoryEquipment
    {
        [SerializeField] public bool perceptionEnabled = true;
        public bool PerceptionEnabled
        {
            get { return perceptionEnabled; }
            set
            {
                perceptionEnabled = value;

                if (value)
                    OnEnable();
            }
        }
        public Transform headPos;

        [SerializeField] float viewAngle = 360;
        [SerializeField] float lookAtDistance = 3.5f;

        //aiLookAt is defined in AITalkative

        public LayerMask targetMask;
        public LayerMask obstacleMask;

        // Define what the AI sees
        public List<VisibleTarget> visibleTargets = new List<VisibleTarget>();
        public List<VisibleNPCActionPoint> visibleActionPoints = new List<VisibleNPCActionPoint>();
        public List<RckAI> visibleAI = new List<RckAI>();
        public List<VisibleEnemy> enemyTargets = new List<VisibleEnemy>();

        // Defines how many times a second a check for sight needs to be made
        [SerializeField] public float checkTick = 1.25f;

        // Information about the Sphere that defines the FOV of the AI
        public float radius = 3.3f;
        public float sphereYOffset = 1.25f;
        public float sphereForwardOffset = 1.63f;
        [SerializeField] private bool visualizeSphere = true;

        public AIOnlineComponents onlineComponents;



        public override void Start()
        {
            base.Start();
        }

        private void OnEnable()
        {
            if (!perceptionEnabled)
                return;

            StopAllCoroutines();

            visibleTargets.Clear();

            StartCoroutine(UpdateVision());

            if (aiLookAt != null)
                StartCoroutine(UpdateLookAt());
        }

        /// <summary>
        /// Determines what the AI can see
        /// </summary>
        /// <returns></returns>
        IEnumerator UpdateVision()
        {
            while (perceptionEnabled)
            {
                if (!perceptionEnabled)
                    yield return null;

                visibleTargets.Clear();
                visibleActionPoints.Clear();
                visibleAI.Clear();

                if (!isAlive || isUnconscious || isInConversation || isInOfflineMode)
                    yield return null;

                Collider[] hitColliders = Physics.OverlapSphere(transform.position + (Vector3.up * sphereYOffset) + (transform.forward * sphereForwardOffset), radius, targetMask);

                for (int i = 0; i < hitColliders.Length; i++)
                {
                    Transform target = hitColliders[i].gameObject.transform;

                    Vector3 dirToTarget = (target.position - transform.position).normalized;
                    if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
                    {
                        float dstToTarget = Vector3.Distance(transform.position, target.position);

                        if (!Physics.Raycast(headPos.position, dirToTarget, dstToTarget, obstacleMask) && !RCKTransform.IsChildOfRecursive(this.transform, target))
                        {
                            if (target.CompareTag("Player"))
                                target = RckPlayer.instance.mainCamera.transform.parent;

                            if (target.CompareTag("RPG Creation Kit/NPCActionPoint"))
                            {
                                visibleActionPoints.Add(new VisibleNPCActionPoint(target.gameObject, dstToTarget));
                            }
                            else if (target.CompareTag("RPG Creation Kit/AI"))
                            {
                                visibleAI.Add(target.GetComponent<RckAI>());
                            }
                            else
                            {
                                visibleTargets.Add(new VisibleTarget(target, dstToTarget));

                                if (target.name == "HeadPos")
                                {
                                    visibleAI.Add(target.GetComponentInParent<RckAI>());
                                }
                            }
                        }
                    }
                }

                if (mainTarget != null)
                {
                    if (visibleTargets.Count > 0)
                    {
                        // Check if we can see the main target
                        foreach (VisibleTarget target in visibleTargets)
                        {
                            if (RCKTransform.IsChildOfRecursive(mainTarget, target.tr, false))
                            {
                                canSeeTarget = true;
                                break;
                            }

                            canSeeTarget = false;
                        }
                    }
                    else
                        canSeeTarget = false;
                }

                OnUpdatingVisionEnds();

                yield return new WaitForSeconds(checkTick);
            }

            yield return null;
        }

        public virtual void OnUpdatingVisionEnds()
        {
            StopLookingAtPlayerWhenOutOfVision();
        }

        public void StopLookingAtPlayerWhenOutOfVision()
        {
            // Stop looking at the player if he got out of vision (useful when sneaking)

            if (aiLookAt == null || isInCombat || isInConversation || aiLookAt.forceTarget)
                return;

            var targetRoot = aiLookAt.target != null ? aiLookAt.target.root : null;
            if (targetRoot == null || !targetRoot.CompareTag("Player"))
                return;

            // Only relevant if we are currently looking at someone
            if (!aiLookAt.IsLookingAtSomeone)
                return;

            bool stillVisible = false;

            if (visibleTargets != null)
            {
                foreach (var t in visibleTargets)
                {
                    var tRoot = t?.tr != null ? t.tr.root : null;
                    if (tRoot != null && tRoot == targetRoot)
                    {
                        stillVisible = true;
                        break;
                    }
                }
            }

            if (!stillVisible)
            {
                aiLookAt.StopForcingLookAtTarget();
            }
        }

        public VisibleTarget currentLookingAt;
        /// <summary>
        /// Updates the AILookAt component in base of what the AI is perceiving 
        /// </summary>
        /// <returns></returns>
        IEnumerator UpdateLookAt()
        {
            while (perceptionEnabled)
            {
                visibleTargets.Sort(VisibleTarget.SortByDistance);
                for (int i = 0; i < visibleTargets.Count; i++)
                {
                    if (visibleTargets[i].distance <= lookAtDistance)
                    {
                        if (!aiLookAt.IsLookingAtSomeone)
                        {
                            aiLookAt.SetTarget(visibleTargets[i].tr);
                            currentLookingAt = visibleTargets[i];
                        }
                        break;
                    }
                }

                if (currentLookingAt != null && currentLookingAt.tr != null && Vector3.Distance(transform.position, currentLookingAt.tr.position) > lookAtDistance)
                {
                    aiLookAt.ClearTarget();
                    currentLookingAt = null;
                }

                yield return new WaitForSeconds(checkTick);
            }
        }

        [AIInvokable]
        public void DisableAILookAt()
        {
            aiLookAt.isEnabled = false;
        }

        [AIInvokable]
        public void EnableAILookAt()
        {
            aiLookAt.isEnabled = true;
        }

        public override void Die()
        {
            base.Die();
            PerceptionEnabled = false;
        }

        [AIInvokable]
        public void GetDistanceFromPlayerBT(BTVariable _f)
        {
            _f.SetValue(Vector3.Distance(this.transform.position, RckPlayer.instance.transform.position));
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (!enableDebugInEditor)
                return;

            if (visualizeSphere)
                UnityEngine.Gizmos.DrawWireSphere(transform.position + (Vector3.up * sphereYOffset) + (transform.forward * sphereForwardOffset), radius);

            Color previousColor = Handles.color;
            foreach (VisibleTarget visibleTarget in visibleTargets)
            {
                if (visibleTarget != null && visibleTarget.tr != null && headPos != null)
                {
                    Handles.color = Color.red;
                    Handles.DrawLine(headPos.position, visibleTarget.tr.position, 1f);
                }
            }
            Handles.color = previousColor;
        }

#endif

    }
}