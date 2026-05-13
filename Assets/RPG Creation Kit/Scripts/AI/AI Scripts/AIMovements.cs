using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEngine.AI;
using UnityEditor;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Player;

namespace RPGCreationKit.AI
{
    /// <summary>
    /// Defines the current MovementType this AI entity is currently using.
    /// </summary>
    public enum MovementType
    {
        Null = 0,
        UnityNavmesh = 1,
        TraversingLinks = 2,
        CustomPathSystem = 3,
        InActionPoint = 4,
        Offline = 5
    };

    /// <summary>
    /// Defines the precision of Obstacle Avoidance while the AI entity is traversing a Link.
    /// The more the precision, the more the performances used.
    /// </summary>
    public enum TLinkObstacleAvoidancePrecision
    {
        Full = 0,
        Good = 1,
        Minimum = 2,
        Null = 3
    };

    public enum SteeringBehaviours
    {
        Seek = 0,
        Arrive = 1,
        Flee = 2,
        FleeAtDistance = 3,
        RigtSideways = 4,
        LeftSideways = 5
    }

    public class AIMovements : AITalkative
    {
        public bool isPersistentReference = false;
        public string inPersistentWorldspaceID = "";    // the worldspace ID this PersistentAI is in
        public string inPersistentCellID = "";          // the cell ID this persistent ai is in
        public float persistentLifePulse = 5.0f;        // the rate at which this persistent AI is updated

        public BodyData bodyData;

        //[Header("General Settings")]
        public bool usesMovements = true;       // AI makes use of movements
        public bool usesOfflineMode = true;     // Make the AI go in Offline mode instead of disabling it when the Persistent Reference is being disabled
        public bool usesTlinkUnstuck = true;    // Attempts to unstuck the AI if during TLink mode it detects itself to be stucked

        [Space(5)]

        //[Header("Components")]
        public Collider physicalCollider;       // The physical collider of the AI
        public Collider interactableCollider;   // The collider that defines the Interact Zone of the AI

        public NavMeshAgent agent;              // The agent component of this AI
        public Rigidbody m_Rigidbody;           // The Rigidbody of this AIa
        public NavMeshPath navmeshPath;         // The NavMeshPath the AI will generate and calculate at runtime while being in UnityNavmesh mode

        [Space(5)]

        //[Header("Status")]
        public MovementType movementType;       // Defines the movement type of the AI  
        public bool isWalking = true;           // Defines wheter the AI is Walking (true) or running (false)
        protected bool isTraversingLink = false;          // True if the AI is traversing a link
        bool isRotating = false;                // True if the AI is rotating
        float agentAngularSpeed;

        bool isFollowingMainTarget = false;     // True if the AI is simply following the MainTarget assigned
        public bool isFollowingAIPath = false;         // True if the AI is following an AIPath

        [Space(5)]

        public bool canSeeTarget = false;
        public bool hasCompletlyLostTarget = false; // this is set to true if the agent reached the last known position of the target and doesn't know where to go from there

        //[Header("Shoulds")]
        public bool overrideShouldFollowMainTargetActive = false;   // If this is enabled, overrideShouldFollowMainTarget will  be used instead of "shouldFollowMainTarget"
        public bool overrideShouldFollowMainTarget = false;

        public bool shouldFollowMainTarget = false;
        public bool shouldFollowOnlyIfCanSee = false;
        public bool lookAtTarget = false;

        public bool shouldFollowAIPath = false;
        public bool shouldWander = false;
        public bool shouldUseNPCActionPoint = false;

        [Space(5)]

        //[Header("Movements Components")]

        public Transform mainTarget;            // The main Target this AI is following
        public bool hasMainTarget = false;

        public Vector3 targetVector;
        public bool shouldFollowTargetVector = false;

        public AICustomPath aiPath;             // The Path this AI is following
        public NPCActionPoint actionPoint;

        public bool lookATransformIfStopped = false;    // If the AI stops, should he look at something? This can be useful if set at runtime, but can cause issues if forgotten on true
        public Transform directionToLookIfStopped;      // If the AI has to look at something if he's stopped, define that something

        public bool lookAtVector3IfStopped = false;
        public Vector3 vector3ToLookAtIfStopped;
        public bool alwaysCacheMainTargetDuringOffline = false;

        [Space(5)]

        //[Header("Movements Settings")]
        public SteeringBehaviours selectedSteeringBehaviour;
        public Deceleration arriveSteeringBehaviourDeceleration;
        public float fleeAtDistanceValue = 20f;


        public float currentSpeed = 0.0f;       // Defines the current speed of the AI
        public float maxSpeed = 0.0f;           // Defines the maxSpeed the AI can travel in the current mode (movementType, isWalking...)
            
        public bool invertAIPath = false;       // Set at true if the AI has to follow its AIPath at the inverse
        public int aiPathCurrentIndex = 0;             // Defines the Index of the AIPath the AI is currently at
        public bool aiPathResumeFromCurrentIndex = false;

        public bool isStopped = false;
        public float stoppingHalt = 0.1f;          // The smoothness applied to stopping this Entity
        public float stoppingDistance = 1.4f;   // Defines the distance at which the current target will be set as reached

        [SerializeField] public float generalRotationSpeed = 3f;   // Defines the general speed of rotation of this AI
        [SerializeField] float rotationThreshold = 5f;      // The value of difference between the oldRotPos and the current RotPos to say if the AI is rotating or not

        float walkModifier;                         // Caches .5f or 1f as 'max' values for the blendtrees of the Animator.
        private Vector3 velocity = Vector3.zero;    // Just a storage for the velocity
        Vector3 oldEulerAngles;                     // Stores the old Euler Angles to determine if the AI has/had to rotate

        public bool cachedMainTarget = false;
        public Vector3 cachedMainTargetPos;

        [Space(5)]

        //[Header("Traversing Link Specific")]
        public TLinkObstacleAvoidancePrecision tLinkObstacleAvoidancePrecision = TLinkObstacleAvoidancePrecision.Good;

        public bool chaseTargetDuringTraversing = true;         // If set to true, while in TLink mode the AI will keep following its target
        public bool rotateToFaceTargetDuringTraversing = true;  // If set to true, while in TLink mode the AI will lookAt its target
        public bool hasToRandomlySearchTarget = false;

        [Space(5)]

        //[Header("Traversing Link Specific - Obstacle Avoidance")]

        // Obstacle avoidance in TraversingLinks mode
        public LayerMask obstacleAvoidanceMask;     // Defines the Objects that are an Obstacles for the AI
        float obstacleDistance = 4f;                // Defines how far is too close for an obstacle
        float leftRightOpeningMult = 1.5f;          // Defines the opening of the raycasts checks that will be performed in order to attempt to detect the most cheap way to get around the obstacle
        float rotationDivider = 35;                 // Defines how fast the AI will rotate if he's facing an obstacle (High value -> Less speed)
        float lerpingToOriginal = .1f;              // Defines how fast the AI will return to it's original trajectory after it was affected by ObstacleAvoidance

        // Raycasthit for obstacle avoidance
        RaycastHit feetLeftHit;
        RaycastHit feetForwardHit;
        RaycastHit feetRightHit;

        RaycastHit bodyLeftHit;
        RaycastHit bodyForwardHit;
        RaycastHit bodyRightHit;

        RaycastHit headLeftHit;
        RaycastHit headForwardHit;
        RaycastHit headRightHit;

        // Information about obstacle avoidance
        bool leftOccupied = false;
        bool forwardOccupied = false;
        bool rightOccupied = false;
        GameObject leftHit;
        GameObject rightHit;

        [Space(5)]

        [Header("Traversing Link Specific - Unstuck")]

        // Variables that manages the unstuck system during TLinkMode
        float tLinkStuckCounter = 0.0f;                                 // Defines the time this AI was seen as Stuck
        float tLinkStuckTime = 3f;                                      // Defines the max time this AI can be seen as Stuck before attempting to unstuck it
        float tLinkUnstuckForce = 25f;                                  // Defines the forece to apply to the AI in order to attempt the unstuck
        Vector3 tLinkStuckThreshold = new Vector3(0.25f, 0.25f, 0.25f); // Defines the values in which the AI will be seen as stuck

        public string startingCellID;
        public string runtimeStartingCell;
        public string cellIDOfLastSaved;
        bool getcellinfo = true;
        public bool isPerformingRandomPlay = false; // set by the BTree
        /// Should the NPC check if he's changing cells? Usually yes if it's loaded in an exterior, no if he's unloaded or inside an interior.
        /// 

        public float goToCoverCooldown = -1;

        // Currently used only for cover
        public bool isCrouched = false;

        public bool navmeshPathFinished = false;
        public bool dialogueLookAt = false;
        public bool fixedFollowNavmeshpathRot;

        // When set true, the AI won't be able to move during an attack animation
        protected bool isInAttackMovementLock = false;
        protected bool isInAttackRotationLock = false;

        public bool GetCellInfo
        {
            get
            {
                return getcellinfo;
            }
            set
            {
                getcellinfo = value;

                if (getcellinfo)
                    StartCoroutine(GetMyCellInfo());
                else
                    StopCoroutine(GetMyCellInfo());
            }
        }

        float getCellInfoTimer = 5.0f;

        public float calculatePathTimex = 0.25f;
        public float calculatePathTimer = 0.0f;

        /// <summary>
        /// When the AI walks he could encounter a door, if he's in front of a door, this reference will be set.
        /// </summary>
        Door doorInTheWay;

        [Space(5)]

        [Header("Debug")]
        public bool generalDebug = true;
        public bool debugPath = true;

        // Used to make the NPC get away from nearby expolisve, if this reference is not null, there's an explosive nearby him
        public GameObject nearbyExplosiveObj;

        public bool nearbyExplosive = false;
        public void SetNearbyExplosive(GameObject _explosive)
        {
            nearbyExplosiveObj = _explosive;
        }

        [AIInvokable]
        public void CheckNearbyExplosive()
        {
            nearbyExplosive = nearbyExplosiveObj == null ? false : true;
        }

        public virtual void Start()
        {
            if(string.IsNullOrEmpty(runtimeStartingCell))
                if(myCellInfo != null)
                runtimeStartingCell = myCellInfo.cell.ID;

            isInAttackMovementLock = false;
            isInAttackRotationLock = false;

            navmeshPath = new NavMeshPath();
            AdjustCurrentSpeed();
            agent.autoTraverseOffMeshLink = false;
            oldEulerAngles = transform.rotation.eulerAngles;

            agentAngularSpeed = agent.angularSpeed;

            if(agent.isOnNavMesh)
                agent.CalculatePath(agent.transform.position + (transform.forward * 0.1f), navmeshPath);

            GetCellInfo = true;
        }

        /// <summary>
        /// Wait for scene to be loaded and get cellinfo
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetMyCellInfo()
        {
            while (GetCellInfo)
            {
                yield return new WaitForSecondsRealtime(.5f);
                yield return new WaitForEndOfFrame();

                while (WorldManager.instance.isLoading || GameStatus.instance.AnyLoading())
                    yield return null;

                if (WorldManager.instance.currentWorldspace.worldSpaceType == Worldspace.WorldSpaceType.Exterior)
                {
                    Cell checkedCenterCell;
                    float shortestDistance = 99999.999f;
                    int shortestDistanceID = 0;
                    for (int i = 0; i < WorldManager.instance.currentWorldspace.cells.Length; i++)
                    {
                        float curDistance = Vector3.Distance(agent.transform.position, WorldManager.instance.currentWorldspace.cells[i].cellInWorldCoordinates);
                        if (curDistance < shortestDistance)
                        {
                            shortestDistance = curDistance;
                            shortestDistanceID = i;
                        }
                    }

                    checkedCenterCell = WorldManager.instance.currentWorldspace.cells[shortestDistanceID];

                    try
                    {
                        myCellInfo = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(checkedCenterCell.sceneRef.ScenePath).GetCellInformation();
                    }
                    catch
                    {

                    }
                }
                else
                {
                    myCellInfo = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(WorldManager.instance.currentCenterCell.sceneRef.ScenePath).GetCellInformation();
                }

                while (WorldManager.instance.isLoading || GameStatus.instance.AnyLoading())
                    yield return null;

                yield return new WaitForSecondsRealtime(getCellInfoTimer);
                yield return null;
            }
        }

