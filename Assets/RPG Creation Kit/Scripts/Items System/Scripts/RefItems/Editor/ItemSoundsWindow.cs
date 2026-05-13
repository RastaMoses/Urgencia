using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class ItemSoundsWindow : EditorWindow
    {
        public ItemWindow childOf;

        SerializedProperty sOnPickUp;
        SerializedProperty sOnDrop;
        SerializedProperty sOnAddedInInventory;
        SerializedProperty sOnRemovedFromInventory;
        SerializedProperty sOnEquipOrUse;

        SerializedProperty sOnDraw;
        SerializedProperty sOnSheathe;

        SerializedObject itemObj;

        Item itemToCopyFrom;

        bool isWeaponItem = false;

        bool isReady = false;
        public virtual void Init(SerializedObject serializedObject, ItemWindow _childOf)
        {
            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.SoundsOfItemWindowIcon);
            GUIContent titleContent = new GUIContent("Item Sounds", icon);
            this.titleContent = titleContent;

            this.childOf = _childOf;

            sOnPickUp = serializedObject.FindProperty("sOnPickUp");
            sOnDrop = serializedObject.FindProperty("sOnDrop");
            sOnAddedInInventory = serializedObject.FindProperty("sOnAddedInInventory");
            sOnRemovedFromInventory = serializedObject.FindProperty("sOnRemovedFromInventory");
            sOnEquipOrUse = serializedObject.FindProperty("sOnEquipOrUse");

            // We copy the Item SerializedObject to not lose reference.
            itemObj = serializedObject;

            if (itemObj.targetObject is WeaponItem)
            {
                sOnDraw = itemObj.FindProperty("sOnDraw");
                sOnSheathe = itemObj.FindProperty("sOnSheathe");
                isWeaponItem = true;
            }

            isReady = true;

            this.Show();
            this.position = new Rect(_childOf.position.center.x,
                                     _childOf.position.center.y, _childOf.position.xMax, _childOf.position.yMax);
        }

        private void OnGUI()
        {
            if (!itemObj.targetObject)
            {
                Debug.LogWarning("ItemObj: NullReferenceException");
                return;
            }

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Configuring Sounds of: " + itemObj.targetObject.name, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("On Pick Up: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(sOnPickUp, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("On Drop: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(sOnDrop, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("On Added In Inventory: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(sOnAddedInInventory, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("On Removed From Inventory: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(sOnRemovedFromInventory, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("On Equip/Use: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(sOnEquipOrUse, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            if (isWeaponItem)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("On Draw: ", GUILayout.ExpandWidth(false));
                EditorGUILayout.PropertyField(sOnDraw, GUIContent.none, GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("On Sheathe: ", GUILayout.ExpandWidth(false));
                EditorGUILayout.PropertyField(sOnSheathe, GUIContent.none, GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10);
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Copy from: ", GUILayout.ExpandWidth(false));
            itemToCopyFrom = EditorGUILayout.ObjectField("", itemToCopyFrom, typeof(Item), false, GUILayout.MaxWidth(125)) as Item;

            if (GUILayout.Button("Apply"))
            {
                sOnPickUp.objectReferenceValue = itemToCopyFrom.sOnPickUp;
                sOnDrop.objectReferenceValue = itemToCopyFrom.sOnDrop;
                sOnAddedInInventory.objectReferenceValue = itemToCopyFrom.sOnAddedInInventory;
                sOnRemovedFromInventory.objectReferenceValue = itemToCopyFrom.sOnRemovedFromInventory;
                sOnEquipOrUse.objectReferenceValue = itemToCopyFrom.sOnEquipOrUse;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("OK", "Close the Window")))
                this.Close();
        }

        private void OnDestroy()
        {
            childOf.soundWinOpened = false;
            for (int i = 0; i < childOf.childWindows.Count; i++)
                if (childOf.childWindows[i] == this)
                    childOf.childWindows.RemoveAt(i);
        }

    }
}