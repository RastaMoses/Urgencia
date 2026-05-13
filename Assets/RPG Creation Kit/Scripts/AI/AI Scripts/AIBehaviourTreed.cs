using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.BehaviourTree;
using System.Linq;
using RPGCreationKit.Player;
using XNode;
using RPGCreationKit.SaveSystem;
using RPGCreationKit.BehaviourTree.Data;
using Unity.VisualScripting;

namespace RPGCreationKit.AI
{
    [System.Serializable]
    public class PurposeNodesDictionary : SerializableDictionary<string, BTNode.RuntimeBTNodeData> { }

    [System.Serializable]
    public class PurposeBTVariablesDictionary : SerializableDictionary<string, BTVariable> { }

    [System.Serializable]
    public struct DynamicBTreeRateSetting
    {
        public float fromDistance;
        public float toDistance;

        public int xFrames;

        public DynamicBTreeRateSetting(float fD, float tD, int xF)
        {
            fromDistance = fD;
            toDistance = tD;
            xFrames = xF;
        }
    }

    /// <summary>
    /// Manages the Behaviour Trees and the interactions with the AI agent from a tree.
    /// </summary>
    public class AIBehaviourTreed : AICombatSystem
    {
        public float pPlayerDist = 99999.9f;    // distance between player and persistent reference ai

        public TreeTickRate tickRate;
        public int xFrames = 5;
        public float xSeconds = 1f;

        public PurposeState purposeState;

        [Space(10)]
        [Header("Behaviour Tree Control")]
        public bool useBT = true;
        public bool pauseBT = false;
        public bool pauseBT2 = false; // used for mounts, when the mounting RckAI is on and in conversation, the mount should pause its BT
        public bool pauseBT3 = false; // used for mounts, when the RckAI is trying to get on the mount, the mount should pause its BT - BT3 ensures this

        public RPGCK_BT currentBehaviour;
        public bool isUsingPurposeBehaviour = true;

        public RPGCK_BT purposeBehaviourTree;
        public PurposeNodesDictionary purposeNodesData;

        [SerializeField]
        public PurposeBTVariablesDictionary btPurposeVarData;

        // --------------------------------------------------------------------------------------------

        public RPGCK_BT combatBehaviourTree;
        public PurposeNodesDictionary combatNodesData;

        public PurposeBTVariablesDictionary btCombatVarData;

        // Dynamic BTree tick rate
        public bool enableDynamicTickRate = false;
        public int defaultXframes;
        public DynamicBTreeRateSetting[] dynamicTickRateSettings = new DynamicBTreeRateSetting[]
        {
            new DynamicBTreeRateSetting(0, 50, 1),
            new DynamicBTreeRateSetting(50, 100, 4),
            new DynamicBTreeRateSetting(100, 99999, 8)
        };

        public BTNode.RuntimeBTNodeData GetNode(bool isCombat, string guid)
        {
            if (isCombat)
                return combatNodesData[guid];
            else
                return purposeNodesData[guid];
        }

        [Header("Settings")]
        public bool keepTicking = true;

        public RckAI forwardRckAI;

        // Other behavioural 
        public bool ignoreVisionWhenAlertedKillNPC = false;

        public bool spawnOnAggroAlertEvent = false;
        public bool ignoreVisionWhenAlertedAggroNPC = false;

        public override void Start()
        {
            base.Start();

            StartCoroutine(IEStart());

            forwardRckAI = GetComponent<RckAI>();
        }

        IEnumerator IEStart()
        {
            if (!isAlive)
                Die();
            else
                if (useBT)
            {
                purposeBehaviourTree.RPGCK_RuntimeNodeAssign(this.gameObject, GetComponent<RckAI>());
                combatBehaviourTree.RPGCK_RuntimeNodeAssign(this.gameObject, GetComponent<RckAI>());

                defaultXframes = xFrames;

                // Always start with the NormalBehaviourTree
                SwitchBehaviourTree(false);

                StartTicking();
            }

            yield return null;
        }

