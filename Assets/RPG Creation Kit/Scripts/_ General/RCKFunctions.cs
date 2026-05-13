using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using System;
using UnityEngine.SceneManagement;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.AI;
using RPGCreationKit.PersistentReferences;
using RPGCreationKit.SaveSystem;

namespace RPGCreationKit
{
    public enum RCKScriptType
    {
        QuestScript = 0,
        QuestStageScript = 1,
        ItemScript = 2,
        ResultScript = 3
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class IsConditionAttribute : Attribute { }

    /// <summary>
    /// This class provides quick static access to useful methods and functions
    /// </summary>
    public static class RCKFunctions 
    {
        public static void ExecuteScript(string fullScriptPath)
        {
            QuestScriptManager.instance.scriptsHolder.AddComponent(System.Type.GetType(fullScriptPath));
        }

        public static void ExecuteEvents(Events events, GameObject caller = null)
        {
            QuestManager.instance.QuestDealerActivator(events.questDealers);
            QuestManager.instance.QuestUpdaterActivator(events.questUpdaters);
            ConsequenceManager.instance.ConsequencesActivator(caller, events.consequences);
        }

        public static void AddQuest(string newQuestID, int atIndex = 0, bool hideAlert = false)
        {
            QuestManager.instance.AddQuest(newQuestID, atIndex, hideAlert);
        }

        public static void AddQuestParam(Quest newQuest, int atIndex = 0, bool hideAlert = false)
        {
            QuestManager.instance.AddQuest(newQuest, atIndex, hideAlert);
        }

        [IsCondition]
        public static int GetStage(string _questID)
        {
            return QuestManager.instance.GetStage(_questID);
        }

        [IsCondition]
        public static int GetStageFailed(string _questID, int _stage)
        {
            return QuestManager.instance.GetStageFailed(_questID, _stage);
        }

        [IsCondition]
        public static int GetStageCompleted(string _questID, int _stage)
        {
            return QuestManager.instance.GetStageCompleted(_questID, _stage);
        }

        [IsCondition]
        public static int GetPlayerEquipped(string _itemID)
        {
            return Equipment.PlayerEquipment.IsEquipped(_itemID) ? 1 : 0;
        }

        public static bool GetPlayerEquippedBool(string _itemID)
        {
            return Equipment.PlayerEquipment.IsEquipped(_itemID);
        }

        public static void SetQuestStage(string _questID, int _stageIndex)
        {
            QuestManager.instance.SetQuestStage(_questID, _stageIndex);
        }

        public static void CompleteQuestStage(string _questID, int _stageIndex)
        {
            QuestManager.instance.CompleteQuestStage(_questID, _stageIndex);
        }

        public static void CompleteQuestStage(string _questID, int _stageIndex, bool hideLog = false, bool _comesFromLoad = false)
        {
            QuestManager.instance.CompleteQuestStage(_questID, _stageIndex, hideLog, _comesFromLoad);
        }


        public static void SetQuestStageParam(Quest _quest, QuestStage _questStage)
        {
            QuestManager.instance.SetQuestStage(_quest, _questStage);
        }


        public static void CompleteQuest(Quest _quest)
        {
            QuestManager.instance.CompleteQuest(_quest);
        }

        public static void FailQuestStage(string _questID, int _stageIndex, bool hideLog = false, bool _comesFromLoad = false)
        {
            QuestManager.instance.FailQuestStage(_questID, _stageIndex, hideLog, _comesFromLoad);
        }


        [IsCondition]
        public static bool IsQuestCompleted(string _questID)
        {
            return QuestManager.instance.IsQuestCompleted(_questID);
        }

        public static int GetIsID(string _id, int c64)
        {
            return 0;
        }

        public static void DisplayCurrentQuestObjective(bool isCompleted)
        {
            QuestAlertManager.instance.DisplayCurrentQuestObjective(isCompleted);
        }

        public static void DisplayQuestObjective(string questID, int stageIndex, bool isCompleted)
        {
            QuestAlertManager.instance.DisplayQuestObjective(questID, stageIndex, isCompleted);
        }

        public static void DisplayQuestObjective(Quest quest, int stageIndex, bool isCompleted)
        {
            QuestAlertManager.instance.DisplayQuestObjective(quest, stageIndex, isCompleted);
        }

