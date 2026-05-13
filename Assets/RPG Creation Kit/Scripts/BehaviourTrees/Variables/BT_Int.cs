using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.BehaviourTree;

namespace RPGCreationKit.BehaviourTree.Data
{
    [System.Serializable]
    public class BT_Int : BTVariable
    {
        [SerializeField] public int value;

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object _obj)
        {
            value = (int)_obj;
        }

        public override void SetDefaultValue()
        {
            base.SetDefaultValue();
            value = 0;
        }
    }
}