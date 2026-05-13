using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree.Data;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CreateNodeMenu("RPGCK_BehaviourTree/Comparison/Bool Check", order = 1)]
    [System.Serializable]
    public class CBoolCheckNode : BTNode
    {
        public bool not = false;
        public BT_Bool boolToCheck;

        public override void OnStart(RckAI eAI)
        {
            eAI.GetNode(isInCombatBehavior, guidStr).STARTED = true;
        }

        public override NodeState Execute(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            if (thisNode.m_NodeState == NodeState.Success || thisNode.m_NodeState == NodeState.Failure)
                if (thisNode.hasEvaluated == true)
                    return thisNode.m_NodeState;

            if (!thisNode.STARTED)
                OnStart(eAI);

            BT_Bool localBool = (BT_Bool)BTReference.SolveReference(this.graph as RPGCK_BT, boolToCheck.name, eAI); ;

            if (not)
                thisNode.m_NodeState = (localBool.value) ? NodeState.Failure : NodeState.Success;
            else
                thisNode.m_NodeState = (localBool.value) ? NodeState.Success : NodeState.Failure;

            thisNode.hasEvaluated = true;
            return thisNode.m_NodeState;
        }

        public override void ReEvaluate(RckAI eAI)
        {
            if (eAI.GetNode(isInCombatBehavior, guidStr).m_NodeState != NodeState.Running)
                base.ReEvaluate(eAI);
        }
    }
}