        public static void InitQuestAlert(Quest newQuest, bool isCompleted)
        {
            QuestAlertManager.instance.InitQuestAlert(newQuest.questName, !isCompleted);
        }

        public static bool VerifyConditions(Condition[] conditions)
        {
            // No conditions
            if (conditions.Length == 0)
                return true;

            // At least 1 condition
            int orConditionsNumber = 1;

            for (int i = 0; i < conditions.Length; i++)
                if (conditions[i].OR)
                    orConditionsNumber++;

            bool[] evaluatedBlocks = new bool[orConditionsNumber];

            int lastIndex = -1;
            for (int i = 0; i < orConditionsNumber; i++)
            {
                List<bool> currentBlockConditions = new List<bool>();
                for (int j = lastIndex + 1; j < conditions.Length; j++)
                {
                    // Add this element
                    currentBlockConditions.Add(conditions[j].conditionValue);

                    if (conditions[j].OR)
                    {
                        // Add this element
                        lastIndex = j;
                        break;
                    }
                }


                /* Debug of Blocks
                Debug.LogWarning("-----BLOCK N: " + i);
                for(int z = 0; z < currentBlockConditions.Count; z++)
                {
                    Debug.Log(currentBlockConditions[z]);
                }
                Debug.LogWarning("-----END N: " + i);
                */

                bool absEvaluatedBlock = false;

                // Fill the evaluetedBlocks.
                for (int p = 0; p < currentBlockConditions.Count; p++)
                {
                    if (currentBlockConditions[p] == false)
                    {
                        absEvaluatedBlock = false;
                        break;
                    }

                    absEvaluatedBlock = true;
                }

                evaluatedBlocks[i] = absEvaluatedBlock;
            }

            // Check if there is at least a single true in blocks
            for (int i = 0; i < evaluatedBlocks.Length; i++)
            {
                if (evaluatedBlocks[i]) return true;
            }

            return false;
        }

        [IsCondition]
        public static int GetItemCount(Inventory inventory, string itemID)
        {
            return inventory.GetItemCount(itemID);
        }

        [IsCondition]
        public static int GetItemCountPlayer(string itemID)
        {
            return Inventory.PlayerInventory.GetItemCount(itemID);
        }

        [IsCondition]
        public static int GetItemCountStolenPlayer(string itemID)
        {
            return Inventory.PlayerInventory.GetItemCountStolen(itemID);
        }

        [IsCondition]
        public static int GetItemCountNotStolenPlayer(string itemID)
        {
            return Inventory.PlayerInventory.GetItemCountNonStolen(itemID);
        }

        [IsCondition]
        public static int GetItemCountBothStolenAndNotPlayer(string itemID)
        {
            return Inventory.PlayerInventory.GetItemCountBothStolenAndNot(itemID);
        }

        [IsCondition]
        public static float GetPlayerMaxHealth()
        {
            return GlobalVariables.PlayerMaxHealth;
        }

        [IsCondition]
        public static float GetCurrentTimeOfDay()
        {
            return TimeOfDayManager.instance.GetCurrentTime();
        }

        [IsCondition]
        public static float GetCurrentHourOfDay()
        {
            return TimeOfDayManager.instance.hours;
        }

        [IsCondition]
        public static float GetCurrentMinutesOfDay()
        {
            return TimeOfDayManager.instance.minutes;
        }

        public static bool IsNumber(object value)
        {
            if (value is sbyte) return true;
            if (value is byte) return true;
            if (value is short) return true;
            if (value is ushort) return true;
            if (value is int) return true;
            if (value is uint) return true;
            if (value is long) return true;
            if (value is ulong) return true;
            if (value is float) return true;
            if (value is double) return true;
            if (value is decimal) return true;
            return false;
        }

        public static Quest GetQuest(string _questID)
        {
            return QuestsDatabase.GetQuest(_questID);
        }

        public static void StartTrading(Merchant merchant)
        {
            if (merchant != null)
                TradeSystemUI.instance.OpenTradePanel(merchant);
            else
                Debug.LogWarning("Tried to StartTrading but there was no Merchant given");
        }

