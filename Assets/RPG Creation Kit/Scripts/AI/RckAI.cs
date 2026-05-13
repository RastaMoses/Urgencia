using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.BehaviourTree;
using UnityEngine.AI;
using RPGCreationKit.SaveSystem;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Player;
using System;
using RPGCreationKit.PersistentReferences;

namespace RPGCreationKit.AI
{
    [DisallowMultipleComponent]
    public class RckAI : AIBehaviourTreed
    {
        public bool _IS_DEMO_FILE = true;

        public Race race;
        public bool hasBeenInstantiated;

        public MountAI aiMountingByRckAI;
        public MountupPoint mountingPoint;
        public bool isDismounting = false;
        public bool isMounted = false;
        public bool isMountedButUnloadedPREF = false;
        public bool goToSleepLogicEnabled = false;

        Vector3 defaultInteractZonePos;

        private void Reset()
        {
            iGUIDReferences = GetComponent<AIGUIDReferences>();
            m_Anim = GetComponent<Animator>();
            attributes = GetComponent<EntityAttributes>();
            ragdoll = GetComponentInChildren<Ragdoll>();
            aiLookAt = GetComponent<AILookAt>();
            headPos = GetComponentInChildren<AIHeadPos>().transform;
            entityFocusPart = headPos;
            physicalCollider = GetComponent<Collider>();
            interactableCollider = RCKFunctions.GetChildWithName(this.gameObject, "InteractZone").GetComponent<Collider>();
            agent = GetComponent<NavMeshAgent>();
            m_Rigidbody = GetComponent<Rigidbody>();
            movementType = MovementType.UnityNavmesh;
            arriveSteeringBehaviourDeceleration = Deceleration.Normal;
            inventory = GetComponent<Inventory>();
            equipment = GetComponent<Equipment>();
            bodyData = GetComponent<BodyData>();
            defaultWeaponOnHand = RCKFunctions.GetChildWithName(this.gameObject, "DefaultWeapon").GetComponent<WeaponOnHand>();
            defaultWeapon = defaultWeaponOnHand.weaponItem;
            lootingPoint = GetComponentInChildren<LootingPoint>();

            lootingPoint.inventory = inventory;
            lootingPoint.equipment = equipment;

            if(GetComponent<RckAI>())
                onlineComponents.ai = GetComponent<RckAI>();

            onlineComponents.GFX = RCKFunctions.GetChildWithName(this.gameObject, "GFX");
            onlineComponents.rigidbody = m_Rigidbody;
            onlineComponents.animator = m_Anim;
            onlineComponents.iGUIDReferences = iGUIDReferences;
            onlineComponents.agent = agent;
            onlineComponents.audioSource = GetComponent<AudioSource>();
        }



        public void DelayedStart()
        {
            base.Start();

            // Ignore children colliders
            Collider coll = GetComponent<CapsuleCollider>();

            if (coll == null)
                coll = GetComponent<BoxCollider>(); // from 1.7+ colliders are box for NPCs 

            var colliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
                Physics.IgnoreCollision(coll, colliders[i]);

            defaultInteractZonePos = interactableCollider.transform.localPosition;

            attributes.derivedAttributes.CalculateFromAttributes(attributes.attributes, true);

            // Set curLevel appropriately
            attributes.DeriveAndSetLevelFromAttributes();
        }

        public IEnumerator QueueSet(bool _combat, string _treeID)
        {
            if (anyWaitForSwitchRunning)
                yield return null;

            SetNewBehaviourTree(_combat, _treeID);
        }

        [AIInvokable]
        public void SetNewBehaviourTree(bool _combat, string _treeID)
        {
            if (anyWaitForSetRunning)
            {
                StartCoroutine(QueueSet(_combat, _treeID));
                return;
            }

            // Execute OnExit node if present
            if (currentBehaviour != null && currentBehaviour.nodes.Count > 0 && ((currentBehaviour.nodes[0] as BTNode).GetOutputPort("onExitNode").Connection != null && ((currentBehaviour.nodes[0] as BTNode).GetOutputPort("onExitNode").Connection.node != null)))
            {
                anyWaitForSetRunning = true;
                // Execute OnExit before changing the tree
                StartCoroutine(SetNewBehaviourTree_WaitForExitNodeTask(_combat, _treeID));
            }
            else
            {
                RPGCK_BT newBT = BehaviourDatabase.GetItem(_treeID);

                if (_combat)
                {
                    combatBehaviourTree = newBT;
                    combatBehaviourTree.RPGCK_RuntimeNodeAssign(gameObject, this);
                }
                else
                {
                    purposeBehaviourTree = newBT;
                    purposeBehaviourTree.RPGCK_RuntimeNodeAssign(gameObject, this);
                }
            }
        }

        IEnumerator SetNewBehaviourTree_WaitForExitNodeTask(bool _combat, string _treeID)
        {
            pauseBT = true;
            yield return new WaitForEndOfFrame();
            var node = (((currentBehaviour.nodes[0] as BTNode).GetOutputPort("onExitNode").Connection.node) as BTNode);
            node.ReEvaluate(this);
            while (GetNode(node.isInCombatBehavior, node.guidStr).m_NodeState == NodeState.Null || GetNode(node.isInCombatBehavior, node.guidStr).m_NodeState == NodeState.Running)
            {
                node.Execute(this);
                yield return new WaitForEndOfFrame();
            }

            RPGCK_BT newBT = BehaviourDatabase.GetItem(_treeID);

            if (_combat)
            {
                combatBehaviourTree = newBT;
                combatBehaviourTree.RPGCK_RuntimeNodeAssign(gameObject, this);
            }
            else
            {
                purposeBehaviourTree = newBT;
                purposeBehaviourTree.RPGCK_RuntimeNodeAssign(gameObject, this);
            }

            anyWaitForSetRunning = false;
            pauseBT = false;
            yield break;
        }

