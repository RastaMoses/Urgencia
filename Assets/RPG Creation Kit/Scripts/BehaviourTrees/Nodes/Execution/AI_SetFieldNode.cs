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
    [CreateNodeMenu("RPGCK_BehaviourTree/Actions/AI/Set Field", order = 1)]
    [System.Serializable]
    public class AI_SetFieldNode : BTNode
    {
        public string ComponentToSet = "RckAI";
        public string FieldToSet = "";
        FieldInfo fieldInfo;

        public bool useVariable = true;
        public BTVariable storedValue;

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

        public override void OnStart(RckAI eAI)
        {
            fieldInfo = eAI.GetType().GetField(FieldToSet);
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

            BTVariable storedValueRuntime = null;

            if (useVariable)
                storedValueRuntime = BTReference.SolveReference(this.graph as RPGCK_BT, storedValue.name, eAI);
            else
                storedValueRuntime = NodesHelper.SetStoredValueWithInstantValue(instantValue);

            // Check if a storedValue exists
            if (useVariable && storedValueRuntime == null)
            {
                Debug.Log("BehaviourTree '"+ eAI.currentBehaviour.name+ "' + Node: '" + this.name + " | " + this.guidStr + "' : Tried to GetField but no storedValue was assigned. Node fails.");
                thisNode.m_NodeState = NodeState.Failure;
                thisNode.hasEvaluated = true;
                return thisNode.m_NodeState;
            }

            var component = eAI;
            // Check if component exists
            if (component == null)
            {
                Debug.Log("BehaviourTree : Tried to GetField but the given Component: \"RckAI\" was not found. Node fails.");
                thisNode.m_NodeState = NodeState.Failure;
                thisNode.hasEvaluated = true;
                return thisNode.m_NodeState;
            }

            fieldInfo.SetValue(component, storedValueRuntime.GetValue());

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
            if (eAI.GetNode(isInCombatBehavior, guidStr).m_NodeState != NodeState.Running)
                base.ReEvaluate(eAI);
        }
    }
}