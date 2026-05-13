using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.BehaviourTree.Data;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

/// <summary>
/// selectedVariableIndex hierarchy:
/// 
/// 0 = Int
/// 1 = Float
/// 2 = Bool
/// 
/// 
/// </summary>


public class BTWindowInspector : EditorWindow
{
    RPGCK_BT bt;
    Vector2 generalScrollViewPos;

    int selectedVariableIndex = 0;
    string menuDisplayValue = "Int";
    string newVarName = "";
    string errorCode = "";

    public RPGCK_BTEditor btEditor;
    public RckAI curAI;
    public PurposeBTVariablesDictionary curVars;

    public void ShowWindow(RPGCK_BT _behaviourTree, RPGCK_BTEditor curEditor)
    {
        bt = _behaviourTree;
        btEditor = curEditor;
        this.Show();
    }

    private void OnGUI()
    {
        if (btEditor == null)
            this.Close();

        generalScrollViewPos =
                            EditorGUILayout.BeginScrollView(generalScrollViewPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        EditorGUILayout.BeginVertical("box");

        if (btEditor.isDebugging)
        {
            EditorGUILayout.HelpBox("Debugging is active.", MessageType.Info, true);
        }

        GUI.enabled = !btEditor.isDebugging;
        EditorGUILayout.LabelField("Add Variable:", EditorStyles.boldLabel);


        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 15;

        EditorGUILayout.LabelField("Name:");
        newVarName = EditorGUILayout.TextField(newVarName, GUILayout.Width(position.width - 100));
        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Type:");

        if (GUILayout.Button(menuDisplayValue, EditorStyles.miniButtonMid, GUILayout.Width(position.width - 100)))
        {
            // create the menu and add items to it
            GenericMenu varMenu = new GenericMenu();

            varMenu.AddDisabledItem(new GUIContent("Variables:"));

            varMenu.AddSeparator("");

            varMenu.AddItem(new GUIContent("Int"), false, VariablesMenuCallback, 0);
            varMenu.AddItem(new GUIContent("Float"), false, VariablesMenuCallback, 1);
            varMenu.AddItem(new GUIContent("Bool"), false, VariablesMenuCallback, 2);

            varMenu.ShowAsContext();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        GUI.enabled = (newVarName != "");
        if (GUILayout.Button("Add", GUILayout.Width(position.width)))
        {
            errorCode = "";

            if (VariableNameAlreadyExists(newVarName))
            {
                errorCode = "000";
            }
            else
            {
                // Add in dictionary
                AddVariable(newVarName, selectedVariableIndex);
            }

            newVarName = "";
            GUI.FocusControl(null);
        }
        GUI.enabled = true;
        GUI.enabled = !btEditor.isDebugging;
        if (errorCode == "000")
        {
            EditorGUILayout.HelpBox("Variabile name already exists!", MessageType.Error);
        }

        EditorGUILayout.Space(50);

        // Start a code block to check for GUI changes
        if (!btEditor.isDebugging)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Variables:", EditorStyles.boldLabel);
            for (int i = 0; i < bt.graphVariables.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(bt.graphVariables[i].Name);
                if (bt.graphVariables[i] is BT_Int)
                {
                    ((BT_Int)bt.graphVariables[i]).value = EditorGUILayout.DelayedIntField(((BT_Int)bt.graphVariables[i]).value);
                }
                else if (bt.graphVariables[i] is BT_Float)
                {
                    ((BT_Float)bt.graphVariables[i]).value = EditorGUILayout.DelayedFloatField(((BT_Float)bt.graphVariables[i]).value);
                }
                else if (bt.graphVariables[i] is BT_Bool)
                {
                    ((BT_Bool)bt.graphVariables[i]).value = EditorGUILayout.Toggle(((BT_Bool)bt.graphVariables[i]).value);
                }

                if (GUILayout.Button("X", GUILayout.MaxWidth(64)))
                {
                    DestroyImmediate(bt.graphVariables[i], true);
                    bt.graphVariables.RemoveAt(i);
                    EditorUtility.SetDirty(bt);
                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.EndHorizontal();
            }

            // End the code block and update the label if a change occurred
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(bt);
                AssetDatabase.SaveAssets();
            }
        }
        GUI.enabled = true;

        if (btEditor.isDebugging)
        {
            Color col = GUI.color;
            GUI.color = Color.yellow;

            // Draw all the BTVariables of the current BTree
            EditorGUILayout.LabelField("Debug Variables:", EditorStyles.boldLabel);
            for (int i = 0; i < bt.graphVariables.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(bt.graphVariables[i].Name);
                if (curVars[bt.graphVariables[i].guidStr])
                {
                    if (curVars[bt.graphVariables[i].guidStr] is BT_Int)
                    {
                        EditorGUILayout.LabelField(((BT_Int)curVars[bt.graphVariables[i].guidStr]).value.ToString());
                    }
                    else if (curVars[bt.graphVariables[i].guidStr] is BT_Float)
                    {
                        EditorGUILayout.LabelField(((BT_Float)curVars[bt.graphVariables[i].guidStr]).value.ToString());
                    }
                    else if (curVars[bt.graphVariables[i].guidStr] is BT_Bool)
                    {
                        EditorGUILayout.LabelField(((BT_Bool)curVars[bt.graphVariables[i].guidStr]).value.ToString());
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            GUI.color = col;
        }

        GUILayout.FlexibleSpace();

        GUIStyle customButton = new GUIStyle("button");
        customButton.fontSize = 20;

        GUI.enabled = !btEditor.isDebugging;
        if (GUILayout.Button("UPDATE AND SAVE", customButton, GUILayout.MinHeight(100)))
        {
            bt.UpdateChildNodesRef();
            EditorUtility.SetDirty(bt);
            AssetDatabase.SaveAssets();
        }
        GUI.enabled = true;

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    private void OnDestroy()
    {
        if (bt != null)
        {
            EditorUtility.SetDirty(bt);
            AssetDatabase.SaveAssets();
        }
    }


    private void AddVariable(string varName, int varIndex)
    {
        switch (varIndex)
        {
            case 0:
                BT_Int newInt = ScriptableObject.CreateInstance<BT_Int>();

                newInt.value = 0;
                newInt.Name = varName;
                newInt.name = varName;
                AssetDatabase.AddObjectToAsset(newInt, AssetDatabase.GetAssetPath(bt));
                bt.graphVariables.Add(newInt);

                EditorUtility.SetDirty(newInt);
                EditorUtility.SetDirty(bt);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                break;

            case 1:
                BT_Float newFloat = ScriptableObject.CreateInstance<BT_Float>();

                newFloat.value = 0;
                newFloat.Name = varName;
                newFloat.name = varName;
                AssetDatabase.AddObjectToAsset(newFloat, AssetDatabase.GetAssetPath(bt));
                bt.graphVariables.Add(newFloat);

                EditorUtility.SetDirty(newFloat);
                EditorUtility.SetDirty(bt);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                break;

            case 2:
                BT_Bool newBool = ScriptableObject.CreateInstance<BT_Bool>();

                newBool.value = false;
                newBool.Name = varName;
                newBool.name = varName;
                AssetDatabase.AddObjectToAsset(newBool, AssetDatabase.GetAssetPath(bt));
                bt.graphVariables.Add(newBool);

                EditorUtility.SetDirty(newBool);
                EditorUtility.SetDirty(bt);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                break;
        }
    }

    private bool VariableNameAlreadyExists(string newVarName)
    {
        return false;
    }

    void VariablesMenuCallback(object _val)
    {
        // 0 = Int
        // 1 = Float
        // 2 = Bool
        int val = (int)_val;

        switch (val)
        {
            case 0:
                menuDisplayValue = "Int";
                selectedVariableIndex = 0;
                break;

            case 1:
                menuDisplayValue = "Float";
                selectedVariableIndex = 1;
                break;

            case 2:
                menuDisplayValue = "Bool";
                selectedVariableIndex = 2;
                break;

        }
    }

    public void OnInspectorUpdate()
    {
        Repaint();
    }

    public void InitDebug(RckAI cur, bool debuggingCombat)
    {
        curAI = cur;
        curVars = debuggingCombat ? curAI.btCombatVarData : curAI.btPurposeVarData;
    }

    public void StopDebug()
    {
        curAI = null;
        curVars = null;
    }


}