        [AIInvokable]
        public void SetNewBehaviourTreeToAI(string _toAI, bool _combat, string _treeID)
        {
            // Get the RckAI if possible
            RckAI sTo = null;
            CellsSystem.CellInformation.TryToGetAI(_toAI, out sTo);

            if (sTo != null)
            {
                sTo.SetNewBehaviourTree(_combat, _treeID);
            }
        }


        public bool TryLoadFromSavefile()
        {
            GetMyCellInfoImmediate();
            var allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

            if (allAI.ContainsKey(entityID))
            {
                // Look if it's in the same original cell
                if (startingCellID == allAI[entityID].saveCellID || hasBeenInstantiated)
                {
                    // Load it
                    StartCoroutine(LoadAI(allAI[entityID]));
                }
                else
                {
                    CellInformation.activeCells[myCellInfo.cell.ID].aiInWorld.Remove(entityID);
                    // Destroy this AI, it will be instantiated by CellInfo
                    Destroy(this.gameObject);
                }
            }
            else
            {
                pauseBT = true;

                StartCoroutine(NewAIWaitsForLoad());
                return true;
            }

            return true;
        }

        public bool TryLoadFromSaveFileAsPersistentReference()
        {
            var allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

            if (allAI.ContainsKey(entityID))
            {
                // Load it
                StartCoroutine(LoadAI(allAI[entityID]));
            }
            else
            {
                pauseBT = true;

                StartCoroutine(NewAIWaitsForLoad());
                return true;
            }

            return true;
        }

        public IEnumerator NewAIWaitsForLoad()
        {
            isLoaded = true;

            // Wait for game to start before letting the ai do its job
            while (!CellInformation.AllActiveCellsLoaded() || WorldManager.instance.isLoading)
                yield return null;

            GetMyCellInfoImmediate();

            pauseBT = false;
        }

