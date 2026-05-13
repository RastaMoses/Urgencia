using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using RPGCreationKit;
using UnityEditor.SceneManagement;

namespace RPGCreationKit
{
    /// <summary>
    /// Draws the LootingPoint Creation Wizard
    /// </summary>
    public class LootingPointCreationWizard : EditorWindow
    {

        private enum CurrentWindow { FirstPage = 0, SecondPage = 1, ThirdPage = 2 };
        private enum LootPointColliderType { BoxCollider = 0, CapsuleCollider = 1, SphereCollider = 2, MeshCollider = 3}

        static CurrentWindow m_Window = CurrentWindow.FirstPage;

        public UnityEngine.Object lootPointModel;

        // ---------- First Page Data ----------\\
        public string lootPointID = "LootPointID";

        public string gameObjectName = "New GameObject";
        public string lootPointName = "New LootPoint";

        public bool applyColliders = true;
        static LootPointColliderType colliderType;

        public Vector3 startPos = Vector3.zero;
        public Vector3 startRot = Vector3.zero;
        static Vector3 startScale = new Vector3(1,1,1);

        // ---------- Second Page Data ----------\\
        public List<ItemInInventory> inventory = new List<ItemInInventory>();

        public static SerializedObject itemObj;

        Vector2 itemsScrollPos;


        // ---------- Third Page Data ----------\\
        public bool spawnInScene = true;
        public bool saveInAssetFolder = false;

        public string savePath = "Assets/RPG Creation Kit/";

        // ---------- END Third Page Data ----------\\

        GameObject gameObjectInWindow;
        Editor gameObjectEditor;
        bool gameObjectInWindowChanged = false;

