using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.AI
{
    public enum PurposeClearTypes
    {
        ClearOnTeleportToCell,
        ClearOnKillsEntity,
        ClearOnDeath,
        ClearOnReachTarget,
        ClearOnEnterCombat,
        ClearOnLeaveCombat,
        ClearOnFinishTalkingWith,
        Undefined
    }

    /// <summary>
    /// Contains extra information about the OnClear functionalities
    /// </summary>
    [System.Serializable]
    public class PurposeStateClearsOnData
    {
        public string stringData;
        public int intData;
        public float floatData;
        public PurposeStateClearsOnData(string stringData)
        {
            this.stringData = stringData;
        }

        public PurposeStateClearsOnData(int intData)
        {
            this.intData = intData;
        }

        public PurposeStateClearsOnData(float floatData)
        {
            this.floatData = floatData;
        }
    }


    /// <summary>
    /// The PurposeState represents a persistent representation of the current goal of the AI.
    /// If the AI has to go to some place, he could meet an enemy on the way that will override his MainTarget.
    /// 
    /// To avoid that, everytime the AI enters in combat or switches BehaviourTree, a new instance of this class is created and the state is preserved.
    /// Whenever the AI leaves combat or returns to the Purpose Behaviour, if a PurposeState exists the MainTarget and other fields will be assigned.
    /// </summary>
    [System.Serializable]
    public class PurposeState
    {
        public RckAI ai;

        public bool isAssigned;
        public GameObject pTarget;

        public string ITargetableID;
        public int ITargetableType;
        public string ITargetableExtraData;

        public PurposeClearTypes clearsOn;
        public PurposeStateClearsOnData clearsOnData;

        // if this is set, after this purpose gets cleared with CompletePurpose this behaviour tree will be executed
        public string nextPurposeBehaviourID;

        public bool IsAssigned
        {
            get { return isAssigned; }
        }

        public GameObject Target
        {
            get { return pTarget; }
        }

        public void AssignPurpose(RckAI _AI, GameObject _target, PurposeClearTypes _clearsOn, PurposeStateClearsOnData _clearOnData, string _nextPurposeBehaviourTree = null)
        {
            if (_target == null || _AI == null)
                return;

            ai = _AI;

            isAssigned = true;
            pTarget = _target;

            ITargetable targetable = pTarget.GetComponent<ITargetable>();
            if (targetable != null)
            {
                ITargetableID = targetable.GetID();
                ITargetableType = (int)targetable.GetTargetableType();
                ITargetableExtraData = targetable.GetExtraData();
            }
            else
                ITargetableID = string.Empty;

            clearsOn = _clearsOn;
            clearsOnData = _clearOnData;

            if (!string.IsNullOrEmpty(_nextPurposeBehaviourTree))
                nextPurposeBehaviourID = _nextPurposeBehaviourTree;
        }

        public void ResumePurpose()
        {
            switch(ITargetableType)
            {
                case 4:
                    if(pTarget != null && ai != null)
                    {
                        ai.aiPath = pTarget.GetComponent<AICustomPath>();
                        ai.FollowCurrentPath();
                    }
                    break;
            }
        }

        public void ClearPurpose()
        {
            isAssigned = false;
            pTarget = null;
            ITargetableID = string.Empty;
            ITargetableExtraData = string.Empty;
            ITargetableType = 0;
            clearsOnData = null;
        }

        public void CompletePurpose()
        {
            ClearPurpose();

            if (!string.IsNullOrEmpty(nextPurposeBehaviourID))
            {
                ai.SetNewBehaviourTree(false, nextPurposeBehaviourID);
                ai.SwitchBehaviourTree(false);
            }
        }

    }
}