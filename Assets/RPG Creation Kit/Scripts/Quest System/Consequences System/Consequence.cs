using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// All consequence types. You can add your own in there
    /// </summary>
    public enum ConsequencesTypes
    {
        Null = 0,
        DeactivateGameObject = 1,
        InstantiateGameObject = 2,
        DestroyGameObject = 3,
        ChangeNPCDialogue = 4,
        SendNPCToSpeakToThePlayer = 5,
        AddPlayerQuestionToADialogue = 6,
        ModifyNPCDialgoueLine = 7,
        PermanentlyDeleteQuestionFromDialogue = 8,
        DeleteQuestionFromCurrentDialogue = 9,
        SetNPCHostility = 10,
        SendNPCsToKillEntity = 11,
        ChangeScene = 12,
        Teleport = 13,
        UpdateKillMultipleCounter = 14,
        ChangeNPCPath = 15,
        NPCFollowCurrentPath = 16,
        AlertMessage = 17,
        ResetAllEvents = 18,
        ResetConsequences = 19,
        ResetQuestDealer = 20,
        ResetQuestUpdater = 21,
        RemoveQuestDealerAtIndex = 22,
        RemoveQuestUpdaterAtIndex = 23,
        RemoveConsequenceAtIndex = 24,
        AddPlayerQuestionToASpecificLine = 25,
        AddQuestDealer = 26,
        AddQuestUpdater = 27,
        AddConsequences = 28,
        NPCFollowTarget = 29,
        NPCStopFollowTarget = 30,
        NPCSpeakToNPC = 31,
        AddInInventory = 32,
        RemoveFromInventory = 33,
        RemoveAllFromInventory = 34,
        MakeNPCsRunInFear = 35,
        StartTrading = 36,
        MutateMutable = 37,
        ChangeRckAIDialogue = 38,
        ChangeRckBTree = 39,
        RckAI_AddInFaction = 40,
        RckAI_RemoveFromFaction = 41,
        LockDoor = 42,
        UnlockDoor = 43,
        ClearPurpose = 44,
        PlayerLearnSpell = 45,
        RckAI_ClearMainTarget = 46,
        ForceEnterInCombatAgainstPlayer = 47,
        RCKAI_SpeakOneLine = 48
    };


    [System.Serializable]
    public struct ObjectToInstantiate 
    {
        public GameObject gameObject;
        [Tooltip("The parent of the instantiated GameObject. Leave this NULL to not set any parent)")]
        public Transform parent;
        public Vector3 coordinates;
        public bool localPosition;
    }

    [System.Serializable]
    public struct ObjectToTeleport
    {
        public GameObject gameObject;
        public Vector3 coordinates;
        public bool localPosition;
    }

    [System.Serializable]
    public struct ChangeOfNPCPath
    {
        public NPC_Path newPath;
        public bool goToFollowPath;
        public int startFromIndex;
    }

    [System.Serializable]
    public struct NPCsToChangeDialogue
    {
        public bool setAsDefaultDialogue;
    }

    [System.Serializable]
    public struct NPCsToSendToSpeakToThePlayer
    {
        public bool mustRun;
    }

    [System.Serializable]
    public struct NPCsToSetHostility
    {
        public bool isHostile;
    }

    [System.Serializable]
    public struct NPCsToFollowCurPath
    {
        public int startFromIndex;
    }

    [System.Serializable]
    public struct NPCsToSpeakToNPC
    {
        public bool mustRun;
    }

    [System.Serializable]
    public struct ItemsToModifyInInventory
    {
        public bool isPlayerInventory;
        public string inventoryID;
        public Item item;
        public ItemMetadata metadata;
        public int amount;
    }


    /// <summary>
    /// All the data of the Consequence, you can add your own data here to add your consequences.
    /// </summary>
    [System.Serializable]
    public class Consequence
    {
        /// <summary>
        /// A lot of this data will be reused for different actions, for example the "NPC_Behaviour - NPC" will be used in
        /// different ConsequencesTypes, like the ChangeNPCPath, NPCFollowCurrentPath. 
        /// When you add multiple consequences that involve the same object don't forget that you can reuse the previous data
        /// </summary>

        public ConsequencesTypes type = ConsequencesTypes.AlertMessage;

        // Activate GameObject - Deactivate - Instantiate - Destroy
        public GameObject[] gameObjects;

        public ObjectToInstantiate[] toInstantiate;
        public ObjectToTeleport[] toTeleport;
        public ChangeOfNPCPath[] npcsToChangePath;
        public NPCsToChangeDialogue[] npcsToChangeDialogue;
        public NPCsToSendToSpeakToThePlayer[] npcsToSendToSpeakToThePlayer;
        public NPCsToSetHostility[] npcsToSetHostility;
        public NPCsToFollowCurPath[] npcsToFollowCurPath;
        public NPCsToSpeakToNPC[] npcsToSpeakToNPC;
        public ItemsToModifyInInventory[] itemsToModifyInInventory;
        public ItemsToModifyInInventory itemToModifyInInventory;

        public Transform m_Parent;
        public float delay = 0f;

        // SendNpcToSpeak.. - Change NPC dialogue -- Kill
        public bool mustRun;


        // AddPlayerQuestionToADialogue
        public PlayerQuestion[] Questions;

        // ModifyNPCDialogueLine
        public bool newUseLengtOfClip;
        public AudioClip newAudioClip;
        public float newLineTime;
        [TextArea] public string newLine;

        // Delete Question from Dialogue
        public PlayerQuestion question;

        // Set Hostility
        public bool setAsHostile;

        // Send to kill entity
        public bool isPlayer;

        // Change scene
        public int sceneIndex;
        public Vector3 coordinates;

        // Teleport
        public GameObject gameObject;

        // Update Kill multiple counter

        // Change NPC path - Send NPC path
        public NPC_Path newPath;
        public bool shouldFollowNewPath;
        public int pathStartIndex;

        // Alert Message
        public string AlertMessage;
        public float duration;


        // Reset
        // Goto, NPC_Behaviour, NPCDialogueLine, InteractiveObjectBehaviour, Killmultiplecounter
        public ResetTypes resetType;
        public Goto gotoObject;
        public InteractiveObject_Behaviour interactiveObject;
        public InteractiveOptions interactiveOption;

        //Remove
        public int removeAtIndex;

        // Add Quest Dealer, Quest Updater, Consequences
        public QuestDealer questDealer;
        public QuestUpdater questUpdater;

        // Npc follow target
        public Transform targetTransform;

        // Add / Remove from inventory
        public Inventory inventory;
        public bool isPlayerInventory;
        public Item itemInInventory;
        public int itemAmount = 1;

        // MakeNPCRunInFear
        public float radius = 50;

        public bool isMerchant;
        public Merchant merchant;

        // MutateMutable = 37,
        public string mutableID;
        public bool restoreMutable = false;

        // ChangeRckAIDialogue = 38
        public string rckAID;
        public DialogueSystem.DialogueGraph dialogueGraph;

        public bool immediatlySwitchToBehaviour;
        public BehaviourTree.RPGCK_BT behaviourTree;

        public string factionID;

        public string doorID;
        public RPGCreationKit.CellsSystem.DoorLockLevel lockLevel;

        public Spell spellToLearn;

        // RCKAI_SpeakLine 48
        public string speakLineID;
        public bool speakLineDisplayAsHeard;
    }


    /// <summary>
    /// Class that contains methods to help perform actions between Consequences and Embedded Consequences
    /// </summary>
    public static class ConsequenceHelper
    {

    }

    /// <summary>
    /// Called from the ConsequenceEditor for resetting/removing/adding events
    /// </summary>
    public enum ResetTypes
    {
        GotoOnEnter = 0,
        GotoOnExit = 1,
        NPC_OnDeath = 2,
        NPC_OnAttack = 3,
        NPC_DialogueLine = 4,
        InteractiveObject = 5,
        KillMultipleCounter = 6
    };
}
