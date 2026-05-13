using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit.DialogueSystem;

namespace RPGCreationKit
{
    /// <summary>
    /// Used on objects that can be talked to, with the player or other Dialoguables.
    /// </summary>
    public interface IDialoguable
    {
        /// <summary>
        /// Sets the Speaker Index to the entity
        /// </summary>
        /// <param name="index"></param>
        void SetSpeakerIndex(int index);

        int GetSpeakerIndex();

        /// <summary>
        /// Initializes the dialogue logic
        /// </summary>
        /// <param name="entities">On the entities:</param>
        /// <param name="dialogue">Given the graph:</param>
        void DialogueLogic(IDialoguable[] entities, DialogueGraph dialogue);

        /// <summary>
        /// Executes a DialogueNode
        /// </summary>
        /// <param name="_node">The node</param>
        /// <param name="_entities">On the entities</param>
        /// <returns></returns>
        IEnumerator ExecuteDialogueNode(XNode.Node _node, IDialoguable[] _entities);

        /// <summary>
        /// The Entity will reach the target and speak to it with its current DialogueGraph
        /// </summary>
        /// <param name="entity"></param>
        void SpeakToEntity(IDialoguable target);

        /// <summary>
        /// The entity will change its current DialogueGraph
        /// </summary>
        /// <param name="_newDialogueGraph">The new DialogueGraph</param>
        void ChangeDialogueGraph(DialogueGraph _newDialogueGraph);

        /// <summary>
        /// Returns true if the Dialoguable Entity can perform a conversation.
        /// </summary>
        /// <returns></returns>
        bool CanDialogue();

        /// <summary>
        /// Gets the name of the dialoguable entity
        /// </summary>
        /// <returns></returns>
        string GetDialoguableName();

        /// <summary>
        /// Code that runs as soon as someone starts talking with this entity
        /// </summary>
        void OnDialogueStarts(GameObject target);

        /// <summary>
        /// Code that runs when the entity has finished dialoguing.
        /// </summary>
        void OnDialogueEnds(GameObject target);

        /// <summary>
        /// Gets the current DialogueGraph of the entity
        /// </summary>
        /// <returns></returns>
        DialogueGraph GetCurrentDialogueGraph();

        /// <summary>
        /// Called when the entity has to speak a line of dialogue.
        /// </summary>
        /// <param name="currentNode"></param>
        void OnSpeakALine(NPCDialogueLineNode currentNode);

        /// <summary>
        /// Called when the entity finish his line, usually used to reset its state.
        /// </summary>
        void OnFinishedSpeakingALine();

        /// <summary>
        /// Gets the entity transform, used to look at the entity while speaking to it. This can be null.
        /// </summary>
        /// <returns></returns>
        Transform GetEntityLookAtPos();

        /// <summary>
        /// Gets the GameObject of this Dialoguable.
        /// </summary>
        /// <returns></returns>
        GameObject GetEntityGameObject();

        /// <summary>
        /// Plays an audio clip, usually the voice file of the current dialogue line.
        /// </summary>
        /// <param name="clip">The clip to play</param>
        void PlayAudioClip(AudioClip clip);

        /// <summary>
        /// Stops any audio clip this IDialoguable is playing.
        /// </summary>
        void StopAudio();

        /// <summary>
        /// Returns wheter or not the entity is speaking a line or not.
        /// </summary>
        /// <returns></returns>
        bool IsTalking();

        Animator GetAnimator();
        void ForceToStopDialogue();
    }
}