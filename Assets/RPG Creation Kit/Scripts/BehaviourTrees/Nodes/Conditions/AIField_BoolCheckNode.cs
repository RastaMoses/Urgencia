using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.AI;
using RPGCreationKit.BehaviourTree.Data;
using System.Reflection;

namespace RPGCreationKit.BehaviourTree
{
    /// <summary>
    /// Allows the bool checking directly on a field of RckAI.
    /// </summary>
    [CreateNodeMenu("RPGCK_BehaviourTree/Actions/This AI/Checks/Field Bool Check", order = 1)]
    [System.Serializable]
    public class AIField_BoolCheckNode : BTNode
    {
        public bool not = false;

        public string ComponentToGet = "RckAI";
        public string FieldToGet;

        FieldInfo fieldInfo;

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

        public override void OnStart(RckAI eAI)
        {
            fieldInfo = eAI.GetType().GetField(FieldToGet);
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

            var component = eAI;

            bool boolToCheck = (bool)fieldInfo.GetValue(component);

            if (not)
                thisNode.m_NodeState = (boolToCheck) ? NodeState.Failure : NodeState.Success;
            else
                thisNode.m_NodeState = (boolToCheck) ? NodeState.Success : NodeState.Failure;

            thisNode.hasEvaluated = true;
            return thisNode.m_NodeState;
        }

        public override void OnRemoveConnection(NodePort port)
        {
            base.OnRemoveConnection(port);
            indexInSequence = -1;
        }

        public override void ReEvaluate(RckAI eAI)
        {
            if (eAI.GetNode(isInCombatBehavior, guidStr).m_NodeState != NodeState.Running)
                base.ReEvaluate(eAI);
        }
    }
}