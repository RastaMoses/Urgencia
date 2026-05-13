using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.AI;
using System.Reflection;
using RPGCreationKit.BehaviourTree.Data;

namespace RPGCreationKit.BehaviourTree
{
    /// <summary>
    /// Allows the Invoking of a method with the attribute [BT_AIInvokable] from a BehaviourTree
    /// </summary>
    [CreateNodeMenu("RPGCK_BehaviourTree/Actions/AI/AI_Invoke", order = 1)]
    [System.Serializable]
    public class AI_InvokeNode : BTNode
    {
        public string MethodToCall;
        public ConditionParameter[] parameters = new ConditionParameter[5];

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

            thisNode.m_NodeState = NodeState.Running;

            NodesHelper.AIInvokeCall(MethodToCall, eAI, parameters);

            thisNode.m_NodeState = NodeState.Success;
            thisNode.hasEvaluated = true;
            return thisNode.m_NodeState;
        }

        public override void OnRemoveConnection(NodePort port)
        {
            base.OnRemoveConnection(port);
            indexInSequence = -1;
        }

        public void SolveEventualBTVariablesParameters(RckAI eAI)
        {
            for (int i = 0; i < parameters.Length; i++)
                if (parameters[i].btVariableValue != null)
                {
                    if (parameters[i].btVariableValue is BT_Float)
                        parameters[i].btVariableValue = (BT_Float)BTReference.SolveReference(this.graph as RPGCK_BT, parameters[i].btVariableValue.name, eAI);
                }

            eAI.GetNode(isInCombatBehavior, guidStr).solvedRef = true;
        }

        public override void OnStart(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            if (!thisNode.solvedRef)
                SolveEventualBTVariablesParameters(eAI);

            thisNode.STARTED = true;
        }


        public override void ReEvaluate(RckAI eAI)
        {
            if (eAI.GetNode(isInCombatBehavior, guidStr).m_NodeState != NodeState.Running)
                base.ReEvaluate(eAI);
        }
    }
}