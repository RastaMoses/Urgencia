using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit.SaveSystem
{
    [System.Serializable]
    public class AIDictionary : SerializableDictionary<string, AISaveData> { }

    [System.Serializable]
    public class AICellDictionary : SerializableDictionary<string, AIIDList> { }

    [System.Serializable]
    public class AISaveData
    {
        // Base AI
        public Vector3 position;
        public Quaternion rotation;
        public EntityAttributesSaveData aiAttributes;
        public InventorySaveData aiInventory;
        public List<string> allFactions = new List<string>();

        // AIStatus
        public bool isAlive;
        public bool isHostile;
        public bool isHostileAgainstPC;
        public bool isInCombat;
        public bool isFleeing;
        public bool isUnconscious;
        public bool enterInCombatCalled;

        public bool isEssential;

        public bool usesRagdoll;

        // AI Talkative
        public bool dialogueSystemEnabled;
        public string currentDialogueID;
        public string previousDialogueID;
        public string defaultDialogueID;

        public bool isInConversation;
        public bool isTalking;
        public bool isListening;
        
        public bool mustSpeakToPlayer;
        public bool mustSpeakToEntity;

        public int speakerIndex;

        // AI Movements
        public bool usesMovements;
        public bool usesOfflineMode;
        public bool usesTlinkUnstuck;
        public bool isWalking = true;

        public bool canSeeTarget;
        public bool hasCompletlyLostTarget;

        public bool shouldFollowMainTarget;
        public bool shouldFollowOnlyIfCanSee;
        public bool lookAtTarget;
        public bool dialogueLookAt;

        public bool lookAtVector3IfStopped;
        public Vector3 vector3ToLookAtIfStopped;

        public bool overrideShouldFollowMainTargetActive;   // If this is enabled, overrideShouldFollowMainTarget will  be used instead of "shouldFollowMainTarget"
        public bool overrideShouldFollowMainTarget;

        public bool shouldFollowAIPath;
        public bool shouldWander;
        public bool shouldUseNPCActionPoint;


        public bool hasMainTarget;
        public bool shouldFollowTargetVector;

        public string aiCustomPathID;

        //serialize and database actionpoints as well
        //public string actionPointID;

        public float fleeAtDistanceValue;

        public int selectedSteeringBehaviour;
        public int arriveSteeringBehaviourDeceleration;

        public bool invertAIPath;
        public int aiPathCurrentIndex;
        public bool isFollowingAIPath;
        public float stoppingHalt;
        public float stoppingDistance;

        public float generalRotationSpeed;

        public bool cachedMainTarget = false;
        public Vector3 cachedMainTargetPos;

        public bool hasToRandomlySearchTarget;

        public bool isReachingActionPoint;
        public bool fixingInPlaceForActionPoint;
        public bool playingEnterAnimationActionPoint;
        public bool isUsingActionPoint;

        // AI Inventory Equipment
        public bool allowsLootWhenDead;
        public bool allowsLootOfEquipment;

        // AI Perception
        public bool perceptionEnabled;
        public float checkTick;
        public float radius;
        public float sphereYoffset;
        public float sphereZoffset;

        // AI CombatSystem
        public List<string> entityHesFighting;
        public bool isDrawingWeapon = false;
        public bool weaponDrawn = false;

        // AI BehaviourTreed

        public int treeTickRate;
        public int xFrames;
        public float xSeconds;

        public bool useBT;
        public bool pauseBT;

        public string curBehaviourTreeID; 
        public bool isUsingPurposeBehaviour;

        public string purposeBehaviourTreeID;
        public string combatBehaviourTreeID; 
        public bool keepTicking;

        public string startingCellID;
        public string saveCellID;
        public string runtimeSaveCellID;

        public Vector3 hipsPosition;
        public Quaternion hipsRotation;

        public RagdollSaveData ragdollSaveData;

        // Save target
        public string mainTargetID; // not always set
        public int mainTargetType; // ITargetableType


        // Door walk
        public bool isReachingDoor;
        public string cellIDMovingTo;
        public bool enteredInADoorLastTime;
        public bool enteredInADoorLastTimeOverridePos;
        public Vector3 enteredDoorLastTimePos;
        public Quaternion enteredDoorLastTimeRot;
        public string doorEnteredID;

        // PurposeState
        public PurposeState purposeState;

        public bool followTargetOutsideOfCell;

        // PDT (Player Damage Tolerance)
        public bool pdtCounting;
        public int pdtCurCount;
        public float pdtCurTimer;

        // From 1.8
        public bool isMounted;
        public string mountedEntityID;
    }

    [System.Serializable]
    public class AIIDList
    {
        [SerializeField] public List<string> allIDs = new List<string>();
    }

    [System.Serializable]
    public class AIData
    {
        [SerializeField] public AIDictionary aiDictionary = new AIDictionary();
        [SerializeField] public AICellDictionary aiCellDictionary = new AICellDictionary();
    }
}