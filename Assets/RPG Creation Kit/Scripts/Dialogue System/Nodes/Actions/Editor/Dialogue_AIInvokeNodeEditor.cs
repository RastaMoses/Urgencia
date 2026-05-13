using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
using RPGCreationKit.DialogueSystem;
using UnityEditor;
using RPGCreationKit;
using System;
using RPGCreationKit.AI;
using System.Linq;
using RPGCreationKit.BehaviourTree.Data;
using System.Reflection;

namespace RPGCreationKit.DialogueSystem
{
    [CustomNodeEditor(typeof(Dialogue_AIInvokeNode))]
    public class Dialogue_AIInvokeNodeEditor : DialogueNodeEditor
    {
        private Dialogue_AIInvokeNode aiInvokeNode;

        string[] methods;     // Contains [AI_INVOKABLE] methods, used to display them in the PropertyDrawer
        string[] returnType;  // Contains the return type of the methods[i]

        public override void OnCreate()
        {
            base.OnCreate();

            // Fill methods
            methods =
            typeof(RckAI)
            .GetMethods()
            .Where(m => m.GetCustomAttributes().OfType<AIInvokableAttribute>().Any())
            .Select(x => x.Name)
            .ToArray();

            // Fill return types
            returnType =
            typeof(RckAI)
            .GetMethods()
            .Where(m => m.GetCustomAttributes().OfType<AIInvokableAttribute>().Any())
            .Select(x => x.ReturnType.Name)
            .ToArray();
        }

        public override void OnBodyGUI()
        {
            if (aiInvokeNode == null) aiInvokeNode = target as Dialogue_AIInvokeNode;
            serializedObject.Update();

            Color dColor = GUI.color;

            EditorGUIUtility.labelWidth = 150;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            int index = 0;

            try
            {
                index = methods
                    .Select((v, i) => new { Name = v, Index = i })
                    .First(x => x.Name == serializedObject.FindProperty("MethodToCall").stringValue)
                    .Index;
            }
            catch
            {
                index = 0;
            }

            foldout = EditorGUILayout.Foldout(foldout, "Show/Hide", true);
            EditorGUI.indentLevel++;

            if (foldout)
            {
                EditorGUILayout.LabelField("Invoke:", EditorStyles.boldLabel);
                serializedObject.FindProperty("MethodToCall").stringValue = methods[EditorGUILayout.Popup(index, methods)];

                var selectedMethod = methods[index];
                MethodInfo method = (typeof(RckAI).GetMethod(selectedMethod));

                // Resize the parameters array in base of the number of parameters of the selected method
                if (serializedObject.FindProperty("parameters").arraySize != method.GetParameters().Length)
                    serializedObject.FindProperty("parameters").arraySize = method.GetParameters().Length;

                // Draw parameters
                for (int i = 0; i < method.GetParameters().Length; i++)
                {
                    SerializedProperty element = serializedObject.FindProperty("parameters").GetArrayElementAtIndex(i);
                    SerializedProperty previousElement = null;

                    if (i > 0)
                        previousElement = serializedObject.FindProperty("parameters").GetArrayElementAtIndex(i-1);

                    NodesHelper.AIInvokeCallEditorDrawParamter(method, element, i, previousElement);
                }
                float paramsPush = 150 + (method.GetParameters().Length) * 150;

                EditorGUILayout.Space(2.5f);
            }
            EditorGUI.indentLevel--;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("output"));


            GUI.color = dColor;
        }

        public static MonoScript[] GetScriptAssetsOfType<T>()
        {
            MonoScript[] scripts = (MonoScript[])UnityEngine.Object.FindObjectsOfTypeIncludingAssets(typeof(MonoScript));

            List<MonoScript> result = new List<MonoScript>();

            foreach (MonoScript m in scripts)
            {
                if (m.GetClass() != null && m.GetClass().IsSubclassOf(typeof(T)) && m.GetType() != typeof(Shader))
                {
                    result.Add(m);
                }
            }
            return result.ToArray();
        }

        void Callback(object obj)
        {
            ScriptElementData questScriptData = (ScriptElementData)obj;
            questScriptData.property.stringValue = questScriptData.value;

            questScriptData.property.serializedObject.ApplyModifiedProperties();
        }

        public override int GetWidth()
        {
            if (serializedObject.FindProperty("events").isExpanded &&
                serializedObject.FindProperty("events").FindPropertyRelative("consequences").isExpanded)
                return 350;
            else
                return 250;
        }

    }
}