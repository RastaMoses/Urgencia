using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit;
using RPGCreationKit.BehaviourTree.Data;
using RPGCreationKit.AI;

namespace RPGCreationKit.DialogueSystem
{
    [CreateNodeMenu("Dialogue System/Actions/AI Invoke", order = 1)]
    public class Dialogue_AIInvokeNode : DialogueNode
    {
        public int speakerID = 0;

        RckAI thisAI;

        public string MethodToCall;
        public ConditionParameter[] parameters = new ConditionParameter[5];

        public void OnStart()
        {

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