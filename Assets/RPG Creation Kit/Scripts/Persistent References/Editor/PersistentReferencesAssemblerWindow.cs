using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using System;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using RPGCreationKit.PersistentReferences;

namespace RPGCreationKit.RCKEditor
{
    /// <summary>
    /// Window that allows to assemble and correctly setup the persistent references among all the scenes you choose to reference.
    /// </summary>
    public class PersistentReferencesAssemblerWindow : EditorWindow
    {
        [SerializeField]
        SerializedObject sObject;

        bool assembleWorldspace = true;

        Worldspace worldspaceToAssemble;

        [SerializeField]
        ReorderableList scenesToAssembleList;


        [SerializeField]
        List<SceneReferenceLite> scenesToAssemble;

        Vector2 scenesScrollView;
        SerializedProperty property;

        static PersistentReferenceManager editorPRefManager;


        [MenuItem("RPG Creation Kit/Persistent References Assembler Window")]
        private static void OpenWindow()
        {
            PersistentReferencesAssemblerWindow window = GetWindow<PersistentReferencesAssemblerWindow>();

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RPGCKEditorWindowIcon);

            GUIContent titleContent = new GUIContent("PRefs Assembler", icon);
            window.titleContent = titleContent;
        }

        private void Awake()
        {
            scenesToAssemble = new List<SceneReferenceLite>();

            ScriptableObject target = this;
            sObject = new SerializedObject(target);
            property = sObject.FindProperty("scenesToAssemble");

            scenesToAssembleList = new ReorderableList(sObject, sObject.FindProperty("scenesToAssemble"), true, true, true, true);
            scenesToAssembleList.list = scenesToAssemble;
        }


        private void OnGUI()
        {
            GUIStyle TitleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            TitleStyle.fontSize = 20;
            TitleStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            ButtonStyle.border = new RectOffset(2, 2, 2, 2);
            ButtonStyle.fontSize = 16;
            ButtonStyle.font = (Font)AssetDatabase.LoadAssetAtPath<Font>("Assets/RPG Creation Kit/Fonts/monofonto.ttf");

            if (sObject == null)
            {
                ScriptableObject target = this;
                sObject = new SerializedObject(target);
                property = sObject.FindProperty("scenesToAssemble");
            }

            if (scenesToAssembleList == null)
            {
                scenesToAssembleList = new ReorderableList(sObject, sObject.FindProperty("scenesToAssemble"), true, true, true, true);
                scenesToAssembleList.list = scenesToAssemble;
            }

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Persistent References Assembler", TitleStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("This window allows you to assemble and correctly setup the persistent references among all the scenes you choose to reference.", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal("box", GUILayout.MaxWidth(50));
            EditorGUILayout.LabelField("Assemble: ", GUILayout.MaxWidth(65));

            if (GUILayout.Button(new GUIContent((assembleWorldspace) ? "Worldspace" : "Scenes"), GUILayout.MaxWidth(120)))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Worldspace"), false, Callback, true);
                menu.AddItem(new GUIContent("Individual Scenes"), false, Callback, false);

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();

            if (assembleWorldspace)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Add from worldspace:");
                worldspaceToAssemble = (Worldspace)EditorGUILayout.ObjectField(worldspaceToAssemble, typeof(Worldspace), false, GUILayout.MaxWidth(250));

                GUI.enabled = (worldspaceToAssemble != null);

                if (GUILayout.Button("Add scenes", GUILayout.ExpandWidth(false)))
                {
                    for (int i = 0; i < worldspaceToAssemble.cells.Length; i++)
                    {
                        scenesToAssembleList.list.Add((SceneReferenceLite)worldspaceToAssemble.cells[i].sceneRef);
                    }

                    sObject.Update();
                }


                EditorGUILayout.EndHorizontal();
            }

            GUI.enabled = true;

            if (GUILayout.Button("Add Current Scene", GUILayout.ExpandWidth(false)))
            {
                scenesToAssembleList.list.Add(EditorSceneManager.GetActiveScene().GetCellInformation().cell.sceneRef);
                sObject.Update();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            scenesScrollView =
                            EditorGUILayout.BeginScrollView(scenesScrollView, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.MaxHeight(300.0f));

            scenesToAssembleList.DoLayoutList();
            scenesToAssembleList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Scenes To Assemble");

            };

            scenesToAssembleList.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = property.GetArrayElementAtIndex(index);
                rect.y += 2;

                EditorGUI.LabelField(rect, "Scene: " + index.ToString());

                rect.x += 65;
                rect.xMax -= 65;
                EditorGUI.PropertyField(
                    rect,
                    element, GUIContent.none);
            };

            sObject.ApplyModifiedProperties();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear All", GUILayout.ExpandWidth(false)))
                ClearScenesToAssemble();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(30);

            if (GUILayout.Button("ASSEMBLE ALL", ButtonStyle))
                AssembleAll();
        }

        private void AssembleAll()
        {
            if (scenesToAssembleList.list.Count <= 0)
                EditorUtility.DisplayDialog("Persistent References Assembler", "You haven't assigned any scenes to assemble.", "Close");
            else
            {
                if (EditorUtility.DisplayDialog("Persistent References Assembler", "Are you sure to assemble " + scenesToAssembleList.list.Count + " scenes? Assembling scenes may take several minutes, depending on the number of scenes, their complexity and the number of Persistent References in them.", "Assemble", "Cancel"))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        // Loads the _PersistentReferences_ scene
                        string[] guidsPR = AssetDatabase.FindAssets("t:scene _PersistentReferences_");
                        var pRefScene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guidsPR[0]), OpenSceneMode.Single);

                        // Get a reference to the PersistentReferenceManager
                        editorPRefManager = FindObjectOfType<PersistentReferenceManager>();

                        if (editorPRefManager == null)
                        {
                            // Abort
                            Debug.LogError("No prefmanager");
                            return;
                        }

                        for (int i = 0; i < scenesToAssemble.Count; i++)
                        {
                            EditorUtility.DisplayProgressBar("Assembling Persistent References...", "Working on " + scenesToAssemble[i].SceneName, ((float)i) / (float)scenesToAssemble.Count);

                            // If reference is not assigned, skip.
                            if (scenesToAssemble[i] == null)
                                continue;

                            // Load the current scene
                            var loadedScene = EditorSceneManager.OpenScene(scenesToAssemble[i].ScenePath, OpenSceneMode.Additive);
                            EditorSceneManager.SetActiveScene(loadedScene);
                            PersistentReference[] references = FindObjectsOfType<PersistentReference>();

                            Debug.Log("Assembled " + references.Length + " references.");

                            for (int j = 0; j < references.Length; j++)
                            {
                                editorPRefManager.RegisterPersistentReference(references[j]);
                            }

                            EditorSceneManager.MarkSceneDirty(loadedScene);

                            EditorSceneManager.SaveScene(loadedScene);
                            EditorSceneManager.CloseScene(loadedScene, true);
                        }

                        EditorUtility.ClearProgressBar();
                        EditorSceneManager.MarkSceneDirty(pRefScene);
                        EditorSceneManager.SaveScene(pRefScene);
                    }
                }
            }
        }

        private void ClearScenesToAssemble()
        {
            scenesToAssembleList.list.Clear();
            sObject.Update();
        }

        private void Callback(object _val)
        {
            assembleWorldspace = (bool)_val;
        }
    }
}