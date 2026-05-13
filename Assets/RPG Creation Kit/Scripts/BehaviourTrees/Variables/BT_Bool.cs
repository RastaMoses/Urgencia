using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.BehaviourTree;

namespace RPGCreationKit.BehaviourTree.Data
{
    [System.Serializable]
    public class BT_Bool : BTVariable
    {
        [SerializeField] public bool value;

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object _obj)
        {
            value = (bool)_obj;
        }

        public override void SetDefaultValue()
        {
            base.SetDefaultValue();
            value = false;
        }
    }
}