        public IEnumerator LoadAI(AISaveData data)
        {
            yield return new WaitForEndOfFrame();
            pauseBT = true;
            yield return new WaitForEndOfFrame();

            agent.enabled = false;

            startingCellID = data.startingCellID;

            transform.position = data.position;
            transform.rotation = data.rotation;

            // Load Entity Attributes
            attributes.attributes = data.aiAttributes.attributes;
            attributes.derivedAttributes = data.aiAttributes.derivedAttributes;
            attributes.ExecuteEffects(data.aiAttributes.activeEffects.ToArray());
            attributes.derivedAttributes.CalculateFromAttributes(attributes.attributes, true);
            ResetRecover();

            inventory.ClearInventoryAndEquipment();

            equipment.OnEquipmentChanges();
            OnEquipmentChangesHands();
            OnEquipmentChangesAmmo();

            // Those items gets equipped after everyone else, used for the shield
            List<ItemInInventory> toEquipAfter = new List<ItemInInventory>();

            for(int i = 0; i < data.aiInventory.items.Count; i++)
            {
                var item = data.aiInventory.items[i];
                var aItem = inventory.AddItem(item.externalItemID, item.Amount);
                aItem.metadata = data.aiInventory.items[i].metadata;

                if (item.isEquipped)
                {
                    if (aItem.item.itemType == ItemTypes.ArmorItem && (RCKFunctions.ContainsBiped(((ArmorItem)(aItem.item)).Bipeds, BipedObject.Shield) || RCKFunctions.ContainsBiped(((ArmorItem)(aItem.item)).Bipeds, BipedObject.Torch)))
                    {
                        // Delay the equipment of shields
                        toEquipAfter.Add(aItem);
                    }
                    else
                        equipment.Equip(aItem);
                }
            }

            equipment.OnEquipmentChanges();
            OnEquipmentChangesHands();
            OnEquipmentChangesAmmo();

            // Equip delayed
            for(int i = 0; i < toEquipAfter.Count; i++)
            {
                equipment.Equip(toEquipAfter[i]);
            }

            inventory.InitializeInventory();

            GetMyCellInfoImmediate();

            // Load important states
            isEssential = data.isEssential;

            if (!data.isAlive)
            {
                Die();

                if (data.usesRagdoll)
                {
                    ragdoll.ForceRagdoll();

                    // Load hips pos/rot
                    ragdoll.LoadFromSaveData(data.ragdollSaveData);
                }
            }
            else // Entity is alive
            {
                
                followTargetOutsideOfCell = data.followTargetOutsideOfCell;

                // Load Behaviour Trees

                if (!string.IsNullOrEmpty(data.purposeBehaviourTreeID))
                {
                    purposeBehaviourTree = BehaviourDatabase.GetItem(data.purposeBehaviourTreeID);
                    purposeBehaviourTree.RPGCK_RuntimeNodeAssign(gameObject, this);
                }

                if (!string.IsNullOrEmpty(data.combatBehaviourTreeID))
                {
                    combatBehaviourTree = BehaviourDatabase.GetItem(data.combatBehaviourTreeID);
                    combatBehaviourTree.RPGCK_RuntimeNodeAssign(gameObject, this);
                }

                SwitchBehaviourTree(!data.isUsingPurposeBehaviour);


                // Movements
                lookAtVector3IfStopped = data.lookAtVector3IfStopped;
                vector3ToLookAtIfStopped = data.vector3ToLookAtIfStopped;

                // Load perception
                radius = data.radius;
                sphereYOffset = data.sphereYoffset;
                sphereForwardOffset = data.sphereZoffset;

                // Load Combat state nad entities fighting
                if (data.isInCombat || data.enterInCombatCalled)
                {
                    for(int i = 0; i < data.entityHesFighting.Count; i++)
                    {
                        if(data.entityHesFighting[i] == "PLAYER_ID")
                        {
                            // Add the player
                            enemyTargets.Add(new VisibleEnemy(RckPlayer.instance, RckPlayer.instance.playerAttributes, new AggroInfo(50)));
                        }
                        else
                        {
                            RckAI ai = null;
                            CellInformation.activeCells[myCellInfo.cell.ID].aiInWorld.TryGetValue(data.entityHesFighting[i], out ai);

                            if (ai != null)
                                enemyTargets.Add(new VisibleEnemy(ai, ai.GetComponent<EntityAttributes>(), new AggroInfo(50)));
                        }
                    }

                    if (enemyTargets.Count > 0)
                        EnterInCombatAgainst(enemyTargets[0].m_entity);
                }

                // Load Dialogues
                if(!string.IsNullOrEmpty(data.currentDialogueID))
                    currentDialogueGraph = DialoguesDatabase.GetItem(data.currentDialogueID);

                if (!string.IsNullOrEmpty(data.previousDialogueID))
                    previousDialogueGraph = DialoguesDatabase.GetItem(data.previousDialogueID);

                if (!string.IsNullOrEmpty(data.defaultDialogueID))
                    defaultDialogueGraph = DialoguesDatabase.GetItem(data.defaultDialogueID);

                dialogueSystemEnabled = data.dialogueSystemEnabled;

                // Load MainTarget
                if(!string.IsNullOrEmpty(data.mainTargetID))
                {
                    ITargetableType targetableType = (ITargetableType)data.mainTargetType;

                    switch (targetableType)
                    {
                        case ITargetableType.Entity:
                            if (data.mainTargetID == "PLAYER_ID")
                                SetTarget(RckPlayer.instance.gameObject);
                            else
                            {
                                RckAI entity = null;
                                CellInformation.TryToGetAI(data.mainTargetID, out entity);

                                if (entity != null)
                                    SetTarget(entity.gameObject);
                            }

                        break;
                    }
                }

                if(!string.IsNullOrEmpty(data.cellIDMovingTo))
                {
                    MoveToCell(data.cellIDMovingTo);
                }

                // Load PurposeState
                purposeState = data.purposeState;

                // Try to solve purpose ref
                if (!string.IsNullOrEmpty(purposeState.ITargetableID))
                {
                    ITargetableType targetableType = (ITargetableType)data.purposeState.ITargetableType;

                    switch (targetableType)
                    {
                        case ITargetableType.Entity:
                            if (data.mainTargetID == "PLAYER_ID")
                                purposeState.AssignPurpose(this, RckPlayer.instance.gameObject, data.purposeState.clearsOn, data.purposeState.clearsOnData, data.purposeState.nextPurposeBehaviourID);
                            else
                            {
                                RckAI entity = null;
                                CellInformation.TryToGetAI(data.mainTargetID, out entity);

                                if (entity != null)
                                    purposeState.AssignPurpose(this, entity.gameObject, data.purposeState.clearsOn, data.purposeState.clearsOnData, data.purposeState.nextPurposeBehaviourID);
                            }

                            break;

                        case ITargetableType.Door:
                            purposeState.AssignPurpose(this, GetTransformToDoor(purposeState.ITargetableExtraData).gameObject, data.purposeState.clearsOn, data.purposeState.clearsOnData, data.purposeState.nextPurposeBehaviourID);
                            break;

                        case ITargetableType.PRef:
                            purposeState.AssignPurpose(this, PersistentReferences.PersistentReferenceManager.instance.refs[purposeState.ITargetableID].gameObject, data.purposeState.clearsOn, data.purposeState.clearsOnData, data.purposeState.nextPurposeBehaviourID);
                            break;

                        case ITargetableType.Transform:
                            if(myCellInfo.allTargetables.ContainsKey(purposeState.ITargetableID))
                                purposeState.AssignPurpose(this, myCellInfo.allTargetables[purposeState.ITargetableID].gameObject, data.purposeState.clearsOn, data.purposeState.clearsOnData, data.purposeState.nextPurposeBehaviourID);
                            break;

                        case ITargetableType.AIPath:
                            if (myCellInfo != null && myCellInfo.allAIPaths.ContainsKey(purposeState.ITargetableID))
                                purposeState.AssignPurpose(this, myCellInfo.allAIPaths[purposeState.ITargetableID].gameObject, data.purposeState.clearsOn, data.purposeState.clearsOnData, data.purposeState.nextPurposeBehaviourID);


                            break;
                    }

                    if (data.isUsingPurposeBehaviour)
                        purposeState.ResumePurpose();
                }
                else
                    purposeState.ClearPurpose();

                // Move to door position if he entered a door
                if (data.enteredInADoorLastTime)
                {
                    if (!data.enteredInADoorLastTimeOverridePos)
                    {
                        if (!string.IsNullOrEmpty(data.doorEnteredID))
                        {
                            // TODO If he entered < 5 minutes ago bring him to the teleport mark 
                            Door door = CellInformation.activeCells[myCellInfo.cell.ID].allDoors[data.doorEnteredID];
                            transform.position = door.teleportMarker.position;

                            // Otherwise if he entered > 5 minutes ago randomize his position
                        }
                        else
                        {
                            // just telport him to the cell entry
                            transform.position = myCellInfo.cellEntry.transform.position;
                            transform.rotation = myCellInfo.cellEntry.transform.rotation;
                        }
                    }
                    else
                    {
                        transform.position = data.enteredDoorLastTimePos;
                        transform.rotation = data.enteredDoorLastTimeRot;

                        // Reset boolean
                        enteredInADoorLastTime = false;
                    }
                }

                if(data.enteredInADoorLastTimeOverridePos)
                {
                    enteredInADoorLastTimeOverridePos = data.enteredInADoorLastTimeOverridePos;
                    enteredDoorLastTimePos = data.enteredDoorLastTimePos;
                    enteredDoorLastTimeRot = data.enteredDoorLastTimeRot;
                }

                if(data.isFollowingAIPath)
                {
                    shouldFollowAIPath = true;
                    aiPathCurrentIndex = data.aiPathCurrentIndex;
                    aiPathResumeFromCurrentIndex = true;
                    invertAIPath = data.invertAIPath;

                    FollowCurrentPath();
                }

                SetOverrideShouldFollowMainTarget(data.overrideShouldFollowMainTargetActive, data.overrideShouldFollowMainTarget);

                // Load factions
                for (int i = 0; i < data.allFactions.Count; i++)
                    AddToFaction(data.allFactions[i]);
            }

            // Assign runtime cell
            if(myCellInfo != null)
                runtimeStartingCell = myCellInfo.cell.ID;

            agent.enabled = true;

            // PDT
            if (RCKSettings.GetSaveVersion(SaveSystemManager.instance.saveFile.SaveFileData.fileVersion) >= 18) // 18 means 1.8
            {
                pdtCounting = data.pdtCounting;
                pdtCurCount = data.pdtCurCount;
                pdtCurTimer = data.pdtCurTimer;
            }

            isLoaded = true;

            // Wait for game to start before letting the ai do its job
            while (!CellInformation.AllActiveCellsLoaded() || WorldManager.instance.isLoading)
                yield return null;

            if (isPersistentReference)
                PRefCycleOnce();


            // Mounting state
            if (RCKSettings.GetSaveVersion(SaveSystemManager.instance.saveFile.SaveFileData.fileVersion) >= 18 && // 18 means 1.8
                data.isMounted) 
            {
                
                RckAI ai = null;
                CellInformation.TryToGetAI(data.mountedEntityID, out ai);

                if (ai != null)
                {
                    MountAI mount = (MountAI)ai;

                    // Determine the closest mounting point
                    float closestDistance = float.MaxValue;
                    int closestIndex = -1;
                    for (int i = 0; i < mount.mountPoints.Length; i++)
                    {
                        float curDist = Vector3.Distance(this.transform.position, mount.mountPoints[i].transform.position);

                        if (curDist < closestDistance)
                        {
                            closestDistance = curDist;
                            closestIndex = i;
                        }
                    }

                    if (closestIndex != -1)
                    {
                        MountupPoint mountPoint = mount.mountPoints[closestIndex];
                        actionPoint = mountPoint;
                    }
                }

                if (isPersistentReference && isInOfflineMode)
                {
                    PersistentReference npcRef = PersistentReferenceManager.instance.GetPersistentReference(data.mountedEntityID);

                    if (npcRef.GetComponent<MountAI>())
                    {
                        isMountedButUnloadedPREF = true;
                        mountUsingPreviously = npcRef.GetComponent<MountAI>();
                    }
                }
            }

            pauseBT = false;

            yield return null;
        }