        public void StartTicking()
        {
            keepTicking = true;

            StopCoroutine(TickBehaviourTree());
            StartCoroutine(TickBehaviourTree());
        }

        IEnumerator TickBehaviourTree()
        {
            while (keepTicking)
            {
                while (pauseBT || pauseBT2 || pauseBT3)
                    yield return null;

                switch(tickRate)
                {
                    case TreeTickRate.EveryFrame:
                        yield return new WaitForEndOfFrame();
                        break;

                    case TreeTickRate.EveryXFrames:

                        if (enableDynamicTickRate)
                            SetDynamicBasedXFrames();

                        int framesPassed = 0;

                        while(framesPassed <= xFrames)
                        {
                            yield return new WaitForEndOfFrame();
                            framesPassed++;
                        }
                        break;

                    case TreeTickRate.EveryXSeconds_Realtime:
                        yield return new WaitForSecondsRealtime(xSeconds);
                        break;

                    case TreeTickRate.EveryXSeconds_GameTime:
                        yield return new WaitForSeconds(xSeconds);
                        break;
                }

                while (pauseBT || pauseBT2 || pauseBT3 || currentBehaviour == null || currentBehaviour.nodes.Count <= 0 || currentBehaviour.nodes[0] == null)
                    yield return null;

                try
                {
                    (currentBehaviour.nodes[0] as BTNode).Execute(forwardRckAI);
                }
                catch(System.Exception e)
                {
                    Debug.LogError("Catched Error in BT Execution: " + e);
                }
                
                /*
                if (firstTick == false)
                {

                    // Tick
                    (currentBehaviour.nodes[0] as BTNode).Execute();
                    firstTick = true;
                }
                else
                {
                    // Find all the nodes that are still running and execute them
                    ((currentBehaviour.nodes[0] as BTNode).GetOutputPort("firstNode").Connection.node as BTNode).Execute();
                }
                */

                yield return null;
            }
        }

        public void DoOneBTreeTick()
        {
            try
            {
                (currentBehaviour.nodes[0] as BTNode).Execute(forwardRckAI);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Catched Error in BT Execution: " + e);
            }
        }

        /*
         * Sets the xFrames value based on the distance with the player 
         */
        public void SetDynamicBasedXFrames()
        {
            float distToPlayer;

            if (mainTarget != null && mainTarget.tag == "Player")
                distToPlayer = distanceFromMainTarget;
            else
                distToPlayer = Vector3.Distance(this.transform.position, RckPlayer.instance.transform.position);

            for(int i = 0; i < dynamicTickRateSettings.Count(); i++)
            {
                // Find the first that satisfies the condition
                if((distToPlayer > dynamicTickRateSettings[i].fromDistance && distToPlayer <= dynamicTickRateSettings[i].toDistance) 
                    || (i+1) >= dynamicTickRateSettings.Count())
                {
                    xFrames = dynamicTickRateSettings[i].xFrames;
                    break;
                }
            }
        }

        public void ChangeTickRate(TreeTickRate newTickRate)
        {
            tickRate = newTickRate;
        }

        protected bool anyWaitForSetRunning = false;
        protected bool anyWaitForSwitchRunning = false;


        public IEnumerator QueueSwitch(bool _combat)
        {
            if (anyWaitForSwitchRunning)
                yield return null;

            SwitchBehaviourTree(_combat);
        }

        [AIInvokable]
        public void SwitchBehaviourTree(bool _combat)
        {
            if (anyWaitForSwitchRunning)
            {
                StartCoroutine(QueueSwitch(_combat));
                return;
            }

            // Execute OnExit node if present
            if (currentBehaviour != null && currentBehaviour.nodes.Count > 0 && ((currentBehaviour.nodes[0] as BTNode).GetOutputPort("onExitNode").Connection != null && ((currentBehaviour.nodes[0] as BTNode).GetOutputPort("onExitNode").Connection.node != null)))
            {
                anyWaitForSwitchRunning = true;
                // Execute OnExit before changing the tree
                StartCoroutine(SwitchBehaviourTree_WaitForExitNodeTask(_combat));
            }
            else
            {

                if (_combat)
                {
                    currentBehaviour = combatBehaviourTree;
                    isUsingPurposeBehaviour = false;
                }
                else
                {
                    currentBehaviour = purposeBehaviourTree;
                    isUsingPurposeBehaviour = true;
                }
            }
        }

