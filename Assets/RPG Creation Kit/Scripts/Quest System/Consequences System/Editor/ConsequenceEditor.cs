using RPGCreationKit;
using RPGCreationKit.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace RPGCreationKit
{

    /// <summary>
    /// Data to hold for the Callback of the menu when selecting a consequence
    /// </summary>
    public struct ConsequenceDrawerData
    {
        public SerializedProperty property; // The property
        public ConsequencesTypes type;      // The type

        // Constructor
        public ConsequenceDrawerData(SerializedProperty _property, ConsequencesTypes _type)
        {
            property = _property;
            type = _type;
        }
    }


    /// <summary>
    /// Display the consequences intelligently in base of the selected Consequence fields
    /// </summary>
    [CustomPropertyDrawer(typeof(Consequence))]
    public class ConsequenceDrawer : PropertyDrawer
    {
        Color consequenceBoxColor = new Color(150, 150, 150, 1f);

        Color unassignedReferenceColor = new Color(1, 0, 0, 0.5f);

        // Callback of Menu (type of consequence)
        void Callback(object obj)
        {
            // Convert the obj to a ConsequenceDrawerData
            ConsequenceDrawerData data = (ConsequenceDrawerData)obj;
            data.property.FindPropertyRelative("type").enumValueIndex = 17;

            // Apply the changes of the type to the property
            Debug.Log(data.property.FindPropertyRelative("type").enumValueIndex + " | " + data.type + " | " + (int)data.type);
            data.property.FindPropertyRelative("type").enumValueIndex = (int)data.type;

            // Save everything
            data.property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(data.property.serializedObject.targetObject);
        }

        // Custom variable Height that will expand in base of the data that we have to show
        float customHeight = 60f;
        float customHeightMultip = 0f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;


            var type = property.FindPropertyRelative("type");

            EditorGUIUtility.labelWidth = 105f;

            if (type.enumValueIndex == (int)ConsequencesTypes.ChangeScene)
            {
                customHeight = 60f;

                // Find properties
                var sceneIndex = property.FindPropertyRelative("sceneIndex");

                EditorGUI.DrawRect(new Rect(position.x, position.y, position.width, 40), consequenceBoxColor);


                // Create rects
                var LabelForSceneIndex = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var SceneIndexField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                // Draw properties
                EditorGUI.LabelField(LabelForSceneIndex, "Scene index:");
                EditorGUI.PropertyField(SceneIndexField, sceneIndex, GUIContent.none);

            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.Teleport)
            {
                customHeight = 60f;
                customHeightMultip = 0;

                // Find the properties
                var array = property.FindPropertyRelative("toTeleport");
                int size = property.FindPropertyRelative("toTeleport").arraySize;


                // If the array is expanded we need more space for the consequence
                if (array.isExpanded)
                    customHeight += (20 * size);

                int expandedFields = 0;
                if (array.isExpanded)
                {
                    for (int i = 0; i < size; i++)
                    {
                        if (array.GetArrayElementAtIndex(i).isExpanded)
                        {
                            customHeight += 60;
                            expandedFields++;
                        }
                    }
                }

                // Overall box
                EditorGUI.DrawRect(new Rect(position.x, position.y, position.width, 40 + (array.isExpanded ? (20 * size) : 0) + (60 * expandedFields)), consequenceBoxColor);

                // Draw the rects for labels & fields
                var gameObjectSize = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - (customHeight + customHeightMultip));
                var LabelForGameObject = new Rect(position.x, position.y + 20, 105, position.height - (customHeight + customHeightMultip));

                int newSize = EditorGUI.DelayedIntField(gameObjectSize, "", size);

                if (!array.isExpanded)
                {
                    for (int i = 0; i < size; i++)
                    {
                        var prop = array.GetArrayElementAtIndex(i);

                        if (property.FindPropertyRelative("toTeleport").GetArrayElementAtIndex(i).FindPropertyRelative("gameObject").objectReferenceValue == null)
                        {
                            EditorGUI.DrawRect(LabelForGameObject, unassignedReferenceColor);
                            break;
                        }
                    }
                }

                // Draw the label
                EditorGUI.PropertyField(LabelForGameObject, array);

                // Draw the size of the array with a DelayedIntField

                // If the size has changed
                if (newSize != size)
                {
                    // Apply in the array
                    array.arraySize = newSize;
                }

                // If  the array is expanded
                if (array.isExpanded)
                {
                    float modifiedPos = 0;
                    // Draw the whole array
                    for (int i = 0; i < newSize; i++)
                    {
                        var gameObjectField = new Rect(position.x, position.y + modifiedPos + 40 + (20 * i), position.width, position.height - (customHeight + customHeightMultip));

                        var prop = array.GetArrayElementAtIndex(i);

                        if (!prop.isExpanded)
                            if (property.FindPropertyRelative("toTeleport").GetArrayElementAtIndex(i).FindPropertyRelative("gameObject").objectReferenceValue == null)
                                EditorGUI.DrawRect(new Rect(position.x, position.y + modifiedPos + 40 + (20 * i), position.width, position.height - (customHeight + customHeightMultip)), unassignedReferenceColor);


                        EditorGUI.PropertyField(gameObjectField, prop, true);

                        if (prop.isExpanded)
                            modifiedPos += 60;
                    }
                }
                else // if the array is not expanded
                {
                    // Do not expand the height as well
                    customHeight = 60f;
                }

            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.AlertMessage)
            {
                customHeight = 60f;

                var AlertMessage = property.FindPropertyRelative("AlertMessage");
                var duration = property.FindPropertyRelative("duration");

                var LabelForAlertMessage = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var AlertMessageField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForAlertMessage, "Alert Message:");
                EditorGUI.PropertyField(AlertMessageField, AlertMessage, GUIContent.none);

                var LabelForduration = new Rect(position.x, position.y + 40, 120, position.height - customHeight);
                var durationField = new Rect(position.x + 105, position.y + 40, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForduration, "Duration (sec):");
                EditorGUI.PropertyField(durationField, duration, GUIContent.none);

            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.AddInInventory)
            {
                customHeight = 105f;

                // Find the properties
                var item = property.FindPropertyRelative("itemToModifyInInventory");
                var itemIsPlayerInventory = property.FindPropertyRelative("itemToModifyInInventory").FindPropertyRelative("isPlayerInventory");
                var itemInventoryID = property.FindPropertyRelative("itemToModifyInInventory").FindPropertyRelative("inventoryID");
                var itemItem = property.FindPropertyRelative("itemToModifyInInventory").FindPropertyRelative("item");
                var itemAmount = property.FindPropertyRelative("itemToModifyInInventory").FindPropertyRelative("amount");

                var LabelForitemIsPlayerInventory = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var itemIsPlayerInventoryField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForitemIsPlayerInventory, "Is Player Inventory?");
                EditorGUI.PropertyField(itemIsPlayerInventoryField, itemIsPlayerInventory, GUIContent.none);

                float space = 0;

                if (!itemIsPlayerInventory.boolValue)
                {
                    space = 20;
                    var LabelForitemInventoryID = new Rect(position.x, position.y + 40, 120, position.height - customHeight);
                    var inventoryIDField = new Rect(position.x + 105, position.y + 40, position.width - 110, position.height - customHeight);

                    EditorGUI.LabelField(LabelForitemInventoryID, "Inventory ID:");
                    EditorGUI.PropertyField(inventoryIDField, itemInventoryID, GUIContent.none);
                }

                var LabelForLabelForitemItem = new Rect(position.x, position.y + 40 + space, 120, position.height - customHeight);
                var LabelForitemItemield = new Rect(position.x + 105, position.y + 40 + space, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForLabelForitemItem, "Item:");
                EditorGUI.PropertyField(LabelForitemItemield, itemItem, GUIContent.none);

                var LabelForLabelForitemAmount = new Rect(position.x, position.y + 60 + space, 120, position.height - customHeight);
                var LabelForitemAmountield = new Rect(position.x + 105, position.y + 60 + space, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForLabelForitemAmount, "Amount:");
                EditorGUI.PropertyField(LabelForitemAmountield, itemAmount, GUIContent.none);
            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.RemoveFromInventory)
            {
                customHeight = 105f;

                // Find the properties
                var item = property.FindPropertyRelative("itemToModifyInInventory");
                var itemIsPlayerInventory = property.FindPropertyRelative("itemToModifyInInventory").FindPropertyRelative("isPlayerInventory");
                var itemInventoryID = property.FindPropertyRelative("itemToModifyInInventory").FindPropertyRelative("inventoryID");
                var itemItem = property.FindPropertyRelative("itemToModifyInInventory").FindPropertyRelative("item");
                var itemAmount = property.FindPropertyRelative("itemToModifyInInventory").FindPropertyRelative("amount");

                var LabelForitemIsPlayerInventory = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var itemIsPlayerInventoryField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForitemIsPlayerInventory, "Is Player Inventory?");
                EditorGUI.PropertyField(itemIsPlayerInventoryField, itemIsPlayerInventory, GUIContent.none);

                float space = 0;

                if (!itemIsPlayerInventory.boolValue)
                {
                    space = 20;
                    var LabelForitemInventoryID = new Rect(position.x, position.y + 40, 120, position.height - customHeight);
                    var inventoryIDField = new Rect(position.x + 105, position.y + 40, position.width - 110, position.height - customHeight);

                    EditorGUI.LabelField(LabelForitemInventoryID, "Inventory ID:");
                    EditorGUI.PropertyField(inventoryIDField, itemInventoryID, GUIContent.none);
                }

                var LabelForLabelForitemItem = new Rect(position.x, position.y + 40 + space, 120, position.height - customHeight);
                var LabelForitemItemield = new Rect(position.x + 105, position.y + 40 + space, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForLabelForitemItem, "Item:");
                EditorGUI.PropertyField(LabelForitemItemield, itemItem, GUIContent.none);

                var LabelForLabelForitemAmount = new Rect(position.x, position.y + 60 + space, 120, position.height - customHeight);
                var LabelForitemAmountield = new Rect(position.x + 105, position.y + 60 + space, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForLabelForitemAmount, "Amount:");
                EditorGUI.PropertyField(LabelForitemAmountield, itemAmount, GUIContent.none);


                // Draw the label
            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.RemoveAllFromInventory)
            {
                customHeight = 80f;

                // Find properties
                var IsPlayerInventory = property.FindPropertyRelative("isPlayerInventory");
                var Inventory = property.FindPropertyRelative("inventory");

                float usingIsPlayerSpace = 0;


                if (!IsPlayerInventory.boolValue)
                {
                    customHeight += 20;
                    usingIsPlayerSpace = 20;
                }
                else
                {
                    customHeight = 80;
                    usingIsPlayerSpace = 0;
                }

                // Create rects
                var isPlayerLabel = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var isPlayerField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                // Draw properties
                EditorGUI.LabelField(isPlayerLabel, "Is Player?");
                EditorGUI.PropertyField(isPlayerField, IsPlayerInventory, GUIContent.none);

                // Create rects
                var LabelForInventory = new Rect(position.x, position.y + 20 + usingIsPlayerSpace, 120, position.height - customHeight);
                var InventoryField = new Rect(position.x + 105, position.y + 20 + usingIsPlayerSpace, position.width - 110, position.height - customHeight);


                if (!IsPlayerInventory.boolValue)
                {
                    // Draw properties
                    EditorGUI.LabelField(LabelForInventory, "Inventory:");
                    EditorGUI.PropertyField(InventoryField, Inventory, GUIContent.none);
                }
            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.MutateMutable)
            {
                customHeight = 60f;

                var mutableID = property.FindPropertyRelative("mutableID");
                var restoreMutable = property.FindPropertyRelative("restoreMutable");

                var LabelForMutableID = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var MutableIDField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForMutableID, "Mutable ID:");
                EditorGUI.PropertyField(MutableIDField, mutableID, GUIContent.none);

                var LabelForrestoreMutable = new Rect(position.x, position.y + 40, 120, position.height - customHeight);
                var restoreMutableField = new Rect(position.x + 105, position.y + 40, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForrestoreMutable, "Restore Mutable?");
                EditorGUI.PropertyField(restoreMutableField, restoreMutable, GUIContent.none);

            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.ChangeRckAIDialogue)
            {
                customHeight = 60f;

                var rckAIID = property.FindPropertyRelative("rckAID");
                var dialogueGraph = property.FindPropertyRelative("dialogueGraph");

                var LabelForRckAI = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var RckAIIDField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForRckAI, "RckAI ID:");
                EditorGUI.PropertyField(RckAIIDField, rckAIID, GUIContent.none);

                var LabelFordialogueGraph = new Rect(position.x, position.y + 40, 120, position.height - customHeight);
                var dialogueGraphField = new Rect(position.x + 105, position.y + 40, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelFordialogueGraph, "Dialogue Graph:");
                EditorGUI.PropertyField(dialogueGraphField, dialogueGraph, GUIContent.none);

            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.ChangeRckBTree)
            {
                customHeight = 60f;

                var rckAIID = property.FindPropertyRelative("rckAID");
                var behaviourTree = property.FindPropertyRelative("behaviourTree");
                var immediatlySwitchToBehaviour = property.FindPropertyRelative("immediatlySwitchToBehaviour");

                
                var LabelForRckAI = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var RckAIIDField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForRckAI, "RckAI ID:");
                EditorGUI.PropertyField(RckAIIDField, rckAIID, GUIContent.none);

                var LabelForbehaviourTree = new Rect(position.x, position.y + 40, 120, position.height - customHeight);
                var dbehaviourTreeField = new Rect(position.x + 105, position.y + 40, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForbehaviourTree, "BTree:");
                EditorGUI.PropertyField(dbehaviourTreeField, behaviourTree, GUIContent.none);

                var LabelForimmediatlySwitchToBehaviour = new Rect(position.x, position.y + 60, 120, position.height - customHeight);
                var immediatlySwitchToBehaviourField = new Rect(position.x + 105, position.y + 60, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForimmediatlySwitchToBehaviour, "Use Immediatly?");
                EditorGUI.PropertyField(immediatlySwitchToBehaviourField, immediatlySwitchToBehaviour, GUIContent.none);

            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.RckAI_AddInFaction ||
                     type.enumValueIndex == (int)ConsequencesTypes.RckAI_RemoveFromFaction)
            {
                customHeight = 60f;

                var rckAIID = property.FindPropertyRelative("rckAID");
                var factionID = property.FindPropertyRelative("factionID");


                var LabelForRckAI = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var RckAIIDField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForRckAI, "RckAI ID:");
                EditorGUI.PropertyField(RckAIIDField, rckAIID, GUIContent.none);

                var LabelForbehaviourTree = new Rect(position.x, position.y + 40, 120, position.height - customHeight);
                var dbehaviourTreeField = new Rect(position.x + 105, position.y + 40, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForbehaviourTree, "Faction ID:");
                EditorGUI.PropertyField(dbehaviourTreeField, factionID, GUIContent.none);
            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.LockDoor)
            {
                customHeight = 60f;

                var doorID = property.FindPropertyRelative("doorID");
                var lockLevel = property.FindPropertyRelative("lockLevel");


                var LabelForDoorID = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var DoorIDField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForDoorID, "Door ID:");
                EditorGUI.PropertyField(DoorIDField, doorID, GUIContent.none);

                var LabelForLockLevel = new Rect(position.x, position.y + 40, 120, position.height - customHeight);
                var lockLevelField = new Rect(position.x + 105, position.y + 40, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForLockLevel, "Lock Level ID:");
                EditorGUI.PropertyField(lockLevelField, lockLevel, GUIContent.none);
            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.UnlockDoor)
            {
                customHeight = 60f;

                var doorID = property.FindPropertyRelative("doorID");


                var LabelForDoorID = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var DoorIDField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForDoorID, "Door ID:");
                EditorGUI.PropertyField(DoorIDField, doorID, GUIContent.none);
            } 
            else if (type.enumValueIndex == (int)ConsequencesTypes.ClearPurpose)
            {
                customHeight = 60f;

                var rckAIID = property.FindPropertyRelative("rckAID");

                var LabelForRckAI = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var RckAIIDField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForRckAI, "RckAI ID:");
                EditorGUI.PropertyField(RckAIIDField, rckAIID, GUIContent.none);
            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.RckAI_ClearMainTarget)
            {
                customHeight = 60f;

                var rckAIID = property.FindPropertyRelative("rckAID");

                var LabelForRckAI = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var RckAIIDField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForRckAI, "RckAI ID:");
                EditorGUI.PropertyField(RckAIIDField, rckAIID, GUIContent.none);
            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.PlayerLearnSpell)
            {
                customHeight = 60f;

                var rckAIID = property.FindPropertyRelative("spellToLearn");

                var LabelForSpellToLearn = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var SpellToLearnField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForSpellToLearn, "Spell TO Learn:");
                EditorGUI.PropertyField(SpellToLearnField, rckAIID, GUIContent.none);
            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.ForceEnterInCombatAgainstPlayer)
            {
                customHeight = 60f;

                var rckAIID = property.FindPropertyRelative("rckAID");

                var LabelForRckAI = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var RckAIIDField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForRckAI, "RckAI ID:");
                EditorGUI.PropertyField(RckAIIDField, rckAIID, GUIContent.none);
            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.RCKAI_SpeakOneLine)
            {
                customHeight = 70f;

                var rckAIID = property.FindPropertyRelative("rckAID");
                var lineID = property.FindPropertyRelative("speakLineID");
                var displayAsHeard = property.FindPropertyRelative("speakLineDisplayAsHeard");

                var LabelForRckAI = new Rect(position.x, position.y + 20, 120, position.height - customHeight);
                var RckAIIDField = new Rect(position.x + 105, position.y + 20, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForRckAI, "RckAI ID:");
                EditorGUI.PropertyField(RckAIIDField, rckAIID, GUIContent.none);

                var LabelForLineID = new Rect(position.x, position.y + 40, 120, position.height - customHeight);
                var LineIDField = new Rect(position.x + 105, position.y + 40, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForLineID, "Line ID:");
                EditorGUI.PropertyField(LineIDField, lineID, GUIContent.none);

                var LabelForDisplayAsHeard = new Rect(position.x, position.y + 60, 120, position.height - customHeight);
                var DisplayAsHeardField = new Rect(position.x + 105, position.y + 60, position.width - 110, position.height - customHeight);

                EditorGUI.LabelField(LabelForDisplayAsHeard, "Display as Heard:");
                EditorGUI.PropertyField(DisplayAsHeardField, displayAsHeard, GUIContent.none);

            }
            // At the end when we've corrected the custom height, we draw the menu


            var LabelForType = new Rect(position.x, position.y, 120, position.height - (customHeight + customHeightMultip));
            var TypeRect = new Rect(position.x + 105, position.y, position.width - 110, position.height - (customHeight + customHeightMultip));

            Event currentEvent = Event.current;

            EditorGUI.LabelField(LabelForType, "Consequence:", EditorStyles.boldLabel);

            GUI.enabled = false;
            EditorGUI.PropertyField(TypeRect, type, GUIContent.none);
            GUI.enabled = true;


            if (currentEvent.type == EventType.MouseDown)
            {
                Vector2 mousePos = currentEvent.mousePosition;
                if (TypeRect.Contains(mousePos))
                {
                    // Now we create the menu, add items and show it
                    GenericMenu menu = new GenericMenu();
                    menu.AddDisabledItem(new GUIContent("Select a consequence:"));


                    menu.AddItem(new GUIContent("Inventory/Add In Inventory"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.AddInInventory));
                    menu.AddItem(new GUIContent("Inventory/Remove From Inventory"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.RemoveFromInventory));
                    menu.AddItem(new GUIContent("Inventory/Remove All From Inventory"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.RemoveAllFromInventory));

                    menu.AddItem(new GUIContent("Others/Alert Message"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.AlertMessage));

                    menu.AddItem(new GUIContent("Others/Lock Door"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.LockDoor));
                    menu.AddItem(new GUIContent("Others/Unlock Door"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.UnlockDoor));
                    menu.AddItem(new GUIContent("Others/Change Scene"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.ChangeScene));

                    menu.AddItem(new GUIContent("Spells/Add Spell To Player"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.PlayerLearnSpell));

                    menu.AddItem(new GUIContent("RckAI/Change Dialogue"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.ChangeRckAIDialogue));
                    menu.AddItem(new GUIContent("RckAI/Change Behaviour"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.ChangeRckBTree));
                    menu.AddItem(new GUIContent("RckAI/Add In Faction"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.RckAI_AddInFaction));
                    menu.AddItem(new GUIContent("RckAI/Remove From Faction"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.RckAI_RemoveFromFaction));
                    menu.AddItem(new GUIContent("RckAI/Clear Purpose"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.ClearPurpose));
                    menu.AddItem(new GUIContent("RckAI/Clear MainTarget"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.RckAI_ClearMainTarget));
                    menu.AddItem(new GUIContent("RckAI/Force Enter Combat against Player"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.ForceEnterInCombatAgainstPlayer));
                    menu.AddItem(new GUIContent("RckAI/Speak Line"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.RCKAI_SpeakOneLine));

                    menu.AddItem(new GUIContent("Mutables/Mutate"), false, Callback, new ConsequenceDrawerData(property, ConsequencesTypes.MutateMutable));

                    menu.ShowAsContext();
                }
            }
        }


        /// <summary>
        /// Manage the Height in base of our values
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var type = property.FindPropertyRelative("type");

            if (type.enumValueIndex == (int)ConsequencesTypes.Teleport)
            {
                var array = property.FindPropertyRelative("toTeleport");

                float tempHeight = 75f;

                if (array.isExpanded)
                {
                    tempHeight += 20 * array.arraySize;

                    for (int i = 0; i < array.arraySize; i++)
                    {
                        if (array.GetArrayElementAtIndex(i).isExpanded)
                            tempHeight += 60;
                    }
                }

                return tempHeight;
            }

            else if (type.enumValueIndex == (int)ConsequencesTypes.AddInInventory ||
                     type.enumValueIndex == (int)ConsequencesTypes.RemoveFromInventory)
            {
                var array = property.FindPropertyRelative("itemsToModifyInInventory");

                float tempHeight = 120f;


                return tempHeight;
            }
            else if (type.enumValueIndex == (int)ConsequencesTypes.RemoveAllFromInventory)
            {
                float tempHeight = 95f;

                var isPlayer = property.FindPropertyRelative("isPlayerInventory");

                if (!isPlayer.boolValue)
                    tempHeight += 20;

                return tempHeight;
            }
            else if(type.enumValueIndex == (int)ConsequencesTypes.RCKAI_SpeakOneLine)
            {
                return 85f;
            }

            // default
            return 75f;
        }

    }
}