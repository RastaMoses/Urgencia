using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree.Data;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CreateNodeMenu("RPGCK_BehaviourTree/Comparison/Math/Float Comparison", order = 1)]
    [System.Serializable]
    public class CFloatComparisonNode : BTNode
    {
        public bool firstUseVariable;
        public BT_Float firstStoredValue;
        public float firstInstantValue;

        public ComparisionOperators operation;

        public bool secondUseVariable;
        public BT_Float secondStoredValue;
        public float secondInstantValue;


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

            // Resolve first value
            float value1 = 0f;
            if (firstUseVariable)
            {
                if (firstStoredValue != null)
                {
                    BTVariable var1 = BTReference.SolveReference(this.graph as RPGCK_BT, firstStoredValue.name, eAI);
                    if (var1 != null && var1 is BT_Float floatVar1)
                    {
                        value1 = floatVar1.value;
                    }
                    else
                    {
                        Debug.LogError($"[FloatComparison] Could not resolve First Variable '{firstStoredValue.name}' on {eAI.name}");
                        thisNode.m_NodeState = NodeState.Failure;
                        return NodeState.Failure;
                    }
                }
                else
                {
                    Debug.LogError("[FloatComparison] First Variable is null in Graph.");
                    thisNode.m_NodeState = NodeState.Failure;
                    return NodeState.Failure;
                }
            }
            else
            {
                value1 = firstInstantValue;
            }

            // Resolve second value
            float value2 = 0f;

            if (secondUseVariable)
            {
                if (secondStoredValue != null)
                {
                    BTVariable var2 = BTReference.SolveReference(this.graph as RPGCK_BT, secondStoredValue.name, eAI);
                    if (var2 != null && var2 is BT_Float floatVar2)
                    {
                        value2 = floatVar2.value;
                    }
                    else
                    {
                        Debug.LogError($"[FloatComparison] Could not resolve Second Variable '{secondStoredValue.name}' on {eAI.name}");
                        thisNode.m_NodeState = NodeState.Failure;
                        return NodeState.Failure;
                    }
                }
                else
                {
                    Debug.LogError("[FloatComparison] Second Variable is null in Graph.");
                    thisNode.m_NodeState = NodeState.Failure;
                    return NodeState.Failure;
                }
            }
            else
            {
                value2 = secondInstantValue;
            }

            // Perform comparison
            switch (operation)
            {
                case ComparisionOperators.Equal:
                    thisNode.m_NodeState = (value1 == value2) ? NodeState.Success : NodeState.Failure;
                    break;

                case ComparisionOperators.GreaterOrEqualThan:
                    thisNode.m_NodeState = (value1 >= value2) ? NodeState.Success : NodeState.Failure;
                    break;

                case ComparisionOperators.GreaterThan:
                    thisNode.m_NodeState = (value1 > value2) ? NodeState.Success : NodeState.Failure;
                    break;

                case ComparisionOperators.LessOrEqualThan:
                    thisNode.m_NodeState = (value1 <= value2) ? NodeState.Success : NodeState.Failure;
                    break;

                case ComparisionOperators.LessThan:
                    thisNode.m_NodeState = (value1 < value2) ? NodeState.Success : NodeState.Failure;
                    break;

                case ComparisionOperators.NotEqual:
                    thisNode.m_NodeState = (value1 != value2) ? NodeState.Success : NodeState.Failure;
                    break;

                default:
                    thisNode.m_NodeState = NodeState.Failure;
                    break;
            }

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