        IEnumerator SwitchBehaviourTree_WaitForExitNodeTask(bool _combat)
        {
            pauseBT = true;
            yield return new WaitForEndOfFrame();

            var node = (((currentBehaviour.nodes[0] as BTNode).GetOutputPort("onExitNode").Connection.node) as BTNode);
            node.ReEvaluate(forwardRckAI);
            while (GetNode(node.isInCombatBehavior, node.guidStr).m_NodeState == NodeState.Null || GetNode(node.isInCombatBehavior, node.guidStr).m_NodeState == NodeState.Running)
            {
                node.Execute(forwardRckAI);
                yield return new WaitForEndOfFrame();
            }

            if (anyWaitForSetRunning)
            {
                yield return new WaitForEndOfFrame();
            }


            if (_combat)
            {
                currentBehaviour = combatBehaviourTree;
                isUsingPurposeBehaviour = false;
            }
            else
            {
                currentBehaviour = purposeBehaviourTree;
                isUsingPurposeBehaviour = true;
            }

            anyWaitForSwitchRunning = false;
            pauseBT = false;
            yield break;
        }


        [AIInvokable]
        public void SwitchBehaviourTreeToAI(string _rckID, bool _combat)
        {
            // Get the RckAI if possible
            RckAI sTo = null;
            CellsSystem.CellInformation.TryToGetAI(_rckID, out sTo);

            if (sTo != null)
            {
                sTo.SwitchBehaviourTree(_combat);
            }
        }

        public override void OnEnterInCombat()
        {
            base.OnEnterInCombat();

            if (combatBehaviourTree.resetVariablesUponStartResume)
                combatBehaviourTree.ResetVariables();

            if (isUsingActionPoint && !leavingActionPoint)
                StopUsingNPCActionPoint();

            if(spawnOnAggroAlertEvent)
            {
                AlertEventData alertEventData = new AlertEventData();
                alertEventData.overrideRadius = true;
                alertEventData.radius = 30;
                alertEventData.killerNPC = GetComponent<RckAI>();

                AlertEvent.NewAlertEvent(AlertEvents.NPCEntersInCombat, transform, alertEventData);
            }

            SwitchBehaviourTree(true);
        }

        [AIInvokable]
        public override void LeaveCombat()
        {
            base.LeaveCombat();

            if (purposeBehaviourTree.resetVariablesUponStartResume)
                purposeBehaviourTree.ResetVariables();

            // reset ishostile
            isHostile = false;
            isHostileAgainstPC = false;

            SwitchBehaviourTree(false);

            // Reset PurposeState
            if (purposeState.IsAssigned)
            {
                SetTarget(purposeState.Target);
                purposeState.ResumePurpose();
            }
        }

        [AIInvokable]
        public void ForceEnterInCombatAgainstPlayer()
        {
            EnterInCombatAgainst(Entity.GetPlayerEntity());
        }

