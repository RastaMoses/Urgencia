using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.BehaviourTree.Data;
using UnityEditor;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CreateNodeMenu("RPGCK_BehaviourTree/Graph/Set BTVariable", order = 1)]
    [System.Serializable]
    public class SetGraphVariableNode : BTNode
    {
        public BTVariable btVariable;

        public BTParameter instantValue;

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

            BTVariable btVariableRuntime = BTReference.SolveReference(this.graph as RPGCK_BT, btVariable.name, eAI);

            switch (instantValue.parameterType)
            {
                case BTParameterType.BOOL:
                    btVariableRuntime.SetValue(instantValue.boolValue);
                    break;

                case BTParameterType.INT:
                    btVariableRuntime.SetValue(instantValue.intValue);
                    break;

                case BTParameterType.FLOAT:
                    btVariableRuntime.SetValue(instantValue.intValue);
                    break;
            }


            thisNode.m_NodeState = NodeState.Success;
            thisNode.hasEvaluated = true;
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
            eAI.GetNode(isInCombatBehavior, guidStr).STARTED = true;
        }
    }
}