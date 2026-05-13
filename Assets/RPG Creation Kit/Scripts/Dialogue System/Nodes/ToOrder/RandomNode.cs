using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit;

namespace RPGCreationKit.DialogueSystem
{
    [CreateNodeMenu("Dialogue System/Random Node", order = 4)]
    [NodeTint("#264f69")]
    [System.Serializable]
    public class RandomNode : DialogueNode
    {
        [Output(dynamicPortList = true)] public List<DialogueNode> nodes;

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

        public DialogueNode PickRandomExit()
        {
            return GetOutputPort("nodes " + Random.Range(0, nodes.Count)).Connection.node as DialogueNode;
        }
    }
}