using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit;

namespace RPGCreationKit.DialogueSystem
{
    [CreateNodeMenu("Dialogue System/End", order = 3)]
    [NodeTint("#8e0000")]
    public class EndNode : DialogueNode 
	{
        [Input(backingValue = ShowBackingValue.Never)] public EndNode endNode;

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