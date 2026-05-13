using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class TargetableTransform : MonoBehaviour, ITargetable
    {
        public string ID;

        string ITargetable.GetExtraData()
        {
            return string.Empty;
        }

        string ITargetable.GetID()
        {
            return ID;
        }

        ITargetableType ITargetable.GetTargetableType()
        {
            return ITargetableType.Transform;
        }
    }
}