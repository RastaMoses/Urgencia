using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit;

namespace RPGCreationKit.DialogueSystem
{
    [CreateNodeMenu("Dialogue System/Events", order = 2)]
    public class EventsNode : DialogueNode
    {
        // Use this for initialization
        protected override void Init()
        {
            base.Init();

        }

        public override void Trigger()
        {

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            return null; // Replace this
        }
    }
}