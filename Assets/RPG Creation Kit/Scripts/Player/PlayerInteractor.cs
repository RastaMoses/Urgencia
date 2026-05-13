using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.DialogueSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RPGCreationKit.AI;
using XNode;

namespace RPGCreationKit.Player
{
    public class PlayerInteractor : PlayerMovements, IDialoguable
    {
        //[SerializeField] private PlayerInteractorUI interactorUI;
        [SerializeField] private LayerMask rayMask;
        [SerializeField] private Transform camT;

        public bool isInConversation;
        public bool isInteracting;
        public bool isReadingBook;
        public bool isLooting;

        Ray ray;
        RaycastHit rayHit;
        [SerializeField] private Transform rayStartPoint;


        // Dialogue
        public IDialoguable speakingTo;
        public HeadBlendshapesManager speakingToHeadBlendshapes; // true only on RckAI
        [SerializeField] private float dialogueDistance = 2.5f; // The distance of which a dialogue is possible

        // Interact object
        public InteractiveObject_Behaviour interactiveObject;
        [SerializeField] private float interactiveObjectDistance = 2.5f; // The distance of which a dialogue is possible


        // Looting
        [HideInInspector] public LootingPoint objLooting; // If so, track the InteractiveObject
        [SerializeField] private float lootingPointDistance = 2.5f;   // The distance to interact with an InteractiveObject

        // Items
        [SerializeField] private float itemDistance = 2.5f;
        [SerializeField] private float doorDistance = 2.5f;
        private bool cameraLookNPC;


        Rigidbody itemRigidbody;
        private ItemInWorld dragItemScript;
        Vector3 dragDirection;
        [SerializeField] private float dragDistance = 2.5f;
        public float dragForce = 50f;
        public bool dragKeyBeingHeld;
        public bool isDragging;

        string cmdStr;

        public void INPUT_DragKey(InputAction.CallbackContext value)
        {
            if (value.started)
                dragKeyBeingHeld = true;
            else if (value.canceled)
                dragKeyBeingHeld = false;
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            base.Update();

            // DialogueFix
            if (isInConversation && !RckInput.isUsingGamepad && uiManager.allPlayerQuestions.Count > 0 && uiManager.playerQuestionsContainer.gameObject.activeInHierarchy)
                Cursor.lockState = CursorLockMode.None;

            if (isInCutsceneMode || !isLoaded || !IsControlledByPlayer())
            {
                if (uiManager.Interact_text.isActiveAndEnabled)
                    uiManager.Interact_text.gameObject.SetActive(false);
                return;
            }

            if (uiManager.Interact_text.isActiveAndEnabled)
                uiManager.Interact_text.gameObject.SetActive(false);

            cmdStr = RckInput.isUsingGamepad ? "(A)" : "E)";

            Raycast();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            ManageDraggable();
        }


        public void Raycast()
        {
            if(!RckPlayer.instance.isInThirdPerson)
                ray = Player.RckPlayer.instance.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            else
            {
                Vector3 rayStart = RckPlayer.instance.tpsCamera.transform.position + (RckPlayer.instance.tpsCamera.transform.forward * 2);
                Ray rayDirection = RckPlayer.instance.tpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                ray = new Ray(rayStart, rayDirection.direction);
                //Debug.DrawRay(ray.origin, ray.direction, Color.yellow, .5f);
            }

            if (Physics.Raycast(ray, out rayHit, RCKSettings.INTERACTOR_RAYCAST_MAXDISTANCE, rayMask))
            {
                if (!RckPlayer.instance.iscrouching)
                {
                    if (rayHit.distance <= dialogueDistance)
                        CheckForIDialoguable();
                }
                else if (rayHit.distance <= dialogueDistance)
                {
                    CheckForLootingPoint(true);
                }

                if (rayHit.distance <= RCKSettings.PLAYER_MOUNT_DISTANCE)
                    CheckForMounts();

                if (rayHit.distance <= itemDistance)
                    CheckForItemsInWorld();
                else
                {
                    uiManager.stealItemIcon.SetActive(false);
                    uiManager.takeItemIcon.SetActive(false);
                }

                if (rayHit.distance <= RCKSettings.PLAYER_DISTANCE_TO_DISPLAY_ENEMY_BAR)
                    CheckForIDamageable();
                else
                    uiManager.enemyHealthContainer.SetActive(false);

                if (rayHit.distance <= interactiveObjectDistance)
                    CheckForInteractiveObjects();

                if (rayHit.distance <= lootingPointDistance)
                    CheckForLootingPoint(false);

                if (rayHit.distance <= doorDistance)
                    CheckForDoors();

                if (rayHit.distance <= dragDistance)
                    CheckForIDraggable();
            }
            else
            {
                uiManager.stealItemIcon.SetActive(false);
                uiManager.takeItemIcon.SetActive(false);
            }
        }

        private void CheckForIDialoguable()
        {
            IDialoguable dialoguable = rayHit.transform.gameObject.GetComponent<IDialoguable>();

            if(dialoguable != null && dialoguable.CanDialogue() && !isInConversation)
            {
                // Set the UI
                uiManager.Interact_text.gameObject.SetActive(true);
                uiManager.Interact_text.text = cmdStr+" Speak: " + dialoguable.GetDialoguableName();


                if (input.actions.FindAction("Interact").triggered)
                {
                    StartDialogue(dialoguable);
                }
            }
        }

