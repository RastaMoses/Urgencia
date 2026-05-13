using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;
using UnityEditor.SceneManagement;

namespace RPGCreationKit
{
    public class BookItem_Window : ItemWindow
    {
        public bool isReady = false;

        public SerializedObject itemObj = null;

        SerializedProperty itemID;
        SerializedProperty itemName;
        SerializedProperty itemIcon;
        SerializedProperty itemWeight;
        SerializedProperty itemValue;

        SerializedProperty usesTooltip;
        SerializedProperty tooltipValue;

        SerializedProperty itemQuestItem;
        SerializedProperty isCumulable;

        SerializedProperty itemInWorld;

        SerializedProperty openedCoverSprite;
        SerializedProperty BookText;

        SerializedProperty isNoteOrScroll;
        SerializedProperty CantBeTaken;

        SerializedProperty itemScript;

        GameObject gameObject;
        Editor gameObjectEditor;
        bool gameObjectChanged = false;

        public override void Init(SerializedObject _item)
        {
            base.Init(_item);
            // Windows is created from 'Configure' button of the Inspector of the Item

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.BookItemWindowIcon);
            GUIContent titleContent = new GUIContent("BookItem", icon);
            this.titleContent = titleContent;

            // We copy the Item SerializedObject to not lose reference.
            SerializedObject itemcopy = new SerializedObject(_item.targetObject);
            itemObj = itemcopy;

            itemID = itemObj.FindProperty("ItemID");
            itemName = itemObj.FindProperty("ItemName");
            itemIcon = itemObj.FindProperty("ItemIcon");
            itemWeight = itemObj.FindProperty("Weight");
            itemValue = itemObj.FindProperty("Value");

            usesTooltip = itemObj.FindProperty("usesTooltip");
            tooltipValue = itemObj.FindProperty("tooltipValue");

            itemQuestItem = itemObj.FindProperty("QuestItem");
            isCumulable = itemObj.FindProperty("isCumulable");

            openedCoverSprite = itemObj.FindProperty("openedCoverSprite");
            BookText = itemObj.FindProperty("BookText");

            isNoteOrScroll = itemObj.FindProperty("isNoteOrScroll");
            CantBeTaken = itemObj.FindProperty("CantBeTaken");

            itemInWorld = itemObj.FindProperty("itemInWorld");

            itemScript = itemObj.FindProperty("itemScript");

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
            EditorGUILayout.LabelField("Item In World: ", GUILayout.ExpandWidth(false));
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


            EditorGUILayout.BeginHorizontal();

            // Vertical of properties
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(225f));

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));

            EditorGUIUtility.labelWidth = 35f;

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


            // Cover 2D UI

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("2D UI", "The sprite that represent the book that will be displayed when the book is opened."), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(openedCoverSprite, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            //ItemScript
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Item Script", "Script to run when something related this item happens."), GUILayout.ExpandWidth(false));

            string menuDisplayValue = string.IsNullOrEmpty(itemScript.stringValue) ? "-None-" : itemScript.stringValue;
            if (GUILayout.Button(new GUIContent(menuDisplayValue, menuDisplayValue), GUILayout.MaxWidth(110)))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                menu.AddDisabledItem(new GUIContent("Item Scripts"));

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("None"), false, Callback, new ScriptElementData(itemScript, ""));
                for (int i = 0; i < allItemScripts.Length; i++)
                {
                    menu.AddItem(new GUIContent(allItemScripts[i].name), (allItemScripts[i].GetClass().Namespace + "." + allItemScripts[i].name) == menuDisplayValue, Callback, new ScriptElementData(itemScript, allItemScripts[i].GetClass().Namespace + "." + allItemScripts[i].name));
                }

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();

            // UseTooltip
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Use Tooltip?", "Do you need a tooltip for this Item?"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(usesTooltip, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Tooltip Text
            if (!usesTooltip.boolValue)
                GUI.enabled = false;

            EditorGUIUtility.labelWidth = 197f;

            EditorGUILayout.LabelField(new GUIContent("Tooltip Text", "The text to display in the tooltip"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(tooltipValue, GUIContent.none, GUILayout.ExpandWidth(false));

            GUI.enabled = true;

            EditorGUILayout.Space();


            // End vertical of properties
            EditorGUILayout.EndVertical();



            EditorGUIUtility.labelWidth = 150f;


            // RIGHT VALUES
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Book Text:", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(BookText, GUIContent.none, GUILayout.MaxHeight(205f));


            // End vertical of properties
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = 130f;

            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(itemQuestItem);
            EditorGUILayout.PropertyField(isCumulable);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(isNoteOrScroll, new GUIContent("Is Note/Scroll?", "Select this option to display the Text on a single page instead of the default Book UI."));
            EditorGUILayout.PropertyField(CantBeTaken);
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
            GUILayout.Space(7.5f);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("OK", "Save changes and close the Window"), ButtonStyle))
            {
                itemObj.ApplyModifiedProperties();
                this.Close();

                Selection.objects = new Object[0];
            }

            if (GUILayout.Button(new GUIContent("Cancel", "Cancel changes and close the Window"), ButtonStyle))
            {
                this.Close();

                Selection.objects = new Object[0];
            }

            GUILayout.EndHorizontal();

        }

        private void OnDestroy()
        {
            for (int i = 0; i < childWindows.Count; i++)
            {
                childWindows[i].Close();
            }
        }

    }
}