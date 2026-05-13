using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CreateNodeMenu("RPGCK_BehaviourTree/Decorators/Return Success", order = 1)]
    [System.Serializable]
    public class DReturnSuccess : BTNode
    {
        [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.None)] public BTNode output;
        BTNode childNode;

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

            // Its state is the state of the child
            childNode = ((BTNode)GetOutputPort("output").Connection.node);
            childNode.Execute(eAI);

            if(eAI.GetNode(isInCombatBehavior, childNode.guidStr).m_NodeState != NodeState.Null && eAI.GetNode(isInCombatBehavior, childNode.guidStr).m_NodeState != NodeState.Running)
            {
                thisNode.m_NodeState = NodeState.Success;
                thisNode.hasEvaluated = true;
                return thisNode.m_NodeState;
            }

            thisNode.m_NodeState = NodeState.Running;
            return thisNode.m_NodeState;
        }

        public override void ReEvaluate(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            if (thisNode.m_NodeState != NodeState.Running)
            {
                base.ReEvaluate(eAI);
                thisNode.hasEvaluated = false;

                // Its state is the state of the child
                childNode = ((BTNode)GetOutputPort("output").Connection.node);
                childNode.ReEvaluate(eAI);

                thisNode.m_NodeState = NodeState.Running;
            }
        }

        public override void OnRemoveConnection(NodePort port)
        {
            base.OnRemoveConnection(port);

            if (port.fieldName == "input")
                indexInSequence = -1;
        }

        public override void OnStart(RckAI eAI)
        {
            eAI.GetNode(isInCombatBehavior, guidStr).STARTED = true;
        }

        public override void ReorderChild()
        {
            //((BTNode)GetOutputPort("output").node).indexInSequence = 0;
        }
    }
}