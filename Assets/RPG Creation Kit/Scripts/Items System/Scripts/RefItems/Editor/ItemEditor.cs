using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Custom editor for Item class, for hide/show fields dinamically
    /// </summary>
    [CustomEditor(typeof(Item))]
    public class ItemEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            Color defaultColor = GUI.color;

            var main = target as Item;

           
            var ItemIcon = serializedObject.FindProperty("ItemIcon");
            var ItemName = serializedObject.FindProperty("ItemName");
            var ItemDescription = serializedObject.FindProperty("ItemDescription");
            var ItemCategory = serializedObject.FindProperty("itemCategory");
            var WeaponCategory = serializedObject.FindProperty("weaponCategory");
            var ApparelCategory = serializedObject.FindProperty("apparelCategory");

            EditorGUILayout.PropertyField(ItemIcon);

            EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ItemName);
            EditorGUILayout.PropertyField(ItemDescription);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ItemCategory);

            if(ItemCategory.enumValueIndex == 0)
            {
                GUI.backgroundColor = Color.red;
                EditorGUILayout.LabelField("(!) You must select an 'ItemCategory' for creating a valid Item", EditorStyles.textArea);
                GUI.backgroundColor = defaultColor;
            }

            if (ItemCategory.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(WeaponCategory);

                GUI.backgroundColor = Color.red;

                if (WeaponCategory.enumValueIndex == 0)
                    EditorGUILayout.LabelField("(!) You must select a WeaponCategory when you use the ItemCategory 'Weapons'", EditorStyles.textArea);

                GUI.backgroundColor = defaultColor;

            }
            else if (ItemCategory.enumValueIndex == 2)
            {
                EditorGUILayout.PropertyField(ApparelCategory);

                GUI.backgroundColor = Color.red;

                if (ApparelCategory.enumValueIndex == 0)
                    EditorGUILayout.LabelField("(!) You must select a ApparelCategory when you use the ItemCategory 'Apparel'", EditorStyles.textArea);

                GUI.backgroundColor = defaultColor;
            }





            serializedObject.ApplyModifiedProperties();
        }
    }
}
