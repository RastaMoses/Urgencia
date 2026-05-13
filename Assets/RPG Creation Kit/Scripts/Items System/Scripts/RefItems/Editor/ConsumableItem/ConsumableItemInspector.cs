using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    [CustomEditor(typeof(ConsumableItem))]
    public class ConsumableItemInspector : Editor
    {

        ConsumableItem refItem;

        void OnEnable()
        {
            refItem = (ConsumableItem)target;
        }

        public override void OnInspectorGUI()
        {
            GUIStyle customButton = new GUIStyle("button");
            customButton.fontSize = 20;

            if (GUILayout.Button("Configure Item", customButton))
            {
                var AllWindows = Resources.FindObjectsOfTypeAll<ConsumableItem_Window>();
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
                    ConsumableItem_Window myWindow = CreateInstance<ConsumableItem_Window>();
                    myWindow.minSize = new Vector2(639, 670);
                    myWindow.maxSize = new Vector2(639, 670);
                    myWindow.Init(serializedObject);
                }
            }

            base.OnInspectorGUI();
        }
    }
}