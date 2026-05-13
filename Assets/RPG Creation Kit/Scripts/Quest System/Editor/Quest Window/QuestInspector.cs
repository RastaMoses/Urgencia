using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    [CustomEditor(typeof(Quest))]
    public class QuestInspector : Editor
    {
        Quest refItem;

        void OnEnable()
        {
            refItem = (Quest)target;
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;


            if (GUILayout.Button("Configure Quest"))
            {
                var AllWindows = Resources.FindObjectsOfTypeAll<QuestWindow>();
                bool winFound = false;

                for (int i = 0; i < AllWindows.Length; i++)
                {
                    if (AllWindows[i].isReady &&
                        AllWindows[i].questObj.FindProperty("questID").stringValue == refItem.questID)
                    {
                        winFound = true;
                        AllWindows[i].Focus();
                    }
                }

                if (!winFound)
                {
                    QuestWindow myWindow = CreateInstance<QuestWindow>();
                    myWindow.minSize = new Vector2(960, 820);
                    myWindow.maxSize = new Vector2(960, 820);
                    myWindow.Init(serializedObject);
                }
            }
        }
    }
}