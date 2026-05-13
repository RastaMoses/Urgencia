using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit
{
    // Who should interact with the current Goto element
    public enum CollisionTargets { Player, NPCs, PlayerAndNpcs, TriggerGameObject };


    /// <summary>
    /// Goto script, use a trigger collider to perform events when the Player or some NPCs (or both) enter in that trigger
    /// </summary>
    public class Goto : MonoBehaviour
    {
        public string onEnterResultScript = string.Empty;
        public string onExitResultScript = string.Empty;

        public Events onEnterEvents;
        public Events onExitEvents;


        [Space(10)]

        [Header("Script")]
        public CollisionTargets collisionTarget;


        // Wait before executing consequences & dealers
        [HideInInspector] public float OnEnterDelayActivation = 0f;
        [HideInInspector] public float OnExitDelayActivation = 0f;

        // Check if the quests and consequences are been already applied
        [HideInInspector] public bool OnEnterAlreadyTriggered = false;
        [HideInInspector] public bool OnExitAlreadyTriggered = false;

        [HideInInspector] public bool AllowMultipleOnEnterTriggering = false;
        [HideInInspector] public bool AllowMultipleOnExitTriggering = false;

        // Select to destroy or keeping this object in the scene after triggering
        [HideInInspector] public bool DestroyAfterOnEnter = false;
        [HideInInspector] public float DestroyDelayOnEnter = 0;

        [HideInInspector] public bool DestroyAfterOnExit = false;
        [HideInInspector] public float DestroyDelayOnExit = 0;

        public Collider TriggerGameObject;

        private void OnTriggerEnter(Collider other)
        {
            StartCoroutine(OnTriggerEnterTask(other));
        }

        private void OnTriggerExit(Collider other)
        {
            StartCoroutine(OnTriggerExitTask(other));
        }

        IEnumerator OnTriggerEnterTask(Collider other)
        {
            while (WorldManager.instance == null || Player.RckPlayer.instance == null ||
               WorldManager.instance.isLoading || !Player.RckPlayer.instance.IsControlledByPlayer() || !CellsSystem.CellInformation.AllActiveCellsLoaded())
                yield return new WaitForEndOfFrame();

            if ((!AllowMultipleOnEnterTriggering && OnEnterAlreadyTriggered) || other == null) yield break;

            // Check the collisionTarget and perform actions if it's the correct case
            switch (collisionTarget)
            {
                case CollisionTargets.Player:
                    if (other.CompareTag("Player"))
                    {
                        OnEnterAlreadyTriggered = true;

                        Invoke("CallOnEnterActivator", OnEnterDelayActivation);
                    }
                    // Check if it's the player but mounted
                    else if(other.CompareTag("RPG Creation Kit/AI"))
                    {
                        MountAI mAI = null;
                        if (mAI = other.GetComponent<MountAI>())
                        {
                            if(mAI.playerMounting)
                            {
                                OnEnterAlreadyTriggered = true;

                                Invoke("CallOnEnterActivator", OnEnterDelayActivation);
                            }
                        }
                    }
                        break;

                case CollisionTargets.NPCs:
                    break;

                case CollisionTargets.PlayerAndNpcs:

                    if (other.CompareTag("Player"))
                    {
                        OnEnterAlreadyTriggered = true;

                        Invoke("CallOnEnterActivator", OnEnterDelayActivation);
                    }
                    else if (other.CompareTag("NPC"))
                    {
                    }
                    break;

                case CollisionTargets.TriggerGameObject:
                    if (other == TriggerGameObject)
                    {
                        OnEnterAlreadyTriggered = true;

                        Invoke("CallOnEnterActivator", OnEnterDelayActivation);
                    }
                    break;
            }
        }
        IEnumerator OnTriggerExitTask(Collider other)
        {
            while (WorldManager.instance == null || Player.RckPlayer.instance == null ||
                    WorldManager.instance.isLoading || !Player.RckPlayer.instance.IsControlledByPlayer() || !CellsSystem.CellInformation.AllActiveCellsLoaded())
                        yield return new WaitForEndOfFrame();

            if ((!AllowMultipleOnExitTriggering && OnExitAlreadyTriggered) || other == null) yield break;

            // Check the collisionTarget and perform actions if it's the correct case
            switch (collisionTarget)
            {
                case CollisionTargets.Player:
                    if (other.CompareTag("Player"))
                    {
                        OnExitAlreadyTriggered = true;

                        Invoke("CallOnExitActivator", OnExitDelayActivation);
                    }
                    break;

                case CollisionTargets.NPCs:
                    if (other.CompareTag("NPC"))
                    {

                    }
                    break;

                case CollisionTargets.PlayerAndNpcs:

                    if (other.CompareTag("Player"))
                    {
                        OnExitAlreadyTriggered = true;

                        Invoke("CallOnExitActivator", OnExitDelayActivation);
                    }
                    else if (other.CompareTag("NPC"))
                    {
                    }
                    break;

                case CollisionTargets.TriggerGameObject:
                    if (other.transform.gameObject == TriggerGameObject.gameObject)
                    {
                        OnExitAlreadyTriggered = true;

                        Invoke("CallOnExitActivator", OnExitDelayActivation);
                    }
                    break;
            }
        }

        /// <summary>
        /// Called from the OnTriggerEnter when the correct case was satisfied, triggers all events
        /// </summary>
        private void CallOnEnterActivator()
        {
            if (onEnterEvents.EvaluateConditions())
            {
                if (!string.IsNullOrEmpty(onEnterResultScript))
                    RCKFunctions.ExecuteScript(onEnterResultScript);

                QuestManager.instance.QuestDealerActivator(onEnterEvents.questDealers);
                QuestManager.instance.QuestUpdaterActivator(onEnterEvents.questUpdaters);
                ConsequenceManager.instance.ConsequencesActivator(gameObject, onEnterEvents.consequences);
            }
            if (DestroyAfterOnEnter)
                Destroy(gameObject, DestroyDelayOnEnter);
        }

        /// <summary>
        /// Called from the OnTriggerExit when the correct case was satisfied, triggers all events
        /// </summary>
        private void CallOnExitActivator()
        {
            if (onExitEvents.EvaluateConditions())
            {
                if (!string.IsNullOrEmpty(onExitResultScript))
                    RCKFunctions.ExecuteScript(onExitResultScript);

                QuestManager.instance.QuestDealerActivator(onExitEvents.questDealers);
                QuestManager.instance.QuestUpdaterActivator(onExitEvents.questUpdaters);
                ConsequenceManager.instance.ConsequencesActivator(gameObject, onExitEvents.consequences);
            }

            if (DestroyAfterOnExit)
                Destroy(gameObject, DestroyDelayOnExit);
        }

    }
}