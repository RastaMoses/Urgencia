using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.AI;

[CreateNodeMenu("RPGCK_BehaviourTree/RootNode", order = 0)]
[System.Serializable]
public class RootNode : BTNode
{
    string state;
    public bool isNpcToNpcDialogue; // Is this a dialogue that doesn't involve the player?

    [Output(ShowBackingValue.Never)] public Node onExitNode;

    [Output] public BTNode firstNode;

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

        // Its state is the state of the child
        firstNode = ((BTNode)GetOutputPort("firstNode").Connection.node);

        switch(thisNode.m_NodeState)
        {
            case NodeState.Failure:
                thisNode.m_NodeState = NodeState.Failure;
                return thisNode.m_NodeState;

            case NodeState.Running:
            case NodeState.Null:
                // Continue ticking
                firstNode.Execute(eAI);
                thisNode.m_NodeState = eAI.GetNode(isInCombatBehavior, firstNode.guidStr).m_NodeState;
                return thisNode.m_NodeState;

            case NodeState.Success:
                thisNode.m_NodeState = NodeState.Success;
                return thisNode.m_NodeState;

            default:
                break;
        }

        return thisNode.m_NodeState;
    }

    public override void OnCreateConnection(NodePort from, NodePort to)
    {
        base.OnCreateConnection(from, to);
    }

    public override void OnRemoveConnection(NodePort port)
    {
        base.OnRemoveConnection(port);
    }

    public override void OnStart(RckAI eAI)
    {
        
    }

    public override void ReorderChild()
    {
        ((BTNode)GetOutputPort("firstNode").node).indexInSequence = 0;
    }
}
