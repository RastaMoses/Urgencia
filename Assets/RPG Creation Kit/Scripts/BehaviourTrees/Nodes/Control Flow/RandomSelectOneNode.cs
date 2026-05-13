using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using UnityEditor;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    /// <summary>
    /// This node selects randomly only one output and attempts to execute it.
    /// </summary>
    [CreateNodeMenu("RPGCK_BehaviourTree/RandomSelectOneNode", order = 1)]
    [System.Serializable]
    public class RandomSelectOneNode : BTNode
    {
        [Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.None)] public BTNode outputs;

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

            if (!thisNode.STARTED)
                OnStart(eAI);

            thisNode.m_NodeState = NodeState.Running;
            List<NodePort> endPorts = GetOutputPort("outputs").GetConnections();

            switch ((endPorts[thisNode.outputToExecute].node as BTNode).Execute(eAI))
            {
                case NodeState.Failure:
                    break;

                case NodeState.Success:
                    thisNode.m_NodeState = NodeState.Success;
                    return thisNode.m_NodeState;

                case NodeState.Running:
                    thisNode.m_NodeState = NodeState.Running;
                    return thisNode.m_NodeState;

                default:
                    break;
            }

            thisNode.m_NodeState = NodeState.Failure;
            return thisNode.m_NodeState;
        }

        public override void ReEvaluate(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            base.ReEvaluate(eAI);
            thisNode.hasEvaluated = false;

            if (thisNode.m_NodeState != NodeState.Running && thisNode.m_NodeState != NodeState.Null)
                OnStart(eAI);

            List<NodePort> endPorts = GetOutputPort("outputs").GetConnections();

            for (int i = 0; i < endPorts.Count; i++)
            {
                (endPorts[i].node as BTNode).ReEvaluate(eAI);
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
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            thisNode.outputToExecute = Random.Range(0, GetOutputPort("outputs").GetConnections().Count);
            thisNode.STARTED = true;
        }
    }
}