        /// <summary>
        /// THis method is called when the AI is being unloaded because the cell is too far away from the Player
        /// </summary>
        public void OnBeingUnloaded(CellInformation calledBy)
        {
            if (calledBy == myCellInfo) // if the ai is in the cell that is being unloaded
            {
                if (teleportIfUnloadedWhileGoingToDoor)
                {
                    if (isReachingDoor)
                    {
                        isReachingDoor = false;
                        enteredInADoorLastTime = true;

                        TeleportToCell(IsReachingCellID);
                    }
                }
            }
        }


        public void SaveOnFile()
        {
            if (isInOfflineMode)    // AI shouldn't be saved in offline mode
                return;

            GetMyCellInfoImmediate();

            string saveCellID = myCellInfo.cell.ID;

            if (!string.IsNullOrEmpty(cellIDOfLastSaved) && cellIDOfLastSaved != saveCellID)
            {
                // If the AI was loaded in a different cell from the starting point 

                // remove cellidoflastsaved

                var dict = SaveSystemManager.instance.saveFile.AIData.aiCellDictionary;

                if (dict.ContainsKey(cellIDOfLastSaved))
                    dict[cellIDOfLastSaved].allIDs.Remove(entityID);
            }

            cellIDOfLastSaved = saveCellID;

            var allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

            // Save this AI to the allAI dictionary
            if (allAI.ContainsKey(entityID))
            {
                // Update entry
                allAI[entityID] = ToSaveData();
            }
            else // create
                allAI.Add(entityID, ToSaveData());


            // Save/Update/DoNothing on the aiCellDictionary
            var allCellAI = SaveSystemManager.instance.saveFile.AIData.aiCellDictionary;

            // If the AI was loaded in a different cell from the starting point 
            if(runtimeStartingCell != saveCellID)
            {
                if (allCellAI.ContainsKey(runtimeStartingCell))
                {
                    if (allCellAI[runtimeStartingCell].allIDs.Contains(entityID))
                    {
                        allCellAI[runtimeStartingCell].allIDs.Remove(entityID);
                    }
                }

                // Add to the new one
                if (!allCellAI.ContainsKey(saveCellID))
                {
                    allCellAI.Add(saveCellID, new AIIDList());
                    allCellAI[saveCellID].allIDs.Add(entityID);
                }
                else
                {
                    if (!allCellAI[saveCellID].allIDs.Contains(entityID))
                    {
                        allCellAI[saveCellID].allIDs.Add(entityID);
                    }
                }

                transform.SetParent(CellInformation.activeCells[saveCellID].aiContainer);

                if(!CellInformation.activeCells[saveCellID].aiInWorld.ContainsKey(entityID))
                    CellInformation.activeCells[saveCellID].aiInWorld.Add(entityID, this);
            }
            else
            {
                if (!allCellAI.ContainsKey(runtimeStartingCell))
                {
                    allCellAI.Add(runtimeStartingCell, new AIIDList());
                    allCellAI[runtimeStartingCell].allIDs.Add(entityID);
                }
                else
                {
                    if (hasBeenInstantiated && !allCellAI[runtimeStartingCell].allIDs.Contains(entityID))
                    {
                        Debug.Log("Fixing save for " + entityID);
                        allCellAI[runtimeStartingCell].allIDs.Add(entityID);
                    }
                }
            }
        }

        public void SaveOnFileAsPersistentReference()
        {
            var allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

            // Save this AI to the allAI dictionary
            if (allAI.ContainsKey(entityID))
            {
                // Update entry
                allAI[entityID] = ToSaveData();
            }
            else // create
                allAI.Add(entityID, ToSaveData());
        }

