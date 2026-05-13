using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    [CustomEditor(typeof(Race))]
    public class RaceInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Configure Race"))
            {
                var AllWindows = Resources.FindObjectsOfTypeAll<RaceEditor>();
                bool winFound = false;

                for (int i = 0; i < AllWindows.Length; i++)
                {
                    if (AllWindows[i])
                    {
                        winFound = true;
                        AllWindows[i].Focus();
                    }
                }

                if (!winFound)
                {
                    RaceEditor.ShowWindow();
                }
            }
        }
    }
}