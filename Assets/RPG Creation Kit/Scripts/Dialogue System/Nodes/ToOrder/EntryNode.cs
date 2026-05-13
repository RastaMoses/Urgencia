using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;


namespace RPGCreationKit.DialogueSystem
{
    [CreateNodeMenu("Dialogue System/Entry", order = 0)]
    [NodeTint("#068e00")]
    public class EntryNode : DialogueNode
    {
        /// Dialogue Settings
        public bool isNpcToNpcDialogue; // Is this a dialogue that doesn't involve the player?

        [Output] public Node firstNode;

        // Use this for initialization
        protected override void Init()
        {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            return null; // Replace this
        }

        public override void Trigger()
        {

        }
    }
}