        public void SaveOnFile_JustInstantaited()
        {
            var allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

            // Save this AI to the allAI dictionary
            if (allAI.ContainsKey(entityID))
            {
                // Update entry
                allAI[entityID] = ToSaveData();
            }
            else // create
                allAI.Add(entityID, ToSaveData());

            // Update CellAI dictionary
            var aiCell = SaveSystem.SaveSystemManager.instance.saveFile.AIData.aiCellDictionary;
            if (aiCell.ContainsKey(cellIDOfLastSaved))
            {
                // Update it
                var res = aiCell[cellIDOfLastSaved];

                if (!res.allIDs.Contains(entityID))
                {
                    res.allIDs.Add(entityID);
                    aiCell[cellIDOfLastSaved] = res;
                }
            }
            else
            {
                AIIDList list = new AIIDList();
                list.allIDs.Add(entityID);
                // Create new entry
                aiCell.Add(cellIDOfLastSaved, list);
            }
        }

        public AISaveData ToSaveData()
        {
            AISaveData newData = new AISaveData()
            {
                position = transform.position,
                rotation = transform.rotation,
                aiAttributes = attributes.ToSaveData(),
                aiInventory = inventory.ToSaveData(),
                isAlive = isAlive,
                isHostile = isHostile,
                isHostileAgainstPC = isHostileAgainstPC,
                isInCombat = isInCombat,
                isFleeing = isFleeing,
                isUnconscious = isUnconscious,
                enterInCombatCalled = enterInCombatCalled,
                isEssential = isEssential,
                usesRagdoll = usesRagdoll,
                dialogueSystemEnabled = dialogueSystemEnabled,
                currentDialogueID = (currentDialogueGraph) ? currentDialogueGraph.ID : string.Empty,
                previousDialogueID = (previousDialogueGraph) ? previousDialogueGraph.ID : string.Empty,
                defaultDialogueID = (defaultDialogueGraph) ? defaultDialogueGraph.ID : string.Empty,
                isInConversation = isInConversation,
                isTalking = isTalking,
                isListening = isListening,
                mustSpeakToPlayer = mustSpeakToPlayer,
                mustSpeakToEntity = mustSpeakToEntity,
                speakerIndex = speakerIndex,
                usesMovements = usesMovements,
                usesOfflineMode = usesOfflineMode,
                usesTlinkUnstuck = usesTlinkUnstuck,
                isWalking = isWalking,
                canSeeTarget = canSeeTarget,
                hasCompletlyLostTarget = hasCompletlyLostTarget,
                shouldFollowMainTarget = shouldFollowMainTarget,
                shouldFollowOnlyIfCanSee = shouldFollowOnlyIfCanSee,
                lookAtTarget = lookAtTarget,
                dialogueLookAt = dialogueLookAt,
                shouldFollowAIPath = shouldFollowAIPath,
                shouldWander = shouldWander,
                shouldUseNPCActionPoint = shouldUseNPCActionPoint,
                hasMainTarget = hasMainTarget,
                shouldFollowTargetVector = shouldFollowTargetVector,
                //aiCustomPathID = aiPath.ID,
                //actionPointID = actionPoint.ID,
                fleeAtDistanceValue = fleeAtDistanceValue,
                selectedSteeringBehaviour = (int)selectedSteeringBehaviour,
                arriveSteeringBehaviourDeceleration = (int)arriveSteeringBehaviourDeceleration,
                invertAIPath = invertAIPath,
                aiPathCurrentIndex = aiPathCurrentIndex,
                isFollowingAIPath = isFollowingAIPath,
                stoppingHalt = stoppingHalt,
                stoppingDistance = stoppingDistance,
                generalRotationSpeed = generalRotationSpeed,
                cachedMainTarget = cachedMainTarget,
                cachedMainTargetPos = cachedMainTargetPos,
                hasToRandomlySearchTarget = hasToRandomlySearchTarget,
                isReachingActionPoint = isReachingActionPoint,
                fixingInPlaceForActionPoint = fixingInPlaceForActionPoint,
                playingEnterAnimationActionPoint = playingEnterAnimationActionPoint,
                isUsingActionPoint = isUsingActionPoint,
                allowsLootWhenDead = allowsLootWhenDead,
                allowsLootOfEquipment = allowsLootOfEquipment,
                perceptionEnabled = perceptionEnabled,
                checkTick = checkTick,
                isDrawingWeapon = isDrawingWeapon,
                weaponDrawn = weaponDrawn,
                treeTickRate = (int)tickRate,
                xFrames = xFrames,
                xSeconds = xSeconds,
                useBT = useBT,
                pauseBT = pauseBT,
                curBehaviourTreeID = (currentBehaviour) ? currentBehaviour.ID : string.Empty,
                purposeBehaviourTreeID = (purposeBehaviourTree) ? purposeBehaviourTree.ID : string.Empty,
                combatBehaviourTreeID = (combatBehaviourTree) ? combatBehaviourTree.ID : string.Empty,
                keepTicking = keepTicking,
                startingCellID = startingCellID,
                saveCellID = (myCellInfo != null) ? myCellInfo.cell.ID : cellIDOfLastSaved,
                runtimeSaveCellID = runtimeStartingCell,
                hipsPosition = bodyData.hips.transform.position,
                hipsRotation = bodyData.hips.transform.rotation,
                isUsingPurposeBehaviour = isUsingPurposeBehaviour,
                cellIDMovingTo = IsReachingCellID,
                enteredInADoorLastTime = enteredInADoorLastTime,
                doorEnteredID = doorEnteredID,
                overrideShouldFollowMainTarget = overrideShouldFollowMainTarget,
                overrideShouldFollowMainTargetActive = overrideShouldFollowMainTargetActive,
                enteredInADoorLastTimeOverridePos = enteredInADoorLastTimeOverridePos,
                enteredDoorLastTimePos = enteredDoorLastTimePos,
                enteredDoorLastTimeRot = enteredDoorLastTimeRot,
                radius = radius,
                sphereYoffset = sphereYOffset,
                sphereZoffset = sphereForwardOffset,
                lookAtVector3IfStopped = lookAtVector3IfStopped,
                vector3ToLookAtIfStopped = vector3ToLookAtIfStopped,
                followTargetOutsideOfCell = followTargetOutsideOfCell,
                pdtCounting = pdtCounting,
                pdtCurTimer = pdtCurTimer,
                pdtCurCount = pdtCurCount,
                isMounted = isMounted
            };
            attributes.RestoreAddsAfterSave(); // fix save bug TODO: allow ai to use consumables

            if (newData.isMounted)
                newData.mountedEntityID = aiMountingByRckAI.entityID;
            else
                newData.mountedEntityID = string.Empty;

            // Save other data
            if (usesRagdoll)
                newData.ragdollSaveData = ragdoll.ToSaveData();

            // Save factions
            for (int i = 0; i < belongsToFactions.Count; i++)
                newData.allFactions.Add(belongsToFactions[i].ID);

            newData.entityHesFighting = new List<string>();
            for (int i = 0; i < enemyTargets.Count; i++)
                newData.entityHesFighting.Add(enemyTargets[i].m_entity.entityID);

            // See if we can save Target
            if(mainTarget != null)
            {
                ITargetable targetable = mainTarget.GetComponent<ITargetable>();

                if(targetable != null)
                {
                    newData.mainTargetID = targetable.GetID();
                    newData.mainTargetType = (int)targetable.GetTargetableType();
                }
                else
                    newData.mainTargetID = string.Empty;
            }
            else 
                newData.mainTargetID = string.Empty;


            // Save PurposeState
            newData.purposeState = purposeState;


            return newData;
        }

