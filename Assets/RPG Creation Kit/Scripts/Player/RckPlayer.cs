using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.SaveSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RPGCreationKit.Player
{
    public class RckPlayer : PlayerUI
    {
        #region Singleton
        public static RckPlayer instance;

        public AudioClip[] maleDeathSounds;
        public AudioClip[] femaleDeathSounds;

        public bool isInDeathSequence = false;
        public float deathSequenceLength = 3.0f;

        public NavMeshObstacle playerObstacle;

        public Vector3 mountPositionFixed;

        private void Awake()
        {
            if (!instance)
                instance = this;
        }
        #endregion

        public override void Start()
        {
            // Set RckSettings
            RckInput.SetPlayerInputInstance(input);

            AssemblePlayerFromSavegame();

            base.Start();
            isInDeathSequence = false;

            LoadSavegamePlayerStats();

            InGameHelpUI.instance.SetWalkToggleUI(isWalking);
        }

        public override void Update()
        {
            base.Update();

            // Mountfix
            if(isMounted && transform.localPosition != mountPositionFixed && aiMounting.isAlive && !isDismounting)
            {
                Debug.Log("Fixing mount position");
                transform.localPosition = mountPositionFixed;
            }

            // Check if is mounting and mount died, if so, dismount
            if (isMounted && !aiMounting.isAlive && !isDismounting)
                DismountFromMount();
        }

        public void AssemblePlayerFromSavegame()
        {
            PlayerData f = SaveSystemManager.instance.saveFile.PlayerData;

            // Load and spawn Character related stuff
            Race pRace = RaceDatabase.GetItem(f.playerRace);

            // Spawn FPS arms and assign all we have to assign
            GameObject arms = (f.playerSex == false) ? pRace.maleFPS.gameObject : pRace.femaleFPS.gameObject;
            PlayerCombat.instance.AssignNewFPSArms(arms);

            GameObject character = (f.playerSex == false) ? pRace.maleModel.gameObject : pRace.femaleModel.gameObject;
            PlayerInInventory.instance.SpawnNewPlayer(pRace ,character, f.playerSex, f.faceData, f.hairType, f.eyesType);
            ThirdPersonPlayer.instance.SpawnNewPlayer(pRace, character, f.playerSex, f.faceData, f.hairType, f.eyesType);
        }

        public void LoadSavegamePlayerStats()
        {
            FreezeAndDisableControl();

            // Load Quests
            QuestManager.instance.LoadQuestDataFromSavefile();

            // Move player first
            PlayerData f = SaveSystemManager.instance.saveFile.PlayerData;
            transform.position = f.playerPos;
            transform.rotation = f.playerRot;

            // Load Mouse state
            mouseLook.LoadSavegameRotation();

            // Load Position
            WorldManager.instance.StartLoadingFromSavegame();

            TimeOfDayManager.instance.LoadFromSavefile();
            TimeOfDayManager.instance.RefreshLightingImmediate();

            // Load Entity Attributes
            playerAttributes.curLevel = f.playerAttributes.curLvl;
            playerAttributes.curXP = f.playerAttributes.curXP;
            playerAttributes.curAttributePoints = f.playerAttributes.curAttributePoints;
            playerAttributes.nextLvlXP = f.playerAttributes.nextLvlXP;

            playerAttributes.attributes = SaveSystemManager.instance.saveFile.PlayerData.playerAttributes.attributes;
            playerAttributes.derivedAttributes = SaveSystemManager.instance.saveFile.PlayerData.playerAttributes.derivedAttributes;
            playerAttributes.derivedAttributes.CalculateFromAttributes(playerAttributes.attributes);
            playerAttributes.ExecuteEffects(SaveSystemManager.instance.saveFile.PlayerData.playerAttributes.activeEffects.ToArray());
            RPGCreationKit.Player.RckPlayer.instance.ResetRecover();

            // Equip after everything else
            List<ItemInInventory> toEquipAfter = new List<ItemInInventory>();

            // Load Inventory
            for (int i = 0; i < SaveSystemManager.instance.saveFile.PlayerData.playerInventory.items.Count; i++)
            {
                ItemInInventory item = SaveSystemManager.instance.saveFile.PlayerData.playerInventory.items[i];
                var aItem = inventory.AddItem(item.externalItemID, item.Amount, false);
                aItem.metadata = item.metadata;

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

            // Load Spells Knoweldge (From Save V 1.3 and forward)
            if (RCKSettings.GetSaveVersion(SaveSystemManager.instance.saveFile.SaveFileData.fileVersion) >= 13) // 13 means 1.3
            {
                List<string> spellsIDs = SaveSystemManager.instance.saveFile.PlayerData.spellsKnowledge.items;

                for (int i = 0; i < spellsIDs.Count; i++)
                    SpellsKnowledge.Player.LearnSpell(spellsIDs[i]);

                if (!string.IsNullOrEmpty(SaveSystemManager.instance.saveFile.PlayerData.spellsKnowledge.equippedSpell))
                    SpellsKnowledge.Player.EquipSpell(SpellsDatabase.GetSpell(SaveSystemManager.instance.saveFile.PlayerData.spellsKnowledge.equippedSpell));
            }

            //else we're loading an old save, so no spells are knew

            // Load regen edits
            if (RCKSettings.GetSaveVersion(SaveSystemManager.instance.saveFile.SaveFileData.fileVersion) >= 17)
            {
                var PlayerData = SaveSystemManager.instance.saveFile.PlayerData;

                if (PlayerData.recoversSet)
                {
                    recoverStaminaAmount = PlayerData.recoverStaminaAmount;
                    recoverAfterActionDelay = PlayerData.recoverAfterActionDelay;
                    recoverHealthAmount = PlayerData.recoverHealthAmount;
                    recoverAfterHitDelay = PlayerData.recoverAfterHitDelay;

                    recoverManaAmount = PlayerData.recoverManaAmount;
                    recoverAfterManaUseDelay = PlayerData.recoverManaAfterUseDelay;


                    RckPlayer.instance.recoverHealthAmount = RCKSettings.GetHealthRegenRate(playerAttributes.attributes.Constitution);
                    RckPlayer.instance.recoverAfterHitDelay = RCKSettings.GetHealthRecoverAfterHitDelay(playerAttributes.attributes.Constitution);

                    RckPlayer.instance.recoverStaminaAmount = RCKSettings.GetStaminaRegenRate(playerAttributes.attributes.Endurance);
                    RckPlayer.instance.recoverAfterActionDelay = RCKSettings.GetStaminaRecoverAfterHitDelay(playerAttributes.attributes.Endurance);

                    RckPlayer.instance.recoverManaAmount = RCKSettings.GetManaRegenRate(playerAttributes.attributes.Willpower);
                    RckPlayer.instance.recoverAfterManaUseDelay = RCKSettings.GetManaRecoverAfterUseDelay(playerAttributes.attributes.Willpower);

                    // Recover health
                    if (recoverHealthAmount > 0.1f)
                        ResetRecoverHealth();
                }
            }


            if (PlayerCombat.instance != null)
                PlayerCombat.instance.OnEquipmentChanges();

            if(PlayerInInventory.instance != null)
                PlayerInInventory.instance.OnEquipmentChangesHands();

            if (PlayerInInventory.instance != null)
                PlayerInInventory.instance.OnEquipmentChangesAmmo();

            for (int i = 0; i < toEquipAfter.Count; i++)
            {
                equipment.Equip(toEquipAfter[i]);
            }


            Inventory.PlayerInventory.InitializeInventory();


            // Load other data
            PlayerData data = SaveSystemManager.instance.saveFile.PlayerData;
            if (data.playerCrouched)
                ForceCrouch();

            if (data.weaponDrawn)
                PlayerCombat.instance.DrawWeapon();

            // Load factions
            for (int i = 0; i < data.playerFactions.Count; i++)
                RckPlayer.instance.AddToFaction(data.playerFactions[i]);

            // Load Camera (From Save V 1.1 and forward)
            if (RCKSettings.GetSaveVersion(SaveSystemManager.instance.saveFile.SaveFileData.fileVersion) >= 11) // 11 means 1.1
            {
                if (data.isInThirdPerson)
                    SwitchToThirdPerson();
                else
                    SwitchToFirstPerson();
            }
            else // If we're loading a 1.0 save, just stick with FPS
                SwitchToFirstPerson();
            
            
            UpdateHealthStaminaGUI();
            UpdateXpSlider();

            //UnfreezeAndEnableControls(); done by WorldManager
            if(RCKSettings.GetSaveVersion(SaveSystemManager.instance.saveFile.SaveFileData.fileVersion) >= 18 && data.isMounted) // 18 means 1.8
            {
                StartCoroutine(LoadMountedState());
            }
        }

        public void UpdateXpSlider()
        {
            uiManager.playerXpSlider.maxValue = playerAttributes.nextLvlXP;
            uiManager.playerXpSlider.value = playerAttributes.curXP;
        }

        public IEnumerator LoadMountedState()
        {
            PlayerData data = SaveSystemManager.instance.saveFile.PlayerData;

            while(!isLoaded)
            {
                yield return null;
            }

            RckAI ai;
            CellInformation.TryToGetAI(data.mountedEntityID, out ai);

            if (ai != null)
            {
                MountAI mount = (MountAI)ai;
                RckPlayer.instance.MountOnMountAI(mount);
            }

            yield return null;
        }


        public void EnterInCutsceneMode()
        {
            isInCutsceneMode = true;

            Player.RckPlayer.instance.FreezeOnly();


            // Disable GUI and other things
            Player.RckPlayer.instance.uiManager.compassGameObject.SetActive(false);
            Player.RckPlayer.instance.uiManager.horCompassGameObject.SetActive(false);
            Player.RckPlayer.instance.uiManager.crosshairGameObject.gameObject.SetActive(false);
            Player.RckPlayer.instance.uiManager.playerHealthSlider.gameObject.SetActive(false);
            Player.RckPlayer.instance.uiManager.playerStaminaSlider.gameObject.SetActive(false);
            Player.RckPlayer.instance.uiManager.playerManaSlider.gameObject.SetActive(false);
            Player.RckPlayer.instance.uiManager.playerXpSlider.gameObject.SetActive(false);

            PlayerCombat.instance.weaponUI.weaponIcon.gameObject.SetActive(false);
            PlayerCombat.instance.equippedSpellUI.gameObject.SetActive(false);
            PlayerCombat.instance.weaponUI.weaponContainer.gameObject.SetActive(false);
            PlayerCombat.instance.weaponUI.spellContainer.gameObject.SetActive(false);

            Player.RckPlayer.instance.cameraAnim.m_Animator.enabled = false;

            if (PlayerCombat.instance.fpcAnim != null)
            {
                PlayerCombat.instance.fpcAnim.SetBool("isMoving", false);
                PlayerCombat.instance.fpcAnim.SetBool("isSprinting", false);

                ThirdPersonPlayer.instance.m_Animator.SetFloat("Speed", 0.0f);
                ThirdPersonPlayer.instance.m_Animator.SetFloat("Sideways", 0.0f);
            }
        }

        public void LeaveCutsceneMode()
        {
            isInCutsceneMode = false;

            Player.RckPlayer.instance.Unfreeze();

            // Restore GUI and other things
            Player.RckPlayer.instance.uiManager.horCompassGameObject.SetActive(true);
            Player.RckPlayer.instance.uiManager.compassGameObject.SetActive(true);
            Player.RckPlayer.instance.uiManager.crosshairGameObject.gameObject.SetActive(true);
            Player.RckPlayer.instance.uiManager.playerHealthSlider.gameObject.SetActive(true);
            Player.RckPlayer.instance.uiManager.playerStaminaSlider.gameObject.SetActive(true);
            Player.RckPlayer.instance.uiManager.playerManaSlider.gameObject.SetActive(true);
            Player.RckPlayer.instance.uiManager.playerXpSlider.gameObject.SetActive(true);
            PlayerCombat.instance.weaponUI.weaponIcon.gameObject.SetActive(true);

            PlayerCombat.instance.weaponUI.weaponIcon.gameObject.SetActive(true);
            PlayerCombat.instance.equippedSpellUI.gameObject.SetActive(true);
            PlayerCombat.instance.weaponUI.weaponContainer.gameObject.SetActive(true);
            PlayerCombat.instance.weaponUI.spellContainer.gameObject.SetActive(true);

            Player.RckPlayer.instance.cameraAnim.m_Animator.enabled = true;
        }

        public override void PlayerDeath()
        {
            base.PlayerDeath();
            StartCoroutine(PlayerDeathSequence());
        }

        public IEnumerator PlayerDeathSequence()
        {
            AudioClip clip;

            if (SaveSystem.SaveSystemManager.instance.saveFile.PlayerData.playerSex == false)
                clip = maleDeathSounds[Random.Range(0, maleDeathSounds.Length - 1)];
            else
                clip = femaleDeathSounds[Random.Range(0, femaleDeathSounds.Length - 1)];

            GameAudioManager.instance.PlayOneShot(AudioSources.Player, clip);

            isInDeathSequence = true;
            SwitchToThirdPerson();

            thirdPersonModel.forceFreeLook = true; // force camera look
            uiManager.crosshairGameObject.SetActive(false);
            PlayerCombat.instance.isAiming = false;
            PlayerCombat.instance.isBlocking = false;


            // Disable UI if on top
            if (InventoryUI.instance.isOpen)
                InventoryUI.instance.UI.SetActive(false);

            if (QuestManagerUI.instance.isUIopened)
                QuestManagerUI.instance.QuestPanelUI.SetActive(false);

            FreezeAndDisableControl();
            mouseLook.lookEnabled = false;

            if(PlayerCombat.instance.weaponDrawn && PlayerCombat.instance.currentWeaponOnHand.weaponItem.clippingWeapon)
            {
                Debug.Log("Return to default");
                thirdPersonModel.m_Animator.SetTrigger("ReturnToDefault");
                thirdPersonModel.m_Animator.Update(0);
            }

            thirdPersonModel.ragdoll.ToggleRagdoll(false);
            Destroy(thirdPersonModel.m_Animator);

            // Drop shield and weapon if any (TP model)
            if(thirdPersonModel.equipment.currentShieldObject != null)
            {
                // Spawn the item in the world
                ItemInWorld itemInWorld = Instantiate(equipment.currentShield.itemInWorld, thirdPersonModel.character.lHand.transform.position, thirdPersonModel.character.lHand.transform.rotation).GetComponent<ItemInWorld>();
                thirdPersonModel.equipment.currentShieldObject.SetActive(false);
            }

            if (thirdPersonModel.equipment.currentTorchInHand != null)
            {
                // Spawn the item in the world
                ItemInWorld itemInWorld = Instantiate(equipment.currentTorch.itemInWorld, thirdPersonModel.character.lHand.transform.position, thirdPersonModel.character.lHand.transform.rotation).GetComponent<ItemInWorld>();
                thirdPersonModel.equipment.currentTorchInHand.SetActive(false);
            }

            // Instantiate the InWorld equivalent 
            if (thirdPersonModel.equipment.currentWeapon.itemInWorld != null)
            {
                // Spawn the item in the world
                ItemInWorld itemInWorld = Instantiate(equipment.currentWeapon.itemInWorld, thirdPersonModel.character.lHand.transform.position, thirdPersonModel.character.lHand.transform.rotation).GetComponent<ItemInWorld>();
                thirdPersonModel.currentWeaponObject.SetActive(false);
            }


            yield return new WaitForSeconds(deathSequenceLength);
            PlayerDeathScreen.instance.ShowDeathScreen();
        }

        public MountAI aiMounting;
        public Transform preMountTPSLookAt;
        public Transform preMountFPSMouseLookTarget;
        MountupPoint mountPoint;
        public void MountOnMountAI(MountAI ai)
        {
            if (!charController.isGrounded)
                Land();

            if (PlayerCombat.instance.weaponDrawn)
                PlayerCombat.instance.UndrawWeapon();

            isRunning = false;
            aiMounting = ai;
            ai.isOccupied = true;

            preMountTPSLookAt = cameraAnim.tpsLookAtTarget;
            preMountFPSMouseLookTarget = mouseLook.player;

            if (RCKSettings.HELPUI_SHOW_DISMOUNT_CMD)
                InGameHelpUI.instance.SetDismountHelpUI(true);

            // zero player's velocity
            ZeroVelocity();

            thirdPersonModel.HardRestorePositionAndRotationToNatural();

            DisablePlayerNavObstacle();

            StartCoroutine(MountOnMountAITask());
        }

        bool wasInFirstPersonBeforeMounting = false;
        public IEnumerator MountOnMountAITask()
        {
            wasInFirstPersonBeforeMounting = !isInThirdPerson;
            isMounting = true;

            // Determine the closest mounting point
            float closestDistance = float.MaxValue;
            int closestIndex = -1;
            for (int i = 0; i < aiMounting.mountPoints.Length; i++)
            {
                float curDist = Vector3.Distance(this.transform.position, aiMounting.mountPoints[i].transform.position);

                if (curDist < closestDistance)
                {
                    closestDistance = curDist;
                    closestIndex = i;
                }
            }

            if (closestIndex != -1)
            {
                mountPoint = aiMounting.mountPoints[closestIndex];

                // Ignore collision between player->mount
                Physics.IgnoreCollision(transform.GetComponent<Collider>(), aiMounting.GetComponent<Collider>(), true);

                for(int i = 0; i < aiMounting.extraColliders.Count; i++)
                {
                    Physics.IgnoreCollision(transform.GetComponent<Collider>(), aiMounting.extraColliders[i], true);

                    if (equipment.isUsingShield)
                    {
                        Physics.IgnoreCollision(equipment.currentShieldObject.GetComponent<Collider>(), aiMounting.GetComponent<Collider>(), true);
                        Physics.IgnoreCollision(equipment.currentShieldObject.GetComponent<Collider>(), aiMounting.extraColliders[i], true);

                        if (equipment.currentFPShieldObject)
                        {
                            Physics.IgnoreCollision(equipment.currentFPShieldObject.GetComponent<Collider>(), aiMounting.GetComponent<Collider>(), true);
                            Physics.IgnoreCollision(equipment.currentFPShieldObject.GetComponent<Collider>(), aiMounting.extraColliders[i], true);
                        }
                    }
                }

                

                // Setups the Mount to be controlled manually by the player
                aiMounting.ManualControlSetup();

                // Snap the player to the position, set the camera, and start the coroutine
                cameraAnim.DisableAnimator();
                isMounted = true;

                // Set parent
                transform.SetParent(aiMounting.transform);

                // If is crouching, getup and wait 
                if (iscrouching)
                    ForceStandupFromCrouch(false);

                while (crouchEditInProgress || hitzoneCrouchEditInProgress)
                    yield return new WaitForEndOfFrame();

                mouseLook.xRotation = 0;

                yield return new WaitForEndOfFrame();

                SwitchToThirdPerson();

                //aiMounting.playerCameraRig.SetActive(true);
                //aiMounting.mouseLook.BindToRCKInput();

                // Move the character controller
                //charController.enabled = false;
                transform.position = mountPoint.pivotPoint.transform.position + new Vector3(0, 1.422f, 0); // 1.422 is the default elevation set in the WordLoader scene for the player
                transform.rotation = mountPoint.pivotPoint.transform.rotation;

                mountPositionFixed = transform.localPosition;

                // Move the camera anim to mounting pos
                mouseLook.player = cameraAnim.transform;
                cameraAnim.FPSMoveToMountingPos(aiMounting.fpsCameraAnimPos);
                cameraAnim.TPSMoveToMountingPos(aiMounting.tpsCameraRootPos);

                // Pause look at, play animation and restore look at
                thirdPersonModel.pauseLookAt = true;
                thirdPersonModel.m_Animator.Play(mountPoint.npcAction.name);

                yield return new WaitForSeconds(mountPoint.npcAction.length);
                thirdPersonModel.pauseLookAt = false;

                if (wasInFirstPersonBeforeMounting)
                    SwitchToFirstPerson();

                isMounting = false;

                // Adjust look at
                //cameraAnim.tpsLookAtTarget = null; // Set null to play mount up anim without look at

                // after animation is done, set mounted look at if needed
                //cameraAnim.tpsLookAtTarget = aiMounting.mountedCharacterLooKAt;
            }

            yield return null;
        }

        bool wasInFirstPersonBeforeDismounting = false;
        public void DismountFromMount()
        {
            // Select mount point to return from
            isDismounting = true;

            EnablePlayerNavObstacle();

            if (RCKSettings.HELPUI_SHOW_DISMOUNT_CMD)
                InGameHelpUI.instance.SetDismountHelpUI(false);

            InGameHelpUI.instance.SetWalkToggleUI(isWalking);

            aiMounting.PlayerStartsDismounting();
            StartCoroutine(DismountFromMountTask());
        }

        public IEnumerator DismountFromMountTask()
        {
            wasInFirstPersonBeforeDismounting = !isInThirdPerson;

            // Revert Ignore collision between player->mount
            Physics.IgnoreCollision(transform.GetComponent<Collider>(), aiMounting.GetComponent<Collider>(), false);

            for (int i = 0; i < aiMounting.extraColliders.Count; i++)
            {
                Physics.IgnoreCollision(transform.GetComponent<Collider>(), aiMounting.extraColliders[i], false);
            }

            yield return new WaitForEndOfFrame();

            SwitchToThirdPerson();

            //aiMounting.playerCameraRig.SetActive(true);
            //aiMounting.mouseLook.BindToRCKInput();

            // Restore parent to WorldLoader 
            transform.SetParent(WorldManager.instance.transform);
            transform.SetParent(null);


            // Move the character controller
            //charController.enabled = false;
            transform.position = mountPoint.pivotPoint.transform.position + new Vector3(0, 1.422f, 0); // 1.422 is the default elevation set in the WordLoader scene for the player
            transform.rotation = mountPoint.pivotPoint.transform.rotation;

            // Move the camera anim to mounting pos
            mouseLook.player = preMountFPSMouseLookTarget;

            cameraAnim.FPSRestoreFromMountingPos();
            cameraAnim.TPSRestoreFromMountingPos();

            // Pause look at, play animation and restore look at
            thirdPersonModel.pauseLookAt = true;
            thirdPersonModel.m_Animator.Play(mountPoint.npcActionReturn.name);

            yield return new WaitForSeconds(mountPoint.npcActionReturn.length);
            thirdPersonModel.pauseLookAt = false;

            aiMounting.PlayerFinishDismounting();

            if (wasInFirstPersonBeforeDismounting)
                SwitchToFirstPerson();

            // Restore collision with shield
            if (equipment.isUsingShield)
            {
                for (int i = 0; i < aiMounting.extraColliders.Count; i++)
                {
                    Physics.IgnoreCollision(equipment.currentShieldObject.GetComponent<Collider>(), aiMounting.GetComponent<Collider>(), false);
                    Physics.IgnoreCollision(equipment.currentShieldObject.GetComponent<Collider>(), aiMounting.extraColliders[i], false);

                    if (equipment.currentFPShieldObject)
                    {
                        Physics.IgnoreCollision(equipment.currentFPShieldObject.GetComponent<Collider>(), aiMounting.GetComponent<Collider>(), false);
                        Physics.IgnoreCollision(equipment.currentFPShieldObject.GetComponent<Collider>(), aiMounting.extraColliders[i], false);
                    }
                }
            }

            aiMounting = null;
            isMounted = false;
            isDismounting = false;
        }

        void DisablePlayerNavObstacle()
        {
            playerObstacle.enabled = false;
        }

        void EnablePlayerNavObstacle()
        {
            playerObstacle.enabled = true;
        }
    }
}
