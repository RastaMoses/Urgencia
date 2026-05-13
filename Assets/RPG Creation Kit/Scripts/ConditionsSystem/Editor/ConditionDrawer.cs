using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;

namespace RPGCreationKit
{
    /// <summary>
    /// Class for Callback info
    /// </summary>
    public class ConditionGlobalValueData
    {
        public SerializedProperty property;
        public string name;
        public object value;

        // Constructor
        public ConditionGlobalValueData(SerializedProperty _property, string _name, object _value)
        {
            property = _property;
            name = _name;
            value = _value;
        }
    }

    [CustomPropertyDrawer(typeof(Condition))]
    public class ConditionDrawer : PropertyDrawer
    {
        static string[] methods;     // Contains [IsCondition] methods, used to display them in the PropertyDrawer
        static string[] returnType;  // Contains the return type of the methods[i]

        static ConditionDrawer()
        {
            // Fill methods
            methods =
            typeof(RCKFunctions)
            .GetMethods()
            .Where(x => x.DeclaringType == typeof(RCKFunctions))
            .Where(m => m.GetCustomAttributes().OfType<IsConditionAttribute>().Any())
            .Select(x => x.Name)
            .ToArray();

            // Fill return types
            returnType =
            typeof(RCKFunctions)
            .GetMethods()
            .Where(x => x.DeclaringType == typeof(RCKFunctions))
            .Where(m => m.GetCustomAttributes().OfType<IsConditionAttribute>().Any())
            .Select(x => x.ReturnType.Name)
            .ToArray();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Custom variable Height that will expand in base of the data that we have to show
            float customHeight = 40f;

            int index;

            try
            {
                index = methods
                    .Select((v, i) => new { Name = v, Index = i })
                    .First(x => x.Name == property.FindPropertyRelative("MethodToCall").stringValue)
                    .Index;
            }
            catch
            {
                index = 0;
            }

            var LabelForCondition = new Rect(position.x, position.y, 120, position.height - (customHeight));
            var ConditionRect = new Rect(position.x, position.y + 20, 120, position.height - (customHeight));

            EditorGUI.LabelField(LabelForCondition, "Condition", EditorStyles.boldLabel);
            property.FindPropertyRelative("MethodToCall").stringValue = methods[EditorGUI.Popup(ConditionRect, index, methods)];

            var LabelForReturnType = new Rect(position.x, position.y + 40, 120, position.height - (customHeight));

            EditorGUI.LabelField(LabelForReturnType, "Returns: " + returnType[index].ToString(), EditorStyles.boldLabel);

            var selectedMethod = methods[index];
            var method = (typeof(RCKFunctions).GetMethod(selectedMethod));

            // Resize the parameters array in base of the number of parameters of the selected method
            if (property.FindPropertyRelative("parameters").arraySize != method.GetParameters().Length)
                property.FindPropertyRelative("parameters").arraySize = method.GetParameters().Length;

            // Draw parameters
            for (int i = 0; i < method.GetParameters().Length; i++)
            {
                var LabelForParameter = new Rect(position.x + 150 + (150 * i), position.y, 140, position.height - (customHeight));
                var ParameterRect     = new Rect(position.x + 150 + (150 * i), position.y + 20, 140, position.height + 3f - (customHeight));

                var element = property.FindPropertyRelative("parameters").GetArrayElementAtIndex(i);
                

                if (method.GetParameters().ElementAt(i).ParameterType == typeof(string))
                {
                    EditorGUI.LabelField(LabelForParameter, method.GetParameters().ElementAt(i).Name + " (String)", EditorStyles.boldLabel);
                    element.FindPropertyRelative("stringValue").stringValue = EditorGUI.TextArea(ParameterRect, element.FindPropertyRelative("stringValue").stringValue);
                    continue;
                }
                else if (method.GetParameters().ElementAt(i).ParameterType == typeof(int))
                {
                    EditorGUI.LabelField(LabelForParameter, method.GetParameters().ElementAt(i).Name + " (Int)", EditorStyles.boldLabel);
                    element.FindPropertyRelative("intValue").intValue = EditorGUI.IntField(ParameterRect, element.FindPropertyRelative("intValue").intValue);
                    continue;
                }
                else if (method.GetParameters().ElementAt(i).ParameterType == typeof(float))
                {
                    EditorGUI.LabelField(LabelForParameter, method.GetParameters().ElementAt(i).Name + " (Float)", EditorStyles.boldLabel);
                    element.FindPropertyRelative("floatValue").floatValue = EditorGUI.FloatField(ParameterRect, element.FindPropertyRelative("floatValue").floatValue);
                    continue;
                }
                else if (method.GetParameters().ElementAt(i).ParameterType == typeof(bool))
                {
                    EditorGUI.LabelField(LabelForParameter, method.GetParameters().ElementAt(i).Name + " (Bool)", EditorStyles.boldLabel);
                    element.FindPropertyRelative("boolValue").boolValue = EditorGUI.Toggle(ParameterRect, element.FindPropertyRelative("boolValue").boolValue);
                }
                else if (method.GetParameters().ElementAt(i).ParameterType == typeof(Quest))
                {
                    EditorGUI.LabelField(LabelForParameter, method.GetParameters().ElementAt(i).Name + " (Quest)", EditorStyles.boldLabel);
                    element.FindPropertyRelative("questValue").objectReferenceValue = EditorGUI.ObjectField(ParameterRect, (Quest)element.FindPropertyRelative("questValue").objectReferenceValue, typeof(Quest), true) as Quest;
                    continue;
                } 
                else
                {
                    EditorGUI.LabelField(LabelForParameter, "NON DRAWABLE PARAMETER (" + method.GetParameters().ElementAt(i).ParameterType + ")");
                    EditorGUI.LabelField(ParameterRect, method.GetParameters().ElementAt(i).ParameterType.Name, EditorStyles.boldLabel);
                }
            }
            float paramsPush = 150 + (method.GetParameters().Length) * 150;

            var LabelForOperator = new Rect(position.x + paramsPush, position.y, 135, position.height - (customHeight));
            var OperatorRect = new Rect(position.x + paramsPush, position.y + 20, 135, position.height - (customHeight));

            EditorGUI.LabelField(LabelForOperator, "Comparision", EditorStyles.boldLabel);
            EditorGUI.PropertyField(OperatorRect, property.FindPropertyRelative("compOperator"), GUIContent.none);

            var LabelForValue = new Rect(position.x + 150 + paramsPush, position.y, 135, position.height - (customHeight));
            var ValueRect = new Rect(position.x + 150 + paramsPush, position.y + 20, 135, position.height + 3f - (customHeight));

            if (!property.FindPropertyRelative("useGlobal").boolValue)
            {
                EditorGUI.LabelField(LabelForValue, "Value", EditorStyles.boldLabel);
                EditorGUI.PropertyField(ValueRect, property.FindPropertyRelative("customValue"), GUIContent.none);
            } else
            {
                EditorGUI.LabelField(LabelForValue, "Value", EditorStyles.boldLabel);

                // Get all global variables
                Type t = typeof(GlobalVariables);
                List<PropertyInfo> properties = new List<PropertyInfo>();

                PropertyInfo[] allProp = t.GetProperties();

                string menuDisplayValue = "";

                if (property.FindPropertyRelative("propertyName") != null)
                    menuDisplayValue = (string.IsNullOrEmpty(property.FindPropertyRelative("propertyName").stringValue) ? "-None-" : property.FindPropertyRelative("propertyName").stringValue);
                else
                    menuDisplayValue = "-None-";

                // Display all global variables
                if (GUI.Button(ValueRect, menuDisplayValue))
                {
                    // create the menu and add items to it
                    GenericMenu menu = new GenericMenu();

                    menu.AddDisabledItem(new GUIContent("Global Variables"));

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("None"), false, Callback, new ConditionGlobalValueData(property, "", null));
                    for (int i = 0; i < allProp.Length; i++)
                    {
                        menu.AddItem(new GUIContent(allProp[i].Name), false, Callback, new ConditionGlobalValueData(property, allProp[i].Name, allProp[i].GetValue(t)));
                    }

                    menu.ShowAsContext();
                }
            }