        public void DestroyThis()
        {
            SaveOnFile();
            myCellInfo.aiInWorld.Remove(entityID);
            Destroy(this.gameObject);
        }

        // Travel doors
        public bool teleportIfUnloadedWhileGoingToDoor = true;
        public bool teleportIfWalkableSurfaceEnds = true;

        public bool isReachingDoor;
        public Door doorIsReaching;
        public string IsReachingCellID;
        public bool enteredInADoorLastTime;
        public bool enteredInADoorLastTimeOverridePos;
        public Vector3 enteredDoorLastTimePos;
        public Quaternion enteredDoorLastTimeRot;
        public string doorEnteredID;

        public string aiwuhje;
        public bool forceRagdollEveryFrame = false;
        bool didAForcedRagdoll;

        [ContextMenu("GO TO CELL")]
        public void ae()
        {
            // Assign Purpose

            MoveToCell(aiwuhje);
            SetNewBehaviourTree(false, "BTP_FollowMainTargetOnly");
            SwitchBehaviourTree(false);

            purposeState.AssignPurpose(this, mainTarget.gameObject, PurposeClearTypes.ClearOnTeleportToCell, new PurposeStateClearsOnData("FarmHouseT"), "BTP_RoamRelax001");
        }

        public MountAI mountUsingPreviously;
        public override void Update()
        {
            base.Update();

            if (!isAlive && forceRagdollEveryFrame && !didAForcedRagdoll)
            {
                ragdoll.ForceRagdoll();
                didAForcedRagdoll = true;
            }

            if (followTargetOutsideOfCell)
                FollowTargetOutsideOfCell();

            if (!isAlive && agent.enabled)
                agent.enabled = false;

            // Check if is mounting and mount died, if so, dismount
            if (isMounted && !aiMountingByRckAI.isAlive && !isDismounting)
            {
                if (isUsingActionPoint && !leavingActionPoint)
                    StopUsingNPCActionPoint();
            }

            // Always make sure this RckAI is a child of the mount he's using, it does change (for example, after saving while the NPC has traveled another cell)
            if (isMounted && transform.parent != aiMountingByRckAI.transform)
                transform.SetParent(aiMountingByRckAI.transform);

            if (isMounted && agent.enabled)
                agent.enabled = false;


            // Pref edge case
            if(isPersistentReference && wantsToMountOnPoint && isInOfflineMode && !isMountedButUnloadedPREF && !isMounted)
            {
                if(!mountUsingPreviously)
                    mountUsingPreviously = ((MountupPoint)actionPoint).mount;

                transform.position = mountUsingPreviously.transform.position - (-mountUsingPreviously.transform.forward * 3.0f);
            }

            // Patch up the situation where a persistent reference ai gets loaded when it was saved on a mount
            if(isMountedButUnloadedPREF)
            {
                pauseBT = true;
                transform.position = mountUsingPreviously.transform.position - (-mountUsingPreviously.transform.forward * 3.0f);

                if(!isInOfflineMode) // if he got back online
                {
                    pauseBT = false;
                    isMountedButUnloadedPREF = false;
                }
            }
        }

        public static Transform GetTransformToDoor(string _cellID)
        {
            bool doorFound = false;

            // Lookup the dictionary that contains "Cell -> DoorsToThatCell"
            var pref = PersistentReferences.PersistentReferenceManager.instance;

            if (pref.cellsDoorsDictionary.ContainsKey(_cellID))
            {
                string doorID = pref.cellsDoorsDictionary[_cellID];

                if (pref.cellsDoorsDictionary.ContainsKey(_cellID) && pref.refs.ContainsKey(doorID))
                {
                    return pref.refs[doorID].transform;
                }
            }
            else
            {
                // The AI has to move to a door (or a PRef) that will bring him there
                Door fDoor = null;

                // Foreach active cell
                foreach (KeyValuePair<string, CellInformation> cellinfo in CellInformation.activeCells)
                {
                    // For each Door in the current cell
                    foreach (KeyValuePair<string, Door> door in cellinfo.Value.allDoors)
                    {
                        if (door.Value.teleports && door.Value.toCell != null && door.Value.toCell.ID == _cellID)
                        {
                            // We found the door in the active cells
                            doorFound = true;
                            fDoor = door.Value;
                            return fDoor.transform;
                        }
                    }

                    if (doorFound)
                        break;
                }
            }

            if (!doorFound) // Door is not in active cells, so look for pref otherwise teleport
            {
                Debug.Log("Tried to GetTransformToDoor: " + _cellID + " | But couldn't be found because it's unloaded and there is no Persistent Reference to that door.");
            }

            return null;
        }

