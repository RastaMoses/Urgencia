using RPGCreationKit.AI;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.SaveSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit
{
    public class CityInterior_StallOwnersGoToSleep : MonoBehaviour
    {
        public RckAI npc;
        public ItemInWorld[] itemsToEnableDisable;

        public string goToSleepBTreeID;
        public string getBackToWorkBTreeID;
        public Vector3 quickTeleportOnStartLocation;

        bool areNpcsActive = false;

        public Coroutine goToSleepCoroutine;
        public Coroutine getBackToWorkCoroutine;

        void Awake()
        {
            areNpcsActive = false;
        }

        private void OnEnable()
        {
            TimeOfDayManager.instance.onHourChanges -= HandleOnHourChange;
            TimeOfDayManager.instance.onHourChanges += HandleOnHourChange;

            if (TimeOfDayManager.instance != null)
                HandleOnHourChange(TimeOfDayManager.instance.hours);
        }

        private void OnDisable()
        {
            TimeOfDayManager.instance.onHourChanges -= HandleOnHourChange;
        }

        private void OnDestroy()
        {
            TimeOfDayManager.instance.onHourChanges -= HandleOnHourChange;
        }

        private void Start()
        {
            StartCoroutine(nameof(DelayedStart));
        }

        IEnumerator DelayedStart()
        {
            if (npc == null)
                yield break;
            else
            {
                while (!npc.isLoaded)
                    yield return null;

                if (npc.isAlive)
                {
                    int curHour = TimeOfDayManager.instance.hours;

                    // If it's daytime 
                    if (curHour >= 7 && curHour < 20)
                    {
                        // Wait a few frames to update all the npc subsystems
                        for (int i = 0; i < 4; i++)
                            yield return new WaitForEndOfFrame();

                        StartCoroutine(nameof(GetBackToWork));
                        HandleOnHourChange(curHour);
                    }
                    else
                    {
                        // Wait a few frames to update all the npc subsystems
                        for (int i = 0; i < 4; i++)
                            yield return new WaitForEndOfFrame();

                        // If it's nighttime, and we are on start, it means we just loaded in this cell.
                        // This NPC will be at the Stall position, so let's just set him/her near their house so they can disappear for the night
                        npc.transform.position = quickTeleportOnStartLocation;
                        HandleOnHourChange(curHour);
                    }
                }
            }
        }

        public void HandleOnHourChange(int curHour)
        {
            if (npc == null || !npc.isLoaded || !npc.isAlive)
                return;

            // It's better to get the renderer's material instance 
            // rather than modifying the public Asset directly.
            areNpcsActive = !npc.isInOfflineMode;

            if (curHour >= 7 && curHour < 20)
            {
                if (!areNpcsActive)
                {
                    if (getBackToWorkCoroutine != null)
                        StopCoroutine(getBackToWorkCoroutine);

                    getBackToWorkCoroutine = StartCoroutine(nameof(GetBackToWork));
                    areNpcsActive = true;
                }
            }
            else if (areNpcsActive)
            {
                if(goToSleepCoroutine != null)
                    StopCoroutine(goToSleepCoroutine);

                goToSleepCoroutine = StartCoroutine(nameof(GoToSleep));

                areNpcsActive = false;
            }
        }

        private IEnumerator GoToSleep()
        {
            // Cover edge cases (npc unloaded/teleported/destroyed)
            if (npc == null)
                yield break;

            while (!npc.isLoaded)
                yield return null;

            // Cover edge cases (npc unloaded/teleported/destroyed)
            if (npc == null || !npc.isAlive)
                yield break;

            DisableItemsOnStall();
            npc.goToSleepLogicEnabled = true;
            npc.SetNewBehaviourTree(false, goToSleepBTreeID);
            npc.SwitchBehaviourTree(false);

            // Make sure to update target vector via BTree call
            npc.DoOneBTreeTick();

            while (Vector2.Distance(npc.transform.position, npc.targetVector) > 1.0f)
            {
                yield return null;
            }

            // Cover edge cases (npc unloaded/teleported/destroyed)
            if (npc == null)
                yield break;

            // Update the save file before setting her as offline
            npc.SaveOnFile();

            while (npc.isInConversation)
                yield return null;

            npc.usesMovements = false;
            npc.EGoOffline();
        }

        private IEnumerator GetBackToWork()
        {
            // Cover edge cases (npc unloaded/teleported/destroyed)
            if (npc == null)
                yield break;

            while (!npc.isLoaded)
                yield return null;

            // Cover edge cases (npc unloaded/teleported/destroyed)
            if (npc == null || !npc.isAlive)
                yield break;

            npc.usesMovements = true;

            npc.EGoOnline();

            npc.SetNewBehaviourTree(false, getBackToWorkBTreeID);
            npc.SwitchBehaviourTree(false);

            EnableItemsOnStall();

            npc.SaveOnFile();

            yield return null;
        }

        public void DisableItemsOnStall()
        {
            foreach(ItemInWorld i in itemsToEnableDisable)
            {
                if (i != null)
                {
                    ItemInWorldSaveData data;
                    SaveSystemManager.instance.saveFile.ItemsInWorldData.allItemsInWorld.TryGetValue(i.GUIDStr, out data);

                    i.gameObject.SetActive(false);
                }
            }
        }

        public void EnableItemsOnStall()
        {
            foreach (ItemInWorld i in itemsToEnableDisable)
            {
                if (i != null)
                {
                    ItemInWorldSaveData data;
                    SaveSystemManager.instance.saveFile.ItemsInWorldData.allItemsInWorld.TryGetValue(i.GUIDStr, out data);

                    i.gameObject.SetActive(true);
                }
            }
        }
    }
}
