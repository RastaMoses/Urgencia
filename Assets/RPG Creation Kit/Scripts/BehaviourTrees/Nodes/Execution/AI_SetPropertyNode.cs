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
    [CreateNodeMenu("RPGCK_BehaviourTree/Actions/AI/Set Property", order = 1)]
    [System.Serializable]
    public class AI_SetPropertyNode : BTNode
    {
        public string ComponentToSet = "RckAI";
        public string PropertyToSet = "";
        PropertyInfo propertyInfo;

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
            propertyInfo = eAI.GetType().GetProperty(PropertyToSet);

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

            object valueToSet = null;

            if (useVariable)
            {
                if (storedValue != null)
                {
                    BTVariable localVariable = BTReference.SolveReference(this.graph as RPGCK_BT, storedValue.name, eAI);

                    if (localVariable != null)
                    {
                        valueToSet = localVariable.GetValue();
                    }
                    else
                    {
                        Debug.LogError($"[SetProperty] Could not resolve Runtime Variable '{storedValue.name}' on {eAI.name}");
                        thisNode.m_NodeState = NodeState.Failure;
                        return NodeState.Failure;
                    }
                }
                else
                {
                    Debug.LogError("[SetProperty] Variable reference is null in Graph.");
                    thisNode.m_NodeState = NodeState.Failure;
                    return NodeState.Failure;
                }
            }
            else
            {
                // FIX: Get Instant Value directly (No 'CreateInstance' garbage)
                switch (instantValue.parameterType)
                {
                    case BTParameterType.INT:
                        valueToSet = instantValue.intValue;
                        break;
                    case BTParameterType.FLOAT:
                        valueToSet = instantValue.floatValue;
                        break;
                    case BTParameterType.BOOL:
                        valueToSet = instantValue.boolValue;
                        break;
                    case BTParameterType.STRING:
                        valueToSet = instantValue.stringValue;
                        break;
                }
            }

            // Apply value
            try
            {
                propertyInfo.SetValue(eAI, valueToSet);
                thisNode.m_NodeState = NodeState.Success;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AI_SetPropertyNode] Error setting property '{PropertyToSet}': {e.Message}");
                thisNode.m_NodeState = NodeState.Failure;
            }

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