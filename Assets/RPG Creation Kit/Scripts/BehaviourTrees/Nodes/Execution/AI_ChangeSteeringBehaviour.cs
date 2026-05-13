using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    /// <summary>
    /// Changes the Steering Behaviour of this AI
    /// </summary>
    [CreateNodeMenu("RPGCK_BehaviourTree/Actions/AI/Change Steering Behaviour", order = 1)]
    [System.Serializable]
    public class AI_ChangeSteeringBehaviour : BTNode
    {
        public SteeringBehaviours behaviour;

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

            eAI.selectedSteeringBehaviour = behaviour;

            thisNode.m_NodeState = NodeState.Success;
            thisNode.hasEvaluated = true;
            return thisNode.m_NodeState;
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

        public override void ReEvaluate(RckAI eAI)
        {
            if (eAI.GetNode(isInCombatBehavior, guidStr).m_NodeState != NodeState.Running)
                base.ReEvaluate(eAI);
        }
    }
}
