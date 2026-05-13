using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;
using UnityEditor.SceneManagement;

namespace RPGCreationKit
{
    public class ConfigBlendshapes_Window : EditorWindow
    {
        public bool isReady = false;

        public SerializedObject itemObj = null;

        public bool editingMale = true;

        public SerializedProperty Name;

        public SerializedProperty male_Blendshapes;
        public SerializedProperty female_Blendshapes;

        string FirstLabel = "";

        public void Init(SerializedObject _item, bool _isMale)
        {
            // Windows is created from 'Configure' button of the Inspector of the Item

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.ArmorItemWindowIcon);
            GUIContent titleContent = new GUIContent("Body Options", icon);
            this.titleContent = titleContent;

            // We copy the Item SerializedObject to not lose reference.
            //SerializedObject itemcopy = new SerializedObject(_item.targetObject);
            itemObj = _item;

            editingMale = _isMale;

            male_Blendshapes = itemObj.FindProperty("Male_Blendshapes");
            female_Blendshapes = itemObj.FindProperty("Female_Blendshapes");
            Name = itemObj.FindProperty("ItemName");

            FirstLabel = "Configuring Body Options of the item: " + Name.stringValue;
            FirstLabel = (_isMale) ? FirstLabel + " (Male)" : FirstLabel + " (Female)";

            isReady = true;
            this.Show();
        }



        Vector2 scrollPos;
        private void OnGUI()
        {

            //vertical space
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(FirstLabel, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Utilize this window to create/remove modifiers for the Body Options for the Character model.", EditorStyles.textArea);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("HIDE BODY PARTS", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            itemObj.FindProperty("hideHead").boolValue = EditorGUILayout.Toggle("Hide Head?", itemObj.FindProperty("hideHead").boolValue, GUILayout.ExpandWidth(false));
            itemObj.FindProperty("hideUpperbody").boolValue = EditorGUILayout.Toggle("Hide Upperbody?", itemObj.FindProperty("hideUpperbody").boolValue, GUILayout.ExpandWidth(false));
            itemObj.FindProperty("hideArms").boolValue = EditorGUILayout.Toggle("Hide Arms?", itemObj.FindProperty("hideArms").boolValue, GUILayout.ExpandWidth(false));
            itemObj.FindProperty("hideHands").boolValue = EditorGUILayout.Toggle("Hide Hands?", itemObj.FindProperty("hideHands").boolValue, GUILayout.ExpandWidth(false));
            itemObj.FindProperty("hideLegs").boolValue = EditorGUILayout.Toggle("Hide Legs?", itemObj.FindProperty("hideLegs").boolValue, GUILayout.ExpandWidth(false));
            itemObj.FindProperty("hideFeet").boolValue = EditorGUILayout.Toggle("Hide Feet?", itemObj.FindProperty("hideFeet").boolValue, GUILayout.ExpandWidth(false));




            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Add button
            if (GUILayout.Button("Add new Blendshape", GUILayout.ExpandWidth(false)))
            {
                if (editingMale)
                    male_Blendshapes.arraySize++;
                else
                    female_Blendshapes.arraySize++;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

            if (editingMale)
                // Draw properties for male
                for (int i = 0; i < male_Blendshapes.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(male_Blendshapes.GetArrayElementAtIndex(i), true);
                    if(male_Blendshapes.GetArrayElementAtIndex(i).isExpanded)
                        if(GUILayout.Button("Remove"))
                            male_Blendshapes.DeleteArrayElementAtIndex(i);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }
            else
                // Draw properties for female
                for (int i = 0; i < female_Blendshapes.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(female_Blendshapes.GetArrayElementAtIndex(i), true);
                    if (female_Blendshapes.GetArrayElementAtIndex(i).isExpanded)
                        if (GUILayout.Button("Remove"))
                            female_Blendshapes.DeleteArrayElementAtIndex(i);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }


            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Close"))
                this.Close();
        }

    }
}