        private void CheckForMounts()
        {
            if (rayHit.transform.CompareTag("RPG Creation Kit/AI"))
            {
                // When we find one, we save this reference
                MountAI ai = rayHit.collider.transform.gameObject.GetComponentInParent<MountAI>();

                if (ai == null || (ai.isOccupied && !ai.playerMounting) || isDismounting || (isMounted && !ai.playerMounting) || !ai.isAlive)
                    return;

                // If the AI is not hostile
                if (!ai.isHostile && !ai.isHostileAgainstPC && !ai.isInCombat)
                {
                    uiManager.Interact_text.gameObject.SetActive(true);
                    uiManager.Interact_text.text = cmdStr + " Mount: " + ai.GetEntityName();

                    if(ai.isOccupied && ai.playerMounting)
                        uiManager.Interact_text.text = cmdStr + " Dismount: " + ai.GetEntityName();

                    // Check for Key to mount
                    if (input.actions.FindAction("Interact").triggered &&
                        !PlayerCombat.instance.isAttacking && PlayerCombat.instance.canAttack && RckPlayer.instance.AreControlsEnabled() &&
                        !RckPlayer.instance.isInteracting && !RckPlayer.instance.isInConversation &&
                        !QuestManagerUI.instance.isUIopened && !InventoryUI.instance.isOpen && !PlayerCombat.instance.isUnbalanced && !RckPlayer.instance.isLooting &&
                        !BookReaderManager.instance.backgroundUI.activeInHierarchy && !RckPlayer.instance.isInCutsceneMode && !TutorialAlertMessage.instance.messageOpened && !RckPlayer.instance.isInDeathSequence)
                    {
                        if (!ai.isOccupied)
                            RckPlayer.instance.MountOnMountAI(ai);
                        else if (ai.isOccupied && ai.playerMounting)
                        {
                            RckPlayer.instance.DismountFromMount();
                        }
                        else
                            AlertMessage.instance.InitAlertMessage("Somebody is already using that.", 2.5f, false);
                    }
                }
            }
        }

        private void CheckForItemsInWorld()
        {
            if(rayHit.transform.CompareTag("RPG Creation Kit/ItemInWorld"))
            {
                // When we find one, we save this reference
                ItemInWorld obj = rayHit.transform.gameObject.GetComponent<ItemInWorld>();

                if (obj == null)
                    return;

                if (obj.hasBeenTaken)
                    return;

                uiManager.Interact_text.gameObject.SetActive(true);

                if (obj.Item is BookItem)
                {
                    uiManager.Interact_text.text = cmdStr+" Read: " + obj.Item.ItemName;
                }
                else
                {
                    string action = obj.metadata.isOwned ? " <color=\"red\">Steal:</color> " : " Take: ";

                    if (obj.Amount == 1)
                        uiManager.Interact_text.text = cmdStr + action + obj.Item.ItemName;
                    else
                        // E) Take item.name (Number)
                        uiManager.Interact_text.text = cmdStr + action + obj.Item.ItemName + " (" + obj.Amount + ")";

                    if (obj.metadata.isOwned)
                    {
                        uiManager.takeItemIcon.SetActive(false);
                        uiManager.stealItemIcon.SetActive(true);
                    }
                    else
                    {

                        uiManager.stealItemIcon.SetActive(false);
                        uiManager.takeItemIcon.SetActive(true);
                    }
                }

                if (input.actions.FindAction("Interact").triggered)
                {
                    if (obj.Item is BookItem)
                    {
                        // ReadBook
                        if(!RckPlayer.instance.isReadingBook)
                            BookReaderManager.instance.ReadBook((BookItem)obj.Item, true, obj);
                    }
                    else
                    {
                        TakeItemInWorld(obj);
                    }
                }

                if(PlayerCombat.instance.isAiming)
                {
                    uiManager.takeItemIcon.SetActive(false);
                    uiManager.stealItemIcon.SetActive(false);
                }
            }
            else
            {
                uiManager.takeItemIcon.SetActive(false);
                uiManager.stealItemIcon.SetActive(false);
            }
        }

        public void TakeItemInWorld(ItemInWorld _item)
        {
            _item.hasBeenTaken = true;

            inventory.AddItem(_item.Item, _item.metadata, _item.Amount);


            // We trigger activators
            if (_item.events.EvaluateConditions())
            {
                QuestManager.instance.QuestDealerActivator(_item.events.questDealers);
                QuestManager.instance.QuestUpdaterActivator(_item.events.questUpdaters);
                ConsequenceManager.instance.ConsequencesActivator(gameObject, _item.events.consequences);
            }

            // Play sound
            if (_item.Item.sOnPickUp)
                GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, _item.Item.sOnPickUp);

            // If It's NOT created item, save the state
            if(!_item.isCreatedItem)
                _item.SaveOnFile(true);
            else
            {
                // Otherwise, remove it from the dictionary
                _item.DeleteCreatedRecord();
            }

            Destroy(_item.gameObject);

            // Check steal
            RCKFunctions.CheckPlayerStealsItem(_item);
            PlayerCombat.instance.SetWeaponUI(); // update weapon ui
        }

        private void CheckForIDamageable()
        {
            IDamageable damageable = rayHit.transform.gameObject.GetComponent<IDamageable>();

            if (damageable != null && damageable.IsAlive() && !damageable.IsUnconscious())
            {
                if (damageable.ShouldDisplayHealthIfHostile() && (damageable.IsHostile() || damageable.IsHostileAgainstPC()))
                {
                    uiManager.enemyHealthSlider.maxValue = damageable.GetMaxHP();
                    uiManager.enemyHealthSlider.value = damageable.GetCurrentHP();
                    uiManager.enemyName.text = damageable.GetEntityName();
                    uiManager.enemyHealthContainer.SetActive(true);
                }
                else
                    uiManager.enemyHealthContainer.SetActive(false);
            }
            else
                uiManager.enemyHealthContainer.SetActive(false);
        }

        /// <summary>
        /// Check with Rays if we're near InteractiveObjects
        /// </summary>
        private void CheckForInteractiveObjects()
        {
            // When we find one, we save this reference
            InteractiveObject_Behaviour obj = rayHit.transform.gameObject.GetComponent<InteractiveObject_Behaviour>();

            if (obj == null ||obj.interactiveObj == null)
                return;

            // If the NPC is interactable (not dead and not hostile and we're not yet in a conversation with him)
            if (obj.canInteract && !isInteracting)
            {

                // Set the UI
                uiManager.Interact_text.gameObject.SetActive(true);
                uiManager.Interact_text.text = cmdStr+" " + obj.interactiveObj.Action + ": " + obj.interactiveObj.Name;

                // Check for Key to speak with him
                if (input.actions.FindAction("Interact").triggered)
                    InteractWithObject(obj);
            }
            else
                uiManager.Interact_text.gameObject.SetActive(false);
        }

