using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CreateNodeMenu("RPGCK_BehaviourTree/Wait", order = 1)]
    [System.Serializable]
    public class WaitNode : BTNode
    {
        public string did = null;

        public bool randomizeWaitTime = false;

        public float waitTime = 0.0f;

        public float minWait = 0.0f;
        public float maxWait = 1.0f;

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

            if(did == "debug")
            {
                //Debug.Log(this.GetInstanceID() + " -----" + (startTime + waitTime).ToString() + " | " + Time.time);
            }

            if (thisNode.startTime + thisNode.waitTime < Time.time)
            {
                thisNode.m_NodeState = NodeState.Success;
                thisNode.STARTED = false;
                thisNode.hasEvaluated = true;
                return thisNode.m_NodeState;
            }

            thisNode.m_NodeState = NodeState.Running;
            return thisNode.m_NodeState;
        }

        public override void ReEvaluate(RckAI eAI)
        {
            if (eAI.GetNode(isInCombatBehavior, guidStr).m_NodeState != NodeState.Running)
                base.ReEvaluate(eAI);
        }


        public override void OnRemoveConnection(NodePort port)
        {
            base.OnRemoveConnection(port);
            indexInSequence = -1;
        }

        public override void OnStart(RckAI eAI)
        {
            var thisNode = eAI.GetNode(isInCombatBehavior, guidStr);

            // Remember the start time.
            thisNode.startTime = Time.time;

            if (randomizeWaitTime)
                thisNode.waitTime = Random.Range(minWait, maxWait);
            else
                thisNode.waitTime = waitTime;

            thisNode.STARTED = true;
        }

    }
}