using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.BehaviourTree;
using System;

namespace RPGCreationKit.BehaviourTree.Data
{
    public enum BTParameterType
    {
        INT = 0,
        FLOAT = 1,
        BOOL = 2,
        STRING = 3,
        DOUBLE_VALUE = 4,
        LONG_VALUE = 5,
        QUATERNION_VALUE = 6,
        VECTOR2_VALUE = 7,
        VECTOR3_VALUE = 8,
        VECTOR4_VALUE = 9
    };

    [System.Serializable]
    public struct BTParameter
    {
        public BTParameterType parameterType;

        // --- Base Parameters --- \\
        public int intValue;
        public float floatValue;
        public bool boolValue;
        public string stringValue;
        public double doubleValue;
        public long longValue;
        public Quaternion quaternionValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Vector4 vector4Value;
    }

    [System.Serializable]
    public class BTVariable : ScriptableObject
    {

        public static List<System.Type> SUPPORTED_TYPES = new List<System.Type>
        {
            typeof(int),
            typeof(float),
            typeof(bool)
        };


        [SerializeField] public string Name;
        [SerializeField] public string Tooltip;
        public string guidStr = Guid.NewGuid().ToString();

        public virtual object GetValue() { return null; }
        public virtual void SetValue(object _obj) { }
        public virtual void OnValueChanges() { }
        public virtual void SetDefaultValue() { }
    }
}