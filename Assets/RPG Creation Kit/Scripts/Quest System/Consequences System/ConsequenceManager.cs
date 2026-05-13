using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.SaveSystem;
using RPGCreationKit.AI;

namespace RPGCreationKit
{

    /// <summary>
    /// Class that execute the consequences. All the consequences logic is written here
    /// </summary>
    public class ConsequenceManager : MonoBehaviour
    {

        #region Singleton
        public static ConsequenceManager instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Debug.LogError("Anomaly detected with the Singleton Pattern of 'ConsequenceManager', are you using multple ConsequenceManager?");
                Destroy(this);
            }
        }

        #endregion

        [Header("References")]
        // Used in DeleteQuestionFromCurrentDialogue
        public Transform PlayerQuestionContent;


        /// <summary>
        /// Activate a List of consequences
        /// </summary>
        /// <param name="gObjectCaller">The gameObject that calls this function</param>
        /// <param name="consequences">The list of consequences to execute</param>
        public void ConsequencesActivator(GameObject gObjectCaller, List<Consequence> consequences)
        {
            for (int i = 0; i < consequences.Count; i++)
            {
                ApplyConsequence(gObjectCaller, consequences[i]);
            }
        }

        /// <summary>
        /// Execute a consequence
        /// </summary>
        /// <param name="gObjectCaller">The gameObject that calls this function</param>
        /// <param name="consec">The consequence to execute</param>
        private void ApplyConsequence(GameObject gObjectCaller, Consequence consec)
        {
            switch (consec.type)
            {
                case ConsequencesTypes.ChangeScene:
                    SceneManager.LoadScene(consec.sceneIndex);
                    break;

                case ConsequencesTypes.Teleport:
                    #region code
                    for (int i = 0; i < consec.toTeleport.Length; i++)
                    {
                        if (consec.toTeleport[i].gameObject != null)
                        {
                            // If we have to teleport a NPC we have firstly to disable him (NavMesh problem)
                            if (consec.toTeleport[i].gameObject.CompareTag("NPC"))
                            {
                                consec.toTeleport[i].gameObject.SetActive(false);

                                if (consec.toTeleport[i].localPosition)
                                    consec.toTeleport[i].gameObject.transform.localPosition = consec.toTeleport[i].coordinates;
                                else
                                    consec.toTeleport[i].gameObject.transform.position = consec.toTeleport[i].coordinates;

                                consec.toTeleport[i].gameObject.SetActive(true);

                            }
                            else if (consec.toTeleport[i].gameObject.CompareTag("Player"))
                            {
                                // If the is in conversation
                                if (Player.RckPlayer.instance.isInConversation)
                                {
                                    // Start the coroutine that will wait until the dialogue ended and repeat this consequence
                                    StartCoroutine(DelayConsequence(consec));
                                }
                                else
                                {
                                    consec.toTeleport[i].gameObject.SetActive(false);

                                    if(consec.toTeleport[i].localPosition)
                                        consec.toTeleport[i].gameObject.transform.localPosition = consec.toTeleport[i].coordinates;
                                    else
                                        consec.toTeleport[i].gameObject.transform.position = consec.toTeleport[i].coordinates;

                                    consec.toTeleport[i].gameObject.SetActive(true);
                                }
                            }
                            else
                            {
                                // For every other type of gameObject
                                if (consec.toTeleport[i].localPosition)
                                    consec.toTeleport[i].gameObject.transform.localPosition = consec.toTeleport[i].coordinates;
                                else
                                    consec.toTeleport[i].gameObject.transform.position = consec.toTeleport[i].coordinates;
                            }
                        }
                        else
                        {
                            Debug.LogError("Tried to teleport an unassigned GameObject in the consequences of the GameObject: " + gObjectCaller.name);
                        }
                    }
                    #endregion
                    break;

                case ConsequencesTypes.AlertMessage:
                    AlertMessage.instance.InitAlertMessage(consec.AlertMessage, consec.duration);
                    break;

                case ConsequencesTypes.AddInInventory:
                    #region code
                    if (consec.itemToModifyInInventory.item != null)
                    {
                        if (consec.itemToModifyInInventory.isPlayerInventory)
                            Inventory.PlayerInventory.AddItem(consec.itemToModifyInInventory.item, consec.itemToModifyInInventory.metadata, consec.itemToModifyInInventory.amount);
                    }
                    #endregion
                    break;

                case ConsequencesTypes.RemoveFromInventory:
                    #region code
                    /*
                    if (consec.inventory != null && !consec.isPlayerInventory || consec.itemInInventory != null)
                    {
                        if (consec.isPlayerInventory)
                            for (int i = 0; i < consec.itemAmount; i++)
                                Inventory.PlayerInventory.ConsequenceRemoveItem(consec.itemInInventory);
                        else
                            for (int i = 0; i < consec.itemAmount; i++)
                                consec.inventory.ConsequenceRemoveItem(consec.itemInInventory);
                    }
                    else
                        Debug.LogError("Tried to remove from Inventory an unassigned Inventory or ItemInInventory in the consequences of the GameObject: " + gObjectCaller.name);
                    */

                    if(consec.itemToModifyInInventory.item != null)
                    {
                        if (consec.itemToModifyInInventory.isPlayerInventory)
                            Inventory.PlayerInventory.RemoveItem(consec.itemToModifyInInventory.item, consec.itemToModifyInInventory.amount);
                    }

                    #endregion
                    break;

                case ConsequencesTypes.RemoveAllFromInventory:
                    #region code
                    if (consec.inventory != null || consec.isPlayerInventory)
                    {
                        if (consec.isPlayerInventory)
                        {
                            Inventory.PlayerInventory.ClearInventory();
                            PlayerCombat.instance.RemoveCurrentWeapon();
                            PlayerInInventory.instance.OnEquipmentChangesHands();
                        }
                        else
                            consec.inventory.ClearInventory();
                    }
                    else
                        Debug.LogError("Tried to remove from Inventory an unassigned Inventory or ItemInInventory in the consequences of the GameObject: " + gObjectCaller.name);
                    #endregion
                    break;

                case ConsequencesTypes.MutateMutable:
                    #region code

                    if (!string.IsNullOrEmpty(consec.mutableID))
                        RCKFunctions.MutateMutable(consec.mutableID, consec.restoreMutable);
                    else
                        Debug.Log("Tried to mutate in Consequences, but no ID was provided.");

                    #endregion
                    break;

                case ConsequencesTypes.ChangeRckAIDialogue:
                    #region code

                    var allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

                    RckAI ai = null;
                    CellInformation.TryToGetAI(consec.rckAID, out ai); // Tries to grab the NPC if he's loaded - null if he's not

                    if (ai != null) 
                    {
                        ai.currentDialogueGraph = consec.dialogueGraph;
                    }
                    else if (allAI.ContainsKey(consec.rckAID))
                    {
                        AISaveData data = allAI[consec.rckAID];

                        allAI[consec.rckAID].currentDialogueID = consec.dialogueGraph.ID;
                    }
                    // Check if it is a persistent reference
                    else if(PersistentReferences.PersistentReferenceManager.instance.refs.TryGetValue(consec.rckAID, out var reference))
                    {
                        ai = reference.GetComponent<RckAI>();
                        ai.currentDialogueGraph = consec.dialogueGraph;
                    }
                    else
                    {
                        Debug.LogWarning("Tried to change dialogue to an NPC that isn't loaded and wasn't met before, this is not supported.");
                    }

                    #endregion
                    break;

                case ConsequencesTypes.ChangeRckBTree:
                    #region code
                    allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

                    ai = null;
                    CellInformation.TryToGetAI(consec.rckAID, out ai); // Tries to grab the NPC if he's loaded - null if he's not

                    if (ai != null)
                    {
                        ai.SetNewBehaviourTree(consec.behaviourTree.IsCombatBehaviour, consec.behaviourTree.ID);

                        if (consec.immediatlySwitchToBehaviour)
                            ai.SwitchBehaviourTree(consec.behaviourTree.IsCombatBehaviour);
                    }
                    else if (allAI.ContainsKey(consec.rckAID))
                    {
                        AISaveData data = allAI[consec.rckAID];

                        if (consec.behaviourTree.IsCombatBehaviour)
                            data.combatBehaviourTreeID = consec.behaviourTree.ID;
                        else
                            data.purposeBehaviourTreeID = consec.behaviourTree.ID;

                        if (consec.immediatlySwitchToBehaviour)
                            data.curBehaviourTreeID = consec.behaviourTree.ID;

                        allAI[consec.rckAID].currentDialogueID = consec.dialogueGraph.ID;
                    }
                    else
                    {
                        Debug.LogWarning("Tried to change dialogue to an NPC that isn't loaded and wasn't met before, this is not supported.");
                    }

                    #endregion
                    break;

                case ConsequencesTypes.RckAI_AddInFaction:
                    #region code
                    allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

                    ai = null;
                    CellInformation.TryToGetAI(consec.rckAID, out ai); // Tries to grab the NPC if he's loaded - null if he's not

                    if (ai != null)
                    {
                        ai.AddToFaction(consec.factionID);
                    }
                    else if (allAI.ContainsKey(consec.rckAID))
                    {
                        AISaveData data = allAI[consec.rckAID];

                        data.allFactions.Add(consec.factionID);
                    }
                    else
                    {
                        Debug.LogWarning("Tried to Add In Faction an NPC that isn't loaded and wasn't met before, this is not supported.");
                    }

                    #endregion
                    break;

                case ConsequencesTypes.RckAI_RemoveFromFaction:
                    #region code
                    allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

                    ai = null;
                    CellInformation.TryToGetAI(consec.rckAID, out ai); // Tries to grab the NPC if he's loaded - null if he's not

                    if (ai != null)
                    {
                        ai.RemoveFromFaction(consec.factionID);
                    }
                    else if (allAI.ContainsKey(consec.rckAID))
                    {
                        AISaveData data = allAI[consec.rckAID];

                        data.allFactions.Remove(consec.factionID);
                    }
                    else
                    {
                        Debug.LogWarning("Tried to Remove From Faction an NPC that isn't loaded and wasn't met before, this is not supported.");
                    }

                    #endregion
                    break;

                case ConsequencesTypes.LockDoor:
                    RCKFunctions.LockDoor(consec.doorID, consec.lockLevel);
                    break;

                case ConsequencesTypes.UnlockDoor:
                    RCKFunctions.UnlockDoor(consec.doorID);
                    break;

                case ConsequencesTypes.ClearPurpose:
                    #region code
                    allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

                    ai = null;
                    CellInformation.TryToGetAI(consec.rckAID, out ai); // Tries to grab the NPC if he's loaded - null if he's not

                    if (ai != null)
                    {
                        ai.purposeState.ClearPurpose();
                    }
                    else if (allAI.ContainsKey(consec.rckAID))
                    {
                        AISaveData data = allAI[consec.rckAID];

                        data.purposeState = null;
                    }
                    else
                    {
                        Debug.LogWarning("Tried to Add Clear Purpose an NPC that isn't loaded and wasn't met before, this is not supported.");
                    }

                    #endregion
                    break;

                case ConsequencesTypes.RckAI_ClearMainTarget:
                    #region code
                    allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

                    ai = null;
                    CellInformation.TryToGetAI(consec.rckAID, out ai); // Tries to grab the NPC if he's loaded - null if he's not

                    if (ai != null)
                    {
                        ai.hasMainTarget = false;
                        ai.mainTarget = null;
                    }
                    else
                    {
                        Debug.LogWarning("Tried to Clear MainTarget of an NPC that isn't loaded, this is not supported.");
                    }

                    #endregion
                    break;

                case ConsequencesTypes.PlayerLearnSpell:
                    #region
                    if(consec.spellToLearn != null)
                        SpellsKnowledge.Player.LearnSpell(consec.spellToLearn);
                    #endregion
                    break;

                case ConsequencesTypes.ForceEnterInCombatAgainstPlayer:
                    #region
                    allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

                    ai = null;
                    CellInformation.TryToGetAI(consec.rckAID, out ai); // Tries to grab the NPC if he's loaded - null if he's not

                    if (ai != null)
                    {
                        ai.ForceEnterInCombatAgainstPlayer();
                    }
                    else
                    {
                        Debug.LogWarning("Tried to Clear ForceEnterInCombatAgainstPlayer of an NPC that isn't loaded, this is not supported.");
                    }
                    #endregion
                    break;

                case ConsequencesTypes.RCKAI_SpeakOneLine:
                    #region
                    allAI = SaveSystemManager.instance.saveFile.AIData.aiDictionary;

                    ai = null;
                    CellInformation.TryToGetAI(consec.rckAID, out ai); // Tries to grab the NPC if he's loaded - null if he's not

                    if (ai != null)
                    {
                        RCKFunctions.MakeAISpeakLine(ai, consec.speakLineID);
                        if (consec.speakLineDisplayAsHeard)
                            RCKFunctions.DisplayHeardLine("...", 3.0f);
                    }
                    else
                    {
                        Debug.LogWarning("Tried to Clear ForceEnterInCombatAgainstPlayer of an NPC that isn't loaded, this is not supported.");
                    }
                    #endregion
                    break;
            }
        }

        /// <summary>
        /// Delay a consequence, only called from Consequences activated from NPCDialogueLines
        /// </summary>
        /// <param name="consec">The consequence to delay</param>
        /// <returns></returns>
        private IEnumerator DelayConsequence(Consequence consec)
        {
            // Delay for conversation
            while (Player.RckPlayer.instance.isInConversation)
            {
                yield return null;
            }

            // You can add other delays here


            // Redo the consequence
            ApplyConsequence(gameObject, consec);
        }

        private IEnumerator DelayConsequence2(Consequence consec)
        {
            // Delay for conversation
            while (Player.RckPlayer.instance.isInConversation)
            {
                yield return null;
            }

            // You can add other delays here


            // Redo the consequence
            ApplyConsequence(gameObject, consec);
        }
    }
}