        public override void Damage(DamageContext context)
        {
            if (context.sender != null && !context.sender.CompareTag("Player"))
            {
                // If they're both members of the same factions chances are they didn't meant to attach each other, so ignore
                for (int i = 0; i < belongsToFactions.Count; i++)
                {
                    if ((context.sender as EntityFactionable).GetInFaction(belongsToFactions[i].ID) && !belongsToFactions[i].friendlyFire)
                        return;
                }
            }

            base.Damage(context);

            // Sounds
            if (aiSounds != null && isAlive)
            {
                if (aiSounds.takeDamageSounds.Length > 0)
                {
                    int n = Random.Range(0, aiSounds.takeDamageSounds.Length);

                    if (aiSounds.takeDamageSounds[n] != null && !audioSource.isPlaying)
                    {
                        audioSource.clip = aiSounds.takeDamageSounds[n];
                        audioSource.Play();

                        if (RCKSettings.NPCS_FACIAL_ANIM_ENABLED)
                            QuickDialogueMouthAnim(audioSource.clip);
                    }
                }
            }

            if (context.sender != null)
            {
                if (context.sender == this)
                    return;

                // If they're both members of the same factions chances are they didn't meant to attach each other, so ignore
                if (isInCombat)
                {
                    for (int i = 0; i < belongsToFactions.Count; i++)
                    {
                        if ((context.sender as EntityFactionable).GetInFaction(belongsToFactions[i].ID))
                            return;
                    }
                }

                if (!enemyTargets.Any(t => t.m_entity == context.sender))
                {
                    if (isInConversation)
                        StopConversation();

                    // PDT (Player Damage Tolerance) check
                    if(pdtEnabled && context.sender.CompareTag("Player"))
                    {
                        pdtCounting = true;
                        pdtCurCount++;

                        // Tolerate the attack
                        if (pdtCurCount < pdtHitsBeforeAggro && isAlive)
                        {
                            // Play audio?
                            if(!(this is MountAI) && !(this is CreatureAI))
                                RCKFunctions.DisplayHeardLine("Stop it!", 1.5f);

                            isHostile = false;
                            return;
                        }
                    }

                    if (!m_isInCombat)
                        EnterInCombatAgainst(context.sender);

                    enemyTargets.Add(new VisibleEnemy(context.sender, context.sender.GetComponent<EntityAttributes>(), new AggroInfo(50)));
                }
                else // this enemy was already in combat with this agent so just update the aggro
                {
                    var entTarget = enemyTargets.Find(x => x.m_entity == context.sender);
                    entTarget.m_aggro.AlterAggroValue(AggroSettings.Modifier.Damage, context.amount);

                    float weaponAggroModifier = 0.0f;
                    if (context.weaponItem != null)
                        weaponAggroModifier = context.weaponItem.aggroModifier;
                    else if (context.spell != null)
                        weaponAggroModifier = context.spell.aggroModifier;

                    entTarget.m_aggro.AlterAggroValue(AggroSettings.Modifier.Weapon, weaponAggroModifier);
                }
            }
        }

        public override void Die()
        {
            base.Die();
            attributes.CurHealth = -1;
            attributes.activeEffects.Clear();
            keepTicking = false;
            useBT = false;
            pauseBT = true;

            // Check if this is a persistent reference, if it is, re-enable the PRefCycle to hide/show the body
            if(isPersistentReference)
                StartCoroutine(PRefCycle());
        }

        #region Methods and Functions specifically for Behaviour Trees
        public bool hasARangedWeapon = false;

        [AIInvokable]
        public void HasRangedWeaponCheck()
        {
            WeaponItem weaponItem = null;

            for (int i = 0; i < inventory.subLists[(int)ItemTypes.WeaponItem].Count; i++)
            {
                weaponItem = (WeaponItem)inventory.subLists[(int)ItemTypes.WeaponItem][i].item;

                if (weaponItem.weaponType == WeaponType.Bow || weaponItem.weaponType == WeaponType.Crossbow)
                {
                    if (HasAmmoForWeapon(weaponItem))
                    {
                        hasARangedWeapon = true;
                        break;
                    }
                    else
                    {
                        Debug.Log("don't have ammo for " + weaponItem.ItemID);
                    }
                }
            }
        }