        /*
        public static void StartTrading(NPC_Behaviour npc)
        {
            TradeSystemUI.instance.OpenTradePanel(npc, true);
        }
        */

        /// <summary>
        /// Gets the distance of the Persistent AI and the player and returns it as a float. Returns the special value 99999.12f if the the Player is in a different cell.
        /// </summary>
        /// <param name="_npcRef"></param>
        /// <returns></returns>
        [IsCondition]
        public static float AI_GetDistanceFromPC(string _npcRef)
        {
            if (IsAILoadedForPlayer(_npcRef))
            {
                // Get the NPC behaviour
                PersistentReference npcRef = PersistentReferenceManager.instance.GetPersistentReference(_npcRef);

                if (npcRef != null)
                    return (Vector3.Distance(Player.RckPlayer.instance.transform.position, npcRef.transform.position));
                else
                    return 99999.12f;
            }
            else
                return 99999.12f;
        }

        public static int GetPCInFaction(string _factionID)
        {
            //return Interactor.instance.
            return 001;
        }

        public static GameObject GetChildWithName(GameObject obj, string name)
        {
            Transform t = obj.transform;
            Transform childT = t.Find(name);
            if (childT != null)
            {
                return childT.gameObject;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns true if the provided AI is loaded and present in the same cell of the Player or in a neighbour cell.
        /// </summary>
        public static bool IsAILoadedForPlayer(string _npcRef)
        {
            var npc = PersistentReferences.PersistentReferenceManager.instance.GetPersistentReference(_npcRef);
            if (npc != null)
            {
                if (npc.isActiveAndEnabled && !npc.GetComponent<RPGCreationKit.AI.RckAI>().isInOfflineMode)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Usage: GameObject.scene.GetCellInformation - Gets the CellInformation of a GameObject, ergo the cell in which the gameobject is at any given point
        /// </summary>
        /// <param name="_scene"></param>
        /// <returns></returns>
        public static CellInformation GetCellInformation(this Scene _scene)
        {
            try
            {
                GameObject[] gos = SceneManager.GetSceneByPath(_scene.path).GetRootGameObjects();

                for (int z = 0; z < gos.Length; z++)
                {
                    if (gos[z].CompareTag("RPG Creation Kit/CellInfo"))
                        return gos[z].GetComponent<CellInformation>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static bool ContainsBiped(BipedObject[] arr, BipedObject target)
        {
            for(int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == target)
                    return true;
            }

            return false;
        }


        [IsCondition]
        public static int IsAIAlive(string _entityID)
        {
            RckAI ai = null;
            CellInformation.TryToGetAI(_entityID, out ai);

            if (ai != null)
                return ai.isAlive ? 1 : 0;
            else if (SaveSystemManager.instance.saveFile.AIData.aiDictionary.ContainsKey(_entityID))
            {
                // Look in the save files
                AISaveData aiData = SaveSystemManager.instance.saveFile.AIData.aiDictionary[_entityID];

                return aiData.isAlive ? 1 : 0;
            }
            else
                return 1; 
        }

        [IsCondition]
        public static int IsAILoaded(string _entityID)
        {
            RckAI ai = null;
            CellInformation.TryToGetAI(_entityID, out ai);

            return (ai != null) ? 1 : 0;

        }


        public static void MutateMutable(string _mutableID, bool _restoreMutable)
        {
            Mutable mutable = null;
            if (CellInformation.TryToGetMutable(_mutableID, out mutable))
            {
                if (!_restoreMutable)
                    mutable.Mutate();
                else
                    mutable.RestoreToNormal();
            }
            // Check in the persistent references
            else if(PersistentReferenceManager.instance.persistentMutableDictionary.TryGetValue(_mutableID, out mutable))
            {
                if (!_restoreMutable)
                    mutable.Mutate();
                else
                    mutable.RestoreToNormal();
            }
            else // Update the save file directly
            {
                var allMutables = SaveSystemManager.instance.saveFile.MutablesData.allMutables;

                if (!_restoreMutable)
                {
                    if (allMutables.ContainsKey(_mutableID))
                        allMutables[_mutableID].isMutated = true;
                    else
                        allMutables.Add(_mutableID, new MutableData(true));
                }
                else
                {
                    if (allMutables.ContainsKey(_mutableID))
                        allMutables[_mutableID].isMutated = false;
                    else
                        allMutables.Add(_mutableID, new MutableData(false));
                }
            }
        }


        /// <summary>
        /// Unlocks a door, loaded or not.
        /// </summary>
        /// <param name="_doorID">The ID of the door</param>
        public static void UnlockDoor(string _doorID)
        {
            Door door = null;

            // If Door is loaded
            if (CellInformation.TryToGetDoor(_doorID, out door))
            {
                door.UnlockDoor();
            }
            else // Update the save file directly
            {
                var allDoors = SaveSystemManager.instance.saveFile.DoorData.allDoors;

                if (allDoors.ContainsKey(_doorID))
                    allDoors[_doorID].isLocked = false;
                else
                    allDoors.Add(_doorID, new DoorSaveData(_doorID, false));
            }
        }

        /// <summary>
        /// Locks a door, loaded or not.
        /// </summary>
        /// <param name="_doorID">The ID of the door</param>
        public static void LockDoor(string _doorID, DoorLockLevel _doorLockLevel)
        {
            Door door = null;
            if (CellInformation.TryToGetDoor(_doorID, out door))
            {
                door.LockDoor(_doorLockLevel);
            }
            else // Update the save file directly
            {
                var allDoors = SaveSystemManager.instance.saveFile.DoorData.allDoors;

                if (allDoors.ContainsKey(_doorID))
                {
                    allDoors[_doorID].isLocked = true;
                    allDoors[_doorID].doorLockLevel = (int)_doorLockLevel;
                }
                else
                    allDoors.Add(_doorID, new DoorSaveData(_doorID, true, (int)_doorLockLevel));
            }
        }

        public static void DisplayHeardLine(string _line, float _time)
        {
            Player.RckPlayer.instance.DisplayHeardLine(_line, _time);
        }

        public static void MakeAISpeakLine(RckAI _ai, string _specialAudioClipID)
        {
            if (_ai != null)
            {
                AudioClip clip = AudioClipsDatabase.GetItem(_specialAudioClipID);

                if (clip != null)
                {
                    _ai.audioSource.PlayOneShot(clip);

                    if(RCKSettings.NPCS_FACIAL_ANIM_ENABLED)
                        _ai.QuickDialogueMouthAnim(clip);
                }
                else
                    Debug.LogWarning("Tried to for an AI to speak the line with ID \"" + _specialAudioClipID + "\" but the clip is not present in the AudioClips database.");
            }
        }


        public static RckAI SpawnAIInCurrentCell(string _ID, Vector3 _pos, Quaternion _rot)
        {
            return CellInformation.activeCells[WorldManager.instance.currentCenterCell.ID].SpawnNewAI(_ID, _pos, _rot);
        }

        public static void SpawnAIInDistantCell(string _ID, string _cellID, Vector3 _pos, Quaternion _rot)
        {
            CellInformation.activeCells[WorldManager.instance.currentCenterCell.ID].SpawnAIInDistantCell(_ID, _cellID, _pos, _rot);
        }


        public static RckAI SpawnAIInCell(string _ID, string _cellID, Vector3 _pos, Quaternion _rot)
        {
            if (CellInformation.activeCells.ContainsKey(_cellID))
            {
                var cell = CellInformation.activeCells[_cellID];

                if (cell.isActiveInScene)
                    return CellInformation.activeCells[_cellID].SpawnNewAI(_ID, _pos, _rot);
                else
                    return CellInformation.activeCells[_cellID].SpawnNewAIWhileCellIsCached(_ID, _pos, _rot);
            }
            else
            {
                SpawnAIInDistantCell(_ID, _cellID, _pos, _rot);
                return null;
            }
        }


        public static bool SendIntoOblivion(string _rckID)
        {
            // If AI is loaded save and destroy it
            RckAI ai = null;
            CellInformation.TryToGetAI(_rckID, out ai);

            if(ai != null)
                ai.DestroyThis();


            // Pick the AI in the save file
            var aiData = SaveSystem.SaveSystemManager.instance.saveFile.AIData;

            var rckAIData = aiData.aiDictionary[_rckID];

            // Remove from CellDictionary
            aiData.aiCellDictionary[rckAIData.saveCellID].allIDs.Remove(_rckID);

            rckAIData.runtimeSaveCellID = "OBLIVION";
            rckAIData.saveCellID = "OBLIVION";

            return true;
        }

        public static bool SummonFromOblivion(string _rckID)
        {
            return true;
        }

        public static bool RckAI_AddInFaction(string _rckID, string _factionID)
        {
            RckAI ai = null;
            CellInformation.TryToGetAI(_rckID, out ai);

            if (ai != null)
            {
                ai.AddToFaction(_factionID);
                return true;
            }
            else
                return false;
        }

        public static bool RckAI_RemoveFromFaction(string _rckID, string _factionID)
        {
            RckAI ai = null;
            CellInformation.TryToGetAI(_rckID, out ai);

            if (ai != null)
            {
                ai.RemoveFromFaction(_factionID);
                return true;
            }
            else
                return false;
        }

        public static bool KillRckAI(string _rckID)
        {
            RckAI ai = null;
            CellInformation.TryToGetAI(_rckID, out ai);

            if (ai != null)
            {
                ai.PlayDeathSound();
                ai.Die();
                return true;
            }

            // Pick the AI in the save file
            var aiData = SaveSystem.SaveSystemManager.instance.saveFile.AIData;
            if (aiData.aiDictionary.ContainsKey(_rckID))
            {
                var rckAIData = aiData.aiDictionary[_rckID];

                rckAIData.isAlive = false;
                return true;
            }

            return false;
        }

        public static bool ChangeDialogueToRckAI(string _rckID, string _dialogueID)
        {
            RckAI ai = null;
            CellInformation.TryToGetAI(_rckID, out ai);

            if (ai != null)
            {
                ai.ChangeDialogueGraph(_dialogueID);
                return true;
            }
            else
            {
                // Pick the AI in the save file
                var aiData = SaveSystem.SaveSystemManager.instance.saveFile.AIData;

                if (aiData.aiDictionary.ContainsKey(_rckID))
                {
                    var rckAIData = aiData.aiDictionary[_rckID];

                    rckAIData.currentDialogueID = _dialogueID;
                    return true;
                }
            }

            return false;
        }

        public static bool CheckPlayerStealsItem(ItemInInventory _item)
        {
            bool seen = false;

            // Check if the item was stolen and the owner saw us
            if (_item.metadata.isOwned)
            {
                RckAI ai;
                if (CellInformation.TryToGetAI(_item.metadata.ownerID, out ai) &&
                    ai.isAlive && ai.isLoaded && !ai.isInOfflineMode)
                {
                    for (int i = 0; i < ai.visibleTargets.Count; i++)
                        if (ai.visibleTargets[i].tr.CompareTag("Player"))
                        {
                            seen = true;
                            break;
                        }

                    if (seen)
                    {
                        ai.EnterInCombatAgainst(Player.RckPlayer.GetPlayerEntity());
                        if (RCKSettings.PICKPOCKET_DIALOGUE_CLIP_PLAYS && !ai.isInCombat)
                        {
                            if (ai.bodyData.isMale)
                                RCKFunctions.MakeAISpeakLine(ai, "DCLIP_SPECIAL_THIEF_MALE");
                            else
                                RCKFunctions.MakeAISpeakLine(ai, "DCLIP_SPECIAL_THIEF_FEMALE");
                        }
                        RCKFunctions.DisplayHeardLine(ai.entityName + ": Help! Thief!", 3.5f);
                    }
                }
                // Check if it is a persistent reference
                else if (PersistentReferences.PersistentReferenceManager.instance.refs.TryGetValue(_item.metadata.ownerID, out var reference))
                {
                    if (reference.isLoaded)
                    {
                        ai = reference.GetComponent<RckAI>();
                        seen = false;
                        for (int i = 0; i < ai.visibleTargets.Count; i++)
                            if (ai.visibleTargets[i].tr.CompareTag("Player"))
                            {
                                seen = true;
                                break;
                            }

                        if (seen)
                            ai.EnterInCombatAgainst(Player.RckPlayer.GetPlayerEntity());
                    }
                }
            }

            return seen;
        }

        public static bool CheckPlayerStealsItem(ItemInWorld _item)
        {
            bool seen = false;

            // Check if the item was stolen and the owner saw us
            if (_item.metadata.isOwned)
            {
                RckAI ai;
                if (CellInformation.TryToGetAI(_item.metadata.ownerID, out ai) && 
                    ai.isAlive && ai.isLoaded && !ai.isInOfflineMode)
                {
                    for (int i = 0; i < ai.visibleTargets.Count; i++)
                        if (ai.visibleTargets[i].tr.CompareTag("Player"))
                        {
                            seen = true;
                            break;
                        }

                    if (seen)
                    {
                        ai.EnterInCombatAgainst(Player.RckPlayer.GetPlayerEntity());
                        if (RCKSettings.PICKPOCKET_DIALOGUE_CLIP_PLAYS && !ai.isInCombat)
                        {
                            if (ai.bodyData.isMale)
                                RCKFunctions.MakeAISpeakLine(ai, "DCLIP_SPECIAL_THIEF_MALE");
                            else
                                RCKFunctions.MakeAISpeakLine(ai, "DCLIP_SPECIAL_THIEF_FEMALE");
                        }
                        RCKFunctions.DisplayHeardLine(ai.entityName + ": Help! Thief!", 3.5f);
                    }
                }
                // Check if it is a persistent reference
                else if (PersistentReferences.PersistentReferenceManager.instance.refs.TryGetValue(_item.metadata.ownerID, out var reference))
                {
                    if (reference.isLoaded)
                    {
                        ai = reference.GetComponent<RckAI>();
                        seen = false;
                        for (int i = 0; i < ai.visibleTargets.Count; i++)
                            if (ai.visibleTargets[i].tr.CompareTag("Player"))
                            {
                                seen = true;
                                break;
                            }

                        if (seen)
                        {
                            ai.EnterInCombatAgainst(Player.RckPlayer.GetPlayerEntity());
                            if (RCKSettings.PICKPOCKET_DIALOGUE_CLIP_PLAYS && !ai.isInCombat)
                            {
                                if (ai.bodyData.isMale)
                                    RCKFunctions.MakeAISpeakLine(ai, "DCLIP_SPECIAL_THIEF_MALE");
                                else
                                    RCKFunctions.MakeAISpeakLine(ai, "DCLIP_SPECIAL_THIEF_FEMALE");
                            }
                            RCKFunctions.DisplayHeardLine(ai.entityName + ": Help! Thief!", 3.5f);
                        }
                    }
                }
            }

            return seen;
        }

        public static float CalculatePickpocketChance(bool isBehind, float itemWeight, int dexterityLevel, bool isEquipped, int amount = 1)
        {
            float chance;

            const float BaseChance = 20f;
            const float WeightModifier = 1.85f;
            const float DexterityModifier = 0.5f;
            const float AmountModifier = 0.3f;

            chance = BaseChance;

            if (isBehind)
                chance += BaseChance / 2;

            chance -= itemWeight * WeightModifier; // Heavier items reduce the chance
            chance += dexterityLevel * DexterityModifier;      // Higher dexterity increases the chance

            chance -= amount * AmountModifier;
            
            // If the item is equipped, the chance is a third of the actual chance
            if (isEquipped)
                chance = chance / 3.0f;


            chance = Mathf.Clamp(chance, 0f, 100f);
            chance = Mathf.Round(chance * 10f) / 10f;

            return chance;
        }

        public static bool CheckAICanSeePlayer(RckAI ai)
        {
            bool seen = false;
            for (int i = 0; i < ai.visibleTargets.Count; i++)
                if (ai.visibleTargets[i].tr.CompareTag("Player"))
                {
                    seen = true;
                    break;
                }

            return seen;
        }

        public static float CalculateFallDamage(float timeAirbone)
        {
            // Apply fall damage
            float fallDmg = RCKSettings.FALLDAMAGE_MULTIPLIER * (MathF.Exp(RCKSettings.FALLDAMAGE_GROWTH_RATE * timeAirbone) - 1f);

            return fallDmg;
        }
    }
}