        public void MoveToCell(string _cellID, Vector3 onceEnteredPos, Quaternion onceEnteredRot)
        {
            enteredInADoorLastTimeOverridePos = true;
            enteredDoorLastTimePos = onceEnteredPos;
            enteredDoorLastTimeRot = onceEnteredRot;

            MoveToCell(_cellID);
        }

        /// <summary>
        /// Says to the AI to find a door and move to the desired cell after 
        /// </summary>
        public void MoveToCell(string _cellID)
        {
            bool doorFound = false;

            // Lookup the dictionary that contains "Cell -> DoorsToThatCell"
            var pref = PersistentReferences.PersistentReferenceManager.instance;

            if (pref.cellsDoorsDictionary.ContainsKey(_cellID))
            {
                string doorID = pref.cellsDoorsDictionary[_cellID];

                if (pref.cellsDoorsDictionary.ContainsKey(_cellID) && pref.refs.ContainsKey(doorID))
                {
                    doorIsReaching = null; // Door is reaching is null because we're following a persistent reference transform
                                           // it will be assigned later, when the AI reaches the door

                    PersistentReferences.PersistentReference TToDoor = pref.refs[doorID];
                    IsReachingCellID = _cellID;

                    doorFound = true;
                    toDoorTransform = TToDoor.transform;
                    isReachingDoor = true;
                    SetTarget(TToDoor.gameObject);
                    StartCoroutine(MovingToCell(_cellID, doorID));
                }
            }
            else
            {
                // The AI has to move to a door (or a PRef) that will bring him there
                Door fDoor = null;

                // Foreach active cell
                foreach (KeyValuePair<string, CellInformation> cellinfo in CellInformation.activeCells)
                {
                    // For each Door in the current cell
                    foreach (KeyValuePair<string, Door> door in cellinfo.Value.allDoors)
                    {
                        if (door.Value.teleports && door.Value.toCell != null && door.Value.toCell.ID == _cellID)
                        {
                            // We found the door in the active cells
                            doorFound = true;
                            fDoor = door.Value;
                            IsReachingCellID = fDoor.toCell.ID;
                            doorEnteredID = fDoor.linkedDoorObjRef;
                            break;
                        }
                    }

                    if (doorFound)
                        break;
                }

                // Door was in active cells
                if (doorFound && fDoor != null)
                {
                    isReachingDoor = true;
                    doorIsReaching = fDoor;

                    toDoorTransform = doorIsReaching.groundPivot;

                    SetTarget(doorIsReaching.groundPivot.gameObject);
                    StartCoroutine(MovingToCell(_cellID));
                }
            }
           
            if(!doorFound) // Door is not in active cells, so look for pref otherwise teleport
            {
                Debug.Log("The AI can't get to Cell: " + _cellID + " | Because it's unloaded and there is no Persistent Reference to that door.");
            }
        }

        bool wasDsEnabled = false;
        public Transform toDoorTransform;
        IEnumerator MovingToCell(string _cellID, string doorID = null)
        {
            // Wait until the AI reaches the door, the Teleport to the other cell
            isReachingDoor = true;

            while (Vector3.Distance(transform.position, toDoorTransform.position) > stoppingDistance || m_isInCombat)
            {
                yield return null;
            }

            wasDsEnabled = dialogueSystemEnabled;
            dialogueSystemEnabled = false;
            m_Anim.CrossFade("OpenDoor", 0.1f);
            yield return new WaitForSeconds(m_Anim.GetCurrentAnimatorClipInfo(0).Length);

            if (doorIsReaching == null && doorID != null)
            {
                if (myCellInfo.allDoors.ContainsKey(doorID))
                {
                    // Assign the door and the linkedDoorObjRef
                    doorIsReaching = myCellInfo.allDoors[doorID];
                    doorEnteredID = doorIsReaching.linkedDoorObjRef;
                }
            }

            isReachingDoor = false;
            IsReachingCellID = string.Empty;
            enteredInADoorLastTime = true;
            dialogueSystemEnabled = true;

            SaveOnFile();

            // We reached the door
            TeleportToCell(_cellID);

            yield return null;
        }

        public void TeleportToCell(string _cellID)
        {
            //// PURPOSE_CALLBACK: ClearOnTeleportToCell \\\\\
            if (purposeState != null && purposeState.IsAssigned && purposeState.clearsOn == PurposeClearTypes.ClearOnTeleportToCell)
            {
                if (purposeState.clearsOnData.stringData == _cellID)
                {
                    // Fix for when AI is teleported abruptely during opening door animation
                    if (wasDsEnabled)
                    {
                        dialogueSystemEnabled = true;
                        wasDsEnabled = false;
                    }

                    // We did it
                    purposeState.CompletePurpose();
                }
            }

            var aiData = SaveSystemManager.instance.saveFile.AIData;

            // Check if cellDictionary contains the cell we're putting this npc into
            if (aiData.aiCellDictionary.ContainsKey(_cellID))
            {
                if (!aiData.aiCellDictionary[_cellID].allIDs.Contains(entityID))
                    aiData.aiCellDictionary[_cellID].allIDs.Add(entityID);
            }
            else
            {
                aiData.aiCellDictionary.Add(_cellID, new AIIDList());
                aiData.aiCellDictionary[_cellID].allIDs.Add(entityID);
            }

            // Manually set the Cell and coor and manually save
            string oldSaveCell = myCellInfo.cell.ID;
            myCellInfo.aiInWorld.Remove(entityID);
            
            AISaveData thisAIData = ToSaveData();
            
            thisAIData.isReachingDoor = false;
            thisAIData.cellIDMovingTo = string.Empty;
            thisAIData.saveCellID = _cellID;
            thisAIData.position = new Vector3(0, 0, 0);

            // If the AI has never been saved we can safely move it to the new cell
            if (!aiData.aiDictionary.ContainsKey(entityID))
                aiData.aiDictionary.Add(entityID, thisAIData);
            else
            {
                aiData.aiDictionary[entityID] = thisAIData;

                // Remove it from his old aiCellDicitonary
                if (aiData.aiCellDictionary.ContainsKey(oldSaveCell))
                    aiData.aiCellDictionary[oldSaveCell].allIDs.Remove(entityID);
            }


            myCellInfo.aiInWorld.Remove(entityID);

            // If the cell is loaded (or cached, move this gameobject, otherwise destroy it, it will be instantiated next time the cell will be loaded)
            if (CellInformation.activeCells.ContainsKey(_cellID))
                CellInformation.activeCells[_cellID].AddToAIToInstantiateIfCached(entityID);

            Destroy(this.gameObject);
        }

