using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;
using UnityEditor.SceneManagement;

namespace RPGCreationKit
{
    public class ArmorItem_Window : ItemWindow
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

        ConfigBlendshapes_Window blendshapesWindow;
        SerializedProperty male_Blendshapes;
        SerializedProperty female_Blendshapes;

        SerializedProperty bipeds;
        SerializedProperty weightType;
        SerializedProperty health;
        SerializedProperty armorRating;
        SerializedProperty hideAmulet;
        SerializedProperty hideRings;
        SerializedProperty hideHair;
        SerializedProperty useMesh;

        SerializedProperty hideHead;
        SerializedProperty hideUpperbody;
        SerializedProperty hideArms;
        SerializedProperty hideHands;
        SerializedProperty hideLegs;
        SerializedProperty hideFeet;


        SerializedProperty isStaticObject;

        SerializedProperty maleBipedModel;
        SerializedProperty maleWorldModel;
        SerializedProperty femaleBipedModel;
        SerializedProperty femaleWorldModel;
        SerializedProperty maleStaticObject;
        SerializedProperty femaleStaticObject;


        SerializedProperty useFirstPersonModel;
        SerializedProperty fpMaleModel;
        SerializedProperty fpFemaleModel;

        SerializedProperty itemScript;

        GameObject gameObjectMale;
        GameObject gameObjectFemale;

        Editor gameObjectMaleEditor;
        Editor gameObjectFemaleEditor;

        bool gameObjectMaleChanged = false;
        bool gameObjectFemaleChanged = false;


        // For Toggles of BipedObject
        bool useHead = false;
        bool useUpperBody = false;
        bool useLowerBody = false;
        bool useHand = false;
        bool useFoot = false;
        bool useRightRing = false;
        bool useLeftRing = false;
        bool useAmulet = false;
        bool useShield = false;
        bool useTorch = false;

        SerializedProperty blockingMultiplier;
        SerializedProperty staminaDrainMultiplier;

        public override void Init(SerializedObject _item)
        {
            base.Init(_item);
            // Windows is created from 'Configure' button of the Inspector of the Item

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.ArmorItemWindowIcon);
            GUIContent titleContent = new GUIContent("ArmorItem", icon);
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

            bipeds = itemObj.FindProperty("Bipeds");
            weightType = itemObj.FindProperty("WeightType");
            health = itemObj.FindProperty("Health");
            armorRating = itemObj.FindProperty("ArmorRating");
            blockingMultiplier = itemObj.FindProperty("blockingMultiplier");
            staminaDrainMultiplier = itemObj.FindProperty("staminaDrainMultiplier");
            hideAmulet = itemObj.FindProperty("HideAmulet");
            hideRings = itemObj.FindProperty("HideRings");
            hideHair = itemObj.FindProperty("HideHair");
            useMesh = itemObj.FindProperty("useMesh");

            hideHead = itemObj.FindProperty("hideHead");
            hideUpperbody = itemObj.FindProperty("hideUpperbody");
            hideArms = itemObj.FindProperty("hideArms");
            hideHands = itemObj.FindProperty("hideHands");
            hideLegs = itemObj.FindProperty("hideLegs");
            hideFeet = itemObj.FindProperty("hideFeet");

            isStaticObject = itemObj.FindProperty("isStaticObject");

            maleBipedModel = itemObj.FindProperty("MaleBipedModel");
            maleWorldModel = itemObj.FindProperty("itemInWorld");
            femaleBipedModel = itemObj.FindProperty("FemaleBipedModel");
            femaleWorldModel = itemObj.FindProperty("itemInWorld");
            maleStaticObject = itemObj.FindProperty("maleStaticObject");
            femaleStaticObject = itemObj.FindProperty("femaleStaticObject");


            useFirstPersonModel = itemObj.FindProperty("useFirstPersonModel");
            fpMaleModel = itemObj.FindProperty("fpMaleModel");
            fpFemaleModel = itemObj.FindProperty("fpFemaleModel");

            itemScript = itemObj.FindProperty("itemScript");

