using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using System.Reflection;
using System.Linq;
using UnityEditor;
using RPGCreationKit.BehaviourTree.Data;

public static class NodesHelper 
{
    /// <summary>
    /// Used to specify the parameters of an AIInvoke (both Dialogue and BTree) and actually call the method.
    /// </summary>
    /// <param name="methodToCall"></param>
    /// <param name="thisAI"></param>
    /// <param name="parameters"></param>
    public static void AIInvokeCall(string methodToCall, RckAI thisAI, ConditionParameter[] parameters)
    {
        switch (methodToCall)
        {
            // Methods without parameters are invoked by default as they're englobed in the default 
            case "GetDistanceFromPC":
                RCKFunctions.AI_GetDistanceFromPC(parameters[0].stringValue);
                break;

            case "SetFollowMainTarget":
                thisAI.SetFollowMainTarget(parameters[0].boolValue);
                break;

            case "SetLookAtTarget":
                thisAI.SetLookAtTarget(parameters[0].boolValue);
                break;

            case "GetRandomTargetVectorFromNavmesh":
                thisAI.GetRandomTargetVectorFromNavmesh(parameters[0].floatValue);
                break;

            case "TargetGetRandomTargetVectorFromNavmesh":
                thisAI.TargetGetRandomTargetVectorFromNavmesh(parameters[0].floatValue);
                break;

            case "TargetGetOnlyReachableRandomTargetVectorFromNavmesh":
                thisAI.TargetGetOnlyReachableRandomTargetVectorFromNavmesh(parameters[0].floatValue);
                break;

            case "GetRandomTargetVectorFromNavmeshWithCenter":
                thisAI.GetRandomTargetVectorFromNavmeshWithCenter(parameters[0].floatValue, parameters[1].vector3Value);
                break;

            case "SetOverrideShouldFollowMainTarget":
                thisAI.SetOverrideShouldFollowMainTarget(parameters[0].boolValue, parameters[1].boolValue);
                break;

            case "SetTargetVector":
                thisAI.SetTargetVector(parameters[0].vector3Value);
                break;


            case "SetBlocking":
                thisAI.SetBlocking(parameters[0].boolValue);
                break;

            case "SendAIToSpeakToPlayer":
                thisAI.SendAIToSpeakToPlayer(parameters[0].boolValue);
                break;

            case "AssignPurpose":

                PurposeStateClearsOnData purposeData = null;

                switch ((PurposeClearTypes)parameters[2].intValue)
                {
                    case PurposeClearTypes.ClearOnFinishTalkingWith:
                        purposeData = new PurposeStateClearsOnData(parameters[3].stringValue);
                        break;
                }

                if(thisAI.mainTarget != null)
                    thisAI.AssignPurpose(thisAI, thisAI.mainTarget.gameObject, (PurposeClearTypes)parameters[2].intValue, purposeData, parameters[4].stringValue);
                break;

            case "SpeakToAI":
                thisAI.SpeakToAI(parameters[0].stringValue);
                break;

            case "SetRckAIAsTarget":
                thisAI.SetRckAIAsTarget(parameters[0].stringValue);
                break;


            case "SetSpeakerIndex":
                thisAI.SetSpeakerIndex(parameters[0].intValue);
                break;

            case "SetSpeakerIndexToAI":
                thisAI.SetSpeakerIndexToAI(parameters[0].stringValue, parameters[1].intValue);
                break;

            case "SetNewBehaviourTree":
                thisAI.SetNewBehaviourTree(parameters[0].boolValue, parameters[1].stringValue);
                break;

            case "SetNewBehaviourTreeToAI":
                thisAI.SetNewBehaviourTreeToAI(parameters[0].stringValue, parameters[1].boolValue, parameters[2].stringValue);
                break;

            case "SwitchBehaviourTree":
                thisAI.SwitchBehaviourTree(parameters[0].boolValue);
                break;

            case "SwitchBehaviourTreeToAI":
                thisAI.SwitchBehaviourTreeToAI(parameters[0].stringValue, parameters[1].boolValue);
                break;

            case "SetLookAtVector3IfStopped":
                thisAI.SetLookAtVector3IfStopped(parameters[0].vector3Value);
                break;

            case "StopUsingNPCActionPoint":
                if(parameters.Length>0)
                    thisAI.StopUsingNPCActionPoint(parameters[0].boolValue);
                break;

            case "SetAIPathFromCell":
                    thisAI.SetAIPathFromCell(parameters[0].stringValue);
                break;

            case "UseArrayActionPoint":
                thisAI.UseArrayActionPoint(parameters[0].stringArrayValue);
                break;

            case "GetDistanceFromPlayerBT":
                thisAI.GetDistanceFromPlayerBT(parameters[0].btVariableValue);
                break;

            case "Magic_SetSpell":
                thisAI.Magic_SetSpell(parameters[0].stringValue);
                break;

            case "Magic_AddSpellToKnowledge":
                thisAI.Magic_AddSpellToKnowledge(parameters[0].stringValue);
                break;


            // To avoid reflection, specify functions even without parameters here
            case "KeepFiringAtTarget":
                thisAI.KeepFiringAtTarget();
                break;

            case "LeaveCombat":
                thisAI.LeaveCombat();
                break;

            case "HasRangedWeaponCheck":
                thisAI.HasRangedWeaponCheck();
                break;

            case "HasMeleeWeaponCheck":
                thisAI.HasMeleeWeaponCheck();
                break;

            case "GetDistanceFromMainTarget":
                thisAI.GetDistanceFromMainTarget();
                break;

            case "Weapon_SwitchToMelee":
                thisAI.Weapon_SwitchToMelee();
                break;

            case "Weapon_SwitchToRanged":
                thisAI.Weapon_SwitchToRanged();
                break;

            case "Weapon_SwitchToDefault":
                thisAI.Weapon_SwitchToDefault();
                break;

            case "GetWeaponsReach":
                thisAI.GetWeaponsReach();
                break;

            case "SpeakToTarget":
                thisAI.SpeakToTarget();
                break;

            case "Purpose_AssignToFollowCurrentPath":
                thisAI.Purpose_AssignToFollowCurrentPath();
                break;

            case "Magic_SwitchToHealingSpell":
                thisAI.Magic_SwitchToHealingSpell();
                break;

            case "Magic_SwitchToDamageSpell":
                thisAI.Magic_SwitchToDamageSpell();
                break;

            case "MeleeAttack":
                thisAI.MeleeAttack();
                break;

            case "CastSpell":
                thisAI.CastSpell();
                break;

            case "ThrowGrenade":
                thisAI.ThrowGrenade();
                break;

            case "CheckIfHasToCover":
                thisAI.CheckIfHasToCover();
                break;

            case "RangedLoadAndAim":
                thisAI.RangedLoadAndAim();
                break;

            case "Get2DTargetDistance":
                thisAI.Get2DTargetDistance();
                break;

            case "UpdateWhichWeaponIsUsing":
                thisAI.UpdateWhichWeaponIsUsing();
                break;

            case "CheckNearbyExplosive":
                thisAI.CheckNearbyExplosive();
                break;

            case "UpdateNavMeshPathStatus":
                thisAI.UpdateNavMeshPathStatus();
                break;

            case "FollowTargetThroughNavmeshPath":
                thisAI.FollowTargetThroughNavmeshPath();
                break;

            case "LookAtTarget":
                thisAI.LookAtTarget();
                break;

            case "FollowNavmeshPath":
                thisAI.FollowNavmeshPath();
                break;

            case "FollowTargetVector":
                thisAI.FollowTargetVector();
                break;

            case "ClearLookAtVector3IfStopped":
                thisAI.ClearLookAtVector3IfStopped();
                break;

            case "IsOnTargetVector":
                thisAI.IsOnTargetVector();
                break;

            case "SnapshotTargetPosition":
                thisAI.SnapshotTargetPosition();
                break;

            case "FollowCurrentPath":
                thisAI.FollowCurrentPath();
                break;

            case "GetAndUseActionPoint":
                thisAI.GetAndUseActionPoint();
                break;

            case "UseCurrentActionPoint":
                thisAI.UseCurrentActionPoint();
                break;

            case "StopUsingNPCActionPointImmediate":
                thisAI.StopUsingNPCActionPointImmediate();
                break;

            case "Movements_SwitchToRun":
                thisAI.Movements_SwitchToRun();
                break;

            case "Movements_SwitchToWalk":
                thisAI.Movements_SwitchToWalk();
                break;

            case "ReachCurrentCover":
                thisAI.ReachCurrentCover();
                break;

            case "LeaveCurrentCover":
                thisAI.LeaveCurrentCover();
                break;

            case "DisableAILookAt":
                thisAI.DisableAILookAt();
                break;

            case "EnableAILookAt":
                thisAI.EnableAILookAt();
                break;

            case "DrawWeapon":
                thisAI.DrawWeapon();
                break;

            default:
                var maybeMethod = thisAI.GetType().GetMethod(methodToCall);
                if (maybeMethod is MethodInfo method)
                {
                    try
                    {
                        Debug.LogWarning("Calling '" + method.Name + "' as an execution node via Reflection. This is not advised, you should enclose the method call in a switch case.");
                        method.Invoke(thisAI, null);
                    }
                    catch (TargetParameterCountException)
                    {
                        Debug.LogError("Method: '" + method.Name + "' could not be called probably because you did not set the paramters inside NodesHelper.cs and the Invoke is being done without paramters.");
                    }
                }

                break;
        }
    }


#if UNITY_EDITOR
    /// <summary>
    /// Used to draw the paramteres of the AI Invokable methods for both Dialogue and BTree
    /// </summary>
    /// <param name="method"></param>
    /// <param name="element"></param>
    /// <param name="i"></param>
    public static void AIInvokeCallEditorDrawParamter(MethodInfo method, SerializedProperty element, int i, SerializedProperty previousElement)
    {
        if (method.GetParameters().ElementAt(i).ParameterType == typeof(string))
        {
            EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (String)");
            element.FindPropertyRelative("stringValue").stringValue = EditorGUILayout.TextArea(element.FindPropertyRelative("stringValue").stringValue);
            return;
        }
        else if (method.GetParameters().ElementAt(i).ParameterType == typeof(string[]))
        {
            EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (String[])");
            EditorGUILayout.PropertyField(element.FindPropertyRelative("stringArrayValue"));
            return;
        }
        else if (method.GetParameters().ElementAt(i).ParameterType == typeof(int))
        {
            EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (Int)");
            element.FindPropertyRelative("intValue").intValue = EditorGUILayout.IntField(element.FindPropertyRelative("intValue").intValue);
            return;
        }
        else if (method.GetParameters().ElementAt(i).ParameterType == typeof(float))
        {
            EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (Float)");
            element.FindPropertyRelative("floatValue").floatValue = EditorGUILayout.FloatField(element.FindPropertyRelative("floatValue").floatValue);
            return;
        }
        else if (method.GetParameters().ElementAt(i).ParameterType == typeof(bool))
        {
            EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (Bool)");
            element.FindPropertyRelative("boolValue").boolValue = EditorGUILayout.Toggle(element.FindPropertyRelative("boolValue").boolValue);
            return;
        }
        else if (method.GetParameters().ElementAt(i).ParameterType == typeof(Vector3))
        {
            EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (Vector3)");
            element.FindPropertyRelative("vector3Value").vector3Value = EditorGUILayout.Vector3Field("Vector3:", element.FindPropertyRelative("vector3Value").vector3Value);
            return;
        }
        else if (method.GetParameters().ElementAt(i).ParameterType == typeof(Quest))
        {
            EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (Quest)");
            element.FindPropertyRelative("questValue").objectReferenceValue = EditorGUILayout.ObjectField((Quest)element.FindPropertyRelative("questValue").objectReferenceValue, typeof(Quest), true) as Quest;
            return;
        }
        else if (method.GetParameters().ElementAt(i).ParameterType == typeof(RPGCreationKit.BehaviourTree.Data.BTVariable))
        {
            EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (BTVariable)");
            element.FindPropertyRelative("btVariableValue").objectReferenceValue = EditorGUILayout.ObjectField((BTVariable)element.FindPropertyRelative("btVariableValue").objectReferenceValue, typeof(BTVariable), true) as BTVariable;
            return;
        }
        else if (method.GetParameters().ElementAt(i).ParameterType == typeof(PurposeClearTypes))
        {
            EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (PurposeClearTypes)");
            element.FindPropertyRelative("intValue").intValue = (int)(PurposeClearTypes)EditorGUILayout.EnumPopup((PurposeClearTypes)System.Enum.GetValues(typeof(PurposeClearTypes)).GetValue((element.FindPropertyRelative("intValue").intValue)));
        }
        else if (method.GetParameters().ElementAt(i).ParameterType == typeof(PurposeStateClearsOnData))
        {
            EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (Extra Data)");
            var clearType = (PurposeClearTypes)System.Enum.GetValues(typeof(PurposeClearTypes)).GetValue((previousElement.FindPropertyRelative("intValue").intValue));

            switch(clearType)
            {
                case PurposeClearTypes.ClearOnFinishTalkingWith:
                    element.FindPropertyRelative("stringValue").stringValue = EditorGUILayout.TextArea(element.FindPropertyRelative("stringValue").stringValue);
                break;
            }
        }
        else
        {
            EditorGUILayout.LabelField("NON DRAWABLE PARAMETER (" + method.GetParameters().ElementAt(i).ParameterType + ")");
            EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).ParameterType.Name, EditorStyles.boldLabel);
            return;
        }
    }


    public static MonoScript[] GetAllResultScripts<T>()
    {
        MonoScript[] scripts = (MonoScript[])Object.FindObjectsOfTypeIncludingAssets(typeof(MonoScript));

        List<MonoScript> result = new List<MonoScript>();

        foreach (MonoScript m in scripts)
        {
            if (m.GetClass() != null && m.GetClass().IsSubclassOf(typeof(T)) && m.GetType() != typeof(Shader))
            {
                result.Add(m);
            }
        }
        var arr = result.ToArray();
        IComparer myComparer = new MonoScriptComparer();
        System.Array.Sort(arr, myComparer);
        return arr;
    }

    public static MonoScript[] GetAllItemsScripts<T>()
    {
        MonoScript[] scripts = (MonoScript[])Object.FindObjectsOfTypeIncludingAssets(typeof(MonoScript));

        List<MonoScript> result = new List<MonoScript>();

        foreach (MonoScript m in scripts)
        {
            if (m.GetClass() != null && m.GetClass().IsSubclassOf(typeof(T)) && m.GetType() != typeof(Shader))
            {
                result.Add(m);
            }
        }
        var arr = result.ToArray();
        IComparer myComparer = new MonoScriptComparer();
        System.Array.Sort(arr, myComparer);
        return arr;
    }