        public bool followTargetOutsideOfCell = false;
        /// <summary>
        /// This function detects if the AI was following its target and that target changed cell, if its enabled, this AI will be teleported where the 
        /// </summary>
        public void FollowTargetOutsideOfCell()
        {
            // If its the player
            if(isAlive && shouldFollowMainTarget && mainTarget != null && mainTarget.CompareTag("Player") && 
                myCellInfo != null && WorldManager.instance != null)
            {
                // Perform the check
                if(myCellInfo.cell != null && myCellInfo.cell.ID != WorldManager.instance.currentCenterCell.ID && this.isInOfflineMode)
                {
                    myCellInfo.aiInWorld.Remove(this.entityID);

                    // Teleport out
                    transform.SetParent(CellInformation.activeCells[WorldManager.instance.currentCenterCell.ID].aiContainer);

                    CellInformation.activeCells[WorldManager.instance.currentCenterCell.ID].aiInWorld.Add(entityID, this);
                    myCellInfo = CellInformation.activeCells[WorldManager.instance.currentCenterCell.ID];

                    transform.position = (mainTarget.position + mainTarget.forward) + (transform.right * UnityEngine.Random.Range(-1,1));
                    onlineComponents.OnGoingOnline();

                    SaveOnFile();
                }
            }
        }

        public void MountOnMountAI(MountAI ai, MountupPoint pointMounting)
        {
            aiMountingByRckAI = ai;
            mountingPoint = pointMounting;
            StartCoroutine(MountOnMountAITask());
        }

        public IEnumerator MountOnMountAITask()
        {
            // Ignore collision between ai->mount
            Physics.IgnoreCollision(physicalCollider, aiMountingByRckAI.GetComponent<Collider>(), true);

            for (int i = 0; i < aiMountingByRckAI.extraColliders.Count; i++)
            {
                Physics.IgnoreCollision(physicalCollider, aiMountingByRckAI.extraColliders[i], true);

                if (equipment.isUsingShield)
                {
                    Physics.IgnoreCollision(equipment.currentShieldObject.GetComponent<Collider>(), aiMountingByRckAI.GetComponent<Collider>(), true);
                    Physics.IgnoreCollision(equipment.currentShieldObject.GetComponent<Collider>(), aiMountingByRckAI.extraColliders[i], true);
                }
            }

            isMounted = true;
            physicalCollider.enabled = false;

            // Set parent
            transform.SetParent(aiMountingByRckAI.transform);

            // Move interact zone
            interactableCollider.transform.position = aiMountingByRckAI.interactZoneOffsetRckAI.position;

            pauseBT = true;

            yield return new WaitForSeconds(mountingPoint.npcAction.length);

            aiMountingByRckAI.pauseBT3 = false;

            yield return null;
        }
        public void DismountFromMountAI(MountAI ai, MountupPoint pointMounting)
        {
            isDismounting = true;
            StartCoroutine(DismountOnMountAITask());
        }

        public IEnumerator DismountOnMountAITask()
        {
            // Revert Ignore collision between ai->mount
            Physics.IgnoreCollision(physicalCollider, aiMountingByRckAI.GetComponent<Collider>(), false);

            for (int i = 0; i < aiMountingByRckAI.extraColliders.Count; i++)
            {
                Physics.IgnoreCollision(physicalCollider, aiMountingByRckAI.extraColliders[i], false);
            }

            isMounted = false;
            physicalCollider.enabled = true;

            // Set parent
            transform.SetParent(null);
            interactableCollider.transform.localPosition = defaultInteractZonePos;

            yield return new WaitForSeconds(mountingPoint.npcActionReturn.length);

            pauseBT = false;

            if (equipment.isUsingShield)
            {
                for (int i = 0; i < aiMountingByRckAI.extraColliders.Count; i++)
                {
                    Physics.IgnoreCollision(equipment.currentShieldObject.GetComponent<Collider>(), aiMountingByRckAI.GetComponent<Collider>(), false);
                    Physics.IgnoreCollision(equipment.currentShieldObject.GetComponent<Collider>(), aiMountingByRckAI.extraColliders[i], false);
                }
            }

            aiMountingByRckAI.pauseBT3 = false;
            aiMountingByRckAI.playerMounting = false;
            aiMountingByRckAI.aiMounting = null;
            aiMountingByRckAI.isOccupied = false;
            aiMountingByRckAI = null;

            isDismounting = false;

            yield return null;
        }

        public override void OnTriggerStay(Collider other)
        {
            if(!isMounted)
                base.OnTriggerStay(other);
        }

        public override void OnTriggerExit(Collider other)
        {
            if(!isMounted)
                base.OnTriggerExit(other);
        }
    }
}