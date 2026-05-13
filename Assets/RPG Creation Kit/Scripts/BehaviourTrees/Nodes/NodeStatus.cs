using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit.BehaviourTree;

namespace RPGCreationKit.BehaviourTree
{
    [System.Serializable]
    public enum NodeState
    {
        Null = 0,
        Running = 1,
        Success = 2,
        Failure = 3
    };

    [System.Serializable]
    public class NodeStatus
    {
        protected NodeState state;

        public NodeState State
        {
            get { return state; }
        }
    }
}