using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CreateNodeMenu("RPGCK_BehaviourTree/Reset Below", order = 1)]
    [System.Serializable]
    public class ResetBelowNode : BTNode
    {
        protected override void Init()
        {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            return null; // Replace this
        }


        public int sortbyindex(BTNode a, BTNode b)
        {
            return a.indexInSequence.CompareTo(b.indexInSequence);
        }

        public override NodeState Execute(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            if (thisNode.m_NodeState == NodeState.Success || thisNode.m_NodeState == NodeState.Failure)
                if (thisNode.hasEvaluated == true)
                    return thisNode.m_NodeState;

            if (!thisNode.STARTED)
                OnStart(eAI);

            List<NodePort> endPorts = GetInputPort("input").GetConnection(0).node.GetOutputPort("outputs").GetConnections();

            List<BTNode> nodes = new List<BTNode>();

            for (int i = 0; i < endPorts.Count; i++)
                nodes.Add((endPorts[i].node as BTNode));

            nodes.Sort(sortbyindex);

            for (int i = indexInSequence+1; i < nodes.Count; i++) 
            {
                //Debug.Log("Resetting: " + ((nodes[i]) as BTNode) + " | " + ((nodes[i]) as BTNode).name); 
                eAI.GetNode(isInCombatBehavior, ((nodes[i]) as BTNode).guidStr).m_NodeState = NodeState.Failure;
                eAI.GetNode(isInCombatBehavior, ((nodes[i]) as BTNode).guidStr).hasEvaluated = true;
                ((nodes[i]) as BTNode).ReEvaluate(eAI);
            }


            thisNode.m_NodeState = NodeState.Success;
            thisNode.hasEvaluated = true;
            return thisNode.m_NodeState;
        }

        public override void ReEvaluate(RckAI eAI)
        {
            if (eAI.GetNode(isInCombatBehavior, guidStr).m_NodeState != NodeState.Running)
                base.ReEvaluate(eAI);
        }


        public override void OnRemoveConnection(NodePort port)
        {
            base.OnRemoveConnection(port);
            indexInSequence = -1;
        }

        public override void OnStart(RckAI eAI)
        {
            eAI.GetNode(isInCombatBehavior, guidStr).STARTED = true;
        }
    }
}