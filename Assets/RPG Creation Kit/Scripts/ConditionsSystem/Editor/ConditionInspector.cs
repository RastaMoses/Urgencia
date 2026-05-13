using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Reflection;
using System;
using RPGCreationKit;

[CustomEditor(typeof(RCKFunctions))]
public class ConditionInspector : Editor
{
    /*
    static string[] methods;
    static string[] ignoreMethods = new string[] { "Start", "Update" };

    static ConditionInspector()
    {
        methods =
            typeof(RCKFunctions)
            .GetMethods()
            .Where(x => x.DeclaringType == typeof(RCKFunctions))
            .Where(x => !ignoreMethods.Any(n => n == x.Name))
            .Select(x => x.Name)
            .ToArray();
    }

    public override void OnInspectorGUI()
    {

        Condition obj = target as Condition;

        if (obj != null)
        {
            int index;

            try
            {
                index = methods
                    .Select((v, i) => new { Name = v, Index = i })
                    .First(x => x.Name == obj.MethodToCall)
                    .Index;
            }
            catch
            {
                index = 0;
            }

            EditorGUILayout.BeginHorizontal("box");

            EditorGUIUtility.labelWidth = 25f;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Condition");
            obj.MethodToCall = methods[EditorGUILayout.Popup(index, methods)];
            EditorGUILayout.EndVertical();

            var selectedMethod = methods[index];
            var method = (typeof(RCKFunctions).GetMethod(selectedMethod));

            for (int i = 0; i < method.GetParameters().Length; i++)
            {
                if (method.GetParameters().ElementAt(i).ParameterType == typeof(string))
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (" + method.GetParameters().ElementAt(i).ParameterType.Name + ")");
                    obj.parameters[i] = EditorGUILayout.TextArea((string)obj.parameters[i]);
                    EditorGUILayout.EndVertical();
                    continue;
                }
                else if(method.GetParameters().ElementAt(i).ParameterType == typeof(int))
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (" + method.GetParameters().ElementAt(i).ParameterType.Name + ")");
                    obj.parameters[i] = EditorGUILayout.IntField(System.Convert.ToInt32(obj.parameters[i]));
                    EditorGUILayout.EndVertical();
                    continue;
                }
                else if (method.GetParameters().ElementAt(i).ParameterType == typeof(float))
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (" + method.GetParameters().ElementAt(i).ParameterType.Name + ")");
                    obj.parameters[i] = EditorGUILayout.FloatField(System.Convert.ToSingle(obj.parameters[i]));
                    EditorGUILayout.EndVertical();
                    continue;
                }
                else if (method.GetParameters().ElementAt(i).ParameterType == typeof(Quest))
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField(method.GetParameters().ElementAt(i).Name + " (" + method.GetParameters().ElementAt(i).ParameterType.Name + ")");
                    obj.parameters[i] = EditorGUILayout.ObjectField((Quest)obj.parameters[i], typeof(Quest)) as Quest;
                    EditorGUILayout.EndVertical();
                    continue;
                }
            }

            obj.absMethod = method;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Operator");
            obj.compOperator = (ComparisionOperators)EditorGUILayout.EnumPopup(obj.compOperator);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Value");
            obj.value = EditorGUILayout.FloatField(obj.value);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("OR");
            obj.OR = EditorGUILayout.Toggle(obj.OR);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
    }
    */

}