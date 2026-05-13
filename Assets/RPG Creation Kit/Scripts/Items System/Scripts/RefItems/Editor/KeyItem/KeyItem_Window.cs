using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;
using UnityEditor.SceneManagement;
using System;

namespace RPGCreationKit
{
    public class KeyItem_Window : ItemWindow
    {
        public bool isReady = false;

        public SerializedObject itemObj = null;

        SerializedProperty itemID;
        SerializedProperty itemName;
        SerializedProperty itemIcon;
        SerializedProperty itemWeight;
        SerializedProperty itemValue;

        SerializedProperty itemQuestItem;
        SerializedProperty isCumulable;

        SerializedProperty onPlayerAddInInventory;
        SerializedProperty itemInWorld;
        SerializedProperty itemScript;

        GameObject gameObject;
        Editor gameObjectEditor;
        bool gameObjectChanged = false;

        public override void Init(SerializedObject _item)
        {
            // Windows is created from 'Configure' button of the Inspector of the Item
            
            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.KeyItemWindowIcon);
            GUIContent titleContent = new GUIContent("KeyItem", icon);
            this.titleContent = titleContent;

            // We copy the Item SerializedObject to not lose reference.
            SerializedObject itemcopy = new SerializedObject(_item.targetObject);
            itemObj = itemcopy;

            itemID = itemObj.FindProperty("ItemID");
            itemName = itemObj.FindProperty("ItemName");
            itemIcon = itemObj.FindProperty("ItemIcon");
            itemWeight = itemObj.FindProperty("Weight");
            itemValue = itemObj.FindProperty("Value");

            itemQuestItem = itemObj.FindProperty("QuestItem");
            isCumulable = itemObj.FindProperty("isCumulable");

            onPlayerAddInInventory = itemObj.FindProperty("onPlayerAddInInventory");

            itemInWorld = itemObj.FindProperty("itemInWorld");

            itemScript = itemObj.FindProperty("itemScript");

            allItemScripts = GetScriptAssetsOfType<ItemScript>();

            isReady = true;
            this.Show();

        }

        void OnGUI()
        {
            if (!itemObj.targetObject)
            {
                Debug.LogWarning("ItemObj: NullReferenceException");
                return;
            }

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Configuring: " + itemObj.targetObject.name, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            // Draw Sprite
            GUILayout.Space(20);

            // Icon Field
            EditorGUILayout.BeginVertical();

            EditorGUIUtility.labelWidth = 50f;
            EditorGUILayout.LabelField("Icon: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemIcon, GUIContent.none, GUILayout.ExpandWidth(false));

            EditorGUILayout.EndVertical();
            // End Icon field


            Sprite s;

            s = (itemIcon.objectReferenceValue) ? itemIcon.objectReferenceValue as Sprite :
                                                  AssetDatabase.LoadAssetAtPath<Sprite>(EditorIconsPath.NoIcon);

            EditorGUI.DrawTextureTransparent(new Rect(105, 90, 100, 100), s.texture);
            // End Draw Sprite

            GUILayout.Space(85);

            EditorGUILayout.BeginVertical();

            // Model Field
            EditorGUIUtility.labelWidth = 50f;
            EditorGUILayout.LabelField("Model: ", GUILayout.ExpandWidth(false));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(itemInWorld, GUIContent.none, GUILayout.ExpandWidth(false));

            if (EditorGUI.EndChangeCheck())
            {
                // Update 
                itemInWorld = itemObj.FindProperty("itemInWorld");
                gameObjectChanged = true;
            }
            // End Model field


            // Draw Model Preview
            gameObject = (GameObject)itemInWorld.objectReferenceValue;

            GUIStyle bgColor = new GUIStyle();
            bgColor.normal.background = Texture2D.blackTexture;

            if (gameObject != null)
            {
                if (gameObjectEditor == null || gameObjectChanged)
                {
                    gameObjectEditor = Editor.CreateEditor(gameObject);
                    gameObjectChanged = false;
                }

                gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(64, 128), bgColor);
            }
            else
            {
                GUILayout.Space(128);
            }

            // End model Preview

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(35);


            EditorGUIUtility.labelWidth = 65f;

            // Vertical of properties
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));

            // ID
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemID, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // Name
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Name", "In-Game Name of the Item"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemName, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Script
            /*
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("ItemScript", "The script"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemScript, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            */
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Item Script", "Script to run when something related this item happens."), GUILayout.ExpandWidth(false));

            string menuDisplayValue = string.IsNullOrEmpty(itemScript.stringValue) ? "-None-" : itemScript.stringValue;
            if (GUILayout.Button(new GUIContent(menuDisplayValue, menuDisplayValue), GUILayout.Width(120)))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                menu.AddDisabledItem(new GUIContent("Item Scripts"));

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("None"), false, Callback, new ScriptElementData(itemScript, ""));
                for (int i = 0; i < allItemScripts.Length; i++)
                {
                    menu.AddItem(new GUIContent(allItemScripts[i].name), false, Callback, new ScriptElementData(itemScript, allItemScripts[i].GetClass().Namespace + "." + allItemScripts[i].name));
                }

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();

            // End vertical of properties
            EditorGUILayout.EndVertical();

            // MIDDLE VALUES
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));

            // Weight
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Weight", "The weight of the Item in the Inventory"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemWeight, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // Value
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Value", "Value (in Golds) of the Item"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemValue, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // End vertical of properties
            EditorGUILayout.EndVertical();


            // RIGHT VALUES
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false));


            // End vertical of properties
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = 80f;

            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(itemQuestItem);
            EditorGUILayout.PropertyField(isCumulable);
            EditorGUILayout.EndVertical();


            if (GUILayout.Button("Configure Sounds"))
            {
                if (soundWinOpened)
                {
                    for (int i = 0; i < childWindows.Count; i++)
                        if (childWindows[i].GetType() == typeof(ItemSoundsWindow))
                        {
                            childWindows[i].Focus();
                            childWindows[i].position = new Rect(this.position.center.x,
                                                                this.position.center.y, this.position.xMax, this.position.yMax);
                        }

                    return;
                }

                //Check if the window wasn't already opened
                ItemSoundsWindow myWindow = CreateInstance<ItemSoundsWindow>();
                myWindow.minSize = new Vector2(400, 220);
                myWindow.maxSize = new Vector2(400, 220);

                myWindow.Init(itemObj, this);

                childWindows.Add(myWindow);

                soundWinOpened = true;
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(25);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("OK", "Save changes and close the Window")))
            {
                itemObj.ApplyModifiedProperties();
                this.Close();

                //Selection.objects = new Object[0];
            }

            if (GUILayout.Button(new GUIContent("Cancel", "Cancel changes and close the Window")))
            {
                this.Close();

                //Selection.objects = new Object[0];
            }

            GUILayout.EndHorizontal();
        }

        private void OnDestroy()
        {
            for(int i = 0; i < childWindows.Count; i++)
            {
                childWindows[i].Close();
            }
        }
    }
}