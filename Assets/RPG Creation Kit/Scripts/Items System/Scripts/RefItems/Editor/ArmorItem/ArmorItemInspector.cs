using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    [CustomEditor(typeof(ArmorItem))]
    [CanEditMultipleObjects]
    public class ArmorItemInspector : Editor
    {

        ArmorItem refItem;

        void OnEnable()
        {
            refItem = (ArmorItem)target;
        }

        public override void OnInspectorGUI()
        {
            GUIStyle customButton = new GUIStyle("button");
            customButton.fontSize = 20;

            if (GUILayout.Button("Configure Item", customButton))
            {
                var AllWindows = Resources.FindObjectsOfTypeAll<ArmorItem_Window>();
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
                    ArmorItem_Window myWindow = CreateInstance<ArmorItem_Window>();
                    myWindow.minSize = new Vector2(690, 780);
                    myWindow.maxSize = new Vector2(690, 780);
                    myWindow.Init(serializedObject);
                }
            }

            base.OnInspectorGUI();
        }
    }
}