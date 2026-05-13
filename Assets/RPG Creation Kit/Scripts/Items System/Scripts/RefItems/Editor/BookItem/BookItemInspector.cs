using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    [CustomEditor(typeof(BookItem))]
    public class BookItemInspector : Editor
    {

        BookItem refItem;

        void OnEnable()
        {
            refItem = (BookItem)target;
        }

        public override void OnInspectorGUI()
        {
            GUIStyle customButton = new GUIStyle("button");
            customButton.fontSize = 20;

            if (GUILayout.Button("Configure Item", customButton))
            {
                var AllWindows = Resources.FindObjectsOfTypeAll<BookItem_Window>();
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
                    BookItem_Window myWindow = CreateInstance<BookItem_Window>();
                    myWindow.minSize = new Vector2(639, 615);
                    myWindow.maxSize = new Vector2(639, 615);
                    myWindow.Init(serializedObject);
                }
            }

            base.OnInspectorGUI();
        }
    }
}