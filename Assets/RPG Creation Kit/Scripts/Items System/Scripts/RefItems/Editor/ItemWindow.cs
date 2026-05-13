using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RPGCreationKit
{
    public class ScriptElementData
    {
        public SerializedProperty property;
        public string value;

        // Constructor
        public ScriptElementData(SerializedProperty _property, string _value)
        {
            property = _property;
            value = _value;
        }
    }

    public class ItemWindow : EditorWindow
    {
        public List<EditorWindow> childWindows = new List<EditorWindow>();

        public MonoScript[] allItemScripts;
        public GUIStyle ButtonStyle;

        public bool soundWinOpened = false;

        public virtual void Init(SerializedObject serializedObject)
        {
            allItemScripts = NodesHelper.GetAllItemsScripts<ItemScript>();


            ButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            ButtonStyle.border = new RectOffset(2, 2, 2, 2);
            ButtonStyle.fontSize = 16;
            ButtonStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");
        }

        public static MonoScript[] GetScriptAssetsOfType<T>()
        {
            MonoScript[] scripts = (MonoScript[])FindObjectsOfTypeIncludingAssets(typeof(MonoScript));

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

        public void Callback(object obj)
        {
            ScriptElementData questScriptData = (ScriptElementData)obj;

            questScriptData.property.stringValue = questScriptData.value;
        }
    }
}
