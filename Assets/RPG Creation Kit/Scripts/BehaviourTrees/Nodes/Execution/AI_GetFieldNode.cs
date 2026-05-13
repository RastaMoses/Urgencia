using RPGCreationKit.AI;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.BehaviourTree.Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using XNode;

namespace RPGCreationKit.BehaviourTree
{
    /// <summary>
    /// Allows the Invoking of a method with the attribute [BT_AIInvokable] from a BehaviourTree
    /// </summary>
    [CreateNodeMenu("RPGCK_BehaviourTree/Actions/AI/Get Field", order = 1)]
    [System.Serializable]
    public class AI_GetFieldNode : BTNode
    {
        public string ComponentToGet = "RckAI";
        public string FieldToGet;
        FieldInfo fieldInfo;

        public BTVariable storedValue;

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

            BTVariable storedValueRuntime = BTReference.SolveReference(this.graph as RPGCK_BT, storedValue.name, eAI);

            // Check if a storedValue exists
            if (storedValueRuntime == null)
            {
                Debug.Log("BehaviourTree : Tried to GetField but no storedValue was assigned. Node fails.");
                thisNode.m_NodeState = NodeState.Failure;
                thisNode.hasEvaluated = true;
                return thisNode.m_NodeState;
            }

            var component = eAI;
            // Check if component exists
            if (component == null)
            {
                Debug.Log("BehaviourTree : Tried to GetField but the given Component: \"" + ComponentToGet + "\" was not found. Node fails.");
                thisNode.m_NodeState = NodeState.Failure;
                thisNode.hasEvaluated = true;
                return thisNode.m_NodeState;
            }

            storedValueRuntime.SetValue(fieldInfo.GetValue(component));

            thisNode.m_NodeState = NodeState.Success;
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
            if(eAI.GetNode(isInCombatBehavior, guidStr).m_NodeState != NodeState.Running)
                base.ReEvaluate(eAI);
        }
    }
}