        [ContextMenu("GetMyCellInfoImmediate()")]
        public void GetMyCellInfoImmediate()
        {
            if(WorldManager.instance.currentWorldspace.worldSpaceType == Worldspace.WorldSpaceType.Exterior)
            {
                Cell checkedCenterCell;
                float shortestDistance = 99999.999f;
                int shortestDistanceID = 0;
                for (int i = 0; i < WorldManager.instance.currentWorldspace.cells.Length; i++)
                {
                    float curDistance = Vector3.Distance(agent.transform.position, WorldManager.instance.currentWorldspace.cells[i].cellInWorldCoordinates);
                    if (curDistance < shortestDistance)
                    {
                        shortestDistance = curDistance;
                        shortestDistanceID = i;
                    }
                }

                checkedCenterCell = WorldManager.instance.currentWorldspace.cells[shortestDistanceID];

                myCellInfo = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(checkedCenterCell.sceneRef.ScenePath).GetCellInformation();
            }
                else
            {
                try
                {
                    myCellInfo = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(WorldManager.instance.currentCenterCell.sceneRef.ScenePath).GetCellInformation();
                    GetCellInfo = false;
                }
                catch
                {
                    Debug.Log("CATCHED smthin");
                }
            }
        }

        public bool isRotatingBecauseOfDialogue = false;
        public virtual void Update()
        {
            if (!usesMovements)
                return;

            if (!isAlive || isUnconscious)
                return;

            calculatePathTimer += 1 * Time.deltaTime;

            // Override states
            if (overrideShouldFollowMainTargetActive)
                shouldFollowMainTarget = overrideShouldFollowMainTarget;

            CheckRotation();
            HandleAnimations();
            CheckForStuckStatus_TLink();

            hasMainTarget = (mainTarget != null) ? true : false;

            // Check rotations
            if (dialogueLookAt)
                shouldLookAtTalker = true;
            else
                shouldLookAtTalker = false;

            if (isInConversation && (shouldLookAtTalker) && speakingTo != null && !isUsingActionPoint && !fixingInPlaceForActionPoint)
            {
                var lookPos = speakingTo.GetEntityGameObject().transform.position - transform.position;
                lookPos.y = 0;
                var lookAt = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAt, 100 * Time.deltaTime);
                isRotatingBecauseOfDialogue = true;
            }
            else
                isRotatingBecauseOfDialogue = false;