#endif

    public static BTVariable SetStoredValueWithInstantValue(BTParameter instantValue)
    {
        switch (instantValue.parameterType)
        {
            case BTParameterType.INT:
                // Create a storedValue from instantreference
                BT_Int newbtInt = ScriptableObject.CreateInstance<BT_Int>();
                newbtInt.Name = "TEMP_BT_INT";
                newbtInt.value = instantValue.intValue;

                return newbtInt;

            case BTParameterType.FLOAT:
                // Create a storedValue from instantreference
                BT_Float newBTFloat = ScriptableObject.CreateInstance<BT_Float>();
                newBTFloat.Name = "TEMP_BT_FLOAT";
                newBTFloat.value = instantValue.floatValue;

                return newBTFloat;

            case BTParameterType.BOOL:
                // Create a storedValue from instantreference
                BT_Bool newBTBool = ScriptableObject.CreateInstance<BT_Bool>();
                newBTBool.Name = "TEMP_BT_BOOL";
                newBTBool.value = instantValue.boolValue;

               return newBTBool;
        }

        return null;
    }

}

#if UNITY_EDITOR
public class MonoScriptComparer : IComparer
{

    // Calls CaseInsensitiveComparer.Compare on the monster name string.
    int IComparer.Compare(System.Object x, System.Object y)
    {
        return ((new CaseInsensitiveComparer()).Compare(((MonoScript)x).name, ((MonoScript)y).name));
    }

}
#endif