        public bool hasAMeleeWeapon = false;
        [AIInvokable]
        public void HasMeleeWeaponCheck()
        {
            WeaponItem weaponItem = null;

            for (int i = 0; i < inventory.subLists[(int)ItemTypes.WeaponItem].Count; i++)
            {
                weaponItem = (WeaponItem)inventory.subLists[(int)ItemTypes.WeaponItem][i].item;

                if (weaponItem.weaponType == WeaponType.BladeOneHand || weaponItem.weaponType == WeaponType.BluntOneHand ||
                    weaponItem.weaponType == WeaponType.BladeTwoHands || weaponItem.weaponType == WeaponType.BluntTwoHands ||
                    weaponItem.weaponType == WeaponType.DaggerOneHand)
                {
                    hasAMeleeWeapon = true;
                    break;
                }
            }
        }


        public float distanceFromMainTarget;
        [AIInvokable]
        public void GetDistanceFromMainTarget()
        {
            if(mainTarget == null)
            {
                //Debug.LogWarning("Called GetDistanceFromMainTarget() but no MainTarget has been set");
                return;
            }

            distanceFromMainTarget = Vector3.Distance(this.transform.position, mainTarget.transform.position);
        }


        [AIInvokable]
        public void Weapon_SwitchToMelee()
        {
            isSwitchingWeapon = true;

            WeaponItem weaponItem = null;

            for (int i = 0; i < inventory.subLists[(int)ItemTypes.WeaponItem].Count; i++)
            {
                weaponItem = (WeaponItem)inventory.subLists[(int)ItemTypes.WeaponItem][i].item;

                if (weaponItem.weaponType == WeaponType.BladeOneHand || weaponItem.weaponType == WeaponType.BluntOneHand ||
                    weaponItem.weaponType == WeaponType.BladeTwoHands || weaponItem.weaponType == WeaponType.BluntTwoHands
                    || weaponItem.weaponType == WeaponType.DaggerOneHand)
                {
                    ChangeWeapon(weaponItem.ItemID);
                    break;
                }
            }
        }

        [AIInvokable]
        public void Weapon_SwitchToRanged()
        {
            isSwitchingWeapon = true;

            WeaponItem weaponItem = null;

            for (int i = 0; i < inventory.subLists[(int)ItemTypes.WeaponItem].Count; i++)
            {
                weaponItem = (WeaponItem)inventory.subLists[(int)ItemTypes.WeaponItem][i].item;

                if (weaponItem.weaponType == WeaponType.Bow || weaponItem.weaponType == WeaponType.Crossbow)
                {
                    // We got a weapon, check for ammo
                    if (EquipAmmoForWeapon(weaponItem))
                    {
                        ChangeWeapon(weaponItem);
                        break;
                    }
                }
            }
        }

        [AIInvokable]
        public void Weapon_SwitchToDefault()
        {
            ChangeWeapon(defaultWeapon.ItemID);
        }

        public float currentWeaponReach = RCKSettings.DEFAULT_WEAPON_REACH;

        [AIInvokable]
        public void GetWeaponsReach()
        {
            if (equipment.currentWeapon != null)
                currentWeaponReach = equipment.currentWeapon.Reach;
            else
                currentWeaponReach = RCKSettings.DEFAULT_WEAPON_REACH; // 4 is default value
        }


        [AIInvokable]
        public void SpeakToTarget()
        {
            if (mainTarget.CompareTag("Player"))
                RPGCreationKit.Player.RckPlayer.instance.StartDialogue(this.GetComponent<IDialoguable>());
            else
            {
                IDialoguable[] targetDialoguable = new IDialoguable[2];
                targetDialoguable[0] = mainTarget.GetComponent<IDialoguable>();

                if (targetDialoguable == null)
                    targetDialoguable[0] = mainTarget.GetComponentInParent<IDialoguable>();

                // Set us
                targetDialoguable[1] = this;

                if (targetDialoguable[0] != null)
                {
                    DialogueLogic(targetDialoguable, GetCurrentDialogueGraph());
                }
            }
        }

        [AIInvokable]
        public void SetRckAIAsTarget(string _rckID)
        {
            // Get the RckAI if possible
            RckAI sTo = null;
            CellsSystem.CellInformation.TryToGetAI(_rckID, out sTo);

            if (sTo != null)
                SetTarget(sTo.gameObject);
        }