        [MenuItem("RPG Creation Kit/Create New/Looting Point")]
        public static void ShowWindow()
        {
            EditorWindow thisWindow = GetWindow(typeof(LootingPointCreationWizard));
            thisWindow.titleContent = new GUIContent("Looting Point Creation");
            thisWindow.minSize = new Vector2(855, 545);
            thisWindow.maxSize = new Vector2(855, 545);

            m_Window = CurrentWindow.FirstPage;

            itemObj = new SerializedObject(ScriptableObject.CreateInstance<LootingPointCreationWizard>());
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
            ButtonStyle.border = new RectOffset(2,2,2,2);
            ButtonStyle.fontSize = 16;
            ButtonStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");


            GUIStyle bgColor = new GUIStyle();
            bgColor.normal.background = Texture2D.blackTexture;


            switch (m_Window)
            {
                case CurrentWindow.FirstPage:

                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField("LOOTING POINT WIZARD", TitleStyle);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.HelpBox("Welcome to the Looting Point Creation Wizard. Here you're able to create your Looting Point GameObjects with few clicks. You can start by filling\nthe information below, the system will take care of the rest.", MessageType.Info);

                    GUILayout.Space(50);

                    EditorGUIUtility.labelWidth = 125;

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(400));

                    EditorGUILayout.LabelField("SETTINGS:", HeaderStyle);

                    GUILayout.Space(10);


                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("LootPoint ID:");
                    lootPointID = EditorGUILayout.TextField(lootPointID, GUILayout.MaxWidth(250));
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("GameObject Name:");
                    gameObjectName = EditorGUILayout.TextField(gameObjectName, GUILayout.MaxWidth(250));
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Loot Point Name:");
                    lootPointName = EditorGUILayout.TextField(lootPointName, GUILayout.MaxWidth(250));
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Model:");
                    EditorGUI.BeginChangeCheck();

                    lootPointModel = EditorGUILayout.ObjectField(lootPointModel, typeof(GameObject), false, GUILayout.MaxWidth(250));

                    if (EditorGUI.EndChangeCheck())
                    {
                        // Update 
                        gameObjectInWindow = (GameObject)lootPointModel;
                        gameObjectInWindowChanged = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(20);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Apply Collider?");
                    applyColliders = EditorGUILayout.Toggle(applyColliders);
                    EditorGUILayout.EndHorizontal();

                    if (!applyColliders)
                    {
                        EditorGUILayout.HelpBox("A Non-Trigger collider is required in order to interact with LootPoints. Remember to add it later or apply it from there.", MessageType.Warning);
                        GUI.enabled = false;
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Collider Type:");
                    colliderType = (LootPointColliderType)EditorGUILayout.EnumPopup(colliderType);
                    EditorGUILayout.EndHorizontal();

                    GUI.enabled = true;

                    GUILayout.Space(20);

                    EditorGUILayout.LabelField("Transform:", HeaderStyle);

                    EditorGUIUtility.labelWidth = 80;

                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Position:");
                    startPos = EditorGUILayout.Vector3Field("", startPos);
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Rotation:");
                    startRot = EditorGUILayout.Vector3Field("", startRot);
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Scale:");
                    startScale = EditorGUILayout.Vector3Field("", startScale);
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    EditorGUILayout.EndVertical();


                    // Draw Model Preview
                    if (lootPointModel != null)
                    {
                        gameObjectInWindow = (GameObject)lootPointModel;                        

                        if (gameObjectInWindow != null)
                        {
                            if (gameObjectEditor == null || gameObjectInWindowChanged)
                            {
                                gameObjectEditor = Editor.CreateEditor(gameObjectInWindow);
                                gameObjectInWindowChanged = false;
                            }

                            gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(248, 248, 248, 248), bgColor);
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    if(applyColliders)
                        GUILayout.Space(80);
                    else
                        GUILayout.Space(37);

                    EditorGUILayout.BeginHorizontal();

                    GUI.enabled = false;
                    if (GUILayout.Button("PREVIOUS", ButtonStyle))
                    {

                    }
                    GUI.enabled = true;


                    if (lootPointModel == null)
                        GUI.enabled = false;

                    if (GUILayout.Button("NEXT", ButtonStyle))
                    {
                        m_Window = CurrentWindow.SecondPage;
                    }

                    GUI.enabled = true;

                    EditorGUILayout.EndHorizontal();

                    break;

                case CurrentWindow.SecondPage:

                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField("LOOTING POINT WIZARD", TitleStyle);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.HelpBox("Setup the Inventory of this LootPoint. Remember that you're able to add, delete and modify its content anytime in the Inspector of the LootPoint.", MessageType.Info);
                    GUILayout.Space(50);

                    EditorGUIUtility.labelWidth = 125;

                    EditorGUILayout.BeginHorizontal();
                    // Draw Model Preview

                    if (lootPointModel != null)
                    {
                        gameObjectInWindow = (GameObject)lootPointModel;

                        if (gameObjectInWindow != null)
                        {
                            if (gameObjectEditor == null || gameObjectInWindowChanged)
                            {
                                gameObjectEditor = Editor.CreateEditor(gameObjectInWindow);
                                gameObjectInWindowChanged = false;
                            }

                            gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(248, 248, 248, 248), bgColor);
                        }
                    }

                    // --

                    EditorGUILayout.BeginVertical("box");

                    itemsScrollPos =
                        EditorGUILayout.BeginScrollView(itemsScrollPos,GUILayout.Height(340));

                    EditorGUILayout.PropertyField(itemObj.FindProperty("inventory"), true);

                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(23);


                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("PREVIOUS", ButtonStyle))
                    {
                        m_Window = CurrentWindow.FirstPage;
                    }

                    if (GUILayout.Button("NEXT", ButtonStyle))
                    {
                        m_Window = CurrentWindow.ThirdPage;
                    }

                    EditorGUILayout.EndHorizontal();

                    break;

                case CurrentWindow.ThirdPage:

                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField("LOOTING POINT WIZARD", TitleStyle);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    


                    GUILayout.Space(5);

                    bool warningAlert = false;

                    if (itemObj.FindProperty("inventory").arraySize == 0)
                    {
                        EditorGUILayout.HelpBox("The Inventory of this new LootPoint is empty.", MessageType.Warning);
                        warningAlert = true;
                    }

                    bool errorAlert = false;

                    for (int i = 0; i < itemObj.FindProperty("inventory").arraySize; i++)
                    {
                        if(itemObj.FindProperty("inventory").GetArrayElementAtIndex(i).FindPropertyRelative("item").objectReferenceValue == null && !errorAlert)
                        {
                            EditorGUILayout.HelpBox("The Inventory of this new LootPoint contains an element with an empty Item. This can lead to errors during runtime.", MessageType.Error);
                            errorAlert = true;
                        }
                    }

                    if (!errorAlert)
                    {
                        EditorGUILayout.HelpBox("Your LootPoint has no errors or mismatch.", MessageType.Info);

                        EditorGUILayout.HelpBox("You've completed the creation of your new LootPoint. Select where to save it.", MessageType.Info);
                    }

                    GUILayout.Space(50);


                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Spawn In Current Scene?");
                    spawnInScene = EditorGUILayout.Toggle(spawnInScene);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Save as Prefab?");
                    saveInAssetFolder = EditorGUILayout.Toggle(saveInAssetFolder);
                    EditorGUILayout.EndHorizontal();

                    if (!saveInAssetFolder) GUI.enabled = false;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Path:");
                    savePath = EditorGUILayout.TextArea(savePath, GUILayout.MaxWidth(250));
                    EditorGUILayout.EndHorizontal();

                    GUI.enabled = true;

                    EditorGUILayout.EndVertical();

                    GUILayout.Space(220);

                    if(errorAlert)
                        GUILayout.Space(85);

                    if(!warningAlert && !errorAlert)
                        GUILayout.Space(40);


                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("PREVIOUS", ButtonStyle))
                    {
                        m_Window = CurrentWindow.SecondPage;
                    }

                    if (errorAlert || !spawnInScene && !saveInAssetFolder)
                        GUI.enabled = false;

                    if (GUILayout.Button("COMPLETE", ButtonStyle))
                    {
                        // Instantiate GameObject
                        GameObject finalGameObject = Instantiate(lootPointModel) as GameObject;
                        finalGameObject.tag = "RPG Creation Kit/Looting Point";

                        finalGameObject.name = gameObjectName;

                        Inventory finalInventory = finalGameObject.AddComponent<Inventory>();
                        Equipment finalEquipment = finalGameObject.AddComponent<Equipment>();


                        // Apply collider
                        if(applyColliders)
                        {
                            switch (colliderType)
                            {
                                case LootPointColliderType.BoxCollider:
                                    finalGameObject.AddComponent<BoxCollider>();
                                    break;

                                case LootPointColliderType.CapsuleCollider:
                                    finalGameObject.AddComponent<CapsuleCollider>();
                                    break;

                                case LootPointColliderType.SphereCollider:
                                    finalGameObject.AddComponent<SphereCollider>();
                                    break;

                                case LootPointColliderType.MeshCollider:
                                    finalGameObject.AddComponent<MeshCollider>();
                                    break;
                            }
                        }

                        // Apply transform
                        finalGameObject.transform.position = startPos;
                        finalGameObject.transform.rotation = new Quaternion(startRot.x, startRot.y, startRot.z, 0);
                        finalGameObject.transform.localScale = startScale;


                        // Add all items
                        for(int i = 0; i < itemObj.FindProperty("inventory").arraySize; i++)
                        {
                            ItemInInventory newItem = new ItemInInventory();
                            newItem.item = (Item)itemObj.FindProperty("inventory").GetArrayElementAtIndex(i).FindPropertyRelative("item").objectReferenceValue;
                            newItem.Amount = itemObj.FindProperty("inventory").GetArrayElementAtIndex(i).FindPropertyRelative("Amount").intValue;

                            finalInventory.Items.Add(newItem);
                        }


                        LootingPoint finalLootingPoint = finalGameObject.AddComponent<LootingPoint>();

                        finalLootingPoint.ID = lootPointID;
                        finalLootingPoint.pointName = lootPointName;
                        finalLootingPoint.inventory = finalInventory;
                        finalLootingPoint.equipment = finalEquipment;

                        // Item is ready
                        if (saveInAssetFolder)
                        {
                            if (!savePath.Contains("Assets"))
                            {
                                Debug.Log("The specified path: " + savePath + " is not valid. Saving the Prefab in 'Assets' folder...'");

                                savePath = "Assets/";
                            }

                            string finalPath = savePath + gameObjectName + ".prefab";
                            finalPath = AssetDatabase.GenerateUniqueAssetPath(finalPath);

                            bool successfullSave = false;
                            PrefabUtility.SaveAsPrefabAsset(finalGameObject, finalPath, out successfullSave);
                            if (!successfullSave)
                                Debug.LogError("Could not save the created LootingPoint  (ID: '" + lootPointName + "'). The provided save path was: '" + savePath + "' and probably incorrect. Paths should start with 'Assets\\...");
                        }

                        if (!spawnInScene)
                            DestroyImmediate(finalGameObject);

                        EditorSceneManager.MarkAllScenesDirty();

                        this.Close();

                        EditorUtility.DisplayDialog("Looting Point Creation Wizard", "Your new LootPoint has been successfully created!", "OK!");
                    }

                    GUI.enabled = true;

                    EditorGUILayout.EndHorizontal();

                    break;
            }


            if(itemObj.targetObject != null)
                itemObj.ApplyModifiedProperties();

        }
    }

}