using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using UnityEditor;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CreateNodeMenu("RPGCK_BehaviourTree/Decorators/Repeater", order = 1)]
    [System.Serializable]
    public class DRepeaterNode : BTNode
    {
        [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.None)] public BTNode output;
        BTNode childNode;

        public bool repeatForever = false;
        public int count = 1;
        public bool endOnFail = false;

        // count became eAI.purposeData[guidStr].outputToExecute to reuse vars

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

            if (!thisNode.STARTED)
                OnStart(eAI);

            // Its state is the state of the child
            childNode = ((BTNode)GetOutputPort("output").Connection.node);

            if(eAI.GetNode(isInCombatBehavior, childNode.guidStr).m_NodeState == NodeState.Running)
            {
                childNode.Execute(eAI);
                return NodeState.Running;
            }

            childNode.ReEvaluate(eAI);
            childNode.Execute(eAI);

            thisNode.m_NodeState = eAI.GetNode(isInCombatBehavior, childNode.guidStr).m_NodeState;

            if(endOnFail)
            {
                thisNode.m_NodeState = NodeState.Failure;
                return thisNode.m_NodeState;
            }

            if (thisNode.m_NodeState == NodeState.Success)
            {
                if (!repeatForever && thisNode.outputToExecute >= count)
                {
                    thisNode.m_NodeState = NodeState.Success;
                    return thisNode.m_NodeState;
                }
                else
                {
                    thisNode.outputToExecute++;
                    thisNode.m_NodeState = NodeState.Running;
                }
            }

            thisNode.m_NodeState = NodeState.Running;
            return thisNode.m_NodeState;
        }


        public override void OnRemoveConnection(NodePort port)
        {
            base.OnRemoveConnection(port);

            if (port.fieldName == "input")
                indexInSequence = -1;
        }

        public override void OnStart(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            thisNode.outputToExecute = 0;
            thisNode.STARTED = true;
        }

        public override void ReorderChild()
        {
            ((BTNode)GetOutputPort("output").node).indexInSequence = 0;
        }
    }
}