            if (transformMovingToActionPoint && actionPoint != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, actionPoint.pivotPoint.position, 2.5f * Time.deltaTime);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, actionPoint.pivotPoint.rotation, 50 * Time.deltaTime);
            }

            if (!isRotatingBecauseOfDialogue && isWaitingAtPathPoint && !isInCombat)
            {
                // Fix the index check to ensure it's within bounds
                if (aiPath != null && aiPathCurrentIndex < aiPath.points.Length)
                {
                    var targetPoint = aiPath.points[aiPathCurrentIndex];

                    if (targetPoint.faceDirection && targetPoint.DirectionToFace != null)
                    {
                        Quaternion targetRot = aiPath.points[aiPathCurrentIndex].DirectionToFace.rotation;
                        if (Quaternion.Angle(transform.rotation, targetRot) > 2f)
                        {
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
                        }
                        else
                        {
                            transform.rotation = aiPath.points[aiPathCurrentIndex].DirectionToFace.rotation;
                        }
                    }
                }
            }

        }

        
        [AIInvokable]
        public void UpdateNavMeshPathStatus()
        {
            if (navmeshPath.corners.Length >= 2)
            {
                if(navmeshPath.corners.Length == 2)
                    navmeshPathFinished = (Vector3.Distance(agent.transform.position, navmeshPath.corners[1]) < stoppingDistance);
                else
                    navmeshPathFinished = (Vector3.Distance(agent.transform.position, navmeshPath.corners[navmeshPath.corners.Length-1]) < stoppingDistance);

            }
            else
                navmeshPathFinished = true;
        }

        protected virtual void FixedUpdate()
        {
            if (!usesMovements)
                return;

            if (!isAlive || isUnconscious)
                return;

            HandleMovements();
            TLinkObstacleAvoidance();
        }


        [ContextMenu("UpdatePathFromGUID")]
        void PathCrossScene()
        {
            WaitAndUpdatePathFromGUID();
        }


        /// Manage everything reguarding Crosscene referencing and GUID Components
        #region Crossscene and GUID 
        /// <summary>
        /// Attempts to set the MainTarget to the GUID corresponding performing different checks over time. 
        /// </summary>
        /// <returns></returns>
        public void WaitAndUpdateTargetFromGUID()
        {
            StopCoroutine(WaitAndUpdateTargetFromGUIDTask());
            StartCoroutine(WaitAndUpdateTargetFromGUIDTask());
        }

        /// <summary>
        /// Immediatly set the MainTarget to the GUID corresponding. It can fail if it is used while the scene where the GUIDReference is being loaded.  
        /// </summary>
        [ContextMenu("UpdateTargetFromGUID"), AIInvokable]
        public void UpdateTargetFromGUID()
        {
            GameObject rTest = iGUIDReferences.TryToGetReferenceGameObject(AIGUIDReferenceType.MainTarget);
            if (rTest != null)
            {
                SetTarget(rTest);
            }
        }

        private IEnumerator WaitAndUpdateTargetFromGUIDTask()
        {
            bool set = false;
            GameObject rTest = null;
            while (!set)
            {
                rTest = iGUIDReferences.TryToGetReferenceGameObject(AIGUIDReferenceType.MainTarget);
                if (rTest != null)
                {
                    SetTarget(rTest);
                    set = true;
                }
                else
                    yield return new WaitForEndOfFrame();
            }
        }


        /// <summary>
        /// Attempts to set the MainTarget to the GUID corresponding performing different checks over time. 
        /// </summary>
        /// <returns></returns>
        public void WaitAndUpdatePathFromGUID()
        {
            StopCoroutine(WaitAndUpdatePathFromGUIDTask());
            StartCoroutine(WaitAndUpdatePathFromGUIDTask());
        }

        /// <summary>
        /// Immediatly set the MainTarget to the GUID corresponding. It can fail if it is used while the scene where the GUIDReference is being loaded.  
        /// </summary>
        public void UpdatePathFromGUID()
        {
            GameObject rTest = iGUIDReferences.TryToGetReferenceGameObject(AIGUIDReferenceType.CurrentPath);
            if (rTest != null)
            {
                aiPath = rTest.GetComponent<AICustomPath>();
            }
        }

        private IEnumerator WaitAndUpdatePathFromGUIDTask()
        {
            bool set = false;
            GameObject rTest = null;
            while (!set)
            {
                rTest = iGUIDReferences.TryToGetReferenceGameObject(AIGUIDReferenceType.CurrentPath);
               
                if (rTest != null)
                {
                    aiPath = rTest.GetComponent<AICustomPath>();
                    set = true;
                }
                else
                    yield return new WaitForSeconds(.25f);
            }
        }

        #endregion

        /// Methods that helps setting the current target, path and thing to do
        #region Movements Setters

        public void SetTarget(GameObject _target, bool updateGUID = true)
        {
            if (_target)
            {
                mainTarget = _target.transform;

                if (updateGUID && iGUIDReferences != null)
                    iGUIDReferences.TryToSetReference(_target, AIGUIDReferenceType.MainTarget);
            }
        }

        public void ClearTarget(bool updateGUID = true)
        {
            mainTarget = null;

            if (updateGUID)
                iGUIDReferences.ClearReference(AIGUIDReferenceType.MainTarget);
        }

        public void SetCustomPath(AICustomPath _path = null, bool updateGUID = true)
        {
            if (!usesMovements)
                return;

            if (_path != null)
                aiPath = _path;

            if (aiPath == null)
            {
                Debug.LogWarning("Tried to call \"UseCustomPath()\" on AI: (ID:\"" + entityID + "\") but no AICustomPath has been given or was present.");
                UseUnitysNavmesh(); // Fallback
                return;
            }
            else
            {
                iGUIDReferences.TryToSetReference(_path.gameObject, AIGUIDReferenceType.CurrentPath);
            }
        }

        public void ClearCustomPath(bool updateGUID = true)
        {
            aiPath = null;

            if (updateGUID)
                iGUIDReferences.ClearReference(AIGUIDReferenceType.CurrentPath);
        }

        #endregion

        #region Movements
        /// <summary>
        /// Handles the different types of movement this AI entity can use.
        /// </summary>
        public void HandleMovements()
        {
            if(movementType == MovementType.UnityNavmesh)
            {
                if (!agent.enabled)
                    return;

                isStopped = (agent.velocity.magnitude < RCKSettings.STOPPING_DETECTION_THRESHOLD);

                if (canSeeTarget)
                {
                    hasCompletlyLostTarget = false;
                    hasToRandomlySearchTarget = false;
                }

                if (!isTraversingLink) // If we're on a navmesh
                {
                    // Determine the cases where the Path needs to be updated
                    if ((shouldFollowMainTarget && canSeeTarget && shouldFollowOnlyIfCanSee) || (shouldFollowMainTarget && !shouldFollowOnlyIfCanSee))
                    {
                        // Update for offline mode
                        if (usesOfflineMode && mainTarget != null)
                        {
                            cachedMainTargetPos = (mainTarget.position);
                            cachedMainTarget = true;
                        }
                    }
                }
            }
            else if(movementType == MovementType.Offline && shouldFollowMainTarget && usesOfflineMode)
            {
                // If the AI is in offline mode, always update the CachedMainTargetPos
                if (mainTarget != null && alwaysCacheMainTargetDuringOffline)
                {
                    cachedMainTargetPos = mainTarget.position;
                    cachedMainTarget = true;
                }

                if(cachedMainTarget)
                {
                    // If the target isn't reached yet
                    if (Vector3.Distance(transform.position, cachedMainTargetPos) >= stoppingDistance)
                    {
                        isStopped = false;
                        // Moves the Transform to the cached position
                        transform.position = Vector3.MoveTowards(transform.position, cachedMainTargetPos, maxSpeed * Time.deltaTime);
                    }
                    else
                        isStopped = true;
                }
            }
            else if(movementType == MovementType.Offline && shouldFollowTargetVector && usesOfflineMode)
            {
                // If the target isn't reached yet
                if (Vector3.Distance(transform.position, targetVector) >= stoppingDistance)
                {
                    isStopped = false;
                    // Moves the Transform to the cached position
                    transform.position = Vector3.MoveTowards(transform.position, targetVector, maxSpeed * Time.deltaTime);
                }
                else
                    isStopped = true;
            }
            else if(movementType == MovementType.TraversingLinks)
            {
                isStopped = m_Rigidbody.linearVelocity.magnitude < RCKSettings.STOPPING_DETECTION_THRESHOLD;
            }
        }

        [AIInvokable]
        public void FollowTargetThroughNavmeshPath()
        {
            if(mainTarget == null)
            {
                //Debug.LogWarning("Called FollowTargetThroughNavmeshPath() but no MainTarget has been set");
                return;
            }

            if (!shouldFollowMainTarget)
                return;

            if (isInAttackMovementLock)
                return;

            bool closestPoint = false;
            if (agent.enabled && agent.isOnNavMesh)
            {
                /*
                if (calculatePathTimer >= calculatePathTimex)
                {
                    calculatePathTimer = 0;
                }
                */
                agent.CalculatePath(mainTarget.position, navmeshPath);

                if (navmeshPath.status == NavMeshPathStatus.PathInvalid)
                {
                    NavMeshHit myNavHit;
                    if (NavMesh.SamplePosition(mainTarget.transform.position, out myNavHit, 5.0f, NavMesh.AllAreas))
                    {
                        //Handles.Label(myNavHit.position, new GUIContent("Closest"));
                        agent.CalculatePath(myNavHit.position, navmeshPath);
                        closestPoint = true;
                    }
                }
            }


            // If there's a significant difference in Z position
            float distanceAgentToTarget = 0;
            if (Mathf.Abs(agent.transform.position.y - mainTarget.transform.position.y) > RCKSettings.NPCS_Z_CHECK_FOR_THREE_DIMENSIONAL_DIST)
                distanceAgentToTarget = Vector3.Distance(agent.transform.position, mainTarget.position);
            else
                distanceAgentToTarget = (RCKTransform.HorizontalDistance(agent.transform.position, mainTarget.position));

            // Stopping distance check
            if ((distanceAgentToTarget >= stoppingDistance && !closestPoint
                && selectedSteeringBehaviour != SteeringBehaviours.Flee && selectedSteeringBehaviour != SteeringBehaviours.FleeAtDistance)
                || (selectedSteeringBehaviour == SteeringBehaviours.RigtSideways || selectedSteeringBehaviour == SteeringBehaviours.LeftSideways)
                || (distanceAgentToTarget >= stoppingDistance && closestPoint && navmeshPath.corners.Length >= 2))
            {
                // If there is room to walk
                if (navmeshPath.corners.Length >= 2)
                {
                    switch (selectedSteeringBehaviour)
                    {
                        case SteeringBehaviours.Seek:
                            agent.velocity = Seek(navmeshPath.corners[1]);
                            break;

                        case SteeringBehaviours.Arrive:
                            agent.velocity = Arrive(navmeshPath.corners[1], arriveSteeringBehaviourDeceleration);
                            break;

                        case SteeringBehaviours.RigtSideways:
                            agent.velocity = RightSideway(navmeshPath.corners[1]);
                            break;

                        case SteeringBehaviours.LeftSideways:
                            agent.velocity = LeftSideway(navmeshPath.corners[1]);
                            break;
                    }
                }
            }
            else if(selectedSteeringBehaviour == SteeringBehaviours.Flee || selectedSteeringBehaviour == SteeringBehaviours.FleeAtDistance)
            {
                // If there is room to walk
                if (navmeshPath.corners.Length >= 2)
                {
                    switch (selectedSteeringBehaviour)
                    {
                        case SteeringBehaviours.Flee:
                            agent.velocity = Flee(navmeshPath.corners[1]);
                            break;

                        case SteeringBehaviours.FleeAtDistance:
                            agent.velocity = FleeAtDistance(navmeshPath.corners[1], fleeAtDistanceValue);
                            break;
                    }
                }
            }
            else
            {
                // Stop the agent
                agent.velocity = Vector3.SmoothDamp(agent.velocity, Vector3.zero, ref velocity, stoppingHalt);

                if (lookATransformIfStopped && directionToLookIfStopped != null && agent.velocity == Vector3.zero && !lookAtTarget)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, directionToLookIfStopped.rotation, Time.deltaTime * generalRotationSpeed);
                }

                if (!lookATransformIfStopped && lookAtVector3IfStopped)
                {
                    Vector3 newDirection = (vector3ToLookAtIfStopped - transform.position).normalized;
                    newDirection.y = 0;
                    Quaternion lookRotation = Quaternion.LookRotation(newDirection);

                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, generalRotationSpeed * Time.deltaTime);
                }
            }

            if (!isTraversingLink && !isInConversation)
            {
                // Manage rotation
                if (distanceAgentToTarget <= RCKSettings.DISTANCE_WHEN_NPCS_START_LOOK_AT)
                {
                    if (lookAtTarget && mainTarget != null)
                    {
                        // Rotate torwards target
                        var lookAt = Quaternion.LookRotation(mainTarget.position - transform.position);
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, Time.deltaTime * generalRotationSpeed);
                    }
                }
                else if (navmeshPath.corners.Length >= 2 && navmeshPath.corners[1] != null)
                {
                    // Rotate towards the direction of movement
                    var lookAt = Quaternion.LookRotation(navmeshPath.corners[1] - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, Time.deltaTime * generalRotationSpeed);
                }
                else
                {
                    // Rotate in direction of the movement (?)
                    var lookAt = Quaternion.LookRotation((transform.forward * 2) - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, Time.deltaTime * generalRotationSpeed);
                }
            }
        }

        [AIInvokable]
        public void LookAtTarget()
        {
            if (isInAttackRotationLock)
                return;

            if (lookAtTarget && mainTarget != null)
            {
                // Rotate torwards target
                var lookAt = Quaternion.LookRotation(mainTarget.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, Time.deltaTime * generalRotationSpeed);
            }
        }    

        [AIInvokable]
        public void FollowNavmeshPath()
        {
            if (navmeshPath.corners.Length >= 2 && navmeshPath.corners.Length - 1 >= 0)
            {
                // Walk trough the path
                if(agent.isOnNavMesh)
                    agent.CalculatePath(navmeshPath.corners[navmeshPath.corners.Length - 1], navmeshPath);

                float distanceAgentFirstCorner = 0;

                try
                {
                    Vector3.Distance(agent.transform.position, navmeshPath.corners[navmeshPath.corners.Length - 1]);
                }
                catch { }

                // stopping distance check
                if (distanceAgentFirstCorner >= stoppingDistance ||
                    (selectedSteeringBehaviour == SteeringBehaviours.RigtSideways || selectedSteeringBehaviour == SteeringBehaviours.LeftSideways)) // THOSE BEHAVIOURS MUST BE PLAYED EVEN IF WE REACHED THE STOPPING DISTANCE
                {
                    // If there is room to walk
                    if (navmeshPath.corners.Length >= 2)
                    {
                        switch (selectedSteeringBehaviour)
                        {
                            case SteeringBehaviours.Seek:
                                agent.velocity = Seek(navmeshPath.corners[1]);
                                break;

                            case SteeringBehaviours.Arrive:
                                agent.velocity = Arrive(navmeshPath.corners[1], arriveSteeringBehaviourDeceleration);
                                break;

                            case SteeringBehaviours.Flee:
                                agent.velocity = Flee(navmeshPath.corners[1]);
                                break;

                            case SteeringBehaviours.FleeAtDistance:
                                agent.velocity = FleeAtDistance(navmeshPath.corners[1], fleeAtDistanceValue);
                                break;

                            case SteeringBehaviours.RigtSideways:
                                agent.velocity = RightSideway(navmeshPath.corners[1]);
                                break;

                            case SteeringBehaviours.LeftSideways:
                                agent.velocity = LeftSideway(navmeshPath.corners[1]);
                                break;
                        }

                        cachedMainTargetPos = (navmeshPath.corners[navmeshPath.corners.Length - 1]);
                        cachedMainTarget = true;
                    }
                }
                else
                {
                    // Stop the agent
                    agent.velocity = Vector3.SmoothDamp(agent.velocity, Vector3.zero, ref velocity, stoppingHalt);

                    if (lookATransformIfStopped && directionToLookIfStopped != null && agent.velocity == Vector3.zero && !lookAtTarget)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, directionToLookIfStopped.rotation, Time.deltaTime * generalRotationSpeed);
                    }

                    if(!lookATransformIfStopped && lookAtVector3IfStopped)
                    {
                        Vector3 newDirection = (vector3ToLookAtIfStopped - transform.position).normalized;
                        newDirection.y = 0;

                        Quaternion lookRotation = Quaternion.LookRotation(newDirection);

                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, generalRotationSpeed * Time.deltaTime);
                    }
                }
            }

            if (!isTraversingLink && !isInConversation)
            {
                if (navmeshPath != null && navmeshPath.corners.Length > 1)
                {
                    // Rotate towards the direction of movement
                    var lookAt = Quaternion.LookRotation(navmeshPath.corners[1] - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, Time.deltaTime * generalRotationSpeed);
                }
                else
                {
                    // Rotate in direction of the movement (?)
                    var lookAt = Quaternion.LookRotation((transform.forward * 2) - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, Time.deltaTime * generalRotationSpeed);
                }
            }
        }

        [AIInvokable]
        public void SetLookAtTarget(bool setLookAt)
        { 
            if (!lookAtTarget && setLookAt)
            {
                lookAtTarget = true;
                agent.angularSpeed = 0;
            }
            else if(!setLookAt && lookAtTarget)
            {
                lookAtTarget = false;
                agent.angularSpeed = agentAngularSpeed;
            }
        }

        [AIInvokable]
        public void FollowTargetVector()
        {
            // PREVIOUS ONE
            shouldFollowTargetVector = true;

            if (!agent.enabled || !agent.isOnNavMesh)
                return;


            agent.CalculatePath(targetVector, navmeshPath);

            if (isInCombat)
            {
                bool hasNextCorner = (navmeshPath != null && navmeshPath.corners != null && navmeshPath.corners.Length >= 2);
                bool nextCornerIsUs = false;

                if (hasNextCorner)
                {
                    Vector3 toCorner = navmeshPath.corners[1] - agent.transform.position;
                    toCorner.y = 0f;
                    nextCornerIsUs = (toCorner.sqrMagnitude < 0.01f);
                }

                bool cannotProgress = !hasNextCorner || nextCornerIsUs || navmeshPath.status == NavMeshPathStatus.PathInvalid;

                if (cannotProgress)
                {
                    agent.velocity = Vector3.SmoothDamp(agent.velocity, Vector3.zero, ref velocity, stoppingHalt);
                    RotateTowardsWorldPos(targetVector);
                    return;
                }
            }


            bool closestPoint = false;
            if (navmeshPath.status == NavMeshPathStatus.PathInvalid)
            {
                NavMeshHit myNavHit;
                if (NavMesh.SamplePosition(targetVector, out myNavHit, Mathf.Infinity, -1))
                {
                    //Handles.Label(myNavHit.position, new GUIContent("Closest"));
                    agent.CalculatePath(myNavHit.position, navmeshPath);
                    closestPoint = true;
                }
            }

            // Stopping distance check
            if ((Vector3.Distance(agent.transform.position, targetVector) >= stoppingDistance && !closestPoint
                && selectedSteeringBehaviour != SteeringBehaviours.Flee && selectedSteeringBehaviour != SteeringBehaviours.FleeAtDistance)
                || (selectedSteeringBehaviour == SteeringBehaviours.RigtSideways || selectedSteeringBehaviour == SteeringBehaviours.LeftSideways)
                || (Vector3.Distance(agent.transform.position, targetVector) >= stoppingDistance && closestPoint && navmeshPath.corners.Length >= 2))
            {
                // If there is room to walk
                if (navmeshPath.corners.Length >= 2 && !isInAttackMovementLock)
                {
                    switch (selectedSteeringBehaviour)
                    {
                        case SteeringBehaviours.Seek:
                            agent.velocity = Seek(navmeshPath.corners[1]);
                            break;

                        case SteeringBehaviours.Arrive:
                            agent.velocity = Arrive(navmeshPath.corners[1], arriveSteeringBehaviourDeceleration);
                            break;

                        case SteeringBehaviours.RigtSideways:
                            agent.velocity = RightSideway(navmeshPath.corners[1]);
                            break;

                        case SteeringBehaviours.LeftSideways:
                            agent.velocity = LeftSideway(navmeshPath.corners[1]);
                            break;
                    }
                }
            }
            else
            {
                // Stop the agent
                agent.velocity = Vector3.SmoothDamp(agent.velocity, Vector3.zero, ref velocity, stoppingHalt);

                if (lookATransformIfStopped && directionToLookIfStopped != null && agent.velocity == Vector3.zero && !lookAtTarget)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, directionToLookIfStopped.rotation, Time.deltaTime * generalRotationSpeed);
                }

                if (!lookATransformIfStopped && lookAtVector3IfStopped && !isWaitingAtPathPoint && !isInAttackRotationLock)
                {
                    Vector3 newDirection = (vector3ToLookAtIfStopped - transform.position).normalized;
                    newDirection.y = 0;

                    Quaternion lookRotation = Quaternion.LookRotation(newDirection);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, 100 * Time.deltaTime);
                }
            }
        }

        [AIInvokable]
        public void SetLookAtVector3IfStopped(Vector3 _vector)
        {
            vector3ToLookAtIfStopped = _vector;
            lookAtVector3IfStopped = true;
        }

        [AIInvokable]
        public void ClearLookAtVector3IfStopped()
        {
            lookAtVector3IfStopped = false;
        }

        [AIInvokable]
        public void GetRandomTargetVectorFromNavmesh(float radius)
        {
            targetVector = PickRandomNavMeshPoint(radius);
        }

        [AIInvokable]
        public void SetTargetVector(Vector3 vector)
        {
            targetVector = vector;
        }

        [AIInvokable] 
        public void GetRandomTargetVectorFromNavmeshWithCenter(float radius, Vector3 center)
        {
            targetVector = PickRandomNavMeshPoint(radius, center);
        }

        [AIInvokable]
        public void TargetGetRandomTargetVectorFromNavmesh(float radius)
        {
            targetVector = PickRandomNavMeshPoint(radius, mainTarget);
        }

        [AIInvokable]
        public void TargetGetOnlyReachableRandomTargetVectorFromNavmesh(float radius)
        {
            shouldFollowTargetVector = false;
            StartCoroutine(GetRandomTargetVectorReachableTask(radius));
        }

        public bool isOnTargetVector;
        [AIInvokable]
        public bool IsOnTargetVector()
        {
            isOnTargetVector = (Vector3.Distance(transform.position, targetVector) <= stoppingDistance);
            return isOnTargetVector;
        }

        private Vector3 PickRandomNavMeshPoint(float radius, Transform center = null)
        {
            if (center == null)
                center = this.transform;

            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += center.position;
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
            {
                finalPosition = hit.position;
            }
            else
                return targetVector;


            return finalPosition;
        }

        private IEnumerator GetRandomTargetVectorReachableTask(float radius)
        {
            bool found = false;
            NavMeshPath path = new NavMeshPath();

            bool triedDuringTraversing = false;

            Vector3 tempVector;
            while(!found)
            {
                if (!agent.isOnNavMesh)
                    yield return null;

                tempVector = PickRandomNavMeshPoint(radius);

                try
                {
                    if(agent.isOnNavMesh)
                        agent.CalculatePath(tempVector, path);

                }
                catch { }

                if (!isTraversingLink)
                {
                    if (path.status == NavMeshPathStatus.PathPartial || path.status == NavMeshPathStatus.PathInvalid)
                        found = false;
                    else
                    {
                        found = true;
                        targetVector = tempVector;
                    }
                }
                else
                {
                    triedDuringTraversing = true;
                    found = true;
                    targetVector = tempVector;
                }

                yield return null;
            }

            shouldFollowTargetVector = true;

            // If it was found during traversing, it is not reliable, try again
            if(triedDuringTraversing)
            {
                found = false;
                while (isTraversingLink)
                {
                    yield return null;
                }

                while (!found)
                {
                    if (!agent.isOnNavMesh)
                        yield return null;

                    tempVector = PickRandomNavMeshPoint(radius);

                    try
                    {
                        if (agent.isOnNavMesh)
                            agent.CalculatePath(tempVector, path);

                    }
                    catch { }

                    if (path.status == NavMeshPathStatus.PathPartial || path.status == NavMeshPathStatus.PathInvalid)
                        found = false;
                    else
                    {
                        targetVector = tempVector;
                        found = true;
                    }

                    yield return null;
                }

                shouldFollowTargetVector = true;

            }

        }

        private Vector3 PickRandomNavMeshPoint(float radius, Vector3 center)
        {

            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += center;
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
            {
                finalPosition = hit.position;
            }
            else
                return targetVector;


            return finalPosition;
        }


        public void CheckRotation()
        {
            // Remove situations where the rotation animation DOESN'T have to be applied
            if (isUsingActionPoint)
            {
                isRotating = false;
                return;
            }

            if (Vector3.Distance(oldEulerAngles, transform.rotation.eulerAngles) < rotationThreshold)
            {
                //NO ROTATION
                isRotating = false;
            }
            else
            {
                oldEulerAngles = transform.rotation.eulerAngles;
                isRotating = true;
            }
        }

        /// <summary>
        /// This methods updates 
        /// </summary>
        [AIInvokable]
        public void SnapshotTargetPosition()
        {
            if(agent.isOnNavMesh)
                agent.CalculatePath(mainTarget.position, navmeshPath);
        }

        /// <summary>
        /// Controls the movements of the AI entity while in TraversingLinks mode
        /// </summary>
        /// <returns></returns>
        protected IEnumerator TraverseNormalSpeed()
        {
            if (!usesMovements)
                yield return null;

            currentTLinkTargetVector = Vector3.zero;
            Vector3 selectedDistance = (shouldFollowTargetVector) ? targetVector : (mainTarget != null) ? mainTarget.position : Vector3.zero;

            while (isTraversingLink)
            {
                // If has a target and chasing it
                if ((chaseTargetDuringTraversing && mainTarget != null && !isInAttackMovementLock) || shouldFollowTargetVector)
                {
                    //Vector3 chasingVector = Vector3.MoveTowards(agent.transform.position, mainTarget.position, ((currentSpeed + maxSpeed) / 2) * Time.deltaTime);
                    selectedDistance = (shouldFollowTargetVector) ? targetVector : mainTarget.position;

                    if (Vector3.Distance(agent.transform.position, selectedDistance) >= stoppingDistance)
                    {
                        cachedMainTargetPos = (selectedDistance);
                        cachedMainTarget = true;

                        Vector3 targetVector = (selectedDistance - agent.transform.position).normalized;
                        targetVector.y = 0;

                        if (currentTLinkTargetVector == Vector3.zero)
                            currentTLinkTargetVector = targetVector;

                        // Obstacle avoidance

                        if (leftOccupied && !rightOccupied)
                        {
                            isSteeringTLink = true;

                            currentTLinkTargetVector += (agent.transform.right / rotationDivider);
                            //Debug.Log("steering right");
                        }
                        else if (rightOccupied && !leftOccupied)
                        {
                            isSteeringTLink = true;

                            currentTLinkTargetVector += (-agent.transform.right / rotationDivider);
                            //Debug.Log("steering left");
                        }
                        else if (leftOccupied && forwardOccupied && rightOccupied)
                        {
                            isSteeringTLink = true;

                            // We're stuck in a wall, let's try to determine the closest end of the obstacle
                            int counter = 2;
                            bool endPointFound = false;
                            RaycastHit leftCheck, rightCheck;
                            bool endPointIsRight = false;

                            // Determine and save which object we've hit
                            while (!endPointFound && counter <= 6)
                            {
                                // Left checks
                                if (Physics.Raycast((transform.position + (Vector3.up * 1.45f)),
                                    transform.TransformDirection(Vector3.forward + Vector3.left * leftRightOpeningMult * counter), out leftCheck, obstacleDistance, obstacleAvoidanceMask))
                                {
                                    // If the object hit by the endpoint check is different from the actual object we may have found a free spot, not always true! 
                                    if (leftHit != leftCheck.transform.gameObject)
                                    {

                                    }
                                }
                                else
                                {
                                    // Free spot
                                    endPointIsRight = false;
                                    endPointFound = true;
                                    break;
                                }

                                // Right checks
                                if (Physics.Raycast((transform.position + (Vector3.up * 1.45f)),
                                    transform.TransformDirection(Vector3.forward + Vector3.right * leftRightOpeningMult * counter), out rightCheck, obstacleDistance, obstacleAvoidanceMask))
                                {

                                }
                                else
                                {
                                    // Free spot
                                    endPointIsRight = true;
                                    endPointFound = true;
                                    break;
                                }
                                counter++;
                                yield return null;
                            }

                            // Couldn't find an endpoint, go right or left in base of position
                            if (!endPointFound)
                            {
                                differenceLeftRight = AngleDir(agent.transform.forward, targetVector, Vector3.up);

                                // Best steering is on the left
                                if (differenceLeftRight == -1)
                                {
                                    // Steer left
                                    endPointIsRight = false;
                                }
                                // Best steering is on the Right
                                else
                                {
                                    // Steer right
                                    endPointIsRight = true;
                                }

                                endPointFound = true;
                            }


                            // Rotate towards the endpoint
                            if (endPointIsRight)
                                currentTLinkTargetVector += (agent.transform.right / rotationDivider);
                            else
                                currentTLinkTargetVector += -(agent.transform.right / rotationDivider);

                        }
                        else if (!leftOccupied && !rightOccupied && forwardOccupied)
                        {
                            differenceLeftRight = AngleDir(agent.transform.forward, targetVector, Vector3.up);

                            // Best steering is on the left
                            if (differenceLeftRight == -1)
                            {
                                // Steer left
                                currentTLinkTargetVector += (-agent.transform.right / rotationDivider);
                            }
                            // Best steering is on the Right
                            else
                            {
                                // Steer right
                                currentTLinkTargetVector += (agent.transform.right / rotationDivider);
                            }
                        }
                        else if (!leftOccupied && forwardOccupied)
                        {
                            // Steer left
                            currentTLinkTargetVector += (-agent.transform.right / rotationDivider);
                        }
                        else if (!rightOccupied && forwardOccupied)
                        {
                            // Steer right
                            currentTLinkTargetVector += (agent.transform.right / rotationDivider);
                        }
                        else if (!rightOccupied && !forwardOccupied && !leftOccupied)
                        {
                            isSteeringTLink = false;

                            currentTLinkTargetVector = Vector3.Lerp(currentTLinkTargetVector, targetVector, lerpingToOriginal);
                        }

                        if (selectedSteeringBehaviour == SteeringBehaviours.Flee || selectedSteeringBehaviour == SteeringBehaviours.FleeAtDistance)
                            m_Rigidbody.linearVelocity = (currentTLinkTargetVector.normalized * currentSpeed);
                        else
                        {
                            // Fix cached target if the target is the player mounted
                            if (mainTarget != null && mainTarget.CompareTag("Player") && RckPlayer.instance.isMounted && RckPlayer.instance.aiMounting)
                                cachedMainTargetPos = RckPlayer.instance.aiMounting.transform.position;

                            if (mainTarget != null && mainTarget.CompareTag("RPG Creation Kit/AI"))
                            {
                                RckAI rckAI = mainTarget.GetComponent<RckAI>();

                                if(rckAI != null && rckAI.isMounted && rckAI.aiMountingByRckAI) 
                                    cachedMainTargetPos = rckAI.aiMountingByRckAI.transform.position;
                            }

                            if (OnSlope())
                            {
                                //Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(currentTLinkTargetVector, slopeHit.normal);
                                //m_Rigidbody.velocity = (slopeMoveDirection.normalized * currentSpeed);

                                
                                //cached main target pos or targetvector
                                transform.position = Vector3.MoveTowards(transform.position, transform.position + (currentTLinkTargetVector.normalized * maxSpeed), maxSpeed * Time.deltaTime);
                            }
                            else
                            {

                                //m_Rigidbody.velocity = (currentTLinkTargetVector.normalized * currentSpeed);
                                transform.position = Vector3.MoveTowards(transform.position, transform.position + (currentTLinkTargetVector.normalized * maxSpeed), maxSpeed * Time.deltaTime);
                            }
                        }
                    }
                    else
                    {
                        // Stop the agent
                        m_Rigidbody.linearVelocity = Vector3.SmoothDamp(m_Rigidbody.linearVelocity, Vector3.zero, ref velocity, stoppingHalt);
                    }
                }
                else // If not has a target (wandering for the world)
                {
                    //agent.transform.position = Vector3.MoveTowards(agent.transform.position, agent.transform.forward, maxSpeed * Time.deltaTime);
                }

                if (((rotateToFaceTargetDuringTraversing && mainTarget != null && !isInAttackRotationLock) || shouldFollowTargetVector) && !isInConversation)
                {
                    var targetPos = currentTLinkTargetVector;

                    if (mainTarget != null && Vector3.Distance(transform.position, mainTarget.position) <= stoppingDistance + 5f)
                        targetPos = (mainTarget.transform.position - transform.position).normalized;
                    else
                        targetPos = (currentTLinkTargetVector);


                    targetPos.y = 0; //set targetPos y equal to mine, so I only look at my own plane
                    var targetDir = Quaternion.LookRotation(targetPos);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetDir, generalRotationSpeed * Time.deltaTime);
                }

                yield return null;
            }

            yield return 0;
        }


        RaycastHit slopeHit;
        public float height = 3;
        private bool OnSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, height / 2 + 0.5f))
            {
                if (slopeHit.normal != Vector3.up)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the AI is stuck in the TraversingLink mode. If it is, calls Unstuck()
        /// </summary>
        void CheckForStuckStatus_TLink()
        {
            if (!usesMovements || !isTraversingLink || mainTarget == null)
            {
                tLinkStuckCounter = 0.0f;
                return;
            }
            if (Vector3.Distance(m_Rigidbody.linearVelocity, tLinkStuckThreshold) < 1)
                tLinkStuckCounter += 1.0f * Time.deltaTime;
            else
                tLinkStuckCounter = 0.0f;

            if (tLinkStuckCounter >= tLinkStuckTime)
            {
                Unstuck();
                tLinkStuckCounter = tLinkStuckTime / 2;
            }

        }


        /// <summary>
        /// Attempts to unstuck the AI by pushing him into a free direction (if found), if he's detected to be stucked while traversing links
        /// </summary>
        public void Unstuck()
        {
            if (!usesMovements)
                return;

            m_Rigidbody.linearVelocity = Vector3.zero;
            currentTLinkTargetVector = Vector3.zero;

            if (!leftOccupied)
                m_Rigidbody.AddForce(transform.right * tLinkUnstuckForce, ForceMode.Impulse);
            else if (!rightOccupied)
                m_Rigidbody.AddForce(-transform.right * tLinkUnstuckForce, ForceMode.Impulse);
            else if (!forwardOccupied)
                m_Rigidbody.AddForce(transform.forward * tLinkUnstuckForce, ForceMode.Impulse);
            else
            {
                m_Rigidbody.AddForce(-transform.forward * tLinkUnstuckForce, ForceMode.Impulse);
                m_Rigidbody.AddForce(transform.right * (Random.Range(0.0f, 1.0f) * tLinkUnstuckForce), ForceMode.Impulse);
                m_Rigidbody.AddForce(-transform.right * (Random.Range(0.0f, 1.0f) * tLinkUnstuckForce), ForceMode.Impulse);
            }
        }

        int AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
        {
            Vector3 perp = Vector3.Cross(fwd, targetDir);
            float dir = Vector3.Dot(perp, up);

            if (dir > 0.0f)
            {
                return 1;
            }
            else if (dir < 0f)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Allows for Obstacle Avoidance while in TraversingLinks mode, the AI is able to detect and obstacle and attempt to get around it, while continuing moving towards its mainTarget.
        /// This method looks for the surroundings and determines if the left, foward or the right is occupied.
        /// </summary>
        void TLinkObstacleAvoidance()
        {
            if (!usesMovements)
                return;

            if (movementType != MovementType.TraversingLinks)
                return;

            switch (tLinkObstacleAvoidancePrecision)
            {
                case TLinkObstacleAvoidancePrecision.Full:
                    leftOccupied = (Physics.Raycast((transform.position + (Vector3.up / 3.0f)), transform.TransformDirection(Vector3.forward + Vector3.left * leftRightOpeningMult), out feetLeftHit, obstacleDistance, obstacleAvoidanceMask) ||
                                Physics.Raycast((transform.position + (Vector3.up * 1.45f)), transform.TransformDirection(Vector3.forward + Vector3.left * leftRightOpeningMult), out bodyLeftHit, obstacleDistance, obstacleAvoidanceMask) ||
                                Physics.Raycast((transform.position + (Vector3.up * 2.0f)), transform.TransformDirection(Vector3.forward + Vector3.left * leftRightOpeningMult), out headLeftHit, obstacleDistance, obstacleAvoidanceMask));


                    forwardOccupied = (Physics.Raycast((transform.position + (Vector3.up / 3.0f)), transform.TransformDirection(Vector3.forward), out feetForwardHit, obstacleDistance, obstacleAvoidanceMask) ||
                                       Physics.Raycast((transform.position + (Vector3.up * 1.45f)), transform.TransformDirection(Vector3.forward), out bodyForwardHit, obstacleDistance, obstacleAvoidanceMask) ||
                                       Physics.Raycast((transform.position + (Vector3.up * 2.0f)), transform.TransformDirection(Vector3.forward), out headForwardHit, obstacleDistance, obstacleAvoidanceMask));

                    rightOccupied = (Physics.Raycast((transform.position + (Vector3.up / 3.0f)), transform.TransformDirection(Vector3.forward + Vector3.right * leftRightOpeningMult), out feetRightHit, obstacleDistance, obstacleAvoidanceMask) ||
                                     Physics.Raycast((transform.position + (Vector3.up * 1.45f)), transform.TransformDirection(Vector3.forward + Vector3.right * leftRightOpeningMult), out bodyRightHit, obstacleDistance, obstacleAvoidanceMask) ||
                                     Physics.Raycast((transform.position + (Vector3.up * 2.0f)), transform.TransformDirection(Vector3.forward + Vector3.right * leftRightOpeningMult), out headRightHit, obstacleDistance, obstacleAvoidanceMask));

                    break;


                case TLinkObstacleAvoidancePrecision.Good:
                    leftOccupied = (Physics.Raycast((transform.position + (Vector3.up / 3.0f)), transform.TransformDirection(Vector3.forward + Vector3.left * leftRightOpeningMult), out feetLeftHit, obstacleDistance, obstacleAvoidanceMask) ||
                                    Physics.Raycast((transform.position + (Vector3.up * 1.45f)), transform.TransformDirection(Vector3.forward + Vector3.left * leftRightOpeningMult), out bodyLeftHit, obstacleDistance, obstacleAvoidanceMask));

                    forwardOccupied = (Physics.Raycast((transform.position + (Vector3.up / 3.0f)), transform.TransformDirection(Vector3.forward), out feetForwardHit, obstacleDistance, obstacleAvoidanceMask) ||
                                       Physics.Raycast((transform.position + (Vector3.up * 1.45f)), transform.TransformDirection(Vector3.forward), out bodyForwardHit, obstacleDistance, obstacleAvoidanceMask));

                    rightOccupied = (Physics.Raycast((transform.position + (Vector3.up / 3.0f)), transform.TransformDirection(Vector3.forward + Vector3.right * leftRightOpeningMult), out feetRightHit, obstacleDistance, obstacleAvoidanceMask) ||
                                     Physics.Raycast((transform.position + (Vector3.up * 1.45f)), transform.TransformDirection(Vector3.forward + Vector3.right * leftRightOpeningMult), out bodyRightHit, obstacleDistance, obstacleAvoidanceMask));
                    break;


                case TLinkObstacleAvoidancePrecision.Minimum:
                    leftOccupied = (Physics.Raycast((transform.position + (Vector3.up / 3.0f)), transform.TransformDirection(Vector3.forward + Vector3.left * leftRightOpeningMult), out feetLeftHit, obstacleDistance, obstacleAvoidanceMask));

                    forwardOccupied = (Physics.Raycast((transform.position + (Vector3.up / 3.0f)), transform.TransformDirection(Vector3.forward), out feetForwardHit, obstacleDistance, obstacleAvoidanceMask));

                    rightOccupied = (Physics.Raycast((transform.position + (Vector3.up / 3.0f)), transform.TransformDirection(Vector3.forward + Vector3.right * leftRightOpeningMult), out feetRightHit, obstacleDistance, obstacleAvoidanceMask));
                    break;


                case TLinkObstacleAvoidancePrecision.Null:
                    leftOccupied = false;
                    forwardOccupied = false;
                    rightOccupied = false;
                    break;
            }

        }

        public void StopFollowingPath()
        {
            forceToStopPath = true;
            StopCoroutine(UnityNavmeshFollowPath());
        }


        [AIInvokable]
        [ContextMenu("FollowCurrentPath")]
        public void FollowCurrentPath()
        {
            if (aiPath == null)
                return;

            forceToStopPath = false;
            lookAtTarget = false;

            shouldFollowMainTarget = true;
            shouldFollowOnlyIfCanSee = false;
            isFollowingAIPath = true;

            StopCoroutine(UnityNavmeshFollowPath());
            StartCoroutine(UnityNavmeshFollowPath());
        }

        public bool forceToStopPath = false;
        /// <summary>
        /// Used to make the AI follow an AI Path while using the Unity Navmesh System.
        /// What it does is to update the MainTarget of this entity, the movements will be still handled by "HandleMovements()"
        /// </summary>
        /// <returns></returns>
        public bool isWaitingAtPathPoint = false;
        IEnumerator UnityNavmeshFollowPath()
        {
            shouldFollowAIPath = true;
            bool pathCompleted = false;

            if (!aiPathResumeFromCurrentIndex)
            {
                aiPathCurrentIndex = aiPath.startsFromIndex;

                if (invertAIPath)
                    aiPathCurrentIndex = aiPath.points.Length - 1;
            }


            // Sets the first target
            SetTarget(aiPath.points[aiPathCurrentIndex].gameObject, false);
            if (isPersistentReference)
            {
                cachedMainTarget = true;
                cachedMainTargetPos = aiPath.points[aiPathCurrentIndex].gameObject.transform.position;
            }

            while (!pathCompleted || forceToStopPath)
            {
                // Check if we've reached the target at out stopping halt
                if(Vector3.Distance(agent.transform.position, aiPath.points[aiPathCurrentIndex].transform.position) <= stoppingDistance)
                {
                    // Check if we have to wait
                    if(aiPath.points[aiPathCurrentIndex].wait)
                    {
                        isWaitingAtPathPoint = true;

                        // Rotation is in Update

                        yield return new WaitForSeconds(aiPath.points[aiPathCurrentIndex].waitTime);

                        isWaitingAtPathPoint = false;
                    }

                    switch(aiPath.type)
                    {
                        case AICustomPathType.BackAndForthLooped:
                            // Check if we're going in positive +
                            if(!invertAIPath)
                            {
                                // If we've finished the path
                                if(aiPathCurrentIndex == aiPath.points.Length-1)
                                {
                                    // We have to go backwards
                                    invertAIPath = true;
                                    aiPathCurrentIndex--;

                                    SetTarget(aiPath.points[aiPathCurrentIndex].gameObject);

                                    if (isPersistentReference)
                                        cachedMainTargetPos = aiPath.points[aiPathCurrentIndex].gameObject.transform.position;
                                }
                                else
                                {
                                    // Set the next target
                                    aiPathCurrentIndex++;

                                    SetTarget(aiPath.points[aiPathCurrentIndex].gameObject);

                                    if (isPersistentReference)
                                        cachedMainTargetPos = aiPath.points[aiPathCurrentIndex].gameObject.transform.position;
                                }
                            }
                            else
                            {
                                // If we've finished the path
                                if (aiPathCurrentIndex == 0)
                                {
                                    // We have to go backwards
                                    invertAIPath = false;
                                    aiPathCurrentIndex++;

                                    SetTarget(aiPath.points[aiPathCurrentIndex].gameObject);

                                    if (isPersistentReference)
                                        cachedMainTargetPos = aiPath.points[aiPathCurrentIndex].gameObject.transform.position;
                                }
                                else
                                {
                                    // Set the next target
                                    aiPathCurrentIndex--;

                                    SetTarget(aiPath.points[aiPathCurrentIndex].gameObject);

                                    if (isPersistentReference)
                                        cachedMainTargetPos = aiPath.points[aiPathCurrentIndex].gameObject.transform.position;
                                }
                            }
                            break;

                        case AICustomPathType.Looped:
                            if(!invertAIPath)
                            {
                                // If it's the last point
                                if(aiPathCurrentIndex == aiPath.points.Length-1)
                                {
                                    // Set the first as current
                                    aiPathCurrentIndex = 0;
                                    SetTarget(aiPath.points[aiPathCurrentIndex].gameObject);

                                    if (isPersistentReference)
                                        cachedMainTargetPos = aiPath.points[aiPathCurrentIndex].gameObject.transform.position;
                                }
                                else // set next
                                {
                                    aiPathCurrentIndex++;
                                    SetTarget(aiPath.points[aiPathCurrentIndex].gameObject);

                                    if (isPersistentReference)
                                        cachedMainTargetPos = aiPath.points[aiPathCurrentIndex].gameObject.transform.position;
                                }
                            }
                            else
                            {
                                // If it's the first point
                                if (aiPathCurrentIndex == 0)
                                {
                                    // Set the last as current
                                    aiPathCurrentIndex = aiPath.points.Length - 1;
                                    SetTarget(aiPath.points[aiPathCurrentIndex].gameObject);

                                    if (isPersistentReference)
                                        cachedMainTargetPos = aiPath.points[aiPathCurrentIndex].gameObject.transform.position;
                                }
                                else // set next
                                {
                                    // Set the last as current
                                    aiPathCurrentIndex--;
                                    SetTarget(aiPath.points[aiPathCurrentIndex].gameObject);

                                    if (isPersistentReference)
                                        cachedMainTargetPos = aiPath.points[aiPathCurrentIndex].gameObject.transform.position;
                                }
                            }
                            break;
                    }
                }

                yield return null;
            }

            isFollowingAIPath = false;
        }


        #endregion


        [AIInvokable]
        public void SetOverrideShouldFollowMainTarget(bool _active, bool _shouldFollow)
        {
            overrideShouldFollowMainTargetActive = _active;
            overrideShouldFollowMainTarget = _shouldFollow;

            if (overrideShouldFollowMainTargetActive)
                shouldFollowMainTarget = overrideShouldFollowMainTarget;
        }

        /// <summary>
        /// Enables the TraversingLinks mode.
        /// </summary>
        public void EnableTraversingLinks()
        {
            if (!usesMovements || movementType == MovementType.Offline)
                return;

            if (!rightOccupied && !forwardOccupied && !leftOccupied)
            {
                isSteeringTLink = false;
            }

            if (!isTraversingLink)
            {
                UseTraversingLinks();
            }
        }

        public virtual void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("RPG Creation Kit/NavmeshLink"))
            {
                if (!isTraversingLink)
                    EnableTraversingLinks();
            }
        }

        public virtual void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("RPG Creation Kit/NavmeshLink"))
            {
                if (isTraversingLink)
                {
                    UseUnitysNavmesh();
                }
            }
        }

        /// <summary>
        /// Adjusts the Animator of the AI accordingly of the Movements he's making
        /// </summary>
        public void HandleAnimations()
        {
            if (!usesMovements)
                return;

            if (movementType == MovementType.UnityNavmesh)
            {
                var localVelocity = transform.InverseTransformDirection(agent.velocity);

                walkModifier = (isWalking) ? .5f : 1f;

                if (lookAtTarget)
                {
                    m_Anim.SetFloat("Speed", (localVelocity.z / currentSpeed) * walkModifier, 1f, Time.deltaTime * 10);
                    m_Anim.SetFloat("Sideways", (localVelocity.x / currentSpeed) * walkModifier, 1f, Time.deltaTime * 10);
                }
                else
                {
                    m_Anim.SetFloat("Speed", (agent.velocity.magnitude / currentSpeed) * walkModifier, 1f, Time.deltaTime * 10);
                    m_Anim.SetFloat("Sideways", (localVelocity.x / currentSpeed) * walkModifier, 1f, Time.deltaTime * 10);
                }
            }
            else if(movementType == MovementType.TraversingLinks)
            {
                var localVelocity = transform.InverseTransformDirection(m_Rigidbody.linearVelocity);

                walkModifier = (isWalking) ? .5f : 1f;

                Vector3 tarDist = transform.position;

                if (mainTarget != null && shouldFollowMainTarget)
                    tarDist = mainTarget.transform.position;
                else if (shouldFollowTargetVector)
                    tarDist = targetVector;

                float distance = Vector3.Distance(transform.position, tarDist);
                float animate = 0;
                if (distance > stoppingDistance)
                    animate = 1;

                if (lookAtTarget)
                {
                    m_Anim.SetFloat("Speed", animate * walkModifier, 1f, Time.deltaTime * 10);
                    m_Anim.SetFloat("Sideways", (localVelocity.x / currentSpeed) * walkModifier, 1f, Time.deltaTime * 10);
                }
                else
                {
                    m_Anim.SetFloat("Speed", animate * walkModifier, 1f, Time.deltaTime * 10);
                }
            }

            m_Anim.SetBool("isRotating", isRotating);
        }

       
        /// <summary>
        /// Switches to the UnityNavmesh movement type.
        /// </summary>
        public void UseUnitysNavmesh()
        {
            if (!usesMovements)
                return;

            movementType = MovementType.UnityNavmesh;
            StopCoroutine("TraverseNormalSpeed");

            isTraversingLink = false;
            agent.enabled = true;
            m_Rigidbody.isKinematic = true;
        }

        /// <summary>
        /// Switches to the TraversingLink movement type.
        /// </summary>
        public void UseTraversingLinks()
        {
            if (!usesMovements)
                return;

            StopCoroutine("TraverseNormalSpeed");

            movementType = MovementType.TraversingLinks;
            isTraversingLink = true;

            agent.enabled = false;
            m_Rigidbody.isKinematic = false;
            StartCoroutine("TraverseNormalSpeed");
        }

        public void UseCustomPathSystem()
        {
            if (!usesMovements)
                return;

            movementType = MovementType.CustomPathSystem;
        }

        public void UseNullMovement()
        {
            if (!usesMovements)
                return;

            movementType = MovementType.Null;
        }

        /// <summary>
        /// Reload the Movements State of the AI, used when it returns from Offline mode.
        /// </summary>
        /// <returns></returns>
        public IEnumerator ReloadState()
        {
            while (WorldManager.instance != null && WorldManager.instance.isLoading)
                yield return null;

            cachedMainTarget = false;

            isInOfflineMode = false;

            if(movementType != MovementType.InActionPoint)
               UseUnitysNavmesh();

            // if it was following the target ->
            // UpdateTargetFromGUID();

            // else if it was following the path
            // UpdatePathFromGUID();


            yield return null;
        }

        public void UpdateStateDelayed()
        {
            StopCoroutine(ReloadState());
            StartCoroutine(ReloadState());
        }

        bool isSteeringTLink = false;

        private float differenceLeftRight;
        Vector3 currentTLinkTargetVector = Vector3.zero;

        public bool failedGettingActionPoint = false;

        [AIInvokable]
        public void GetAndUseActionPoint()
        {
            if (myCellInfo == null)
                return;

            NPCActionPoint point = myCellInfo.GetAnUnusedActionPoint();

            if (point == null)
            {
                isUsingActionPoint = false;
                isReachingActionPoint = false;
                useNPCActionPointCalled = false;
                failedGettingActionPoint = true;
                return;
            }

            actionPoint = point;
            UseNPCActionPoint(point);
        }

        [AIInvokable]
        public void UseCurrentActionPoint()
        {
            if (isPersistentReference && isInOfflineMode && actionPoint is MountupPoint && !wantsToMountOnPoint)
                wantsToMountOnPoint = true;

            if (myCellInfo == null)
                return;

            if (actionPoint == null)
            {
                isUsingActionPoint = false;
                isReachingActionPoint = false;
                useNPCActionPointCalled = false;
                failedGettingActionPoint = true;
                return;
            }
            else
                UseNPCActionPoint(actionPoint);
        }

        [AIInvokable]
        public void UseArrayActionPoint(string[] arr)
        {
            if (myCellInfo == null)
                return;

            int tries = 0;
            bool found = false;
            while(found == false && tries < arr.Length)
            {
                tries++;

                int random = Random.Range(0, arr.Length-1);

                NPCActionPoint point = myCellInfo.allActionPoints[arr[random]];
                if(!point.isOccupied)
                {
                    actionPoint = point;
                    found = true;
                    break;
                }
                else
                {
                    actionPoint = null;
                    continue;
                }
            }

            if (actionPoint == null)
            {
                isUsingActionPoint = false;
                isReachingActionPoint = false;
                useNPCActionPointCalled = false;
                failedGettingActionPoint = true;
                return;
            }
            else
                UseNPCActionPoint(actionPoint);
        }

        public bool isReachingActionPoint = false;
        public bool useNPCActionPointCalled = false; // to disallow multiple calls

        public bool wantsToMountOnPoint = false;

        public void UseNPCActionPoint(NPCActionPoint _aPoint)
        {
            MountupPoint mountPoint = _aPoint as MountupPoint;
            if (mountPoint != null && !mountPoint.mount.isAlive)
                return;

            actionPoint = _aPoint;

            actionPoint.OnUserAssigned(this);

            SetTarget(actionPoint.pivotPoint.gameObject);

            useNPCActionPointCalled = true;

            StartCoroutine(ReachingActionPoint());
        }

        public void ResetActionPointAgentState()
        {
            if(actionPoint != null)
                actionPoint.OnUserForceLeaves();

            StopCoroutine(ReachingActionPoint());

            isReachingActionPoint = false;
            actionPoint = null;
            isUsingActionPoint = false;
            shouldUseNPCActionPoint = false;
            useNPCActionPointCalled = false;
        }

        public bool fixingInPlaceForActionPoint = false;
        public bool transformMovingToActionPoint = false;
        public IEnumerator ReachingActionPoint()
        {
            isReachingActionPoint = true;

            while (actionPoint != null && Vector3.Distance(transform.position, actionPoint.pivotPoint.position) > stoppingDistance)
            {
                yield return null;
            }

            if (actionPoint == null)
                yield break;

            DisableMovements();
            m_Anim.applyRootMotion = true;

            movementType = MovementType.InActionPoint;

            bool cancelled = false;

            while (isReachingActionPoint && (transform.position != actionPoint.pivotPoint.position ||
                   transform.rotation != actionPoint.pivotPoint.rotation))
            {
                fixingInPlaceForActionPoint = true;

                if (enterInCombatCalled)
                {
                    cancelled = true;
                    m_Anim.applyRootMotion = false;
                    UseUnitysNavmesh();

                    isUsingActionPoint = false;
                    EnableMovements();
                    actionPoint.OnUserForceLeaves();
                    yield break;
                }

                if (transform.position != actionPoint.pivotPoint.position)
                    m_Anim.SetFloat("Speed", (isWalking) ? .5f : 1f);
                else
                    m_Anim.SetFloat("Speed", 0f);

                transformMovingToActionPoint = true;
                

                yield return null;
            }

            if (cancelled)
                yield break;

            transform.position = actionPoint.pivotPoint.position;
            transform.rotation = actionPoint.pivotPoint.rotation;

            transformMovingToActionPoint = false;

            m_Anim.CrossFade(actionPoint.npcAction.name, 0f);
            playingEnterAnimationActionPoint = true;

            Invoke("OnHasFinishedPlayingActionPointEnterAnimation", actionPoint.npcAction.length);

            actionPoint.OnUserUses(this);

            isReachingActionPoint = false;
            isUsingActionPoint = true;
            fixingInPlaceForActionPoint = false;
        }

        public void OnHasFinishedPlayingActionPointEnterAnimation()
        {
            playingEnterAnimationActionPoint = false;
        }

        public GameObject currentDynamicObject;

        public void DynamicObjectStartAnimationEvent()
        {
            if(actionPoint.spawnDynamicObject)
            {
                switch(actionPoint.dynamicObjectBone)
                {
                    case DynamicObjectBone.Rhand:
                        currentDynamicObject = Instantiate(actionPoint.dynamicObject, bodyData.rHand);
                        break;

                    case DynamicObjectBone.Lhand:
                        currentDynamicObject = Instantiate(actionPoint.dynamicObject, bodyData.lHand);
                        break;
                }
            }
        }

        public void DynamicObjectEndAnimationEvent()
        {
            if (actionPoint.spawnDynamicObject)
            {
                Destroy(currentDynamicObject);
            }
        }

        public bool playingEnterAnimationActionPoint = false;
        public bool isUsingActionPoint = false;

        [AIInvokable]
        public void StopUsingNPCActionPoint(bool _immediate = false)
        {
            StopCoroutine(StopUsingNPCActionPointTask());
            StartCoroutine(StopUsingNPCActionPointTask(_immediate));
        }

        [AIInvokable]
        public void StopUsingNPCActionPointImmediate()
        {
            StopCoroutine(StopUsingNPCActionPointTask());
            StartCoroutine(StopUsingNPCActionPointTask(true));
        }

        public bool leavingActionPoint = false;
        private IEnumerator StopUsingNPCActionPointTask(bool immediate = false)
        {
            if (!immediate)
            {
                while (playingEnterAnimationActionPoint || isInConversation)
                    yield return new WaitForEndOfFrame();

                fixingInPlaceForActionPoint = false;
                actionPoint.OnUserIsLeaving(this);
                m_Anim.CrossFade(actionPoint.npcActionReturn.name, 0f);
                leavingActionPoint = true;
                yield return new WaitForSeconds(actionPoint.npcActionReturn.length);
            }
            else
            {
                m_Anim.Play("Base Locomotion", 0);
            }

            DisableMovements();
            m_Anim.applyRootMotion = false;
            UseUnitysNavmesh();

            isReachingActionPoint = false;
            isUsingActionPoint = false;
            useNPCActionPointCalled = false;
            if (actionPoint != null)
                actionPoint.OnUserHasLeft(this);
            leavingActionPoint = false;
        }

        /// <summary>
        /// Adjusts the current speed. (Walking/Running)
        /// </summary>
        [ContextMenu("Adjust Current Speed")]
        private void AdjustCurrentSpeed()
        {
            if (!usesMovements)
                return;

            currentSpeed = (isWalking ? attributes.derivedAttributes.walkSpeed : attributes.derivedAttributes.runSpeed);
            maxSpeed = (isWalking ? attributes.derivedAttributes.walkSpeed : attributes.derivedAttributes.runSpeed);
        }

        [AIInvokable]
        public void Movements_SwitchToRun()
        {
            isWalking = false;
            AdjustCurrentSpeed();
        }

        [AIInvokable]
        public void Movements_SwitchToWalk()
        {
            isWalking = true;
            AdjustCurrentSpeed();
        }

        [AIInvokable]
        public void SetFollowMainTarget(bool shouldFollow)
        {
            shouldFollowMainTarget = shouldFollow;
        }

        [AIInvokable]
        public void SetAIPathFromCell(string _pathID)
        {
            if (CellInformation.TryToGetPath(_pathID, out aiPath))
            {
                // All good
            }
            else
            {
                Debug.LogWarning("Tried to assign the path with id '" + _pathID + "' to the AI: '" + entityID + "', but the path wasn't found. Maybe you haven't updated the cell or mispelled the PathID? " + " MyCellInfo was: " + myCellInfo.cell.ID +". \nTrying again in 1 second.");
                if(!setAIPathFromCellRetrying)
                {
                    StartCoroutine(SetAIPathFromCellRetry(_pathID));
                    setAIPathFromCellRetrying = true;
                }
            }
        }

        bool setAIPathFromCellRetrying = false;
        private IEnumerator SetAIPathFromCellRetry(string _pathID)
        {
            yield return new WaitForSeconds(1);
            setAIPathFromCellRetrying = false;
            SetAIPathFromCell(_pathID);
        }

        #region SteeringBehaviours
        public enum Deceleration { Fast = 1, Normal = 2, Slow = 3, VerySlow = 4 };


        /// <summary>
        /// Returns a force that directs the agent toward the target position.
        /// </summary>
        /// <param name="targetPos">The target position.</param>
        /// <returns></returns>
        protected Vector3 Seek(Vector3 targetPos)
        {
            Vector3 desiredVelocity = Vector3.Normalize(targetPos - transform.position) * currentSpeed;
            return (desiredVelocity - agent.desiredVelocity);
        }

        /// <summary>
        /// Returns a force that directs the agent away from the target position.
        /// </summary>
        /// <param name="targetPos">The target position.</param>
        /// <returns></returns>
        protected Vector3 Flee(Vector3 targetPos)
        {
            Vector3 desiredVelocity = Vector3.Normalize(transform.position - targetPos) * currentSpeed;
            return (desiredVelocity - agent.desiredVelocity);
        }

        /// <summary>
        /// Returns a force that directs the agent away from the target position if the target is within the range of fleeDistance.
        /// </summary>
        /// <param name="targetPos">The target position.</param>
        /// <param name="fleeDistance">The distance at wich the agent will start fleeing.</param>
        /// <returns></returns>
        protected Vector3 FleeAtDistance(Vector3 targetPos, float fleeDistance)
        {
            if (Vector3.Distance(transform.position, targetPos) >= fleeDistance)
                return Vector3.zero;

            Vector3 desiredVelocity = Vector3.Normalize(transform.position - targetPos) * currentSpeed;
            return (desiredVelocity - agent.desiredVelocity);
        }

        /// <summary>
        /// Seek but with gentle halt at the target position.
        /// </summary>
        /// <param name="targetPos">The target position.</param>
        /// <param name="deceleration">The deceleration type.</param>
        /// <returns></returns>
        protected Vector3 Arrive(Vector3 targetPos, Deceleration deceleration)
        {
            // Arrive works only when the point to arrive to is the last one of the path
            if (navmeshPath.corners.Length <= 2)
            {
                Vector3 toTarget = targetPos - transform.position;
                float dist = Vector3.Distance(transform.position, targetPos);

                if (dist > 0)
                {
                    const float decelerationTweaker = 0.3f;

                    currentSpeed = dist / ((float)deceleration * decelerationTweaker);
                    currentSpeed = Mathf.Clamp(currentSpeed, currentSpeed, maxSpeed);

                    Vector3 desiredVelocity = toTarget * currentSpeed / dist;
                    return (desiredVelocity - agent.desiredVelocity);
                }

                return Vector3.zero;
            }
            else // If there is a path that has corners we do not want to arrive on coners but just pass them
                return Seek(targetPos);
        }


        protected Vector3 RightSideway(Vector3 targetPos)
        {
            Vector3 desiredVelocity = Vector3.Normalize(transform.right) * currentSpeed;
            return (desiredVelocity);
        }

        protected Vector3 LeftSideway(Vector3 targetPos)
        {
            Vector3 desiredVelocity = Vector3.Normalize(-transform.right) * currentSpeed;
            return (desiredVelocity);
        }

        protected Vector3 Pursuit()
        {
            return Vector3.zero;
        }

        #endregion

        public void EnableMovements()
        {
            if (!usesMovements)
                return;

            // Release the AI
            if (movementType == MovementType.UnityNavmesh)
                agent.enabled = true;
            else if (movementType == MovementType.TraversingLinks)
                m_Rigidbody.isKinematic = false;
        }

        public void DisableMovements()
        {
            if (!usesMovements)
                return;

            // Stop the AI
            if (movementType == MovementType.UnityNavmesh)
                agent.enabled = false;
            else if (movementType == MovementType.TraversingLinks)
                m_Rigidbody.isKinematic = true;
        }

        public void EnablePhysicalCollider()
        {
            if(isAlive)
                physicalCollider.enabled = true;
        }

        public void DisablePhysicalCollider()
        {
            physicalCollider.enabled = false;
        }

        public void EnableInteractableCollider()
        {
            interactableCollider.enabled = true;
        }

        public void DisableInteractableCollider()
        {
            interactableCollider.enabled = false;
        }

        public void StopMovingForSeconds(float seconds)
        {
            DisableMovements();
            Invoke("EnableMovements", seconds);
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.transform.CompareTag("RPG Creation Kit/Door"))
            {
                doorInTheWay = collision.gameObject.GetComponent<Door>();

                if (!doorInTheWay.isOpened && !doorInTheWay.teleports)
                {
                    // AI OPENS DOOR
                    if (doorInTheWay.anim != null)
                    {
                        m_Anim.CrossFade("OpenDoor", 0.1f);
                        doorInTheWay.ForceOpenCrossFade();
                        StopMovingForSeconds(doorInTheWay.anim["Open"].length);
                    }
                }
            }
        }

        public FCover currentCover;
        public CoverPoint currentCoverPoint = null;
        public bool failedToFindCover = false;
        public bool isGoingToCover = false;
        public bool isInCover = false;
        public float findCoverRadius = 20.0f;
        public void FindClosestCover()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, findCoverRadius);

            currentCover = null;
            currentCoverPoint = null;
            failedToFindCover = false;
            isGoingToCover = false;

            bool coverFound = false;
            float currentCoverDist = Mathf.Infinity;

            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].transform.CompareTag("RPG Creation Kit/FCover"))
                {
                    FCover thisCover = hitColliders[i].GetComponent<FCover>();
                    float thisDist = Vector3.Distance(this.transform.position, hitColliders[i].transform.position);
                    if (thisCover.HasSpotAvailable() && (currentCover == null || currentCoverDist > thisDist))
                    {
                        coverFound = true;
                        currentCover = thisCover;
                        currentCoverDist = thisDist;
                    }
                }
            }

            if (coverFound)
            {
                // Go to cover
                currentCoverPoint = currentCover.AssignSpotToAI(this);
                if (currentCoverPoint != null)
                {
                    GotoCover();
                }
                else
                {
                    // return, next call will try again
                    currentCover = null;
                    currentCoverPoint = null;
                    failedToFindCover = false;
                    isGoingToCover = false;
                }
            }
            else
            {
                // Will not attempt again until next combat
                failedToFindCover = true;
                SetCanCoverCooldown();
            }
        }

        public void SetCanCoverCooldown()
        {
            goToCoverCooldown = Random.Range(RCKSettings.COVER_AI_COOLDOWN_MIN, RCKSettings.COVER_AI_COOLDOWN_MAX);
        }

        public void GotoCover()
        {
            Movements_SwitchToRun();
            isInCover = false;
            isGoingToCover = true;
        }

        [AIInvokable]
        public void ReachCurrentCover()
        {
            if (isInCover)
                return;

            if (currentCoverPoint == null)
            {
                Debug.LogWarning("Called ReachCurrentCover() but no currentCoverPoint has been set");
                return;
            }


            stoppingDistance = RCKSettings.COVER_AI_STOPPING_DISTANCE;

            if (isWalking)
                Movements_SwitchToRun();

            bool closestPoint = false;
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.CalculatePath(currentCoverPoint.t.position, navmeshPath);

                if (navmeshPath.status == NavMeshPathStatus.PathInvalid)
                {
                    NavMeshHit myNavHit;
                    if (NavMesh.SamplePosition(currentCoverPoint.t.position, out myNavHit, Mathf.Infinity, -1))
                    {
                        //Handles.Label(myNavHit.position, new GUIContent("Closest"));
                        agent.CalculatePath(myNavHit.position, navmeshPath);
                        closestPoint = true;
                    }
                }
            }


            float distanceAgentToTarget = (Vector3.Distance(agent.transform.position, currentCoverPoint.t.position));

            // Stopping distance check
            if ((distanceAgentToTarget >= stoppingDistance && !closestPoint
                && selectedSteeringBehaviour != SteeringBehaviours.Flee && selectedSteeringBehaviour != SteeringBehaviours.FleeAtDistance)
                || (selectedSteeringBehaviour == SteeringBehaviours.RigtSideways || selectedSteeringBehaviour == SteeringBehaviours.LeftSideways)
                || (distanceAgentToTarget >= stoppingDistance && closestPoint && navmeshPath.corners.Length >= 2))
            {
                // If there is room to walk
                if (navmeshPath.corners.Length >= 2)
                {
                    switch (selectedSteeringBehaviour)
                    {
                        case SteeringBehaviours.Seek:
                            agent.velocity = Seek(navmeshPath.corners[1]);
                            break;

                        case SteeringBehaviours.Arrive:
                            agent.velocity = Arrive(navmeshPath.corners[1], arriveSteeringBehaviourDeceleration);
                            break;

                        case SteeringBehaviours.RigtSideways:
                            agent.velocity = RightSideway(navmeshPath.corners[1]);
                            break;

                        case SteeringBehaviours.LeftSideways:
                            agent.velocity = LeftSideway(navmeshPath.corners[1]);
                            break;
                    }
                }
            }
            else if (selectedSteeringBehaviour == SteeringBehaviours.Flee || selectedSteeringBehaviour == SteeringBehaviours.FleeAtDistance)
            {
                // If there is room to walk
                if (navmeshPath.corners.Length >= 2)
                {
                    switch (selectedSteeringBehaviour)
                    {
                        case SteeringBehaviours.Flee:
                            agent.velocity = Flee(navmeshPath.corners[1]);
                            break;

                        case SteeringBehaviours.FleeAtDistance:
                            agent.velocity = FleeAtDistance(navmeshPath.corners[1], fleeAtDistanceValue);
                            break;
                    }
                }
            }
            else
            {
                // Stop the agent
                agent.velocity = Vector3.SmoothDamp(agent.velocity, Vector3.zero, ref velocity, stoppingHalt);

                if (lookATransformIfStopped && directionToLookIfStopped != null && agent.velocity == Vector3.zero && !lookAtTarget)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, directionToLookIfStopped.rotation, Time.deltaTime * generalRotationSpeed);
                }

                if (!lookATransformIfStopped && lookAtVector3IfStopped)
                {
                    Vector3 newDirection = (vector3ToLookAtIfStopped - transform.position).normalized;
                    newDirection.y = 0;
                    Quaternion lookRotation = Quaternion.LookRotation(newDirection);

                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, generalRotationSpeed * Time.deltaTime);
                }

                if(isStopped)
                {
                    isGoingToCover = false;
                    isInCover = true;

                    // Crouch?
                    isCrouched = currentCoverPoint.crouched;
                    m_Anim.SetBool("isCrouched", isCrouched);
                    return;
                }
            }

            if (!isTraversingLink && !isInConversation)
            {
                // Manage rotation
                if (distanceAgentToTarget <= RCKSettings.DISTANCE_WHEN_NPCS_START_LOOK_AT)
                {
                    if (lookAtTarget && currentCoverPoint.t != null)
                    {
                        // Rotate torwards target
                        var lookAt = Quaternion.LookRotation(currentCoverPoint.t.position - transform.position);
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, Time.deltaTime * generalRotationSpeed);
                    }
                }
                else if (navmeshPath.corners.Length >= 2 && navmeshPath.corners[1] != null)
                {
                    // Rotate towards the direction of movement
                    var lookAt = Quaternion.LookRotation(navmeshPath.corners[1] - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, Time.deltaTime * generalRotationSpeed);
                }
                else
                {
                    // Rotate in direction of the movement (?)
                    var lookAt = Quaternion.LookRotation((transform.forward * 2) - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, Time.deltaTime * generalRotationSpeed);
                }
            }
        }

        private void RotateTowardsWorldPos(Vector3 worldPos, float speedMult = 1f)
        {
            if (isInAttackRotationLock || isInConversation)
                return;

            Vector3 dir = worldPos - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f)
                return;

            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * generalRotationSpeed * speedMult);
        }

        [AIInvokable]
        public void LeaveCurrentCover()
        {
            SetCanCoverCooldown();
            currentCover.FreeSpotFromAI(currentCoverPoint);

            if (currentCoverPoint.crouched)
            {
                isCrouched = false;
                m_Anim.SetBool("isCrouched", false);
            }

            currentCover = null;
            currentCoverPoint = null;
            failedToFindCover = false;
            isGoingToCover = false;
            isInCover = false;
    }


        #region Overrides

        public override void OnDialogueStarts(GameObject target)
        {
            base.OnDialogueStarts(target);

            // Stop the AI
            DisableMovements();

            // Rotate Towards Target
        }

        public override void OnDialogueEnds(GameObject target)
        {
            base.OnDialogueEnds(target);

            //Release the AI
            EnableMovements();

            // Return to do what you were doing
        }

        public override void Die()
        {
            DisableMovements();
            agent.enabled = false;
            DisablePhysicalCollider();
            DisableInteractableCollider();
            if(aiLookAt) 
                aiLookAt.isEnabled = false;

            if(isInCover)
                LeaveCurrentCover();

            base.Die();
        }

        #endregion

