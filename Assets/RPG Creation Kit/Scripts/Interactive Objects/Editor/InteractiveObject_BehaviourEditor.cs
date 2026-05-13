using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;


namespace RPGCreationKit
{
    /// <summary>
    /// Custom Editor for the InteractiveObject_Behaviour
    /// </summary>
    [CustomEditor(typeof(InteractiveObject_Behaviour))]
    public class InteractiveObject_BehaviourEditor : Editor
    {

        // InteractiveObject_Behaviour data
        SerializedProperty interactiveObj = null;
        SerializedObject newObject = null;
        SerializedProperty options = null;
        bool canDraw = false;

        public MonoScript[] allResultScripts;


        /// <summary>
        /// On the enable we instantiate the array of events in base of how much options the "interactiveObj" allows us to perform the events on options
        /// </summary>
        private void OnEnable()
        {
            interactiveObj = serializedObject.FindProperty("interactiveObj");

            if (interactiveObj.objectReferenceValue != null)
            {
                serializedObject.FindProperty("events").ClearArray();
                serializedObject.FindProperty("resultScripts").ClearArray();

                newObject = new SerializedObject(interactiveObj.objectReferenceValue);
                options = newObject.FindProperty("InteractiveOptions");

                for (int i = 0; i < options.arraySize; i++)
                {
                    serializedObject.FindProperty("events").InsertArrayElementAtIndex(i);
                    serializedObject.FindProperty("resultScripts").InsertArrayElementAtIndex(i);
                }

                canDraw = true;
            }

            allResultScripts = GetAllResultScripts<ResultScript>();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (allResultScripts == null) allResultScripts = GetAllResultScripts<ResultScript>();

            interactiveObj = serializedObject.FindProperty("interactiveObj");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(interactiveObj);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
            {
                // If the EditorGUI changes
                UpdateData();
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            if (canDraw)
            {
                if (interactiveObj != null && newObject != null) // Extra check
                {
                    // For each options draw events (with the correspondent index value of events)
                    for (int i = 0; i < options.arraySize; i++)
                    {
                        try
                        {
                            // Drawa the events on each interactiveoption
                            EditorGUILayout.LabelField("On " + (InteractiveOptions)options.GetArrayElementAtIndex(i).enumValueIndex, EditorStyles.boldLabel);

                            EditorGUILayout.PropertyField(serializedObject.FindProperty("events").GetArrayElementAtIndex(i), new GUIContent(("Events")), true);

                            EditorGUILayout.BeginHorizontal();
                            EditorGUIUtility.fieldWidth = 5;

                            EditorGUILayout.LabelField("Result Script:");
                            var resultScript = serializedObject.FindProperty("resultScripts").GetArrayElementAtIndex(i);

                            string resultScriptMenuValue = string.IsNullOrEmpty(resultScript.stringValue) ? "-None-" : resultScript.stringValue;
                            if (GUILayout.Button(resultScriptMenuValue))
                            {
                                // create the menu and add items to it
                                GenericMenu menu = new GenericMenu();

                                menu.AddDisabledItem(new GUIContent("Result Scripts"));

                                menu.AddSeparator("");

                                menu.AddItem(new GUIContent("None"), false, Callback, new ScriptElementData(resultScript, ""));
                                for (int j = 0; j < allResultScripts.Length; j++)
                                {
                                    menu.AddItem(new GUIContent(allResultScripts[j].name), false, Callback, new ScriptElementData(resultScript, allResultScripts[j].GetClass().Namespace + "." + allResultScripts[j].name));
                                }

                                menu.ShowAsContext();
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                        }
                        catch (System.Exception e)
                        {
                            // If the array is not updated with the InteractiveOptions
                            if (e.GetType().ToString() == "System.NullReferenceException")
                            {
                                // Delete and re-do the reference to the interactive object
                                UpdateData(true);
                            }
                        }

                    }

                    if (options.arraySize <= 0)
                    {
                        EditorGUILayout.LabelField("The selected interactive object has no InteractiveOptions. Assign them.");
                    }

                }
            }


            // Save all
            serializedObject.ApplyModifiedProperties();

            if (newObject != null)
                newObject.ApplyModifiedProperties();


        }

        void Callback(object obj)
        {
            ScriptElementData questScriptData = (ScriptElementData)obj;
            questScriptData.property.stringValue = questScriptData.value;

            questScriptData.property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Update the array data. When a new Option is added, add the array element of events as well
        /// We use the 'forced' to force to read again the interactiveObject, this is done when we add 
        /// a new InteractiveOption to a InteractiveObject already present in the scene
        /// </summary>
        public void UpdateData(bool forced = false)
        {
            Debug.Log("called");
            // If everything have a reference
            if (interactiveObj != null && interactiveObj.objectReferenceValue != null
                || forced)
            {
                // Initialize the "interactiveObj" as a SerializedObject to retrive the InteractiveOptions
                newObject = new SerializedObject(interactiveObj.objectReferenceValue);
                options = newObject.FindProperty("InteractiveOptions");

                // We clear the array
                serializedObject.FindProperty("events").ClearArray();
                serializedObject.FindProperty("resultScripts").ClearArray();

                // We insert as many array elements as many InteractiveOptions we can perform
                for (int i = 0; i < options.arraySize; i++)
                {
                    serializedObject.FindProperty("events").InsertArrayElementAtIndex(i);
                    serializedObject.FindProperty("resultScripts").InsertArrayElementAtIndex(i);
                }


                EditorGUILayout.Space();
                EditorGUILayout.Space();

                // Now that we're read we can draw the Editor GUI
                canDraw = true;
            }
            else
            {
                canDraw = false;
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

    }
}