        /// <summary>
        /// Called when we select to interact with an Object
        /// </summary>
        private void InteractWithObject(InteractiveObject_Behaviour obj)
        {
            input.SwitchCurrentActionMap("InteractUI");
            // Flag that we're interacting
            isInteracting = true;


            // If we can choose between multiple options
            if (obj.interactiveObj.InteractiveOptions.Length > 1 || obj.interactiveObj.openUIEvenWithOneOption)
            {
                // Disable player controls
                RckPlayer.instance.EnableDisableControls(false);

                // Set UI
                if (RCKSettings.INTERACTIVE_OBJECT_STOPS_TIME)
                    Time.timeScale = 0.0f;

                uiManager.interactiveObjectUIContainer.SetActive(true);

                uiManager.interactiveObjectName.text = obj.interactiveObj.Name;
                uiManager.interactiveObjectDescription.text = obj.interactiveObj.Description;

                // Instantiate InteractiveOptionsUI
                if (obj.interactiveObj.InteractiveOptions.Length > 0)
                {
                    for (int i = 0; i < obj.interactiveObj.InteractiveOptions.Length; i++)
                    {
                        GameObject newOption = Instantiate(uiManager.interactiveOptionUIPrefab, uiManager.interactiveOptionContent);
                        newOption.GetComponent<InteractiveObjectUI>().Init(obj, obj.interactiveObj, obj.interactiveObj.InteractiveOptions[i]);

                        if (i == 0 && RckInput.isUsingGamepad)
                            newOption.GetComponent<Selectable>().Select();
                    }
                }
                // Otherwise if we have only one option
            }
            else if (obj.interactiveObj.InteractiveOptions.Length == 1)
            {
                // We just trigger the events
                DoInteractiveAction(obj, obj.interactiveObj.InteractiveOptions[0]);
            }
        }

        /// <summary>
        /// Called by InteractiveObject UI Buttons, perform interactive actions
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="option"></param>
        public void DoInteractiveAction(InteractiveObject_Behaviour obj, InteractiveOptions option)
        {
            // We initialize the array element
            int arrayElement = -1;

            // We find at what index are the Events of the selected 'option'
            for (int i = 0; i < obj.interactiveObj.InteractiveOptions.Length; i++)
            {
                if (option == obj.interactiveObj.InteractiveOptions[i])
                {
                    // We save it
                    arrayElement = i;
                    break;
                }
            }

            // We trigger activators
            if (obj.events[arrayElement].EvaluateConditions())
            {
                QuestManager.instance.QuestDealerActivator(obj.events[arrayElement].questDealers);
                QuestManager.instance.QuestUpdaterActivator(obj.events[arrayElement].questUpdaters);
                ConsequenceManager.instance.ConsequencesActivator(gameObject, obj.events[arrayElement].consequences);
            }

            // Trigger the Result Script
            if (!string.IsNullOrEmpty(obj.resultScripts[arrayElement]))
                RCKFunctions.ExecuteScript(obj.resultScripts[arrayElement]);

            // Enable player controls
            EnableDisableControls(true);

            uiManager.interactiveObjectUIContainer.SetActive(false);

            if(RCKSettings.INTERACTIVE_OBJECT_STOPS_TIME)
                Time.timeScale = 1.0f;

            // Remove all the previous instantiated options
            foreach (Transform t in uiManager.interactiveOptionContent)
                Destroy(t.gameObject);


            StartCoroutine(StopInteractingFixed());
            input.SwitchCurrentActionMap("Player");

        }

        IEnumerator StopInteractingFixed()
        {
            yield return new WaitForEndOfFrame();
            isInteracting = false;
        }

        private void CheckForLootingPoint(bool isPickPocket)
        {
            if (isPickPocket)
            {
                if (rayHit.transform.CompareTag("RPG Creation Kit/AI"))
                {
                    // When we find one, we save this reference
                    RckAI ai = rayHit.collider.transform.gameObject.GetComponentInParent<RckAI>();

                    if (ai == null || ai is MountAI)
                        return;

                    // If the AI is not hostile
                    if (!ai.isHostile && !ai.isHostileAgainstPC && !ai.isInCombat)
                    {
                        // Set the UI
                        if (!isLooting)
                        {
                            uiManager.Interact_text.gameObject.SetActive(true);
                            uiManager.Interact_text.text = cmdStr + " <color=\"red\">Pickpocket:</color> " + ai.GetEntityName();
                        }

                        // Check for Key to speak with him
                        if (input.actions.FindAction("Interact").triggered)
                        {
                            if (ai.canBePickPocketed)
                            {
                                if(!ai.isInConversation)
                                    OpenLootPoint(ai);
                                else
                                    AlertMessage.instance.InitAlertMessage("You can't do that now!", 2.5f, false);
                            }
                            else
                                AlertMessage.instance.InitAlertMessage("You cannot pickpocket " + ai.entityName, 2.5f, false);
                        }
                    }
                }
            }
            else
            {
                if (rayHit.transform.CompareTag("RPG Creation Kit/Looting Point"))
                {
                    // When we find one, we save this reference
                    LootingPoint obj = rayHit.collider.transform.gameObject.GetComponent<LootingPoint>();

                    // Set the UI
                    if (!isLooting)
                    {
                        uiManager.Interact_text.gameObject.SetActive(true);

                        string action = obj.metadata.isOwned ? " <color=\"red\">Steal:</color> " : " Search: ";

                        if (obj.inventory.Items.Count > 0)
                            uiManager.Interact_text.text = cmdStr + action + obj.pointName;
                        else
                            uiManager.Interact_text.text = cmdStr + action + obj.pointName + " (Empty)";
                    }

                    // Check for Key to speak with him
                    if (input.actions.FindAction("Interact").triggered)
                        OpenLootPoint(obj);
                }
            }
        }

