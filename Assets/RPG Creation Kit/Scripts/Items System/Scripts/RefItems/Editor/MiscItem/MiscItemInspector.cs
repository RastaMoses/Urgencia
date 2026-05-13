using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    [CustomEditor(typeof(MiscItem))]
    public class MiscItemInspector : Editor
    {
        MiscItem refItem;

        void OnEnable()
        {
            refItem = (MiscItem)target;
        }

        public override void OnInspectorGUI()
        {
            GUIStyle customButton = new GUIStyle("button");
            customButton.fontSize = 20;

            if (GUILayout.Button("Configure Item"))
            {
                var AllWindows = Resources.FindObjectsOfTypeAll<MiscItem_Window>();
                bool winFound = false;

                for (int i = 0; i < AllWindows.Length; i++)
                {
                    if (AllWindows[i].isReady &&
                        AllWindows[i].itemObj.FindProperty("ItemID").stringValue == refItem.ItemID)
                    {
                        winFound = true;
                        AllWindows[i].Focus();
                    }
                }

                if (!winFound)
                {
                    MiscItem_Window myWindow = CreateInstance<MiscItem_Window>();
                    myWindow.minSize = new Vector2(639, 425);
                    myWindow.maxSize = new Vector2(639, 425);
                    myWindow.Init(serializedObject);
                }
            }

            base.OnInspectorGUI();
        }
    }
}