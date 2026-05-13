using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit;

namespace RPGCreationKit.DialogueSystem
{
    [System.Serializable]
    public abstract class DialogueNode : Node
    {
        [Input(backingValue = ShowBackingValue.Never)] public DialogueNode input;
        [Output(backingValue = ShowBackingValue.Never)] public DialogueNode output;

        public string resultScript;

        [SerializeField] public Events events;

        abstract public void Trigger();

        public override object GetValue(NodePort port)
        {
            return null;
        }
    }
}