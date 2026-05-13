using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEditor;

namespace RPGCreationKit
{
    [CustomPropertyDrawer(typeof(ItemInInventory))]
    public class ItemInInventoryPropertyDrawer : PropertyDrawer
    {

        Color boxColor = new Color(50, 50, 50, .2f);

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            EditorGUI.DrawRect(new Rect(position.x, position.y, position.width, 70), boxColor);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float customHeight = 80;

            if (property.FindPropertyRelative("metadata").isExpanded)
                customHeight += 60;

            // Calculate rects
            var iconRect = new Rect(position.x, position.y, 40, 40);
            var itemRect = new Rect(position.x + 55, position.y + 10, position.width - 55, position.height - (customHeight));

            var AmountLabelRect = new Rect(position.x, position.y + 50, 50, position.height - (customHeight));
            var AmountRect = new Rect(position.x + 55, position.y + 50, 50, position.height - (customHeight));

            var isEquippedRect = new Rect(position.x + 90, position.y + 50, position.width - 90, position.height - (customHeight));
            var metadataRect = new Rect(position.x, position.y + 75, position.width - 15, position.height - (customHeight));



            Item Item = null;

            if (property.FindPropertyRelative("item").objectReferenceValue != null)
                Item = (Item)property.FindPropertyRelative("item").objectReferenceValue;


            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(itemRect, property.FindPropertyRelative("item"), GUIContent.none);


            if (Item != null && !Item.isCumulable)
            {
                property.FindPropertyRelative("Amount").intValue = 1;
                GUI.enabled = false;
                EditorGUI.LabelField(AmountLabelRect, "Amount:");
                EditorGUI.PropertyField(AmountRect, property.FindPropertyRelative("Amount"), GUIContent.none);

                GUI.enabled = true;
                var AmountInfoRect = new Rect(position.x + 110, position.y + 50, position.width, position.height - (customHeight));

                EditorGUI.LabelField(AmountInfoRect, "(Non-Cumul.)");
            } else
            {
                if (property.FindPropertyRelative("Amount").intValue < 1)
                    property.FindPropertyRelative("Amount").intValue = 1;

                EditorGUI.LabelField(AmountLabelRect, "Amount:");
                EditorGUI.PropertyField(AmountRect, property.FindPropertyRelative("Amount"), GUIContent.none);
            }

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(metadataRect, property.FindPropertyRelative("metadata"), true);

            //EditorGUI.PropertyField(isEquippedRect, property.FindPropertyRelative("isEquipped"), GUIContent.none);

            Sprite s;

            /*
            if (property.FindPropertyRelative("item").objectReferenceValue != null)
            {
                

                s = (Item != null && Item.ItemIcon != null) ? Item.ItemIcon as Sprite :
                                                        AssetDatabase.LoadAssetAtPath<Sprite>(EditorIconsPath.NoIcon);

                EditorGUI.DrawTextureTransparent(iconRect, s.texture);
            }
            else
                EditorGUI.DrawTextureTransparent(iconRect, AssetDatabase.LoadAssetAtPath<Sprite>(EditorIconsPath.NoIcon).texture);
            */

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            property.serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(property.serializedObject.targetObject);


            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.FindPropertyRelative("metadata").isExpanded)
                return 155;
            else
                return 95;
        }

    }
}