            // Initialize Biped Object values
            for(int i = 0; i < bipeds.arraySize; i++)
            {
                if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.Head) useHead = true;
                if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.UpperBody) useUpperBody = true;
                if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.LowerBody) useLowerBody = true;
                if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.Hand) useHand = true;
                if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.Foot) useFoot = true;
                if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.RightRing) useRightRing = true;
                if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.LeftRing) useLeftRing = true;
                if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.Amulet) useAmulet = true;
                if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.Shield) useShield = true;
                if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.Torch) useTorch = true;
            }

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

            EditorGUI.DrawTextureTransparent(new Rect(55, 90, 100, 100), s.texture);
            // End Draw Sprite

            GUILayout.Space(85);

            EditorGUILayout.BeginHorizontal();

            // Model Field
            EditorGUIUtility.labelWidth = 85f;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Male", EditorStyles.boldLabel);

            if (!useAmulet && !useShield && !useLeftRing && !useRightRing && !useTorch)
            {
                EditorGUILayout.LabelField("Male Biped Model: ", GUILayout.ExpandWidth(false));

                // Model + Preview Button
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(maleBipedModel, GUIContent.none, GUILayout.ExpandWidth(false));
                if (GUILayout.Button("Body Options", GUILayout.ExpandWidth(false)))
                {

                    if (maleBipedModel.objectReferenceValue)
                    {
                        if (blendshapesWindow != null) blendshapesWindow.Close();

                        blendshapesWindow = CreateInstance<ConfigBlendshapes_Window>();
                        blendshapesWindow.minSize = new Vector2(500, 569);
                        blendshapesWindow.maxSize = new Vector2(500, 569);
                        blendshapesWindow.Init(itemObj, true);
                    }
                }
                EditorGUILayout.EndHorizontal();
            } else
            {
                EditorGUILayout.LabelField("Male Static Model: ", GUILayout.ExpandWidth(false));

                // Model + Preview Button
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(maleStaticObject, GUIContent.none, GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();
            }
            // End Model + Preview Button

            if(useUpperBody)
            {
                EditorGUILayout.LabelField("Different FP model?", GUILayout.ExpandWidth(false));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(useFirstPersonModel, GUIContent.none, GUILayout.ExpandWidth(false));

                if(useFirstPersonModel.boolValue)
                    EditorGUILayout.PropertyField(fpMaleModel, GUIContent.none, GUILayout.ExpandWidth(false));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.LabelField("Male World Model: ", GUILayout.ExpandWidth(false));

            // Model + Preview Button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(maleWorldModel, GUIContent.none, GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Preview", GUILayout.ExpandWidth(false)))
            {
                if (maleWorldModel.objectReferenceValue)
                {
                    // Draw Model Preview
                    gameObjectMale = (GameObject)maleWorldModel.objectReferenceValue;
                    gameObjectMaleChanged = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            // End Model + Preview Button

            // Preview for Male
            GUIStyle bgColor = new GUIStyle();
            bgColor.normal.background = Texture2D.blackTexture;

            if (gameObjectMale != null)
            {
                if (gameObjectMaleEditor == null || gameObjectMaleChanged)
                {
                    gameObjectMaleEditor = Editor.CreateEditor(gameObjectMale);
                    gameObjectMaleChanged = false;
                }

                gameObjectMaleEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(64, 128), bgColor);
            }
            else
            {
                GUILayout.Space(128);
            }
            // End preview for male

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Female", EditorStyles.boldLabel);

            if (!useAmulet && !useShield && !useLeftRing && !useRightRing)
            {
                EditorGUILayout.LabelField("Female Biped Model: ", GUILayout.ExpandWidth(false));

                // Model + Preview Button
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(femaleBipedModel, GUIContent.none, GUILayout.ExpandWidth(false));
                if (GUILayout.Button("Body Options", GUILayout.ExpandWidth(false)))
                {
                    if (femaleBipedModel.objectReferenceValue)
                    {
                        if (blendshapesWindow != null) blendshapesWindow.Close();

                        blendshapesWindow = CreateInstance<ConfigBlendshapes_Window>();
                        blendshapesWindow.minSize = new Vector2(500, 569);
                        blendshapesWindow.maxSize = new Vector2(500, 569);
                        blendshapesWindow.Init(itemObj, false);
                    }
                }
                EditorGUILayout.EndHorizontal();
            } else
            {
                EditorGUILayout.LabelField("Female Static Model: ", GUILayout.ExpandWidth(false));

                // Model + Preview Button
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(femaleStaticObject, GUIContent.none, GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();
            }

            // End Model + Preview Button

            if (useUpperBody)
            {
                EditorGUILayout.LabelField("Different FP model?", GUILayout.ExpandWidth(false));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(useFirstPersonModel, GUIContent.none, GUILayout.ExpandWidth(false));

                if (useFirstPersonModel.boolValue)
                    EditorGUILayout.PropertyField(fpMaleModel, GUIContent.none, GUILayout.ExpandWidth(false));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.LabelField("Female World Model: ", GUILayout.ExpandWidth(false));

            // Model + Preview Button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(femaleWorldModel, GUIContent.none, GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Preview", GUILayout.ExpandWidth(false)))
            {
                if(femaleWorldModel.objectReferenceValue)
                {
                    // Draw Model Preview
                    gameObjectFemale = (GameObject)femaleWorldModel.objectReferenceValue;
                    gameObjectFemaleChanged = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            // End Model + Preview Button

            // Preview for female
            // GUIStyle bgColor = new GUIStyle();
            bgColor.normal.background = Texture2D.blackTexture;

            if (gameObjectFemale != null)
            {
                if (gameObjectFemaleEditor == null || gameObjectFemaleChanged)
                {
                    gameObjectFemaleEditor = Editor.CreateEditor(gameObjectFemale);
                    gameObjectFemaleChanged = false;
                }

                gameObjectFemaleEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(64, 128), bgColor);
            }
            else
            {
                GUILayout.Space(128);
            }


            // End preview for male

            EditorGUILayout.EndVertical();


            // End Model field
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();


            // End model Preview


            GUILayout.Space(35);


            EditorGUIUtility.labelWidth = 35f;

            EditorGUILayout.BeginHorizontal();
            // Vertical of properties

            EditorGUIUtility.labelWidth = 55f;

            EditorGUILayout.BeginVertical("box", GUILayout.Width(230));

            EditorGUILayout.LabelField("ArmorItem", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));

            // ID
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("ID: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemID, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);

            // Name
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(new GUIContent("Name", "In-Game Name of the Item"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemName, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);

            // Weight
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(new GUIContent("Weight", "The weight of the Item in the Inventory"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemWeight, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);

            // Weight Type
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(new GUIContent("Armor Type", "Heavy/Light ArmorItem"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(weightType, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);

            // Health
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(new GUIContent("Health", "The duration of the ArmorItem"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(health, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);

            // ArmorRating
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(new GUIContent("Armor Rating", "How much defence against physical attacks"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(armorRating, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);

            // Value
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(new GUIContent("Value", "Value (in Golds) of the Item"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemValue, GUIContent.none, GUILayout.ExpandWidth(false));
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
                    menu.AddItem(new GUIContent(allItemScripts[i].name), false, Callback, new ScriptElementData(itemScript, allItemScripts[i].GetClass().Namespace + "." + allItemScripts[i].name));
                }

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Biped Object", EditorStyles.boldLabel);

            EditorGUIUtility.labelWidth = 100f;
            EditorGUI.BeginChangeCheck();
            useHead = EditorGUILayout.Toggle("Head", useHead, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                if(useHead)
                {
                    int prevSize = bipeds.arraySize;

                    bipeds.InsertArrayElementAtIndex(prevSize);
                    bipeds.GetArrayElementAtIndex(prevSize).enumValueIndex = (int)BipedObject.Head;
                }
                else
                {
                    for(int i = 0; i < bipeds.arraySize; i++)
                    {
                        if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.Head)
                            bipeds.DeleteArrayElementAtIndex(i);
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            useUpperBody = EditorGUILayout.Toggle("UpperBody", useUpperBody, GUILayout.ExpandWidth(false));
            if(EditorGUI.EndChangeCheck())
            {
                if (useUpperBody)
                {
                    int prevSize = bipeds.arraySize;

                    bipeds.InsertArrayElementAtIndex(prevSize);
                    bipeds.GetArrayElementAtIndex(prevSize).enumValueIndex = (int)BipedObject.UpperBody;
                }
                else
                {
                    for (int i = 0; i < bipeds.arraySize; i++)
                    {
                        if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.UpperBody)
                            bipeds.DeleteArrayElementAtIndex(i);
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            useLowerBody = EditorGUILayout.Toggle("LowerBody", useLowerBody, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                if (useLowerBody)
                {
                    int prevSize = bipeds.arraySize;

                    bipeds.InsertArrayElementAtIndex(prevSize);
                    bipeds.GetArrayElementAtIndex(prevSize).enumValueIndex = (int)BipedObject.LowerBody;
                }
                else
                {
                    for (int i = 0; i < bipeds.arraySize; i++)
                    {
                        if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.LowerBody)
                            bipeds.DeleteArrayElementAtIndex(i);
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            useHand = EditorGUILayout.Toggle("Hand", useHand, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                if (useHand)
                {
                    int prevSize = bipeds.arraySize;

                    bipeds.InsertArrayElementAtIndex(prevSize);
                    bipeds.GetArrayElementAtIndex(prevSize).enumValueIndex = (int)BipedObject.Hand;
                }
                else
                {
                    for (int i = 0; i < bipeds.arraySize; i++)
                    {
                        if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.Hand)
                            bipeds.DeleteArrayElementAtIndex(i);
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            useFoot = EditorGUILayout.Toggle("Foot", useFoot, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                if (useFoot)
                {
                    int prevSize = bipeds.arraySize;

                    bipeds.InsertArrayElementAtIndex(prevSize);
                    bipeds.GetArrayElementAtIndex(prevSize).enumValueIndex = (int)BipedObject.Foot;
                }
                else
                {
                    for (int i = 0; i < bipeds.arraySize; i++)
                    {
                        if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.Foot)
                            bipeds.DeleteArrayElementAtIndex(i);
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            useRightRing = EditorGUILayout.Toggle("RightRing", useRightRing, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                if (useRightRing)
                {
                    int prevSize = bipeds.arraySize;

                    bipeds.InsertArrayElementAtIndex(prevSize);
                    bipeds.GetArrayElementAtIndex(prevSize).enumValueIndex = (int)BipedObject.RightRing;
                }
                else
                {
                    for (int i = 0; i < bipeds.arraySize; i++)
                    {
                        if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.RightRing)
                            bipeds.DeleteArrayElementAtIndex(i);
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            useLeftRing = EditorGUILayout.Toggle("LeftRing", useLeftRing, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                if (useLeftRing)
                {
                    int prevSize = bipeds.arraySize;

                    bipeds.InsertArrayElementAtIndex(prevSize);
                    bipeds.GetArrayElementAtIndex(prevSize).enumValueIndex = (int)BipedObject.LeftRing;
                }
                else
                {
                    for (int i = 0; i < bipeds.arraySize; i++)
                    {
                        if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.LeftRing)
                            bipeds.DeleteArrayElementAtIndex(i);
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            useAmulet = EditorGUILayout.Toggle("Amulet", useAmulet, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                if (useAmulet)
                {
                    int prevSize = bipeds.arraySize;

                    bipeds.InsertArrayElementAtIndex(prevSize);
                    bipeds.GetArrayElementAtIndex(prevSize).enumValueIndex = (int)BipedObject.Amulet;
                }
                else
                {
                    for (int i = 0; i < bipeds.arraySize; i++)
                    {
                        if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.Amulet)
                            bipeds.DeleteArrayElementAtIndex(i);
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            useShield = EditorGUILayout.Toggle("Shield", useShield, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                if (useShield)
                {
                    int prevSize = bipeds.arraySize;

                    bipeds.InsertArrayElementAtIndex(prevSize);
                    bipeds.GetArrayElementAtIndex(prevSize).enumValueIndex = (int)BipedObject.Shield;
                }
                else
                {
                    for (int i = 0; i < bipeds.arraySize; i++)
                    {
                        if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.Shield)
                            bipeds.DeleteArrayElementAtIndex(i);
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            useTorch = EditorGUILayout.Toggle("Torch", useTorch, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                if (useTorch)
                {
                    int prevSize = bipeds.arraySize;

                    bipeds.InsertArrayElementAtIndex(prevSize);
                    bipeds.GetArrayElementAtIndex(prevSize).enumValueIndex = (int)BipedObject.Torch;
                }
                else
                {
                    for (int i = 0; i < bipeds.arraySize; i++)
                    {
                        if (bipeds.GetArrayElementAtIndex(i).enumValueIndex == (int)BipedObject.Torch)
                            bipeds.DeleteArrayElementAtIndex(i);
                    }
                }
            }

            if (useShield)
            {
                EditorGUIUtility.labelWidth = 55f;

                // Blocking Multiplier
                EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField(new GUIContent("Blocking", "How much the block of an incoming attack will absorb the damage."), GUILayout.ExpandWidth(false));
                blockingMultiplier.floatValue = EditorGUILayout.Slider(new GUIContent("BM", "Blocking Multiplier, how much the block of an incoming attack will absorb the damage"), blockingMultiplier.floatValue, 0f, 1f, GUILayout.ExpandWidth(false));
                staminaDrainMultiplier.floatValue = EditorGUILayout.Slider(new GUIContent("SDM", "Stamina Drain Multiplier, how much blocking the damage with this shield will reduce the stamina drain. (Value should be less for shields, higher for weapons)"), staminaDrainMultiplier.floatValue, 0f, 10f, GUILayout.ExpandWidth(false));
                //EditorGUILayout.PropertyField(blockingMultiplier, GUIContent.none, GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = 100f;
            }


            EditorGUILayout.EndVertical();           


            // RIGHT VALUES
            EditorGUILayout.BeginVertical("box", GUILayout.Width(100));
            EditorGUIUtility.labelWidth = 80f;

            EditorGUILayout.LabelField("Constraints", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));


            // Use Mesh
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Use mesh?", "Should this armor item spawn a mesh?"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(useMesh, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);

            // Quest Item
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Quest Item", "Is this ArmorItem needed for a Quest?"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemQuestItem, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);

            // Cumulable
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Is Cumulable", "Is this ArmorItem cumulable?"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(isCumulable, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);

            // Hide Hair
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Hide Hair", "Hide the Character Hair when ArmorItem this is equipped."), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(hideHair, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);

            // Hide Amulet
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Hide Amulet", "Hide the Amulet when this ArmorItem is equipped."), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(hideAmulet, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3);

            // Hide Rings
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Hide Rings", "Hide Rings when this ArmorItem is equipped."), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(hideRings, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // End vertical of properties
            EditorGUILayout.EndVertical();            

            EditorGUIUtility.labelWidth = 80f;

            EditorGUILayout.EndHorizontal();

            GUILayout.ExpandWidth(false);

            EditorGUILayout.BeginHorizontal("box", GUILayout.MaxWidth(250.0f));
            
            // Tooltip
            EditorGUILayout.BeginVertical();
            EditorGUIUtility.labelWidth = 80f;

            EditorGUILayout.LabelField("Others", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));

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

            EditorGUILayout.Space();

            GUILayout.Space(15);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("OK", "Save changes and close the Window"), ButtonStyle))
            {
                isStaticObject.boolValue = (!useAmulet && !useShield && !useLeftRing && !useRightRing && !useTorch) ? false : true;

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
            // If the blendshapes window is opened, close it.
            if(blendshapesWindow != null)
                blendshapesWindow.Close();

            for (int i = 0; i < childWindows.Count; i++)
            {
                childWindows[i].Close();
            }
        }

    }
}