        public override void OnDialogueEnds(GameObject target)
        {
            //// PURPOSE_CALLBACK: ClearOnTeleportToCell \\\\\
            if (purposeState != null && purposeState.IsAssigned && purposeState.clearsOn == PurposeClearTypes.ClearOnFinishTalkingWith)
            {
                if (purposeState.clearsOnData.stringData == speakingTo.GetEntityGameObject().GetComponent<Entity>().entityID)
                {
                    // We did it
                    purposeState.CompletePurpose();
                }
            }

            base.OnDialogueEnds(target);
        }

        /// <summary>
        /// Forces the AI to stop the dialogue
        /// </summary>
        public void StopConversation()
        {
            if (entitiesTalkingWith != null)
            {
                // Copy before destroying entitiesTalkingWith
                List<IDialoguable> dial = new List<IDialoguable>();

                for (int i = 0; i < entitiesTalkingWith.Length; i++)
                    dial.Add(entitiesTalkingWith[i]);

                for (int i = 0; i < dial.Count; i++)
                    dial[i].ForceToStopDialogue();
            }
        }

        bool goingToSpeak = false;

        [AIInvokable]
        public void SendAIToSpeakToPlayer(bool run)
        {
            if (goingToSpeak)
                return;

            goingToSpeak = true;

            StopCoroutine(SendAIToSpeakToPlayerTask());

            if ( isReachingActionPoint || isUsingActionPoint)
            {
                ResetActionPointAgentState();
                StopUsingNPCActionPoint(true);
            }

            if (run)
                Movements_SwitchToRun();
            else
                Movements_SwitchToWalk();

            // Set maintarget
            SetTarget(Player.RckPlayer.instance.gameObject);

            // Follow until he reaches it
            StartCoroutine(SendAIToSpeakToPlayerTask());
        }

        public IEnumerator SendAIToSpeakToPlayerTask()
        {
            if (!isLoaded)
            {
                goingToSpeak = false;
                yield break;
            }

            while (Vector3.Distance(this.gameObject.transform.position, Player.RckPlayer.instance.transform.position) > RCKSettings.NPC_TO_NPC_CONVERSATION_DISTANCE || !CanDialogue() ||
                !Player.RckPlayer.instance.CanDialogue() || !WorldManager.instance.isLoading && !Player.RckPlayer.instance.IsControlledByPlayer() || isInOfflineMode || !isLoaded)
                yield return new WaitForEndOfFrame();

            Player.RckPlayer.instance.StartDialogue(this);

            goingToSpeak = false;
            yield break;
        }

        [AIInvokable]
        public void AssignPurpose(RckAI _AI, GameObject _target, PurposeClearTypes _clearsOn, PurposeStateClearsOnData _clearOnData, string _nextPurposeBehaviourTree = null)
        {
            purposeState.AssignPurpose(_AI, _target, _clearsOn, _clearOnData, _nextPurposeBehaviourTree);
        }

        [AIInvokable]
        public void Purpose_AssignToFollowCurrentPath()
        {
            if(aiPath != null && aiPath.gameObject != null)
                purposeState.AssignPurpose(GetComponent<RckAI>(), aiPath.gameObject, PurposeClearTypes.ClearOnDeath, null, null);
        }

        [AIInvokable]
        public void Magic_SwitchToHealingSpell()
        {
            // Check if the AI already has an healing spell
            if (spellsKnowledge.spellInUse != null && spellsKnowledge.spellInUse.tag == SpellTag.RestoreHealth)
                return;

            for(int i = 0; i < spellsKnowledge.Spells.Count; i++)
            {
                if(spellsKnowledge.Spells[i].tag == SpellTag.RestoreHealth)
                {
                    spellsKnowledge.spellInUse = spellsKnowledge.Spells[i];
                    return;
                }
            }
        }