            var LabelUseGlobal = new Rect(position.x + 150 + paramsPush, position.y + 40, 135, position.height - (customHeight));
            var UseGlobalBoolRect = new Rect(position.x + 230 + paramsPush, position.y + 40, 135, position.height - (customHeight));
            
            EditorGUI.LabelField(LabelUseGlobal, "Use Global?", EditorStyles.boldLabel);
            EditorGUI.PropertyField(UseGlobalBoolRect, property.FindPropertyRelative("useGlobal"), GUIContent.none);

            var LabelForOR = new Rect(position.x + 300 + paramsPush, position.y, 135, position.height - (customHeight));
            var ORRect = new Rect(position.x + 300 + paramsPush, position.y + 20, 135, position.height - (customHeight));

            EditorGUI.LabelField(LabelForOR, "OR", EditorStyles.boldLabel);
            EditorGUI.PropertyField(ORRect, property.FindPropertyRelative("OR"), GUIContent.none);
        }

        private void Callback(object obj)
        {
            ConditionGlobalValueData data = (ConditionGlobalValueData)obj;
            data.property.FindPropertyRelative("propertyName").stringValue = data.name;

            if (data.value != null)
            {
                if (data.value.GetType() == typeof(int))
                {
                    data.property.FindPropertyRelative("propertyIntValue").intValue = (int)data.value;
                    data.property.FindPropertyRelative("propertyType").enumValueIndex = 1; // 1 = int
                }
                else if (data.value.GetType() == typeof(float))
                {
                    data.property.FindPropertyRelative("propertyFloatValue").floatValue = (float)data.value;
                    data.property.FindPropertyRelative("propertyType").enumValueIndex = 2; // 2 = float
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 55f;
        }       
    }
}