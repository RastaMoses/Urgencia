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

namespace RPGCreationKit.DialogueSystem
{
    [CustomNodeEditor(typeof(Dialogue_AISetFieldNode))]
    public class Dialogue_AISetFieldNodeEditor : DialogueNodeEditor
    {
        private Dialogue_AISetFieldNode setFieldNode;

        string[] allFields;

        string typeToDisplay = "";
        Type fieldType;

        public override void OnCreate()
        {
            base.OnCreate();

            // Fill fields
            allFields =
            typeof(RckAI).GetFields()
            .Where(x => x.IsPublic)
            .Where(m => BTVariable.SUPPORTED_TYPES.Contains(m.FieldType))
            .Select(x => x.Name)
            .ToArray();
        }

        public override void OnBodyGUI()
        {
            base.OnBodyGUI();

            if (setFieldNode == null) setFieldNode = target as Dialogue_AISetFieldNode;
            if (allResultScripts == null) allResultScripts = NodesHelper.GetAllResultScripts<ResultScript>();

            // Initalize
            if (allFields.Length > 0 && string.IsNullOrEmpty(serializedObject.FindProperty("FieldToSet").stringValue))
                serializedObject.FindProperty("FieldToSet").stringValue = allFields[0];


            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));


            int index = 0;

            try
            {
                index = allFields
                    .Select((v, i) => new { Name = v, Index = i })
                    .First(x => x.Name == serializedObject.FindProperty("FieldToSet").stringValue)
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
                EditorGUILayout.Space(2.5f);

                if (string.IsNullOrEmpty(typeToDisplay) || fieldType == null)
                {
                    fieldType = typeof(RckAI).GetField(serializedObject.FindProperty("FieldToSet").stringValue).FieldType;
                    typeToDisplay = fieldType.Name.ToString();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("FieldToSet").stringValue = allFields[EditorGUILayout.Popup(index, allFields)];

                if (EditorGUI.EndChangeCheck())
                {
                    fieldType = typeof(RckAI).GetField(serializedObject.FindProperty("FieldToSet").stringValue).FieldType;
                    typeToDisplay = fieldType.Name.ToString();
                }


                EditorGUILayout.LabelField("(" + typeToDisplay + ")", GUILayout.MaxWidth(80));
                EditorGUILayout.EndHorizontal();

                if (fieldType == typeof(int))
                {
                    setFieldNode.instantValue.parameterType = BTParameterType.INT;
                    setFieldNode.instantValue.intValue = EditorGUILayout.IntField("Int Value", setFieldNode.instantValue.intValue);
                }
                else if (fieldType == typeof(float))
                {
                    setFieldNode.instantValue.parameterType = BTParameterType.FLOAT;
                    setFieldNode.instantValue.floatValue = EditorGUILayout.FloatField("Float Value", setFieldNode.instantValue.floatValue);
                }
                else if (fieldType == typeof(bool))
                {
                    setFieldNode.instantValue.parameterType = BTParameterType.BOOL;
                    setFieldNode.instantValue.boolValue = EditorGUILayout.Toggle("Bool Value", setFieldNode.instantValue.boolValue);
                }

                EditorGUILayout.Space(2.5f);

            }
            EditorGUI.indentLevel--;



            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("output"));


            EditorGUILayout.Space(2.5f);
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