        private void OpenLootPoint(LootingPoint _point)
        {
            isLooting = true;
            objLooting = _point;

            if (_point.openSound)
                GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, _point.openSound);

            LootingInventoryUI.instance.OpenLootingPanel(_point);
        }

        private void OpenLootPoint(RckAI _AI)
        {
            isLooting = true;
            LootingInventoryUI.instance.OpenLootingPanel(_AI.gameObject.GetComponentInChildren<LootingPoint>(), true, true, _AI);
        }

        private void CheckForDoors()
        {
            if(rayHit.transform.CompareTag("RPG Creation Kit/Door") && !isMounted)
            {
                // When we find one, we save this reference
                Door obj = rayHit.transform.gameObject.GetComponent<Door>();

                // Set the UI
                uiManager.Interact_text.gameObject.SetActive(true);

                if (obj.teleports)
                    uiManager.Interact_text.text = cmdStr+" Door to: " + obj.toCell.cellName;
                else
                {
                    if(!obj.isOpened)
                        uiManager.Interact_text.text = cmdStr+" Open: Door";
                    else
                        uiManager.Interact_text.text = cmdStr + " Close: Door";
                }

                // Check for opening door
                if (input.actions.FindAction("Interact").triggered && !WorldManager.instance.isLoading)
                {
                    if (obj.locked)
                    {
                        // We have the key
                        if (obj.key != null && Inventory.PlayerInventory.GetItemCount(obj.key.ItemID) > 0)
                        {
                            if (obj.onDoorOpenLocked != null)
                                GameAudioManager.instance.PlayOneShot(AudioSources.PlayerFPS, obj.onDoorOpenUnlocked);

                            obj.UnlockDoor();
                            OpenDoor(obj);
                        }
                        else
                        {
                            // We don't have the key, alert message the player
                            AlertMessage.instance.InitAlertMessage("You don't have the key to open this door", 3f);

                            if (obj.onDoorOpenLocked != null)
                                GameAudioManager.instance.PlayOneShot(AudioSources.PlayerFPS, obj.onDoorOpenLocked);
                        }
                    }
                    else
                    {
                        if (obj.onDoorOpen != null)
                            GameAudioManager.instance.PlayOneShot(AudioSources.PlayerFPS, obj.onDoorOpen);

                        OpenDoor(obj);
                    }
                }
            }
        }

        private void OpenDoor(Door obj)
        {
            if (obj.onDoorOpen)
                GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, obj.onDoorOpen);

            if (obj.teleports)
            {
                FreezeAndDisableControl();
                WorldManager.instance.LoadCell(obj.toCell, Random.Range(int.MinValue, int.MaxValue), false, obj, null, true);
                RckPlayer.instance.FadeScreen(true);
            }
            else
            {
                // Open door by rotating pivot/playing animation
                obj.OpenCloseDoor();
            }
        }

        public void StartDialogue(IDialoguable dialoguable)
        {
            speakingTo = dialoguable;

            speakingToHeadBlendshapes = dialoguable.GetEntityGameObject().GetComponentInChildren<HeadBlendshapesManager>();

            dialoguable.SetSpeakerIndex(0); // Always 0 here because the player is talking to someone
            dialoguable.OnDialogueStarts(this.gameObject);
            
            DialogueLogic(dialoguable.GetCurrentDialogueGraph());
        }

        void DialogueLogic(DialogueGraph dialogue)
        {
            if (dialogue != null)
                StartCoroutine(ExecuteNode(dialogue.GetEntryNode()));
        }

        public bool isPlainLineDialogue = false;
        IEnumerator ExecuteNode(XNode.Node _node)
        {
            // Trigger Events and Script
            DialogueNode globalNode = (DialogueNode)_node;

            RCKFunctions.ExecuteEvents(globalNode.events);

            if (!string.IsNullOrEmpty(globalNode.resultScript))
                RCKFunctions.ExecuteScript(globalNode.resultScript);

            if (_node is EntryNode)
            {
                EntryNode thisNode = (EntryNode)_node;
                StartCoroutine(ExecuteNode(thisNode.GetOutputPort("firstNode").Connection.node));
            }
            else if (_node is ConditionsNode)
            {
                ConditionsNode thisNode = (ConditionsNode)_node;

                bool conditionsResult = RCKFunctions.VerifyConditions(thisNode.conditions);

                if (conditionsResult)
                    StartCoroutine(ExecuteNode(thisNode.GetOutputPort("resultTrue").Connection.node));
                else
                    StartCoroutine(ExecuteNode(thisNode.GetOutputPort("resultFalse").Connection.node));

            }
            else if (_node is NPCDialogueLineNode)
            {
                NPCDialogueLineNode thisNode = (NPCDialogueLineNode)_node;

                // Set the Dialoguable state
                speakingTo.OnSpeakALine(thisNode);

                if (!thisNode.plainLine)
                {
                    input.SwitchCurrentActionMap("DialogueUI");

                    // Flag that we're in conversation
                    isInConversation = true;

                    // Disable player controls
                    mouseLook.SetLookAtTarget(speakingTo.GetEntityLookAtPos());
                    EnableDisableControls(false);

                    // Set the UI properly
                    uiManager.heardDialogues.gameObject.SetActive(false);

                    uiManager.playerQuestionsContainer.SetActive(false);
                    uiManager.compassGameObject.SetActive(false);
                    uiManager.crosshairGameObject.SetActive(false);


                    // Npc Name
                    uiManager.npc_Name.text = speakingTo.GetDialoguableName();
                    uiManager.npc_Name.gameObject.SetActive(true);

                    // Npc Line
                    uiManager.npc_Line.text = thisNode.line;
                    uiManager.npc_Line.gameObject.SetActive(true);

                    // Whole Dialogue UI
                    uiManager.dialogueContainer.SetActive(true);

                    cameraLookNPC = true;
                }
                else
                {
                    // Flag that we're in conversation
                    isInConversation = true;
                    isPlainLineDialogue = true;

                    uiManager.heardDialogues.text = thisNode.line;
                    uiManager.heardDialogues.gameObject.SetActive(true);
                }

                // - End UI

                // - End Updaters
                // Play the audio if exists
                float[] audioData = new float[0];
                if (thisNode.audioClip != null)
                {
                    speakingTo.PlayAudioClip(thisNode.audioClip);
                    audioData = new float[thisNode.audioClip.samples * thisNode.audioClip.channels];
                    thisNode.audioClip.GetData(audioData, 0);
                }

                // Play talking animation if choosen
                if(!string.IsNullOrEmpty(thisNode.dialogueAnimationStr))
                {
                    speakingTo.GetAnimator().CrossFade(thisNode.dialogueAnimationStr, .1f);
                }

                // Check for the timing of the line
                bool outOfTime = false;
                float startTime = Time.time;

                // To wait until the line ends
                while (!outOfTime)
                {
                    // If we press X button
                    if (input.actions.FindAction("SkipDialogue").triggered && Time.time - startTime >= 0.5f && !thisNode.plainLine || 
                        WorldManager.instance.isLoading)
                        outOfTime = true;

                    float currentMouthOpenValue = RckAIFacialAnim.minMouthOpen; // Initialize the current blendshape value.
                    if (audioData.Length > 0)
                    {
                        if (RCKSettings.NPCS_FACIAL_ANIM_ENABLED && speakingToHeadBlendshapes != null)
                        {
                            float mappedMouthOpenValue = RckAIFacialAnim.GetMouthMovementValue(startTime, thisNode.audioClip.frequency, audioData);

                            // Smoothly interpolate between the current and new blendshape values
                            while (Mathf.Abs(currentMouthOpenValue - mappedMouthOpenValue) > 1)
                            {
                                currentMouthOpenValue = Mathf.Lerp(currentMouthOpenValue, mappedMouthOpenValue, Time.deltaTime * RckAIFacialAnim.smoothingSpeed);
                                speakingToHeadBlendshapes.SetMouthBlendshape(currentMouthOpenValue);

                                // Prioritize user input
                                if (input.actions.FindAction("SkipDialogue").triggered && Time.time - startTime >= 0.5f && !thisNode.plainLine ||
                                    WorldManager.instance.isLoading)
                                {
                                    outOfTime = true;
                                    break;
                                }

                                yield return null;
                            }
                        }
                    }

                    // If the time expires
                    if (!thisNode.useLenghtOfClip)
                    {
                        if (Time.time - startTime >= thisNode.lineTime)
                            outOfTime = true;
                    }
                    else
                    {
                        if (Time.time - startTime >= (thisNode.audioClip.length + RCKSettings.DIALOGUE_ENDLINE_DELAY))
                            outOfTime = true;
                    }

                    yield return null;
                }

                speakingToHeadBlendshapes.ResetMouthBlendshapes();

                // Line has finished and player was teleported
                if (WorldManager.instance.isLoading)
                {
                    // Reset the player status
                    isInConversation = false;
                    isPlainLineDialogue = false;

                    // Disable the UI
                    if (!thisNode.plainLine)
                    {
                        uiManager.dialogueContainer.SetActive(false);


                        uiManager.horCompassGameObject.SetActive(true);
                        uiManager.compassGameObject.SetActive(true);
                        uiManager.crosshairGameObject.SetActive(true);
                        EnableDisableControls(false);

                        cameraLookNPC = false;
                    }
                    else
                    {
                        uiManager.heardDialogues.gameObject.SetActive(false);
                    }

                    // Destroy every previous question
                    foreach (Transform t in uiManager.playerQuestionsContent)
                        Destroy(t.gameObject);

                    speakingTo = null;
                    speakingToHeadBlendshapes = null;

                    yield break;
                }

                // Stop the audio source
                if(speakingTo != null)
                    speakingTo.StopAudio();


                // Check what we have to do next
                switch (thisNode.afterLine)
                {
                    case AfterLine.NPC_DialogueLine:
                        // If we have to trigger another dialogue line, we just re-start this courutine with the new line
                        StartCoroutine(ExecuteNode(thisNode.GetOutputPort("nextLine").Connection.node));
                        yield break;

                    case AfterLine.PlayerQuestions:

                        // Play listening animation if choosen
                        if (!string.IsNullOrEmpty(thisNode.dialogueAnimationListeningStr))
                        {
                            speakingTo.GetAnimator().CrossFade(thisNode.dialogueAnimationListeningStr, .1f);
                        }

                        if (thisNode.removePreviousQuestions)
                        {
                            foreach (Transform t in uiManager.playerQuestionsContent)
                                Destroy(t.gameObject);
                        }
                        else if (thisNode.questionsToRemove.Length > 0)
                        {
                            List<GameObject> toDelete = new List<GameObject>();
                            foreach (Transform t in uiManager.playerQuestionsContent)
                            {
                                for (int i = 0; i < thisNode.questionsToRemove.Length; i++)
                                {
                                    if (t.GetComponent<PlayerQuestionUI>().question.qID == thisNode.questionsToRemove[i])
                                        toDelete.Add(t.gameObject);
                                }
                            }

                            foreach (GameObject go in toDelete)
                            {
                                Destroy(go);
                            }
                        }

                        List<PlayerQuestionUI> questions = new List<PlayerQuestionUI>();
                        // Check if we have to add some new question
                        for (int i = 0; i < thisNode.playerQuestions.Count; i++)
                        {
                            int tempPosition = thisNode.playerQuestions[i].Position;
                            // In case, add them
                            GameObject newQuestion = Instantiate(uiManager.playerQuestionPrefab, uiManager.playerQuestionsContent);

                            if (tempPosition == 0)
                                tempPosition = i;

                            questions.Add(newQuestion.GetComponent<PlayerQuestionUI>());

                            newQuestion.GetComponent<PlayerQuestionUI>()
                                .Init(thisNode.playerQuestions[i], speakingTo,
                                (DialogueNode)thisNode.GetOutputPort("playerQuestions " + i).Connection.node,
                                thisNode.playerQuestions[i].Question, thisNode.playerQuestions[i].DeleteAfterAnswer,
                                tempPosition);

                            newQuestion.GetComponent<RectTransform>().SetSiblingIndex(tempPosition);
                        }


                        // Set the UI to allow the player to ask questions
                        uiManager.npc_Line.gameObject.SetActive(false);
                        uiManager.playerQuestionsContainer.SetActive(true);

                        // Reset the scroll bar of the PlayerQuestionScrollView
                        if (uiManager.playerQuestionsContainer.GetComponentInChildren<Scrollbar>() != null)
                            uiManager.playerQuestionsContainer.GetComponentInChildren<Scrollbar>().value = 1f;

                        speakingTo.OnFinishedSpeakingALine();

                        yield return new WaitForEndOfFrame();
                        uiManager.ReorderQuestions();
                        uiManager.PreventUnselectedQuestionWithGamepad();

                        yield return new WaitForEndOfFrame();
                        uiManager.PreventUnselectedQuestionWithGamepad();

                        yield break;

                    case AfterLine.EndDialogue:

                        speakingTo.OnDialogueEnds(this.gameObject);

                        // Reset the player status
                        isInConversation = false;

                        // Disable the UI
                        if (!thisNode.plainLine)
                        {
                            uiManager.dialogueContainer.SetActive(false);


                            uiManager.compassGameObject.SetActive(true);
                            uiManager.crosshairGameObject.SetActive(true);
                            EnableDisableControls(true);

                            cameraLookNPC = false;
                            EndDialogue();
                        }
                        else
                        {
                            isPlainLineDialogue = false;
                            uiManager.heardDialogues.gameObject.SetActive(false);
                        }

                        // Destroy every previous question
                        foreach (Transform t in uiManager.playerQuestionsContent)
                            Destroy(t.gameObject);

                        // Reset speaking to animator
                        if(speakingTo != null && speakingTo.GetAnimator() != null)
                            speakingTo.GetAnimator().SetTrigger("CancelDialogueAnimation");

                        speakingTo = null;
                        speakingToHeadBlendshapes = null;

                        yield break;

                    case AfterLine.Continue:
                        StartCoroutine(ExecuteNode(thisNode.GetOutputPort("output").Connection.node));
                        break;
                }
            }
            else if (_node is PlayerQuestionsNode)
            {
                PlayerQuestionsNode thisNode = (PlayerQuestionsNode)_node;

                // Play listening animation if choosen
                if (!string.IsNullOrEmpty(thisNode.dialogueAnimationListeningStr))
                {
                    speakingTo.GetAnimator().CrossFade(thisNode.dialogueAnimationListeningStr, .1f);
                }
                else
                {
                    // Reset speaking to animator
                    speakingTo.GetAnimator().SetTrigger("CancelDialogueAnimation");
                }

                // Flag that we're in conversation
                isInConversation = true;

                input.SwitchCurrentActionMap("DialogueUI");

                mouseLook.SetLookAtTarget(speakingTo.GetEntityLookAtPos());
                // Disable player controls
                EnableDisableControls(false);

                // Set the UI properly
                uiManager.heardDialogues.gameObject.SetActive(false);

                uiManager.playerQuestionsContainer.SetActive(false);
                uiManager.compassGameObject.SetActive(false);
                uiManager.crosshairGameObject.SetActive(false);


                // Npc Name
                uiManager.npc_Name.text = speakingTo.GetDialoguableName();
                uiManager.npc_Name.gameObject.SetActive(true);

                // Whole Dialogue UI
                uiManager.dialogueContainer.SetActive(true);

                cameraLookNPC = true;

                if (thisNode.removePreviousQuestions)
                {
                    foreach (Transform t in uiManager.playerQuestionsContent)
                        Destroy(t.gameObject);
                }
                else if (thisNode.questionsToRemove.Length > 0)
                {
                    List<GameObject> toDelete = new List<GameObject>();
                    foreach (Transform t in uiManager.playerQuestionsContent)
                    {
                        for (int i = 0; i < thisNode.questionsToRemove.Length; i++)
                        {
                            if (t.GetComponent<PlayerQuestionUI>().question.qID == thisNode.questionsToRemove[i])
                                toDelete.Add(t.gameObject);
                        }
                    }

                    foreach (GameObject go in toDelete)
                    {
                        Destroy(go);
                    }
                }

                List<PlayerQuestionUI> questions = new List<PlayerQuestionUI>();
                // Check if we have to add some new question
                for (int i = 0; i < thisNode.playerQuestions.Count; i++)
                {
                    int tempPosition = thisNode.playerQuestions[i].Position;
                    // In case, add them
                    GameObject newQuestion = Instantiate(uiManager.playerQuestionPrefab, uiManager.playerQuestionsContent);

                    if (tempPosition == 0)
                        tempPosition = i;

                    questions.Add(newQuestion.GetComponent<PlayerQuestionUI>());

                    newQuestion.GetComponent<PlayerQuestionUI>()
                        .Init(thisNode.playerQuestions[i], speakingTo,
                        (DialogueNode)thisNode.GetOutputPort("playerQuestions " + i).Connection.node,
                        thisNode.playerQuestions[i].Question, thisNode.playerQuestions[i].DeleteAfterAnswer,
                        tempPosition);

                    newQuestion.GetComponent<RectTransform>().SetSiblingIndex(tempPosition);
                }

                // Set the UI to allow the player to ask questions
                uiManager.npc_Line.gameObject.SetActive(false);
                uiManager.playerQuestionsContainer.SetActive(true);

                // Reset the scroll bar of the PlayerQuestionScrollView
                if (uiManager.playerQuestionsContainer.GetComponentInChildren<Scrollbar>() != null)
                    uiManager.playerQuestionsContainer.GetComponentInChildren<Scrollbar>().value = 1f;

                speakingTo.OnFinishedSpeakingALine();


                yield return new WaitForEndOfFrame();
                uiManager.ReorderQuestions();

                uiManager.PreventUnselectedQuestionWithGamepad();

                yield return new WaitForEndOfFrame();
                uiManager.PreventUnselectedQuestionWithGamepad();
            }
            else if (_node is ChangeDialogueNode)
            {
                ChangeDialogueNode thisNode = (ChangeDialogueNode)_node;

                if (thisNode.onNPCWhosSpeaking)
                {
                    speakingTo.ChangeDialogueGraph(thisNode.newDialogue);

                    if (thisNode.startDialogueImmediatly)
                    {
                        var npcSpeakingSaved = speakingTo;
                        EndDialogue();

                        StartDialogue(npcSpeakingSaved);
                    }
                    else
                        StartCoroutine(ExecuteNode(thisNode.GetOutputPort("output").Connection.node));

                }
                else
                {
                    RCKFunctions.ChangeDialogueToRckAI(thisNode.npcRef, thisNode.newDialogue.ID);
                    StartCoroutine(ExecuteNode(thisNode.GetOutputPort("output").Connection.node));
                }

                yield break;
            }
            else if (_node is ChangeStateNode)
            {
                ChangeStateNode thisNode = (ChangeStateNode)_node;

                switch (thisNode.stateChange)
                {
                    case ChangeStateNode.DialogueStateChange.Trade:

                        if (thisNode.toCurrentNPC)
                            RCKFunctions.StartTrading(speakingTo.GetEntityGameObject().GetComponent<Merchant>());

                        // TODO find npc and do stuff
                        break;

                    case ChangeStateNode.DialogueStateChange.Loot:

                        // TODO find looting point and do stuff

                        break;

                    case ChangeStateNode.DialogueStateChange.Teleport:

                        //WorldStreaming.manager.Teleport(thisNode.coordinates);

                        break;

                    case ChangeStateNode.DialogueStateChange.ChangeCell:

                        //WorldStreaming.manager.StartLoadingInterior(thisNode.cell.worldspace, thisNode.cell, null, thisNode.coordinates);

                        break;

                    case ChangeStateNode.DialogueStateChange.SpeakToNPC:

                        // TODO Find the NPC and do stuff

                        break;
                }

                if (thisNode.interruptsDialogue)
                    EndDialogue();

                StartCoroutine(ExecuteNode(thisNode.GetOutputPort("output").Connection.node));
            }
            else if (_node is RandomNode)
            {
                RandomNode thisNode = (RandomNode)_node;
                StartCoroutine(ExecuteNode(thisNode.PickRandomExit()));
            }
            else if(_node is Dialogue_AISetFieldNode)
            {
                Dialogue_AISetFieldNode thisNode = (Dialogue_AISetFieldNode)_node;
                thisNode.OnStart();

                if(thisNode.speakerID == 0) // Its to the NPC we're talking to
                {
                    var component = speakingTo.GetEntityGameObject().GetComponent<RckAI>(); // it's always the speaker with id 0 because it's player to ai dialogue

                    if (component == null)
                    {
                        Debug.Log("Dialogue Node : Tried to SetField but the given Component: \"RckAI\" was not found on Speaker Index 0. Node fails.");
                        yield break;
                    }

                    var field = component.GetType().GetField(thisNode.FieldToSet);

                    field.SetValue(component, thisNode.storedValue.GetValue());
                }

                StartCoroutine(ExecuteNode(thisNode.GetOutputPort("output").Connection.node));
            }
            else if(_node is Dialogue_AIInvokeNode)
            {
                Dialogue_AIInvokeNode thisNode = (Dialogue_AIInvokeNode)_node;

                NodesHelper.AIInvokeCall(thisNode.MethodToCall, speakingTo.GetEntityGameObject().GetComponent<RckAI>(), thisNode.parameters);

                StartCoroutine(ExecuteNode(thisNode.GetOutputPort("output").Connection.node));
            }
            else if (_node is EndNode)
            {
                EndNode thisNode = (EndNode)_node;

                // Reset speaking to animator
                speakingTo.GetAnimator().SetTrigger("CancelDialogueAnimation");
                speakingTo.OnDialogueEnds(this.gameObject);

                EndDialogue();
                yield break;
            }

            yield return null;
        }

        void EndDialogue()
        {
            //speakingTo.OnDialogueEnds(this.gameObject);
            mouseLook.ApplyCurrentRotation();
            mouseLook.ClearLookAt();

            // Reset the player status
            isInConversation = false;
            isPlainLineDialogue = false;

            // Disable the UI
            uiManager.dialogueContainer.SetActive(false);


            uiManager.compassGameObject.SetActive(true);
            uiManager.crosshairGameObject.SetActive(true);
            EnableDisableControls(true);

            cameraLookNPC = false;
            uiManager.heardDialogues.gameObject.SetActive(false);

            // Destroy every previous question
            foreach (Transform t in uiManager.playerQuestionsContent)
                Destroy(t.gameObject);

            speakingTo = null;
            speakingToHeadBlendshapes = null;

            // Input scheme
            input.SwitchCurrentActionMap("Player");

        }
        public static int SortByPosition(PlayerQuestionUI p1, PlayerQuestionUI p2)
        {
            return p1.position.CompareTo(p2.position);
        }

        /// <summary>
        /// Method called from PlayerQuestionUI to ask a question
        /// </summary>
        /// <param name="npc">The NPC with we are speaking with</param>
        /// <param name="response">His response to our question</param>
        /// <param name="DestroyQuestion">Destroy the question after answer?</param>
        /// <param name="theQuestion">The question to destroy</param>
        public void AskQuestion(IDialoguable _dialoguable, XNode.Node response, bool DestroyQuestion, GameObject theQuestion = null, bool isNpcToNpcDialogue = false, IDialoguable[] _dialoguables = null)
        {
            if (!isNpcToNpcDialogue)
                StartCoroutine(ExecuteNode(response));
            else
            {
                isInConversation = false;
                speakingTo = null;
                speakingToHeadBlendshapes = null;
                _dialoguable.ExecuteDialogueNode(response, _dialoguables);
            }

            // DestroyQuestion = What we said in the ScriptableObject
            // Destroy after .05f seconds to not have bad effects in the PlayerQuestionUI (Question disappear before the panel's fadeout)
            if (DestroyQuestion)
                Destroy(theQuestion, .05f);
        }

        private void CheckForIDraggable()
        {
            IDraggable draggable = rayHit.transform.gameObject.GetComponent<IDraggable>();

            if (draggable != null && !isDragging)
            {
                if(!isDragging && dragKeyBeingHeld)
                {
                    itemRigidbody = draggable.GetGameObject().GetComponent<Rigidbody>();
                    dragItemScript = draggable.GetGameObject().GetComponent<ItemInWorld>();

                    if (itemRigidbody != null)
                    {
                        draggable.OnBeingDragged();

                        BeginDragItem();
                    }
                }
            }
        }


        public float powerVector = 5f;

        public float droppingDistance = 3f;
        public float moveThresold = .1f;
        public float stopThresold = .1f;

        [SerializeField] float holdDistance = 1.2f;
        [SerializeField] float maxHoldDistance = 3.0f;

        public float spring = 80f;     // how strongly it pulls to target
        public float damping = 12f;    // how much it resists oscillation
        public float maxAccel = 120f;
        public float dragWhileHeld = 6f;
        public float angularDragWhileHeld = 6f;

        float originalDrag, originalAngularDrag;
        RigidbodyInterpolation originalInterpolation;

        private void BeginDragItem()
        {
            itemRigidbody.angularVelocity = Vector3.zero;
            itemRigidbody.linearVelocity = Vector3.zero;

            itemRigidbody.useGravity = false;

            originalDrag = itemRigidbody.linearDamping;
            originalAngularDrag = itemRigidbody.angularDamping;
            originalInterpolation = itemRigidbody.interpolation;

            itemRigidbody.linearDamping = dragWhileHeld;
            itemRigidbody.angularDamping = angularDragWhileHeld;
            itemRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            isDragging = true;
        }

        private void ManageDraggable()
        {
            if (!isDragging || itemRigidbody == null) { isDragging = false; return; }

            if (!dragKeyBeingHeld || !dragItemScript.canBeDragged)
            {
                StopDragging();
                return;
            }

            Vector3 targetPos = mainCamera.transform.position + mainCamera.transform.forward * holdDistance;

            float distToTarget = Vector3.Distance(itemRigidbody.position, targetPos);
            if (distToTarget > maxHoldDistance)
            {
                StopDragging();
                return;
            }

            // Spring-damper toward target
            Vector3 error = targetPos - itemRigidbody.position;
            Vector3 accel = (error * spring) - (itemRigidbody.linearVelocity * damping);

            // Clamp acceleration to avoid snapping
            accel = Vector3.ClampMagnitude(accel, maxAccel);

            itemRigidbody.AddForce(accel, ForceMode.Acceleration);
        }


        public void DisplayHeardLine(string _line, float _time)
        {
            uiManager.heardDialogues.text = _line;
            uiManager.heardDialogues.gameObject.SetActive(true);

            Invoke("HideHeardLine", _time);
        }

        public void HideHeardLine()
        {
            uiManager.heardDialogues.gameObject.SetActive(false);
        }

        /// <summary>
        /// Called when we stop to drag the item, reset its state to default world item
        /// </summary>
        public void StopDragging()
        {
            itemRigidbody.useGravity = true;

            itemRigidbody.linearVelocity = Vector3.zero;
            itemRigidbody.angularDamping = originalAngularDrag;
            itemRigidbody.linearDamping = originalDrag;

            itemRigidbody = null;
            isDragging = false;
        }

        public override void PlayerDeath()
        {
            base.PlayerDeath();
        }

        public void SetSpeakerIndex(int index)
        {
            throw new System.NotImplementedException();
        }

        public void DialogueLogic(IDialoguable[] entities, DialogueGraph dialogue)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator ExecuteDialogueNode(Node _node, IDialoguable[] _entities)
        {
            throw new System.NotImplementedException();
        }

        public void SpeakToEntity(IDialoguable target)
        {
            throw new System.NotImplementedException();
        }

        public void ChangeDialogueGraph(DialogueGraph _newDialogueGraph)
        {
            throw new System.NotImplementedException();
        }

        public bool CanDialogue()
        {
            return (isAlive && !isInConversation && !isInteracting && !isLooting && isLoaded);
        }

        public string GetDialoguableName()
        {
            throw new System.NotImplementedException();
        }

        public void OnDialogueStarts(GameObject target)
        {
            throw new System.NotImplementedException();
        }

        public void OnDialogueEnds(GameObject target)
        {
            throw new System.NotImplementedException();
        }

        public DialogueGraph GetCurrentDialogueGraph()
        {
            throw new System.NotImplementedException();
        }

        public void OnSpeakALine(NPCDialogueLineNode currentNode)
        {
            throw new System.NotImplementedException();
        }

        public void OnFinishedSpeakingALine()
        {
            throw new System.NotImplementedException();
        }

        public Transform GetEntityLookAtPos()
        {
            throw new System.NotImplementedException();
        }

        public GameObject GetEntityGameObject()
        {
            return gameObject;
        }

        public void PlayAudioClip(AudioClip clip)
        {
            throw new System.NotImplementedException();
        }

        public void StopAudio()
        {
            throw new System.NotImplementedException();
        }

        public bool IsTalking()
        {
            throw new System.NotImplementedException();
        }

        Animator IDialoguable.GetAnimator()
        {
            throw new System.NotImplementedException();
        }

        int IDialoguable.GetSpeakerIndex()
        {
            throw new System.NotImplementedException();
        }


        void IDialoguable.ForceToStopDialogue()
        {
            throw new System.NotImplementedException();
        }
    }
}