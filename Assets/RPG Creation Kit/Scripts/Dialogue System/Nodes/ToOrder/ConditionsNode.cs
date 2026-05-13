using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit;

namespace RPGCreationKit.DialogueSystem
{
    [CreateNodeMenu("Dialogue System/Condition", order = 3)]
    [NodeTint("#9a5d13")]
    public class ConditionsNode : DialogueNode 
	{

        [Output(backingValue = ShowBackingValue.Never)] public DialogueNode resultTrue;
        [Output(backingValue = ShowBackingValue.Never)] public DialogueNode resultFalse;

        [TextArea] public string comment = "#Comment";
        public Condition[] conditions;

		// Use this for initialization
		protected override void Init() {
			base.Init();
			
		}

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
			return null; // Replace this
		}

        public override void Trigger()
        {
            throw new System.NotImplementedException();
        }
    }
}