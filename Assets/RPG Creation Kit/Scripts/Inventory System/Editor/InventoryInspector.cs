using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    [CustomEditor(typeof(Inventory))]
    public class InventoryInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            /*
            var main = target as Inventory;

            EditorGUILayout.Space(); EditorGUILayout.Space();
            EditorGUILayout.Space(); EditorGUILayout.Space();

            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;

            EditorGUILayout.LabelField("Inventory:", EditorStyles.boldLabel);

            var golds = serializedObject.FindProperty("Golds");
            var items = serializedObject.FindProperty("Items");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Golds"));

            EditorGUILayout.BeginVertical("box");

            for(int i = 0; i < items.arraySize; i++)
            {
                Debug.Log(items.GetArrayElementAtIndex(0).FindPropertyRelative("item").objectReferenceValue);

                var newitem = (Item)items.GetArrayElementAtIndex(0).FindPropertyRelative("item").objectReferenceValue;
                Debug.Log(newitem.ItemID);

                Sprite s;

                s = (newitem != null) ? newitem.ItemIcon as Sprite :
                                                      AssetDatabase.LoadAssetAtPath<Sprite>(EditorIconsPath.NoIcon);

                EditorGUI.DrawTextureTransparent(new Rect(105, 90, 100, 100), s.texture);
            }
            


            EditorGUILayout.EndVertical();




            serializedObject.ApplyModifiedProperties();
            */
        }
    }
}