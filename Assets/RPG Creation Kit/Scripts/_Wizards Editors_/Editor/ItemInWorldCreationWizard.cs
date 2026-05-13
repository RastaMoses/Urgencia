using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using RPGCreationKit;
using UnityEditor.SceneManagement;

namespace RPGCreationKit
{
    public class ItemInWorldCreationWizard : EditorWindow
    {
        [SerializeField] GameObject itemGameObject;

        private string itemName = "New Item InWorld";

        private enum ColliderType { BoxCollider = 0, CapsuleCollider = 1, SphereCollider = 2, MeshColliderConvex = 3, MeshCollider = 4 };

        private ColliderType physicalCollider = ColliderType.MeshColliderConvex;
        private ColliderType triggerCollider = ColliderType.BoxCollider;

        private Item itemRef;
        private int amount = 1;

        private bool saveInAssetFolder = false;
        public string savePath = "Assets/RPG Creation Kit/";


        GameObject gameObjectInWindow;
        Editor gameObjectEditor;
        bool gameObjectInWindowChanged = false;

        [MenuItem("RPG Creation Kit/Create New/Item InWorld")]
        public static void ShowWindow()
        {
            EditorWindow thisWindow = GetWindow(typeof(ItemInWorldCreationWizard));
            thisWindow.titleContent = new GUIContent("Item InWorld Creation");
            thisWindow.minSize = new Vector2(755, 545);
            thisWindow.maxSize = new Vector2(755, 545);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUIStyle TitleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            TitleStyle.fontSize = 16;
            TitleStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            GUIStyle HeaderStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            HeaderStyle.fontSize = 14;
            HeaderStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            ButtonStyle.border = new RectOffset(2, 2, 2, 2);
            ButtonStyle.fontSize = 16;
            ButtonStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");


            GUIStyle bgColor = new GUIStyle();
            bgColor.normal.background = Texture2D.blackTexture;

            // Draw Window
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("ITEM IN WORLD WIZARD", TitleStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Welcome to the Item InWorld Creation Wizard. Here you're able to create your Items InWorld GameObjects with few clicks. You can start by filling\nthe information below, the system will take care of the rest.", MessageType.Info);

            GUILayout.Space(50);

            EditorGUIUtility.labelWidth = 135;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(400));

            EditorGUILayout.LabelField("SETTINGS:", HeaderStyle);

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("GameObject:");
            EditorGUI.BeginChangeCheck();

            itemGameObject = (GameObject)EditorGUILayout.ObjectField(itemGameObject, typeof(GameObject), true, GUILayout.MaxWidth(250));

            if (EditorGUI.EndChangeCheck())
            {
                // Update 
                gameObjectInWindow = (GameObject)itemGameObject;
                gameObjectInWindowChanged = true;

                if (itemGameObject != null)
                    itemName = itemGameObject.name;
                else
                    itemName = "New Item InWorld";

            }
            EditorGUILayout.EndHorizontal();

            GUI.enabled = (itemGameObject != null ? true : false);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Item Name:");
            itemName = EditorGUILayout.TextField(itemName, GUILayout.MaxWidth(250));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Physical Collider Type:");
            physicalCollider = (ColliderType)EditorGUILayout.EnumPopup(physicalCollider);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Trigger Collider Type:");
            triggerCollider = (ColliderType)EditorGUILayout.EnumPopup(triggerCollider);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Item Ref:");
            itemRef = (Item)EditorGUILayout.ObjectField(itemRef, typeof(Item), false, GUILayout.MaxWidth(250));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Amount:");
            amount = EditorGUILayout.DelayedIntField(amount, GUILayout.MaxWidth(250));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Save as Prefab?");
            saveInAssetFolder = EditorGUILayout.Toggle(saveInAssetFolder);
            EditorGUILayout.EndHorizontal();

            bool previousGUIState = GUI.enabled;

            if (!saveInAssetFolder) GUI.enabled = false;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Path:");
            savePath = EditorGUILayout.TextArea(savePath, GUILayout.MaxWidth(250));
            EditorGUILayout.EndHorizontal();

            GUI.enabled = previousGUIState;

            EditorGUILayout.EndVertical();


            // Draw Model Preview
            if (itemGameObject != null)
            {
                gameObjectInWindow = (GameObject)itemGameObject;

                if (gameObjectInWindow != null)
                {
                    if (gameObjectEditor == null || gameObjectInWindowChanged)
                    {
                        gameObjectEditor = Editor.CreateEditor(gameObjectInWindow);
                        gameObjectInWindowChanged = false;
                    }

                    gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(228, 228, 228, 228), bgColor);
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(120);

            EditorGUILayout.BeginHorizontal();

            previousGUIState = GUI.enabled;

            GUI.enabled = true;
            if (GUILayout.Button("CANCEL", ButtonStyle))
            {
                this.Close();
            }
            GUI.enabled = previousGUIState;

            if (itemGameObject == null || itemRef == null)
                GUI.enabled = false;

            if (GUILayout.Button("COMPLETE & RESET", ButtonStyle))
            {
                Complete();
                EditorUtility.DisplayDialog("Item InWorld Creation Wizard", "Your new Item InWorld has been successfully created!", "OK!");
                Reset();
            }

            if (GUILayout.Button("COMPLETE & CLOSE", ButtonStyle))
            {
                Complete();
                EditorUtility.DisplayDialog("Item InWorld Creation Wizard", "Your new Item InWorld has been successfully created!", "OK!");
                this.Close();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void Complete()
        {
            // Set Tag
            itemGameObject.tag = "RPG Creation Kit/ItemInWorld";

            // Add Rigidbody
            itemGameObject.AddComponent<Rigidbody>();

            // Add Colliders
            Collider col = null;

            switch (physicalCollider)
            {
                case ColliderType.BoxCollider:
                    itemGameObject.AddComponent<BoxCollider>();
                    break;
                case ColliderType.CapsuleCollider:
                    itemGameObject.AddComponent<CapsuleCollider>();
                    break;
                case ColliderType.MeshCollider:
                    itemGameObject.AddComponent<MeshCollider>();
                    break;
                case ColliderType.MeshColliderConvex:
                    var meshCol = itemGameObject.AddComponent<MeshCollider>();
                    meshCol.convex = true;
                    break;
                case ColliderType.SphereCollider:
                    itemGameObject.AddComponent<SphereCollider>();
                    break;
            }

            switch (triggerCollider)
            {
                case ColliderType.BoxCollider:
                    col = itemGameObject.AddComponent<BoxCollider>();
                    col.isTrigger = true;
                    break;
                case ColliderType.CapsuleCollider:
                    col = itemGameObject.AddComponent<CapsuleCollider>();
                    col.isTrigger = true;
                    break;
                case ColliderType.MeshCollider:
                    col = itemGameObject.AddComponent<MeshCollider>();
                    col.isTrigger = true;
                    break;
                case ColliderType.MeshColliderConvex:
                    var meshCol = itemGameObject.AddComponent<MeshCollider>();
                    meshCol.convex = true;
                    meshCol.isTrigger = true;
                    break;
                case ColliderType.SphereCollider:
                    col = itemGameObject.AddComponent<SphereCollider>();
                    col.isTrigger = true;
                    break;
            }

            // Add script
            ItemInWorld inw = itemGameObject.AddComponent<ItemInWorld>();
            inw.Item = itemRef;
            inw.Amount = amount;

            itemGameObject.name = itemName;

            // Item is ready
            if (saveInAssetFolder)
            {
                if (!savePath.Contains("Assets"))
                {
                    Debug.Log("The specified path: " + savePath + " is not valid. Saving the Prefab in 'Assets' folder...'");

                    savePath = "Assets/";
                }

                string finalPath = savePath + itemName + ".prefab";
                finalPath = AssetDatabase.GenerateUniqueAssetPath(finalPath);

                bool successfullSave = false;
                PrefabUtility.SaveAsPrefabAsset(itemGameObject, finalPath, out successfullSave);

                if (!successfullSave)
                    Debug.LogError("Could not save the created ItemInWorld " + itemName + ". The provided save path was: '" + savePath + "' and probably incorrect. Paths should start with 'Assets\\...");
            }

            EditorSceneManager.MarkAllScenesDirty();
        }

        private void Reset()
        {
            itemGameObject = null;
            itemName = "New Item InWorld";

            physicalCollider = ColliderType.MeshColliderConvex;
            triggerCollider = ColliderType.BoxCollider;

            itemRef = null;
            amount = 1;
        }
    }
}