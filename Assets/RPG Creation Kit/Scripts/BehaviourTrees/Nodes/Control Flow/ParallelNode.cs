using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using UnityEditor;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CreateNodeMenu("RPGCK_BehaviourTree/Parallel", order = 1)]
    [System.Serializable]
    public class ParallelNode : BTNode
    {
        [Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.None)] public BTNode outputs;

        public bool stopIfChildFails = false;
        public bool stopIfChildSucceed = false;

        // Use this for initialization
        protected override void Init()
        {
            base.Init();
        }

        public override void ReorderChild()
        {
            List<NodePort> endPorts = GetOutputPort("outputs").GetConnections();

            List<BTNode> nodes = new List<BTNode>();

            for (int i = 0; i < endPorts.Count; i++)
                nodes.Add((endPorts[i].node as BTNode));

            nodes.Sort(SortInGraphYPos);

            for (int i = 0; i < nodes.Count; i++)
            {
                BTNode btNode = nodes[i] as BTNode;
                btNode.indexInSequence = i;
            }

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            return null; // Replace this
        }

        public override NodeState Execute(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            thisNode.m_NodeState = NodeState.Running;

            if (thisNode.m_NodeState == NodeState.Success || thisNode.m_NodeState == NodeState.Failure)
                if (thisNode.hasEvaluated == true)
                    return thisNode.m_NodeState;

            bool anyChildStillRunning = false;
            List<NodePort> endPorts = GetOutputPort("outputs").GetConnections();

            for (int i = 0; i < endPorts.Count; i++)
            {
                BTNode node = (endPorts[i].node as BTNode);
                
                // If it's a parallel and a node has finished, restart it without waiting
                if ((eAI.GetNode(isInCombatBehavior, node.guidStr).hasEvaluated && (eAI.GetNode(isInCombatBehavior, node.guidStr).m_NodeState == NodeState.Success || eAI.GetNode(isInCombatBehavior, node.guidStr).m_NodeState == NodeState.Failure)))
                {
                    eAI.GetNode(isInCombatBehavior, node.guidStr).hasEvaluated = false;
                    node.ReEvaluate(eAI);
                }
                else if (eAI.GetNode(isInCombatBehavior, node.guidStr).hasEvaluated)
                    continue;

                switch ((endPorts[i].node as BTNode).Execute(eAI))
                {
                    case NodeState.Failure:
                        if (stopIfChildFails)
                        {
                            thisNode.m_NodeState = NodeState.Failure;

                            for (int j = 0; j < endPorts.Count; j++)
                                eAI.GetNode(isInCombatBehavior, (endPorts[j].node as BTNode).guidStr).m_NodeState = NodeState.Failure;

                            return thisNode.m_NodeState;
                        }
                        continue;

                    case NodeState.Success:
                        continue;

                    case NodeState.Running:
                        thisNode.m_NodeState = NodeState.Running;
                        anyChildStillRunning = true;
                        continue;

                    default:
                        thisNode.m_NodeState = NodeState.Running;
                        return thisNode.m_NodeState;
                }
            }
            
            thisNode.m_NodeState = (anyChildStillRunning) ? NodeState.Running : NodeState.Success;

            if (thisNode.m_NodeState == NodeState.Success)
                thisNode.hasEvaluated = true;

            return thisNode.m_NodeState;
        }

        public override void ReEvaluate(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            if (thisNode.m_NodeState != NodeState.Running)
            {
                base.ReEvaluate(eAI);
                thisNode.hasEvaluated = false;

                List<NodePort> endPorts = GetOutputPort("outputs").GetConnections();

                for (int i = 0; i < endPorts.Count; i++)
                {
                    (endPorts[i].node as BTNode).ReEvaluate(eAI);
                }
            }
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            base.OnCreateConnection(from, to);

#if UNITY_EDITOR
            EditorApplication.delayCall += ReorderChild;
#endif
        }

        public override void OnRemoveConnection(NodePort port)
        {
            if (port.fieldName == "input")
                indexInSequence = -1;
            else if (port.fieldName == "outputs")
            {
#if UNITY_EDITOR
                EditorApplication.delayCall += ReorderChild;
#endif
            }
        }

        public override void OnStart(RckAI eAI)
        {

        }
    }
}