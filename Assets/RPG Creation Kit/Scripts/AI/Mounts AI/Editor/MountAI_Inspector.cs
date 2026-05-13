using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit.AI
{
    [CustomEditor(typeof(MountAI))]
    public class MountAI_Inspector : Editor
    {
        RckAI tAI;

        public Texture _logo;
        int selected;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUIStyle CHeaderStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            CHeaderStyle.fontSize = 14;
            CHeaderStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/Ren.ttf");
            
            tAI = (MountAI)target;

            GUI.DrawTexture(new Rect(0, 30, EditorGUIUtility.currentViewWidth, 80), _logo, ScaleMode.StretchToFill, true, 5.0f);
            EditorGUILayout.Space(130);

            string[] buttons = { "Entity", "Status", "Dialogue", "Movements", "Inventory &\nEquipment", "Perception", "Combat", "Behaviour" };
            selected = GUILayout.SelectionGrid(selected, buttons, 4);

            EditorGUILayout.Space(15);
            Color dguicolor = GUI.color;

            switch (selected)
            {
                // DRAW ENTITY TAB
                case 0:
                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.LabelField("GENERAL", CHeaderStyle);
                    EditorGUILayout.Separator();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("entityID"), new GUIContent("Entity ID", "The ID of this Entity. If this is a Persisent Reference it MUST be the same as the Persistent Ref ID."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("entityName"), new GUIContent("Entity Name", "The name of this Entity, it's what will be displayed in game."));

                    EditorGUILayout.Space(5);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("entityFocusPart"), new GUIContent("Entity Focus Part", "The focus part of this entity. Usually it's the head. It is used from other entities to look at this one."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Anim"), new GUIContent("Animator", "The Animator of this Entity."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("audioSource"), new GUIContent("AudioSource", "The AudioSource of this Entity."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("aiSounds"), new GUIContent("AI Sounds", "The AISounds Component of this Entity."));

                    EditorGUILayout.Space(5);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("myCellInfo"), new GUIContent("Current CellInfo", "The CellInfo of the cell where this entity is located. It will be automatically set at runtime."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("iGUIDReferences"), new GUIContent("GUID References", "The GUID References of this Entity (if used)."));

                    EditorGUILayout.Space(5);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("belongsToFactions"), new GUIContent("Factions", "The factions of which this Entity belongs to."));

                    EditorGUILayout.Space(5);

                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isPersistentReference"), new GUIContent("Is Persistent Reference?", ""));
                    GUI.enabled = true;

                    if (serializedObject.FindProperty("isPersistentReference").boolValue)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inPersistentWorldspaceID"), new GUIContent("In WorldSpace ID:", ""));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inPersistentCellID"), new GUIContent("In Cell ID:", ""));
                    }

                    EditorGUILayout.Space(10);

                    EditorGUILayout.Separator();
                    EditorGUILayout.LabelField("MOUNT", CHeaderStyle);
                    EditorGUILayout.Separator();

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("mountType"), new GUIContent("Mount Type:", ""));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("mountPoints"),new GUIContent("Mount Type:", ""), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("extraColliders"), new GUIContent("Extra Colliders:", ""), true);

                    EditorGUILayout.Space(5);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fpsCameraAnimPos"), new GUIContent("FPS Cam Pos:", ""), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tpsCameraRootPos"), new GUIContent("TPS Cam Pos:", ""), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("interactZoneOffsetRckAI"), new GUIContent("InteractZone Pos:", ""), true);

                    EditorGUILayout.Space(5);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("mountMaxSpeed"), new GUIContent("Max Speed:", ""), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("mountMaxSpeedReverse"), new GUIContent("Max Speed Reverse:", ""), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("negativeZMax"), new GUIContent("Negative Z Max:", ""), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("movMult"), new GUIContent("Movement Multiplier:", ""), true);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("mountRotationSpeed"), new GUIContent("Rot Speed:", ""), true);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("animSpeed"), new GUIContent("Anim Speed:", ""), true);

                    EditorGUILayout.Space(5);

                    EditorGUILayout.EndVertical();
                    break;

                // DRAW STATUS TAB
                case 1:
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("STATUS", CHeaderStyle);
                    EditorGUILayout.HelpBox("Gray fields should not be set from the inspector.", MessageType.Info);

                    GUI.color = Color.gray;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isAlive"), new GUIContent("Is Alive", "Is this Entity alive?"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isHostile"), new GUIContent("Is Hostile", "Is this Entity hostile against some other entity or the player?"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isHostileAgainstPC"), new GUIContent("Is Hostile (PC)?", "Is this Entity hostile against the PlayerCharacter?."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isInCombat"), new GUIContent("Is In Combat", "Is this Entity in combat?"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isFleeing"), new GUIContent("Is Fleeing", "Is this Entity fleeing?."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isUnconscious"), new GUIContent("Is Unconcious", "Is this Entity unconcious?"));

                    GUI.color = dguicolor;

                    EditorGUILayout.Space(7.5f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isEssential"), new GUIContent("Is Essential?", "If this is set to true, this Entity will never die, when its health will reach 0 he will go unconcious instead of dying."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("attributes"), new GUIContent("Attributes", "The EntityAttributes component of this Entity."));

                    EditorGUILayout.Space(5f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("usesRagdoll"), new GUIContent("Use Ragdoll", "Does this Entity use Ragdoll when it dies/go unconcious?"));

                    if (serializedObject.FindProperty("usesRagdoll").boolValue)
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ragdoll"), new GUIContent("Ragdoll", "The Ragdoll component."));


                    EditorGUILayout.EndVertical();

                    break;

                // DRAW DIALOGUE TAB
                case 2:
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("DIALOGUE", CHeaderStyle);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("aiLookAt"), new GUIContent("AI LookAt", "The AILookAt component of this Entity, leave blank if it shouldn't have any."));

                    EditorGUILayout.Space(5);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueSystemEnabled"), new GUIContent("Dialogue System Enabled?", "If this is set to true, this Entity can talk to others or can be talked to."));

                    EditorGUILayout.Space(5);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("currentDialogueGraph"), new GUIContent("Current Dialogue Graph", "The Dialogue Graph that will be used as soon as this Entity talks to or is requested to talk to someone."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultDialogueGraph"), new GUIContent("Default Dialogue Graph", "A fallback if the \"Current Dialogue Graph\" shouldn't work for any reason - this should ALWAYS be assigned."));

                    EditorGUILayout.EndVertical();
                    break;


                // DRAW MOVEMENTS TAB
                case 3:
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("MOVEMENTS", CHeaderStyle);

                    EditorGUILayout.Space(5f);

                    EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("usesMovements"), new GUIContent("Use Movements?", "Should this Entity move?"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("usesOfflineMode"), new GUIContent("Use Offline Mode?", "[Read More on the Documentation] Enable/Disable the Offline mode for this Entity."));

                    EditorGUILayout.Space(7.5f);

                    EditorGUILayout.LabelField("General Components", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("physicalCollider"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("interactableCollider"));

                    EditorGUILayout.Space(1f);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("agent"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Rigidbody"));

                    EditorGUILayout.Space(7.5f);

                    GUI.color = Color.gray;

                    EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("movementType"), new GUIContent("Movement Type", "The current Movement Type of this Entity, it shouldn't be changed from the Inspector."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isInOfflineMode"), new GUIContent("Is In Offline Mode?", "The current Movement Type of this Entity, it shouldn't be changed from the Inspector."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isWalking"), new GUIContent("Is Walking?", "Set to true if the Entity is walking, otherwise it is running."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("canSeeTarget"), new GUIContent("Can See Target?", "If this Entity has a target, this will be true only if it is visible for this Entity."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shouldFollowMainTarget"), new GUIContent("Should Follow Main Target", "If this is set to true (unless overridden) this Entity will follow its Main Target."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shouldFollowOnlyIfCanSee"), new GUIContent("Only If Can See", "If this is set to true this Entity will not be able to follow its Main Target unless it sees it (Can See Target set to true)."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shouldFollowAIPath"), new GUIContent("Should Follow AIPath", "If this is set to true (unless overridden) this Entity will follow its AI Path."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shouldUseNPCActionPoint"), new GUIContent("Should Use NPCActionPoint", "If this is set to true (unless overridden) this Entity will follow and use its NPCActionPoint."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isUsingActionPoint"), new GUIContent("Using ActionPoint", "If this is set to true this Entity is using an ActionPoint."));

                    GUI.color = dguicolor;


                    EditorGUILayout.Space(7.5f);

                    EditorGUILayout.LabelField("Movement Components", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("mainTarget"), new GUIContent("Main Target", "The current Main Target."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("hasMainTarget"), new GUIContent("Has Main Target", "True if this Entity has a Main Target."));

                    EditorGUILayout.Space(2.5f);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("targetVector"), new GUIContent("Target Vector", "[Read More on the Documentation] An alternative way to determine where this Entity should go, it doesn't use a Transform but a Vector3 instead."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shouldFollowTargetVector"), new GUIContent("Follow Target Vector?", "If true (unless overriden) this Entity will move to the Target Vector and not to the Main Target Transform."));

                    EditorGUILayout.Space(2.5f);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("aiPath"), new GUIContent("AI Path", "The current AI Path assigned to this Entity, having it assigned doesn't mean it is following it, refer to [Should Follow AIPath] instead."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("actionPoint"), new GUIContent("NPCActionPoint", "The current NPCActionPoint assigned to this Entity, having it assigned doesn't mean it is following it, refer to [Should Use NPCActionPoint] instead."));

                    EditorGUILayout.Space(2.5f);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lookATransformIfStopped"), new GUIContent("LookAt Transform If Stopped", "If this is true when this Entity is not moving it will look at a set Transform [Direction To Look At]."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("directionToLookIfStopped"), new GUIContent("Direction to LookAt", "The direction to look at if stopped."));

                    EditorGUILayout.Space(7.5f);

                    EditorGUILayout.LabelField("Movement Settings", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedSteeringBehaviour"), new GUIContent("Steering Behaviour", "The current Steering Behaviour."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("arriveSteeringBehaviourDeceleration"), new GUIContent("Arrive Deceleration", "How fast the Entity will decelerate if the Steering Behaviour is 'Arrive'."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fleeAtDistanceValue"), new GUIContent("Flee At Distance", "The distance of which the Entity will start to flee if the Steering Behaviour says so."));

                    EditorGUILayout.Space(2.5f);

                    GUI.color = Color.gray;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("currentSpeed"), new GUIContent("Current Speed", "Current Speed of this Entity."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSpeed"), new GUIContent("Max Speed", "The Max Speed of this Entity."));
                    GUI.color = dguicolor;

                    EditorGUILayout.Space(2.5f);

                    GUI.color = Color.gray;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isStopped"), new GUIContent("Is Stopped", "True if this Entity is not moving."));
                    GUI.color = dguicolor;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stoppingHalt"), new GUIContent("Stopping Halt", "The smoothness applied to stopping this Entity."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stoppingDistance"), new GUIContent("Stopping Distance", "Defines the distance at which the current target will be set as reached."));

                    EditorGUILayout.Space(2.5f);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("generalRotationSpeed"), new GUIContent("General Rot Speed", "Defines the general speed of rotation of this AI."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationThreshold"), new GUIContent("Rotation Threshold", "The value of difference between the oldRotPos and the current RotPos to say if the AI is rotating or not."));

                    EditorGUILayout.Space(7.5f);

                    EditorGUILayout.LabelField("Traversing Links", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tLinkObstacleAvoidancePrecision"), new GUIContent("TLink Obstacle Avoidance Precision", "[Read More on the Documentation] The precision of the Obstacle Avoidance while in Traversing Links mode."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("chaseTargetDuringTraversing"), new GUIContent("Chase Target During TLink", "[Read More on the Documentation] Set this to true if you want this Entity to keep following the Target even on Links."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rotateToFaceTargetDuringTraversing"), new GUIContent("Face Target during TLink", "Set this to true if you want this Entity to always face the Target while traversing the link."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("usesTlinkUnstuck"), new GUIContent("Use TLink Unstuck", "[Read More on the Documentation] Enable/Disable the Unstuck measure if this Entity is seen to be stuck."));

                    EditorGUILayout.Space(2.5f);



                    EditorGUILayout.EndVertical();
                    break;

                // DRAW INVENTORY & EQUIPMENT
                case 4:
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("INVENTORY & EQUIPMENT", CHeaderStyle);

                    EditorGUILayout.Space(5f);

                    EditorGUILayout.LabelField("References", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("inventory"), new GUIContent("Inventory", "The Inventory component of this Entity."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("equipment"), new GUIContent("Equipment", "The Equipment component of this Entity."));

                    EditorGUILayout.Space(2.5f);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyData"), new GUIContent("BodyData", "The BodyData of this Entity."));


                    EditorGUILayout.Space(5f);

                    EditorGUILayout.LabelField("Looting", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lootingPoint"), new GUIContent("Looting Point", "The Looting Point of this Entity."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("allowsLootWhenDead"), new GUIContent("Allow Loot When Dead", "If this is true when this Entity dies its body will be lootable."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("allowsLootOfEquipment"), new GUIContent("Allow Loot of Equipment", "If this is true not only the Inventory is lootable, but also what this Entity has equipped."));

                    EditorGUILayout.Space(5.5f);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("equippedItems"), new GUIContent("Equipped Items", "The items this Entity has currently Equipped."));

                    EditorGUILayout.EndVertical();
                    break;


                // DRAW PERCEPTION TAB
                case 5:
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("PERCEPTION", CHeaderStyle);

                    EditorGUILayout.Space(5f);

                    EditorGUILayout.LabelField("Settings & References", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("perceptionEnabled"), new GUIContent("Perception Enabled?"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("checkTick"), new GUIContent("Check Tick", "How many seconds before performing a Perception Check. Low values can give a more realistic result and more active AI, but it can weigh on performances."));

                    EditorGUILayout.Space(2.5f);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("headPos"), new GUIContent("Head Pos", "The Head transform of this Entity."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("onlineComponents"), new GUIContent("Online Components", "The OnlineComponents of this Entity"));

                    EditorGUILayout.Space(2.5f);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lookAtDistance"), new GUIContent("Look At Distance", "The distance where this Entity will start to look at other Entities."));

                    EditorGUILayout.Space(2.5f);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("targetMask"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("obstacleMask"));

                    EditorGUILayout.Space(5.5f);

                    EditorGUILayout.LabelField("Visible", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("visibleTargets"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("visibleActionPoints"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("visibleAI"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyTargets"));

                    EditorGUILayout.Space(5.5f);

                    EditorGUILayout.LabelField("Vision Settings", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("viewAngle"), new GUIContent("View Angle"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"), new GUIContent("Radius", "The Radius of the Sphere that determines what the Entity can see"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sphereYOffset"), new GUIContent("Sphere Y Offset", "The Y offset of the Sphere"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sphereForwardOffset"), new GUIContent("Sphere Z Offset", "The forward Sphere Offset"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("visualizeSphere"), new GUIContent("Debug Sphere", "If this is true you can see the Perception Sphere in the Editor"));

                    EditorGUILayout.EndVertical();
                    break;


                // DRAW COMBAT TAB
                case 6:
                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.LabelField("COMBAT", CHeaderStyle);
                    EditorGUILayout.Space(5f);

                    EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultAnimatorController"), new GUIContent("Default Anim Controller"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("blockingArea"), new GUIContent("Blocking Area"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("combatAudioSource"), new GUIContent("Combat AudioSource"));

                    EditorGUILayout.Space(5f);
                    EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultWeapon"), new GUIContent("Default Weapon"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultWeaponOnHand"), new GUIContent("Default Weapon OnHand"));


                    EditorGUILayout.Space(5f);
                    GUI.color = Color.gray;
                    EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponDrawn"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("canAttack"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isAttacking"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isBlocking"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("curAttackType"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("curChargedAttackType"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("combatType"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_isInCombat"));
                    GUI.color = dguicolor;

                    EditorGUILayout.EndVertical();
                    break;


                // DRAW BEHAVIOUR TAB
                case 7:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tickRate"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("xFrames"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("xSeconds"));


                    EditorGUILayout.PropertyField(serializedObject.FindProperty("useBT"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pauseBT"));


                    EditorGUILayout.PropertyField(serializedObject.FindProperty("currentBehaviour"));


                    EditorGUILayout.PropertyField(serializedObject.FindProperty("purposeBehaviourTree"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("combatBehaviourTree"));


                    EditorGUILayout.PropertyField(serializedObject.FindProperty("keepTicking"));

                    EditorGUILayout.Space(7.5f);

                    EditorGUILayout.LabelField("Purpose:", EditorStyles.boldLabel);

                    EditorGUILayout.Space(2.5f);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("State:");

                    bool purposeAssigned = serializedObject.FindProperty("purposeState").FindPropertyRelative("isAssigned").boolValue;
                    EditorGUILayout.LabelField(purposeAssigned ? "Assigned" : "Not Assigned");
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    if (purposeAssigned)
                    {
                        if (GUILayout.Button("CLEAR PURPOSE"))
                        {

                        }
                    }
                    else
                    {
                        if (GUILayout.Button("ASSIGN PURPOSE"))
                        {

                        }
                    }


                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}