using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.DialogueSystem;
using XNode;

namespace RPGCreationKit.AI
{
    public class AITalkative : AIStatus, IDialoguable
    {
        public bool dialogueSystemEnabled = true;

        public IDialoguable speakingTo;
        public AILookAt aiLookAt;

        public DialogueGraph currentDialogueGraph;
        [HideInInspector] public DialogueGraph previousDialogueGraph;
        public DialogueGraph defaultDialogueGraph;

        // Variables to update the NPC state
        [HideInInspector] public bool isInConversation = false;
        [HideInInspector] public bool isTalking = false;
        [HideInInspector] public bool isListening = false;
        protected bool shouldLookAtTalker = false; // allows auto-rotate if the AI is speaking with his back turned to the speakingTo


        // Setting this in the consequences will allow NPC to follow and speak to the player
        [HideInInspector] public bool mustSpeakToPlayer = false;
        [HideInInspector] public bool mustSpeakToEntity = false;

        public int speakerIndex = -1;

        IEnumerator dialogueCoroutine = null;

        public void ChangeDialogueGraph(DialogueGraph _newDialogueGraph)
        {
            previousDialogueGraph = currentDialogueGraph;
            currentDialogueGraph = _newDialogueGraph;
        }

        public void ChangeDialogueGraph(string newDialogueID)
        {
            DialogueGraph newDialogueGraph = DialoguesDatabase.GetItem(newDialogueID);

            previousDialogueGraph = currentDialogueGraph;
            currentDialogueGraph = newDialogueGraph;
        }


        [AIInvokable]
        public void SpeakToAI(string _speakToID)
        {
            // Get the RckAI if possible
            RckAI sTo = null;
            CellsSystem.CellInformation.TryToGetAI(_speakToID, out sTo);

            if(sTo != null)
            {
                IDialoguable[] ppl = { this, sTo };
                DialogueLogic(ppl, currentDialogueGraph);
            }
        }

        [AIInvokable]
        public void SetSpeakerIndexToAI(string _rckAI, int _index)
        {
            // Get the RckAI if possible
            RckAI sTo = null;
            CellsSystem.CellInformation.TryToGetAI(_rckAI, out sTo);

            if (sTo != null)
            {
                sTo.SetSpeakerIndex(_index);
            }
        }

        public void DialogueLogic(IDialoguable[] entities, DialogueGraph dialogue)
        {
            dialogueCoroutine = (ExecuteDialogueNode(dialogue.GetEntryNode(), entities));
            StartCoroutine(dialogueCoroutine);
        }

        [SerializeField] public IDialoguable[] entitiesTalkingWith;
        public Node currentNode;

