using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    [CustomEditor(typeof(WeaponItem))]
    public class WeaponItemInspector : Editor
    {

        WeaponItem refItem;

        void OnEnable()
        {
            refItem = (WeaponItem)target;
        }

        public override void OnInspectorGUI()
        {
            GUIStyle customButton = new GUIStyle("button");
            customButton.fontSize = 20;

            if (GUILayout.Button("Configure Item", customButton))
            {
                var AllWindows = Resources.FindObjectsOfTypeAll<WeaponItem_Window>();
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
                    WeaponItem_Window myWindow = CreateInstance<WeaponItem_Window>();
                    myWindow.minSize = new Vector2(650, 720);
                    myWindow.maxSize = new Vector2(650, 720);
                    myWindow.Init(serializedObject);
                }
            }

            base.OnInspectorGUI();
        }
    }
}