using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using UnityEditor;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CreateNodeMenu("RPGCK_BehaviourTree/Random Selector", order = 1)]
    [System.Serializable]
    public class RandomSelectorNode : BTNode
    {
        [Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.None)] public BTNode outputs;


        // Use this for initialization
        protected override void Init()
        {
            base.Init();
        }

        [ContextMenu("Work")]
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

            if (thisNode.m_NodeState == NodeState.Success || thisNode.m_NodeState == NodeState.Failure)
                if (thisNode.hasEvaluated == true)
                    return thisNode.m_NodeState;

            if (!thisNode.STARTED)
                OnStart(eAI);

            List<NodePort> endPorts = GetOutputPort("outputs").GetConnections();

            if (thisNode.nodeExecuting != null && eAI.GetNode(isInCombatBehavior, thisNode.nodeExecuting.guidStr).m_NodeState != NodeState.Failure && eAI.GetNode(isInCombatBehavior, thisNode.nodeExecuting.guidStr).m_NodeState != NodeState.Success)
            {
                switch (thisNode.nodeExecuting.Execute(eAI))
                {
                    case NodeState.Failure:
                        break;

                    case NodeState.Success:
                        thisNode.m_NodeState = NodeState.Success;
                        thisNode.hasEvaluated = true;
                        return thisNode.m_NodeState;

                    case NodeState.Running:
                        thisNode.m_NodeState = NodeState.Running;
                        return thisNode.m_NodeState;

                    default:
                        break;
                }
            }
            else if (thisNode.nodeExecuting != null && eAI.GetNode(isInCombatBehavior, thisNode.nodeExecuting.guidStr).m_NodeState == NodeState.Success)
            {
                thisNode.m_NodeState = NodeState.Success;
                thisNode.hasEvaluated = true;
                return thisNode.m_NodeState;
            }

            int randomIndex = Random.Range(0, endPorts.Count);
            thisNode.nodeExecuting = endPorts[randomIndex].node as BTNode;

            return NodeState.Running;
        }

        public override void ReEvaluate(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            if (thisNode.m_NodeState != NodeState.Running)
            {
                base.ReEvaluate(eAI);
                thisNode.hasEvaluated = false;
                thisNode.nodeExecuting = null;
                OnStart(eAI);

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
            base.OnRemoveConnection(port);

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
            eAI.GetNode(isInCombatBehavior, guidStr).STARTED = true;
        }
    }
}