        public IEnumerator ExecuteDialogueNode(Node _node, IDialoguable[] _entities)
        {
            // Trigger Events and Script
            DialogueNode globalNode = (DialogueNode)_node;

            RCKFunctions.ExecuteEvents(globalNode.events);

            if (!string.IsNullOrEmpty(globalNode.resultScript))
                RCKFunctions.ExecuteScript(globalNode.resultScript);

            currentNode = _node;
            entitiesTalkingWith = _entities;

            for (int i = 0; i < _entities.Length; i++)
            {
                _entities[i].OnDialogueStarts(null);
            }

            if (_node is EntryNode)
            {
                EntryNode thisNode = (EntryNode)_node;
                dialogueCoroutine = (ExecuteDialogueNode(thisNode.GetOutputPort("firstNode").Connection.node, _entities));
                StartCoroutine(dialogueCoroutine);
            }
            else if (_node is ConditionsNode)
            {
                ConditionsNode thisNode = (ConditionsNode)_node;

                bool conditionsResult = RCKFunctions.VerifyConditions(thisNode.conditions);

                if (conditionsResult)
                {
                    dialogueCoroutine = (ExecuteDialogueNode(thisNode.GetOutputPort("resultTrue").Connection.node, _entities));
                    StartCoroutine(dialogueCoroutine);
                }
                else
                {
                    dialogueCoroutine = (ExecuteDialogueNode(thisNode.GetOutputPort("resultFalse").Connection.node, _entities));
                    StartCoroutine(dialogueCoroutine);
                }
            }
            else if(_node is NPCDialogueLineNode)
            {

                NPCDialogueLineNode thisNode = (NPCDialogueLineNode)_node;

                int speakerID = -1;

                // reset anim
                for (int i = 0; i < _entities.Length; i++)
                {
                    if (thisNode.speakerID == _entities[i].GetSpeakerIndex())
                    {
                        speakerID = _entities[i].GetSpeakerIndex();
                    }

                    _entities[i].GetAnimator().SetTrigger("CancelDialogueAnimation");
                }

                for (int i = 0; i < _entities.Length; i++) 
                {
                    if (thisNode.speakerID == _entities[i].GetSpeakerIndex())
                    {
                        // Set look at
                        AILookAt lookAt = _entities[i].GetEntityGameObject().GetComponent<AILookAt>();

                        if (lookAt != null)
                            lookAt.ForceToLookAtTarget(_entities[thisNode.lookAtEntityID].GetEntityLookAtPos());

                        _entities[i].GetAnimator().ResetTrigger("CancelDialogueAnimation");

                        // We found who have to speak:
                        _entities[i].OnSpeakALine(thisNode);

                        // Play talking animation if choosen
                        if (!string.IsNullOrEmpty(thisNode.dialogueAnimationStr))
                        {
                            _entities[i].GetAnimator().CrossFade(thisNode.dialogueAnimationStr, .1f);
                        }
                        else
                            _entities[i].GetAnimator().SetTrigger("CancelDialogueAnimation");


                        // if player is close enough
                        if (!isInOfflineMode && !Player.RckPlayer.instance.isInConversation && Vector3.Distance(Player.RckPlayer.instance.transform.position, _entities[i].GetEntityGameObject().transform.position) <= RCKSettings.PLAYER_HEARS_NPC_DIALOGUES_DISTANCE)
                            RCKFunctions.DisplayHeardLine(thisNode.line, thisNode.lineTime);

                        // Play the audio if exists
                        float[] audioData = new float[0];
                        if (thisNode.audioClip != null)
                        {
                            _entities[i].PlayAudioClip(thisNode.audioClip);
                            audioData = new float[thisNode.audioClip.samples * thisNode.audioClip.channels];
                            thisNode.audioClip.GetData(audioData, 0);
                        }

                        HeadBlendshapesManager headBlendshapes = _entities[i].GetEntityGameObject().GetComponentInChildren<HeadBlendshapesManager>();

                        // Check for the timing of the line
                        bool outOfTime = false;
                        float startTime = Time.time;

                        // To wait until the line ends
                        while (!outOfTime)
                        {
                            float currentMouthOpenValue = RckAIFacialAnim.minMouthOpen; // Initialize the current blendshape value.
                            if (audioData.Length > 0)
                            {
                                if (RCKSettings.NPCS_FACIAL_ANIM_ENABLED && headBlendshapes != null)
                                {
                                    float mappedMouthOpenValue = RckAIFacialAnim.GetMouthMovementValue(startTime, thisNode.audioClip.frequency, audioData);

                                    // Smoothly interpolate between the current and new blendshape values
                                    while (Mathf.Abs(currentMouthOpenValue - mappedMouthOpenValue) > 1)
                                    {
                                        currentMouthOpenValue = Mathf.Lerp(currentMouthOpenValue, mappedMouthOpenValue, Time.deltaTime * RckAIFacialAnim.smoothingSpeed);
                                        headBlendshapes.SetMouthBlendshape(currentMouthOpenValue);
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

                        if(headBlendshapes)
                            headBlendshapes.ResetMouthBlendshapes();
                        _entities[i].StopAudio();

                        // Check what we have to do next
                        switch (thisNode.afterLine)
                        {
                            case AfterLine.NPC_DialogueLine:
                                // If we have to trigger another dialogue line, we just re-start this courutine with the new line
                                dialogueCoroutine = (ExecuteDialogueNode(thisNode.GetOutputPort("nextLine").Connection.node, _entities));
                                StartCoroutine(dialogueCoroutine);
                                yield break;

                            case AfterLine.PlayerQuestions:
                                /*
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

                                questions.Sort(SortByPosition);
                                for (int i = 0; i < questions.Count; i++)
                                {
                                    questions[i].GetComponent<RectTransform>().SetSiblingIndex(questions[i].position);
                                }

                                // Set the UI to allow the player to ask questions
                                uiManager.npc_Line.gameObject.SetActive(false);
                                uiManager.playerQuestionsContainer.SetActive(true);

                                // Reset the scroll bar of the PlayerQuestionScrollView
                                if (uiManager.playerQuestionsContainer.GetComponentInChildren<Scrollbar>() != null)
                                    uiManager.playerQuestionsContainer.GetComponentInChildren<Scrollbar>().value = 1f;

                                speakingTo.OnFinishedSpeakingALine();

                                // Sleect first for input system if gamepad
                                if (RckInput.isUsingGamepad)
                                    questions[0].GetComponent<Button>().Select();
                                */
                                yield break;

                            case AfterLine.EndDialogue:
                                _entities[i].OnDialogueEnds(this.gameObject);

                                // Reset the player status
                                isInConversation = false;

                                
                                // Reset speaking to animator
                                _entities[i].GetAnimator().SetTrigger("CancelDialogueAnimation");

                                if (lookAt != null)
                                    lookAt.StopForcingLookAtTarget();

                                speakingTo = null;
                                yield break;

                            case AfterLine.Continue:
                                dialogueCoroutine = (ExecuteDialogueNode(thisNode.GetOutputPort("output").Connection.node, _entities));
                                StartCoroutine(dialogueCoroutine);
                                break;
                        }
                    }
                    else
                    {
                        //AILookAt lookAt = _entities[i].GetEntityGameObject().GetComponent<AILookAt>();

                        //if (lookAt != null)
                        //    lookAt.ForceToLookAtTarget(_entities[speakerID].GetEntityLookAtPos());
                    }
                }
            }
            else if (_node is ChangeDialogueNode)
            {
                ChangeDialogueNode thisNode = (ChangeDialogueNode)_node;

                if (!string.IsNullOrEmpty(thisNode.npcRef))
                    RCKFunctions.ChangeDialogueToRckAI(thisNode.npcRef, thisNode.newDialogue.ID);

                StartCoroutine(ExecuteDialogueNode(thisNode.GetOutputPort("output").Connection.node, _entities));
            }
            else if(_node is EndNode)
            {
                for (int i = 0; i < _entities.Length; i++)
                {
                    _entities[i].OnDialogueEnds(this.gameObject);
                    // Reset speaking to animator
                    _entities[i].GetAnimator().SetTrigger("CancelDialogueAnimation");

                    AILookAt lookAt = _entities[i].GetEntityGameObject().GetComponent<AILookAt>();

                    if (lookAt != null)
                        lookAt.StopForcingLookAtTarget();
                }

                entitiesTalkingWith = null;
            }

            yield return null;
        }

        [AIInvokable]
        public void SetSpeakerIndex(int index)
        {
            speakerIndex = index;
        }

        public virtual void SpeakToEntity(IDialoguable target)
        {
            throw new System.NotImplementedException();
        }

        public bool CanDialogue()
        {
            return (dialogueSystemEnabled && isAlive && !isHostile && !isHostileAgainstPC && !isInCombat && !isInConversation && !isDrawingWeapon
                    && (currentDialogueGraph != null || defaultDialogueGraph != null && isLoaded));
        }

        public string GetDialoguableName()
        {
            return entityName;
        }

        [SerializeField] private bool excludeFromAutoRotate = false;
        public virtual void OnDialogueStarts(GameObject target)
        {
            if(target != null)
            speakingTo = target.GetComponent<IDialoguable>();

            // Rotate the Npc to look at us
            isInConversation = true;

            // The following is used to auto-rotate AI if they are talking behind their speakingTo even if dialogueLookAt is disabled
            shouldLookAtTalker = false;
            if (isInConversation && speakingTo != null && (RCKSettings.AUTO_ROTATE_AI_IF_DIALOGUE_BEHIND && !excludeFromAutoRotate))
            {
                // Check if the AI is behind his speakingTo
                float dotProduct = Vector3.Dot(transform.position - speakingTo.GetEntityGameObject().transform.position, transform.forward);
                shouldLookAtTalker = dotProduct > 0;
            }

            if (target != null && aiLookAt != null)
                aiLookAt.ForceToLookAtTarget(target.transform);

            //npc.SetTarget(transform);
            //npc.FaceTarget(true);

            // Stop the agent
            //npc.components.agent.isStopped = true;

        }

        public virtual void OnDialogueEnds(GameObject target)
        {
            isInConversation = false;
            isTalking = false;
            isListening = false;
            speakingTo = null;
            entitiesTalkingWith = null;

            if (aiLookAt != null)
                aiLookAt.StopForcingLookAtTarget();

            UpdateConversationsAnimator();

            if(m_Anim)
                m_Anim.SetTrigger("CancelDialogueAnimation");

            // Reset the NPC state
            //npcSpeaking.ResetStates();

            if (!isHostile)
            {
                // Send the NPC to his path if he have got one
                //if (npcSpeaking.PatrolPath != null)
                //    npcSpeaking.StartCoroutine("GoToPath");
            }
        }

        public DialogueGraph GetCurrentDialogueGraph()
        {
            if (currentDialogueGraph != null)
                return currentDialogueGraph;
            else if(defaultDialogueGraph != null)
            {
                Debug.Log("AITalkative: " + this.gameObject.name + " has no currentDialogueGraph assigned, falling back to default");
                return defaultDialogueGraph;
            }
            else
            {
                Debug.LogWarning("AITalkative: " + this.gameObject.name + " has no currentDialogueGraph or defaultDialogueGraph assigned.");
                OnDialogueEnds(null);
                return null;
            }
        }

        public virtual void OnSpeakALine(NPCDialogueLineNode currentNode)
        {
            // Set the NPC state
            isTalking = true;
            isListening = false;

            UpdateConversationsAnimator();
            //npcSpeaking.state = NPC_State.Speaking;
        }

        public virtual void OnFinishedSpeakingALine()
        {
            // Set the NPC state
            isTalking = false;
            isListening = true;
            UpdateConversationsAnimator();
        }

        public void QuickDialogueMouthAnim(AudioClip clip)
        {
            // Play the audio if exists
            float[] audioData = new float[0];
            if (clip != null)
            {
                audioData = new float[clip.samples * clip.channels];
                clip.GetData(audioData, 0);
            }

            HeadBlendshapesManager headBlendshapes = GetComponentInChildren<HeadBlendshapesManager>();

            StopCoroutine("QuickDialogueMouthAnimTask");
            StartCoroutine(QuickDialogueMouthAnimTask(headBlendshapes, audioData, clip));
        }

        IEnumerator QuickDialogueMouthAnimTask(HeadBlendshapesManager headBlendshapes, float[] audioData, AudioClip clip)
        {
            // Check for the timing of the line
            bool outOfTime = false;
            float startTime = Time.time;

            // To wait until the line ends
            while (!outOfTime)
            {
                float currentMouthOpenValue = RckAIFacialAnim.minMouthOpen; // Initialize the current blendshape value.
                if (audioData.Length > 0)
                {
                    if (RCKSettings.NPCS_FACIAL_ANIM_ENABLED && headBlendshapes != null)
                    {
                        float mappedMouthOpenValue = RckAIFacialAnim.GetMouthMovementValue(startTime, clip.frequency, audioData);

                        // Smoothly interpolate between the current and new blendshape values
                        while (Mathf.Abs(currentMouthOpenValue - mappedMouthOpenValue) > 1)
                        {
                            currentMouthOpenValue = Mathf.Lerp(currentMouthOpenValue, mappedMouthOpenValue, Time.deltaTime * RckAIFacialAnim.smoothingSpeed);
                            headBlendshapes.SetMouthBlendshape(currentMouthOpenValue);
                            yield return null;
                        }
                    }
                }

                if (Time.time - startTime >= clip.length)
                    outOfTime = true;

                yield return null;
            }

            headBlendshapes.ResetMouthBlendshapes();
        }

        public Transform GetEntityLookAtPos()
        {
            return entityFocusPart;
        }

        public void PlayAudioClip(AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }

        public void StopAudio()
        {
            if(audioSource)
                audioSource.Stop();
        }

        public override void Die()
        {
            base.Die();
        }

        public GameObject GetEntityGameObject()
        {
            return this.gameObject;
        }

        public bool IsTalking()
        {
            return isTalking;
        }

        public void UpdateConversationsAnimator()
        {
            if (!isAlive) return;

            if (m_Anim != null)
            {
                m_Anim.SetBool("Talking", isTalking);
                m_Anim.SetBool("Listening", isListening);
            }
        }

        Animator IDialoguable.GetAnimator()
        {
            return m_Anim;
        }

        int IDialoguable.GetSpeakerIndex()
        {
            return speakerIndex;
        }

        public bool forceStopDialogue = false;
        public void ForceToStopDialogue()
        {
            if(dialogueCoroutine != null)
                StopCoroutine(dialogueCoroutine);

            StopCoroutine(ExecuteDialogueNode(currentNode, entitiesTalkingWith));

            isInConversation = false;
            isTalking = false;
            isListening = false;
            speakingTo = null;
            entitiesTalkingWith = null;

            if (aiLookAt != null)
                aiLookAt.StopForcingLookAtTarget();

            UpdateConversationsAnimator();
            m_Anim.SetTrigger("CancelDialogueAnimation");

            OnDialogueEnds(null);
        }

        public HeadBlendshapesManager GetHeadBlendshapesManager()
        {
            return null;
        }
    }
}