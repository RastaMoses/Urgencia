using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using System.Reflection;
using System;

namespace RPGCreationKit
{
    public enum ComparisionOperators
    {
        Equal = 0,
        LessThan = 1,
        GreaterThan = 2,
        LessOrEqualThan = 3,
        GreaterOrEqualThan = 4,
        NotEqual = 5
    };


    [System.Serializable]
    public class Condition
    {
        public string MethodToCall;
        public ConditionParameter[] parameters = new ConditionParameter[5];

        public ComparisionOperators compOperator;

        public float customValue = 1.0f;

        public bool OR = false;

        public bool conditionValue { get { return VerifyCondition(); } }

        // -----------------------------------------------------------------------------
        // For using global variables in Condition
        // -----------------------------------------------------------------------------
        public bool useGlobal;

        public string propertyName;
        public ConditionParameterType propertyType;
        public int propertyIntValue;
        public float propertyFloatValue;


        private bool VerifyCondition()
        {
            // Take the condition result
            float conditionResult = ExecuteCondition(MethodToCall, parameters);

            // Do the comparision
            bool comparisionResult = false;

            switch(compOperator)
            {
                case ComparisionOperators.Equal:
                    if (useGlobal) // if we're using a global variable
                    {
                        // Select the correct type and compare

                        if(propertyType == ConditionParameterType.INT)
                            comparisionResult = ((int)conditionResult == propertyIntValue);
                        else if(propertyType == ConditionParameterType.FLOAT)
                            comparisionResult = (conditionResult == propertyFloatValue);
                        else
                            comparisionResult = (conditionResult == propertyIntValue); // default
                    }
                    else
                        comparisionResult = (conditionResult == customValue);
                    break;

                case ComparisionOperators.GreaterOrEqualThan:
                    if (useGlobal)
                    {
                        if (propertyType == ConditionParameterType.INT)
                            comparisionResult = (conditionResult >= propertyIntValue);
                        else if (propertyType == ConditionParameterType.FLOAT)
                            comparisionResult = (conditionResult >= propertyFloatValue);
                        else
                            comparisionResult = (conditionResult >= propertyIntValue);
                    }
                    else
                    comparisionResult = (conditionResult >= customValue);
                    break;

                case ComparisionOperators.GreaterThan:
                    if (useGlobal)
                    {
                        if (propertyType == ConditionParameterType.INT)
                            comparisionResult = (conditionResult > propertyIntValue);
                        else if (propertyType == ConditionParameterType.FLOAT)
                            comparisionResult = (conditionResult > propertyFloatValue);
                        else
                            comparisionResult = (conditionResult > propertyIntValue);
                    }
                    else
                        comparisionResult = (conditionResult > customValue);
                    break;

                case ComparisionOperators.LessOrEqualThan:
                    if (useGlobal)
                    {
                        if (propertyType == ConditionParameterType.INT)
                            comparisionResult = (conditionResult <= propertyIntValue);
                        else if (propertyType == ConditionParameterType.FLOAT)
                            comparisionResult = (conditionResult <= propertyFloatValue);
                        else
                            comparisionResult = (conditionResult <= propertyIntValue);
                    }
                    else
                        comparisionResult = (conditionResult <= customValue);
                    break;

                case ComparisionOperators.LessThan:
                    if (useGlobal)
                    {
                        if (propertyType == ConditionParameterType.INT)
                            comparisionResult = (conditionResult < propertyIntValue);
                        else if (propertyType == ConditionParameterType.FLOAT)
                            comparisionResult = (conditionResult < propertyFloatValue);
                        else
                            comparisionResult = (conditionResult < propertyIntValue);
                    }
                    else
                        comparisionResult = (conditionResult < customValue);
                    break;

                case ComparisionOperators.NotEqual:
                    if (useGlobal)
                    {
                        if (propertyType == ConditionParameterType.INT)
                            comparisionResult = (conditionResult != propertyIntValue);
                        else if (propertyType == ConditionParameterType.FLOAT)
                            comparisionResult = (conditionResult != propertyFloatValue);
                        else
                            comparisionResult = (conditionResult != propertyIntValue);
                    }
                    else
                        comparisionResult = (conditionResult != customValue);
                    break;

                default:
                    comparisionResult = false;
                    break;
            }

            // Return the value
            return comparisionResult;
        }

        /// <summary>
        /// Executes a Condition
        /// </summary>
        /// <param name="_methodName">The name of the method</param>
        /// <param name="parameters">Its parameters</param>
        /// <returns></returns>
        public static float ExecuteCondition(string _methodName, ConditionParameter[] parameters)
        {
            int result = 0;

            switch (_methodName)
            {
                case "GetStage":
                    return RCKFunctions.GetStage(parameters[0].stringValue);
                case "IsQuestCompleted":
                    return Convert.ToSingle(RCKFunctions.IsQuestCompleted(parameters[0].stringValue));
                case "GetDistanceFromPC": 
                    return RCKFunctions.AI_GetDistanceFromPC(parameters[0].stringValue);

                case "GetStageCompleted":
                    return RCKFunctions.GetStageCompleted(parameters[0].stringValue, parameters[1].intValue);

                case "GetStageFailed":
                    return RCKFunctions.GetStageFailed(parameters[0].stringValue, parameters[1].intValue);

                case "IsAIAlive":
                    return RCKFunctions.IsAIAlive(parameters[0].stringValue);

                case "GetPlayerEquipped":
                    return RCKFunctions.GetPlayerEquipped(parameters[0].stringValue);

                case "IsAILoaded":
                    return RCKFunctions.IsAILoaded(parameters[0].stringValue);

                case "GetItemCountPlayer":
                    return RCKFunctions.GetItemCountPlayer(parameters[0].stringValue);

                case "GetCurrentTimeOfDay":
                    return RCKFunctions.GetCurrentTimeOfDay();

                case "GetCurrentHourOfDay":
                    return RCKFunctions.GetCurrentHourOfDay();

                case "GetCurrentMinutesOfDay":
                    return RCKFunctions.GetCurrentMinutesOfDay();

                default:
                    Debug.LogWarning("WARNING! Tried to execute a [IsCondition] Method with name " + _methodName + " but this method is not specified in ExecuteCondition(...) inside the script Condition.cs, please specify the parameters.");
                    break;
            }

            return result;
        }
    }


    public enum ConditionParameterType
    {
        STRING = 0,
        INT = 1,
        FLOAT = 2,
        BOOL = 3,
        DOUBLE_VALUE = 4,
        LONG_VALUE = 5,
        QUATERNION_VALUE = 6,
        VECTOR2_VALUE = 7,
        VECTOR3_VALUE = 8,
        VECTOR4_VALUE = 9,
        QUEST_VALUE = 10,
        QUESTSTAGE_VALUE = 11,
    };

    [System.Serializable]
    public struct ConditionParameter
    {
        public ConditionParameterType conditionType;

        // --- Base Parameters --- \\
        public string stringValue;
        public string[] stringArrayValue;
        public int intValue;
        public float floatValue;
        public bool boolValue;
        public double doubleValue;
        public long longValue;
        public Quaternion quaternionValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Vector4 vector4Value;
        public BehaviourTree.Data.BTVariable btVariableValue;
    }
}