using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;


namespace RPGCreationKit
{
    /// <summary>
    /// Custom editor for Goto class, just for hide/show fields dinamically
    /// </summary>
    [CustomEditor(typeof(Goto))]
    public class GotoEditor : Editor
    {
        public MonoScript[] allResultScripts;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            var main = target as Goto;

            if (allResultScripts == null) allResultScripts = NodesHelper.GetAllResultScripts<ResultScript>();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("OnEnter Result Script:");

            var onEnterResultScript = serializedObject.FindProperty("onEnterResultScript");

            string onEnterResultScriptMenuValue = string.IsNullOrEmpty(onEnterResultScript.stringValue) ? "-None-" : onEnterResultScript.stringValue;
            if (GUILayout.Button(new GUIContent(onEnterResultScriptMenuValue, onEnterResultScriptMenuValue)))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                menu.AddDisabledItem(new GUIContent("Result Scripts"));

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("None"), false, Callback, new ScriptElementData(onEnterResultScript, ""));
                for (int i = 0; i < allResultScripts.Length; i++)
                {
                    menu.AddItem(new GUIContent(allResultScripts[i].name), (allResultScripts[i].GetClass().Namespace + "." + allResultScripts[i].name) == onEnterResultScriptMenuValue, Callback, new ScriptElementData(onEnterResultScript, allResultScripts[i].GetClass().Namespace + "." + allResultScripts[i].name));
                }

                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("OnEnter Result Script:");

            var onExitResultScript = serializedObject.FindProperty("onExitResultScript");

            string onExitResultScriptMenuValue = string.IsNullOrEmpty(onExitResultScript.stringValue) ? "-None-" : onExitResultScript.stringValue;
            if (GUILayout.Button(new GUIContent(onExitResultScriptMenuValue, onExitResultScriptMenuValue)))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                menu.AddDisabledItem(new GUIContent("Result Scripts"));

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("None"), false, Callback, new ScriptElementData(onExitResultScript, ""));
                for (int i = 0; i < allResultScripts.Length; i++)
                {
                    menu.AddItem(new GUIContent(allResultScripts[i].name), (allResultScripts[i].GetClass().Namespace + "." + allResultScripts[i].name) == onExitResultScriptMenuValue, Callback, new ScriptElementData(onExitResultScript, allResultScripts[i].GetClass().Namespace + "." + allResultScripts[i].name));
                }

                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            var onEnterEvents = serializedObject.FindProperty("onEnterEvents");
            var onExitEvents = serializedObject.FindProperty("onExitEvents");

            var collisionTarget = serializedObject.FindProperty("collisionTarget");
            var npcs = serializedObject.FindProperty("NPCs");

            var OnEnterDelayActivation = serializedObject.FindProperty("OnEnterDelayActivation");
            var OnExitDelayActivation = serializedObject.FindProperty("OnExitDelayActivation");

            var OnEnterAlreadyTriggered = serializedObject.FindProperty("OnEnterAlreadyTriggered");
            var OnExitAlreadyTriggered = serializedObject.FindProperty("OnExitAlreadyTriggered");

            var AllowMultipleOnEnterTriggering = serializedObject.FindProperty("AllowMultipleOnEnterTriggering");
            var AllowMultipleOnExitTriggering = serializedObject.FindProperty("AllowMultipleOnExitTriggering");

            var DestroyAfterOnEnter = serializedObject.FindProperty("DestroyAfterOnEnter");
            var DestroyDelayOnEnter = serializedObject.FindProperty("DestroyDelayOnEnter");

            var DestroyAfterOnExit = serializedObject.FindProperty("DestroyAfterOnExit");
            var DestroyDelayOnExit = serializedObject.FindProperty("DestroyDelayOnExit");


            var triggerGameObject = serializedObject.FindProperty("TriggerGameObject");

            EditorGUILayout.LabelField("OnTriggerEnter Events:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onEnterEvents, true);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(OnEnterDelayActivation, true);
            EditorGUILayout.PropertyField(AllowMultipleOnEnterTriggering, true);
            EditorGUILayout.PropertyField(DestroyAfterOnEnter, true);
            if(DestroyAfterOnEnter.boolValue)
                EditorGUILayout.PropertyField(DestroyDelayOnEnter, true);


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("OnTriggerExit Events:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onExitEvents, true);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(OnExitDelayActivation, true);
            EditorGUILayout.PropertyField(AllowMultipleOnExitTriggering, true);
            EditorGUILayout.PropertyField(DestroyAfterOnExit, true);
            if (DestroyAfterOnExit.boolValue)
                EditorGUILayout.PropertyField(DestroyDelayOnExit, true);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(collisionTarget, true);

            EditorGUILayout.Space();
            EditorGUILayout.Space();


            switch (main.collisionTarget)
            {
                case CollisionTargets.Player:
                    break;
                case CollisionTargets.NPCs:
                case CollisionTargets.PlayerAndNpcs:
                    EditorGUILayout.PropertyField(npcs, true);
                    break;
                case CollisionTargets.TriggerGameObject:
                    EditorGUILayout.PropertyField(triggerGameObject);
                    break;

            }

            serializedObject.ApplyModifiedProperties();
        }


        public static MonoScript[] GetScriptAssetsOfType<T>()
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
            return result.ToArray();
        }

        void Callback(object obj)
        {
            ScriptElementData questScriptData = (ScriptElementData)obj;
            questScriptData.property.stringValue = questScriptData.value;

            questScriptData.property.serializedObject.ApplyModifiedProperties();
        }

    }
}