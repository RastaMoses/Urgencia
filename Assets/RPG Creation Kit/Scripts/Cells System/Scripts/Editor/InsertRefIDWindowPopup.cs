using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Represent the settings of the InsertRefIDWindowPopup
    /// </summary>
    public struct InsertRefIDWindowSettings
    {
        public int settingsCount { get; private set; }

        public bool usesFocusOnRefID;

        public InsertRefIDWindowSettings(bool _usesFocusOnRefID)
        {
            settingsCount = 0;

            usesFocusOnRefID = _usesFocusOnRefID;
            if (usesFocusOnRefID) settingsCount++;

        }

        public static InsertRefIDWindowSettings DefaultSettings()
        {
            InsertRefIDWindowSettings _settings = new InsertRefIDWindowSettings(false);
            return _settings;
        }
    }

    public class InsertRefIDWindowPopup : EditorWindow
    {
        static InsertRefIDWindowSettings settings;
        public static InsertRefIDWindowSettings settingsValues;


        string insertedRefID = null;
        public static string lastInsertedRefID { get; private set; }

        bool goodClose = false; //says if the user has clicked on the X button instead of confirming the text

        public static void OpenWindow(InsertRefIDWindowSettings? _settings = null)
        {
            InsertRefIDWindowPopup window = CreateInstance<InsertRefIDWindowPopup>();
            window.titleContent = new GUIContent("Insert RefID:");

            if (_settings.HasValue)
            {
                settings = _settings.Value;
                settingsValues = settings;

                if (settings.settingsCount < 3)
                {
                    window.minSize = new Vector2(350.0f, 100.0f);
                    window.maxSize = new Vector2(350.0f, 100.0f);
                } else
                {
                    // Make window max size bigger
                }
            }
            else
            {
                settings = InsertRefIDWindowSettings.DefaultSettings();
                settingsValues = settings;

                window.minSize = new Vector2(350.0f, 80.0f);
                window.maxSize = new Vector2(350.0f, 80.0f);
            }

            window.ShowModal();
            window.Focus();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("RefID:", EditorStyles.boldLabel);
            insertedRefID = EditorGUILayout.TextField(insertedRefID);

            EditorGUILayout.BeginHorizontal();
            if (settings.usesFocusOnRefID)
            {
                EditorGUILayout.Space(2);

                settingsValues.usesFocusOnRefID = EditorGUILayout.Toggle(new GUIContent("Focus on Reference?", "If this is clicked when the current action is performed, the inserted RefID will be focused in the Scene View"), settingsValues.usesFocusOnRefID);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            

            GUI.enabled = !string.IsNullOrEmpty(insertedRefID);
            if(GUILayout.Button("Confirm"))
            {
                lastInsertedRefID = insertedRefID;
                goodClose = true;
                this.Close();
            }
            GUI.enabled = true;
        }

        private void OnDestroy()
        {
            if(!goodClose)
                lastInsertedRefID = null;
        }

    }
}