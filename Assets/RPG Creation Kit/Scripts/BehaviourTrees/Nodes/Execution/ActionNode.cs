using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CreateNodeMenu("RPGCK_BehaviourTree/ActionNode", order = 1)]
    [System.Serializable]
    public class ActionNode : BTNode 
	{
        public string debugs = "";
        public bool executionGoesWell = false;

        public float timer = 0f;

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

        public override NodeState Execute(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            if (thisNode.m_NodeState == NodeState.Success || thisNode.m_NodeState == NodeState.Failure)
                if (thisNode.hasEvaluated == true)
                    return thisNode.m_NodeState;

            if (!thisNode.STARTED)
                OnStart(eAI);

            thisNode.startTime += 1 * Time.deltaTime;
            if (thisNode.startTime >= timer)
            {
                if(!string.IsNullOrEmpty(debugs))
                    Debug.Log(debugs);


                thisNode.m_NodeState = (executionGoesWell) ? NodeState.Success : NodeState.Failure;
                return thisNode.m_NodeState;
            }

            thisNode.hasEvaluated = true;

            thisNode.m_NodeState = NodeState.Running;
            return thisNode.m_NodeState;
        }

        public override void ReEvaluate(RckAI eAI)
        {
            if (eAI.GetNode(isInCombatBehavior, guidStr).m_NodeState != NodeState.Running)
            {
                base.ReEvaluate(eAI);
                OnStart(eAI);
            }
        }


        public override void OnRemoveConnection(NodePort port)
        {
            base.OnRemoveConnection(port);
            indexInSequence = -1;
        }

        public override void OnStart(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            thisNode.STARTED = true;
            thisNode.startTime = 0;
        }
    }
}