        [AIInvokable]
        public void Magic_SwitchToDamageSpell()
        {
            // Check if the AI already has an healing spell
            if (spellsKnowledge.spellInUse != null && spellsKnowledge.spellInUse.tag == SpellTag.DamageProjectile || spellsKnowledge.spellInUse.tag == SpellTag.DamageTouch)
                return;

            for (int i = 0; i < spellsKnowledge.Spells.Count; i++)
            {
                if (spellsKnowledge.Spells[i].tag == SpellTag.DamageProjectile || spellsKnowledge.Spells[i].tag == SpellTag.DamageTouch)
                {
                    spellsKnowledge.spellInUse = spellsKnowledge.Spells[i];
                    return;
                }
            }
        }

        [AIInvokable]
        public void Magic_SetSpell(string _spellIDToSet)
        {
            Spell _spellToSet = SpellsDatabase.GetSpell(_spellIDToSet);

            if (_spellToSet != null)
                spellsKnowledge.spellInUse = _spellToSet;
        }


        [AIInvokable]
        public void Magic_AddSpellToKnowledge(string _spellIDToAdd)
        {
            Spell _spellToAdd = SpellsDatabase.GetSpell(_spellIDToAdd);

            if (_spellToAdd != null)
                spellsKnowledge.Spells.Add(_spellToAdd);
        }
        #endregion

        [ContextMenu("Go Online")]
        public void EGoOnline()
        {
            onlineComponents.OnGoingOnline();
        }

        [ContextMenu("Go Offline")]
        public void EGoOffline()
        {
            onlineComponents.OnGoingOffline();
        }

        public IEnumerator PRefCycle()
        {
            while (true)
            {
                while (!isLoaded)
                    yield return null;

                if (WorldManager.instance.currentWorldspace.worldSpaceID == inPersistentWorldspaceID)
                {
                    if(WorldManager.instance.currentWorldspace.worldSpaceType == CellsSystem.Worldspace.WorldSpaceType.Exterior ||
                       (WorldManager.instance.currentWorldspace.worldSpaceType == CellsSystem.Worldspace.WorldSpaceType.Interior && WorldManager.instance.currentWorldspace.worldSpaceID == inPersistentCellID))
                    {
                        // Check if this ref should be active or not
                        pPlayerDist = (Vector3.Distance(RckPlayer.instance.gameObject.transform.position, this.gameObject.transform.position));

                        if (this.isInOfflineMode)
                        {
                            if (pPlayerDist < 140)
                                EGoOnline();
                        }
                        else
                        {
                            if (pPlayerDist > 150)
                                EGoOffline();
                        }
                    }
                    else if(!this.isInOfflineMode)
                    {
                        EGoOffline();
                    }
                }
                else if (!this.isInOfflineMode)
                {
                    EGoOffline();
                }

                yield return new WaitForSeconds(persistentLifePulse);
            }
        }

        public void PRefCycleOnce()
        {
            if (WorldManager.instance.currentWorldspace.worldSpaceID == inPersistentWorldspaceID)
            {
                if (WorldManager.instance.currentWorldspace.worldSpaceType == CellsSystem.Worldspace.WorldSpaceType.Exterior ||
                   (WorldManager.instance.currentWorldspace.worldSpaceType == CellsSystem.Worldspace.WorldSpaceType.Interior && WorldManager.instance.currentWorldspace.worldSpaceID == inPersistentCellID))
                {
                    // Check if this ref should be active or not
                    pPlayerDist = (Vector3.Distance(RckPlayer.instance.gameObject.transform.position, this.gameObject.transform.position));

                    if (this.isInOfflineMode)
                    {
                        if (pPlayerDist < RCKSettings.PREF_SHOW_DISTANCE)
                            EGoOnline();
                    }
                    else
                    {
                        if (pPlayerDist > RCKSettings.PREF_HIDE_DISTANCE)
                            EGoOffline();
                    }
                }
                else if (!this.isInOfflineMode)
                {
                    EGoOffline();
                }
            }
            else if (!this.isInOfflineMode)
            {
                EGoOffline();
            }
        }
    }
}