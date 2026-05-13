using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit;
using UnityEditor;
using RPGCreationKit.AI;
using System;
using System.Linq;

namespace RPGCreationKit.BehaviourTree
{
    [System.Serializable]
    public abstract class BTNode : Node
    {
        [Input(backingValue = ShowBackingValue.Never)] public BTNode input;

        // used to debug the nodes on running trees
        public NodeState m_NodeDebugState;

        public bool isInCombatBehavior;

        public int indexInSequence = -1;

        public string resultScript;

        public Events events;

        abstract public NodeState Execute(RckAI eAI); // executingAI

        abstract public void OnStart(RckAI eAI);

        [System.Serializable]
        public class RuntimeBTNodeData
        {
            // Header
            public bool isCombatBehavior;

            // Base data
            public bool STARTED;
            public NodeState m_NodeState;
            public bool hasEvaluated;

            // Extra node data
            public int outputToExecute;
            public BTNode nodeExecuting;
            public float startTime;
            public float waitTime;
            public bool solvedRef;

            public RuntimeBTNodeData(bool isCombatBehavior, bool STARTED, NodeState nodeStatus, bool hasEvaluated)
            {
                this.isCombatBehavior = isCombatBehavior;
                this.STARTED = STARTED;
                this.m_NodeState = nodeStatus;
                this.hasEvaluated = hasEvaluated;
                outputToExecute = 0;
                nodeExecuting = null;
                startTime = 0;
                waitTime = 0;
                solvedRef = false;
            }

            public void Clense()
            {
                this.STARTED = false;
                this.m_NodeState = NodeState.Null;
                this.hasEvaluated = false;
                outputToExecute = 0;
                nodeExecuting = null;
                startTime = 0;
                waitTime = 0;
                solvedRef = false;
            }
        }

        public void Awake()
        {
            RegenerateGUID();
        }

        [ContextMenu("Regenerate GUID")]
        public void RegenerateGUID()
        {
            guidStr = System.Guid.NewGuid().ToString();
        }

        public virtual void ReEvaluate(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            thisNode.hasEvaluated = false;
            thisNode.m_NodeState = NodeState.Null;
            thisNode.hasEvaluated = false;
        }

        public override object GetValue(NodePort port)
        {
            return null;
        }

        public static int SortInGraphYPos(Node a, Node b)
        {
            return a.position.y.CompareTo(b.position.y);
        }

        public virtual void ReorderChild()
        {

        }
    }
}