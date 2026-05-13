using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;
using System;

namespace RPGCreationKit
{
    [CustomPropertyDrawer(typeof(EffectOnEntity))]
    public class EffectOnEntityDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Custom variable Height that will expand in base of the data that we have to show
            float customHeight = 35f;

            if (!property.isExpanded)
                customHeight = 10f;
            else
                customHeight = 75f;
            
            if (property.isExpanded)
            {
                var LabelForType = new Rect(position.x + 5, position.y + 20, 100, position.height - (customHeight));
                var TypeRect = new Rect(position.x + 5 + 80, position.y + 20, 135, position.height - (customHeight));

                EditorGUI.LabelField(LabelForType, "Effect Type:", EditorStyles.boldLabel);
                EditorGUI.PropertyField(TypeRect, property.FindPropertyRelative("effectType"), GUIContent.none);

                var effectIndex = property.FindPropertyRelative("effectType").enumValueIndex;

                if ((ConsumableEffectType)effectIndex == ConsumableEffectType.DamageAttribute ||
                    (ConsumableEffectType)effectIndex == ConsumableEffectType.FortifyAttribute ||
                      (ConsumableEffectType)effectIndex == ConsumableEffectType.RestoreAttribute)
                {
                    var OnAttributeRect = new Rect(position.x + 5 + 210, position.y + 20, 135, position.height - (customHeight));
                    EditorGUI.PropertyField(OnAttributeRect, property.FindPropertyRelative("onAttribute"), GUIContent.none);
                }

                var LabelForDuration = new Rect(position.x + 5, position.y + 40, 100, position.height - (customHeight));
                var DurationRect = new Rect(position.x + 5 + 80, position.y + 40, 60, position.height - (customHeight));

                EditorGUI.LabelField(LabelForDuration, "Duration:", EditorStyles.boldLabel);
                EditorGUI.PropertyField(DurationRect, property.FindPropertyRelative("duration"), GUIContent.none);

                var LabelForMagnitude = new Rect(position.x + 150, position.y + 40, 100, position.height - (customHeight));
                var MagnitudeRect = new Rect(position.x + 5 + 225, position.y + 40, 100, position.height - (customHeight));

                EditorGUI.LabelField(LabelForMagnitude, "Magnitude:", EditorStyles.boldLabel);
                EditorGUI.PropertyField(MagnitudeRect, property.FindPropertyRelative("magnitude"), GUIContent.none);

                var LabelForIcon = new Rect(position.x + 5, position.y + 60, 100, position.height - (customHeight));
                var IconRect = new Rect(position.x + 5 + 80, position.y + 60, 100, position.height - (customHeight));

                EditorGUI.LabelField(LabelForIcon, "Icon:", EditorStyles.boldLabel);
                //EditorGUI.PropertyField(IconRect, property.FindPropertyRelative("effectIcon"), GUIContent.none);
                string display = (property.FindPropertyRelative("effectIconID").stringValue);
                if (string.IsNullOrEmpty(display))
                    display = "NONE";

                if(GUI.Button(IconRect, new GUIContent(display)))
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddDisabledItem(new GUIContent("Icons:"));
                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("NONE"), display == "NONE", SelectIcon, "NONE");
                    menu.AddItem(new GUIContent("HealthUp"), display == "HealthUp", SelectIcon, "HealthUp");
                    menu.AddItem(new GUIContent("HealthDown"), display == "HealthDown", SelectIcon, "HealthDown");
                    menu.AddItem(new GUIContent("StaminaUp"), display == "StaminaUp", SelectIcon, "StaminaUp");
                    menu.AddItem(new GUIContent("StaminaDown"), display == "StaminaDown", SelectIcon, "StaminaDown");

                    menu.ShowAsContext();
                }

                void SelectIcon(object s)
                {
                    property.FindPropertyRelative("effectIconID").stringValue = (string)s;
                    property.serializedObject.ApplyModifiedProperties();
                }

                var LabelForShowInEUI = new Rect(position.x + 200, position.y + 60, 130, position.height - (customHeight));
                var ShowInEUIRect = new Rect(position.x + 5 + 315, position.y + 60, 100, position.height - (customHeight));

                EditorGUI.LabelField(LabelForShowInEUI, "Show in effects UI?:", EditorStyles.boldLabel);
                EditorGUI.PropertyField(ShowInEUIRect, property.FindPropertyRelative("showInEffectsUI"), GUIContent.none);

                property.serializedObject.ApplyModifiedProperties();
            }

            var LabelFoldout = new Rect(position.x + 5, position.y, 100, position.height - (customHeight));
            var FoldoutRect = new Rect(position.x + 5 + 80, position.y, 135, position.height - (customHeight));

            property.isExpanded = EditorGUI.Foldout(LabelFoldout, property.isExpanded, new GUIContent(Enum.GetName(typeof(ConsumableEffectType), property.FindPropertyRelative("effectType").enumValueIndex)));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return 25f;
            else
                return 90f;
        }

    }
}