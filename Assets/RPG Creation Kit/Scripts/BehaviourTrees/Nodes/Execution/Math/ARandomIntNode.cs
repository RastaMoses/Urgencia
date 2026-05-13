using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree.Data;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CreateNodeMenu("RPGCK_BehaviourTree/Math/Random Int", order = 1)]
    [System.Serializable]
    public class ARandomIntNode : BTNode
    {
        public int min;
        public int max;

        public bool inclusive;

        public bool useVariable = true;
        public BT_Int storedValue;

        public int instantValue;


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

            BT_Int localVariable = (BT_Int)BTReference.SolveReference(this.graph as RPGCK_BT, storedValue.name, eAI);

            if (inclusive)
                localVariable.value = Random.Range(min, max + 1);
            else
                localVariable.value = Random.Range(min, max);

            thisNode.m_NodeState = NodeState.Success;
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