#if UNITY_EDITOR
        public virtual void OnDrawGizmos()
        {
            if (!usesMovements)
                return;

            if (generalDebug && enableDebugInEditor)
            {
                switch(tLinkObstacleAvoidancePrecision)
                {
                    case TLinkObstacleAvoidancePrecision.Full:
                        Gizmos.color = (leftOccupied ? Color.red : Color.green);
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up / 3.0f), transform.TransformDirection(Vector3.forward * 2 + Vector3.left * leftRightOpeningMult));
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up * 2.0f), transform.TransformDirection(Vector3.forward * 2 + Vector3.left * leftRightOpeningMult));
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up * 1.45f), transform.TransformDirection(Vector3.forward * 2 + Vector3.left * leftRightOpeningMult));

                        Gizmos.color = (forwardOccupied ? Color.red : Color.green);
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up / 3.0f), transform.TransformDirection(Vector3.forward * 2));
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up * 1.45f), transform.TransformDirection(Vector3.forward * 2));
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up * 2.0f), transform.TransformDirection(Vector3.forward * 2));

                        Gizmos.color = (rightOccupied ? Color.red : Color.green);
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up / 3.0f), transform.TransformDirection(Vector3.forward * 2 + Vector3.right * leftRightOpeningMult));
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up * 1.45f), transform.TransformDirection(Vector3.forward * 2 + Vector3.right * leftRightOpeningMult));
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up * 2.0f), transform.TransformDirection(Vector3.forward * 2 + Vector3.right * leftRightOpeningMult));
                        break;

                    case TLinkObstacleAvoidancePrecision.Good:
                        Gizmos.color = (leftOccupied ? Color.red : Color.green);
                        Gizmos.color = (leftOccupied ? Color.red : Color.green);
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up / 3.0f), transform.TransformDirection(Vector3.forward * 2 + Vector3.left * leftRightOpeningMult));
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up * 1.45f), transform.TransformDirection(Vector3.forward * 2 + Vector3.left * leftRightOpeningMult));

                        Gizmos.color = (forwardOccupied ? Color.red : Color.green);
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up / 3.0f), transform.TransformDirection(Vector3.forward * 2));
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up * 1.45f), transform.TransformDirection(Vector3.forward * 2));

                        Gizmos.color = (rightOccupied ? Color.red : Color.green);
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up / 3.0f), transform.TransformDirection(Vector3.forward * 2 + Vector3.right * leftRightOpeningMult));
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up * 1.45f), transform.TransformDirection(Vector3.forward * 2 + Vector3.right * leftRightOpeningMult));
                        break;

                    case TLinkObstacleAvoidancePrecision.Minimum:
                        Gizmos.color = (leftOccupied ? Color.red : Color.green);
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up / 3.0f), transform.TransformDirection(Vector3.forward * 2 + Vector3.left * leftRightOpeningMult));

                        Gizmos.color = (forwardOccupied ? Color.red : Color.green);
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up / 3.0f), transform.TransformDirection(Vector3.forward * 2));

                        Gizmos.color = (rightOccupied ? Color.red : Color.green);
                        GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up / 3.0f), transform.TransformDirection(Vector3.forward * 2 + Vector3.right * leftRightOpeningMult));
                        break;

                    case TLinkObstacleAvoidancePrecision.Null:
                        break;
                }

                if(shouldFollowTargetVector)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(targetVector, new Vector3(.8f, .8f, .8f));

                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.red;
                    Handles.Label(targetVector, "R", style);
                }

                // Ground check
                Gizmos.color = Color.white;

                if(agent != null)
                    GizmosExtension.ArrowForGizmo(agent.transform.position + Vector3.up, Vector3.down);             
            }

            if (debugPath)
            {
                if (navmeshPath != null)
                {
                    Gizmos.color = Color.green;
                    for (int i = 0; i < navmeshPath.corners.Length - 1; i++)
                        Gizmos.DrawLine(navmeshPath.corners[i], navmeshPath.corners[i + 1]);

                    Gizmos.color = Color.gray;
                    for (int i = 0; i < navmeshPath.corners.Length; i++)
                        Handles.Label(navmeshPath.corners[i], new GUIContent(i.ToString()));
                }
            }

        }
#endif
    }
}