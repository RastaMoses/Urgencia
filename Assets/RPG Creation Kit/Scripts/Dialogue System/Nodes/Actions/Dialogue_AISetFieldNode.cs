using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit;
using RPGCreationKit.BehaviourTree.Data;

namespace RPGCreationKit.DialogueSystem
{
    [CreateNodeMenu("Dialogue System/Actions/AI SetField", order = 1)]
    public class Dialogue_AISetFieldNode : DialogueNode
    {
        public int speakerID = 0;

        public string ComponentToSet = "RckAI";
        public string FieldToSet = "";
        public BTParameter instantValue;
        public BTVariable storedValue;

        public void OnStart()
        {
            storedValue = NodesHelper.SetStoredValueWithInstantValue(instantValue);
        }

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