using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree.Data;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CreateNodeMenu("RPGCK_BehaviourTree/Comparison/Math/Int Comparison", order = 1)]
    [System.Serializable]
    public class CIntComparisonNode : BTNode
    {
        public bool firstUseVariable;
        public BT_Int firstStoredValue;
        public int firstInstantValue;

        public ComparisionOperators operation;

        public bool secondUseVariable;
        public BT_Int secondStoredValue;
        public int secondInstantValue;


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

            // Resolve first
            int value1 = 0;

            if (firstUseVariable)
            {
                if (firstStoredValue != null)
                {
                    BTVariable var1 = BTReference.SolveReference(this.graph as RPGCK_BT, firstStoredValue.name, eAI);
                    if (var1 != null && var1 is BT_Int intVar1)
                    {
                        value1 = intVar1.value;
                    }
                    else
                    {
                        Debug.LogError($"[IntComparison] Could not resolve First Variable '{firstStoredValue.name}' on {eAI.name}");
                        thisNode.m_NodeState = NodeState.Failure;
                        return NodeState.Failure;
                    }
                }
                else
                {
                    Debug.LogError("[IntComparison] First Variable is null in Graph.");
                    thisNode.m_NodeState = NodeState.Failure;
                    return NodeState.Failure;
                }
            }
            else
            {
                value1 = firstInstantValue;
            }

            // Resolve second
            int value2 = 0;

            if (secondUseVariable)
            {
                if (secondStoredValue != null)
                {
                    BTVariable var2 = BTReference.SolveReference(this.graph as RPGCK_BT, secondStoredValue.name, eAI);
                    if (var2 != null && var2 is BT_Int intVar2)
                    {
                        value2 = intVar2.value;
                    }
                    else
                    {
                        Debug.LogError($"[IntComparison] Could not resolve Second Variable '{secondStoredValue.name}' on {eAI.name}");
                        thisNode.m_NodeState = NodeState.Failure;
                        return NodeState.Failure;
                    }
                }
                else
                {
                    Debug.LogError("[IntComparison] Second Variable is null in Graph.");
                    thisNode.m_NodeState = NodeState.Failure;
                    return NodeState.Failure;
                }
            }
            else
            {
                value2 = secondInstantValue;
            }

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