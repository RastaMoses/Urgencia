using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
using RPGCreationKit.BehaviourTree;
using UnityEditor;
using RPGCreationKit;
using System.Linq;
using System.Reflection;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree
{
    [CustomNodeEditor(typeof(AI_InvokeNode))]
    public class AI_InvokeEditor : BTNodeNodeEditor
    {
        private AI_InvokeNode aiInvokeNode;
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
            if (aiInvokeNode == null) aiInvokeNode = target as AI_InvokeNode;
            btNode = aiInvokeNode;

            Color dColor = GUI.color;

            if (aiInvokeNode.m_NodeDebugState == NodeState.Success)
                GUI.color = Color.green;
            else if (aiInvokeNode.m_NodeDebugState == NodeState.Running)
                GUI.color = Color.yellow;
            else if (aiInvokeNode.m_NodeDebugState == NodeState.Failure)
                GUI.color = Color.red;

            // Update serialized object's representation
            serializedObject.Update();

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
                var method = (typeof(RckAI).GetMethod(selectedMethod));

                // Resize the parameters array in base of the number of parameters of the selected method
                if (serializedObject.FindProperty("parameters").arraySize != method.GetParameters().Length)
                    serializedObject.FindProperty("parameters").arraySize = method.GetParameters().Length;

                // Draw parameters
                for (int i = 0; i < method.GetParameters().Length; i++)
                {
                    var element = serializedObject.FindProperty("parameters").GetArrayElementAtIndex(i);
                    SerializedProperty previousElement = null;
                    
                        if (i > 0)
                        previousElement = serializedObject.FindProperty("parameters").GetArrayElementAtIndex(i-1);

                    NodesHelper.AIInvokeCallEditorDrawParamter(method, element, i, previousElement);
                }
                float paramsPush = 150 + (method.GetParameters().Length) * 150;

                EditorGUILayout.Space(2.5f);
            }
            EditorGUI.indentLevel--;

            GUI.color = dColor;
        }

        public override int GetWidth()
        {
            return 250;
        }
    }
}