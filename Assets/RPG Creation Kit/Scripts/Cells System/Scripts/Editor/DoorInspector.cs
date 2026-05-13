using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit.CellsSystem;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;

namespace RPGCreationKit.CellsSystem
{
    [CustomEditor(typeof(Door))]
    public class DoorInspector : Editor
    {
        bool init = false;
        bool changed = false;

        [SerializeField] Door myDoor;
        Texture refreshButtonIcon;

        public override void OnInspectorGUI()
        {
            if(!init)
            {
                refreshButtonIcon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RefreshButton);
                init = true;
            }
            EditorGUIUtility.labelWidth = 140f;

            myDoor = (Door)target;
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            myDoor.objReference = EditorGUILayout.DelayedTextField("Obj Reference ID:", myDoor.objReference);

            EditorGUILayout.Space(5);

            myDoor.persistentReference = EditorGUILayout.Toggle("Persistent Reference", myDoor.persistentReference);

            GUI.enabled = myDoor.persistentReference;
            myDoor.persistentReferenceID = EditorGUILayout.DelayedTextField("Persistent Reference ID:", myDoor.persistentReferenceID);
            GUI.enabled = true;

            EditorGUILayout.Space();

            myDoor.anim = EditorGUILayout.ObjectField("Animation:", myDoor.anim, typeof(Animation), true) as Animation;
            myDoor.groundPivot = EditorGUILayout.ObjectField("Ground Pivot:", myDoor.groundPivot, typeof(Transform), true) as Transform;

            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = 100f;

            EditorGUILayout.LabelField("Door Data", EditorStyles.boldLabel);
            myDoor.locked = EditorGUILayout.Toggle("Locked?", myDoor.locked);

            GUI.enabled = myDoor.locked;
            myDoor.lockLevel = (DoorLockLevel)EditorGUILayout.EnumPopup("Lock Level:", myDoor.lockLevel);
            myDoor.key = EditorGUILayout.ObjectField("Key:", myDoor.key, typeof(KeyItem), false) as KeyItem;
            GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Door Teleport", EditorStyles.boldLabel);
            myDoor.teleports = EditorGUILayout.Toggle("Teleports?", myDoor.teleports);
            GUI.enabled = myDoor.teleports;
            EditorGUI.BeginChangeCheck();
            myDoor.toCell = EditorGUILayout.ObjectField("To Cell:", myDoor.toCell, typeof(Cell), false) as Cell;
            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
            }

            if (myDoor.toCell != null && changed)
            {
                bool hadToLoad = false;
                Scene openedScene = EditorSceneManager.GetActiveScene(); // dummy scene

                Dictionary<string, string> scenes = new Dictionary<string, string>(); 
                for (int i = 0; i < SceneManager.sceneCount; i++)
                    scenes.Add(SceneManager.GetSceneAt(i).name, SceneManager.GetSceneAt(i).path);

                if (!scenes.ContainsKey(myDoor.toCell.sceneRef))
                {
                    hadToLoad = true;
                    openedScene = EditorSceneManager.OpenScene(myDoor.toCell.sceneRef.ScenePath, OpenSceneMode.Additive);
                }

                GameObject[] goArray = SceneManager.GetSceneByPath(myDoor.toCell.sceneRef.ScenePath).GetRootGameObjects();

                List<Door> doorsArray = new List<Door>();
                foreach (GameObject go in goArray)
                {
                    Door[] goDoors = go.GetComponentsInChildren<Door>();

                    for (int i = 0; i < goDoors.Length; i++)
                        doorsArray.Add(goDoors[i]);
                }

                //for (int i = 0; i < doorz.Count; i++)
                //    Debug.Log(doorz[i].objReference);

                //Door[] doorsArray = FindObjectsOfType<Door>();

                myDoor.allDoors.Clear();
                for (int i = 0; i < doorsArray.Count; i++)
                    myDoor.allDoors.Add(doorsArray[i].objReference);


                // display the GenericMenu when pressing a button
                if (hadToLoad)
                {
                    // unload
                    EditorSceneManager.CloseScene(openedScene, true);
                    Array.Clear(goArray, 0, goArray.Length);
                }

                changed = false;
            }
            else if(myDoor.toCell == null)
            {
                myDoor.allDoors.Clear();
                myDoor.linkedDoorObjRef = "";
            }

            myDoor.teleportMarker = EditorGUILayout.ObjectField("Teleport Marker:", myDoor.teleportMarker, typeof(Transform), false) as Transform;

            EditorGUILayout.Space(5);

            GUI.enabled = true;

            EditorGUILayout.LabelField("Door Sounds", EditorStyles.boldLabel);

            myDoor.onDoorOpen = EditorGUILayout.ObjectField("Open:", myDoor.onDoorOpen, typeof(AudioClip), false) as AudioClip;
            myDoor.onDoorOpenLocked = EditorGUILayout.ObjectField("Locked Open:", myDoor.onDoorOpenLocked, typeof(AudioClip), false) as AudioClip;
            myDoor.onDoorOpenUnlocked = EditorGUILayout.ObjectField("Unlocked Open:", myDoor.onDoorOpenUnlocked, typeof(AudioClip), false) as AudioClip;
            myDoor.onDoorClose = EditorGUILayout.ObjectField("Close:", myDoor.onDoorClose, typeof(AudioClip), false) as AudioClip;

            EditorGUILayout.Space(5);

            GUI.enabled = myDoor.teleports;

            EditorGUILayout.LabelField("Door Link", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Linked Door:");

            if (GUILayout.Button(new GUIContent(refreshButtonIcon, "Refreshes the Door References List for the 'ToCell' value."), GUILayout.MaxWidth(32)))
                changed = true;

            EditorGUILayout.EndHorizontal();
            string menuDisplayValue = string.IsNullOrEmpty(myDoor.linkedDoorObjRef) ? "-None-" : myDoor.linkedDoorObjRef;
            if (GUILayout.Button(menuDisplayValue))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();
                menu.allowDuplicateNames = true;

                menu.AddDisabledItem(new GUIContent("Linked Door:"));

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("None"), false, Callback, "");
                for (int i = 0; i < myDoor.allDoors.Count; i++)
                {
                    if (!string.IsNullOrEmpty(myDoor.allDoors[i]))
                        menu.AddItem(new GUIContent(myDoor.allDoors[i]), false, Callback, myDoor.allDoors[i]);
                    else
                        menu.AddDisabledItem(new GUIContent("EMPTY_DOOR_OBJ_REFERENCE"), false);

                }

                menu.ShowAsContext();
            }
            void Callback(object obj)
            {
                if (myDoor != null)
                    myDoor.linkedDoorObjRef = (string)obj;
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !string.IsNullOrEmpty(myDoor.linkedDoorObjRef) && myDoor.teleports;
            if (GUILayout.Button("View Linked Door"))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    RPGCreationKitEditor.StopEditorWorldStreaming();
                    

                    string linkedDoorID = myDoor.linkedDoorObjRef;
                    EditorSceneManager.OpenScene(myDoor.toCell.sceneRef.ScenePath, OpenSceneMode.Single);

                    Door[] doorsArray = FindObjectsOfType<Door>();

                    GameObject linkedDoorGO = null;
                    for(int i = 0; i < doorsArray.Length; i++)
                    {
                        if(doorsArray[i].objReference == linkedDoorID)
                        {
                            // found the door
                            linkedDoorGO = doorsArray[i].gameObject;
                        }
                    }

                    if (linkedDoorGO != null)
                    {
                        Selection.activeGameObject = linkedDoorGO;
                        SceneView.FrameLastActiveSceneView();
                    }
                    else
                        Debug.LogWarning("The DoorObjectReferenceID: '" + linkedDoorID + "' does not exists.");
                }
            }

            if (GUILayout.Button("View Linked Teleport Marker"))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    string linkedDoorID = myDoor.linkedDoorObjRef;
                    EditorSceneManager.OpenScene(myDoor.toCell.sceneRef.ScenePath, OpenSceneMode.Single);

                    Door[] doorsArray = GameObject.FindObjectsOfType<Door>();

                    GameObject linkedDoorGO = null;
                    for (int i = 0; i < doorsArray.Length; i++)
                    {
                        if (doorsArray[i].objReference == linkedDoorID)
                        {
                            // found the door
                            if(doorsArray[i].teleportMarker)
                                linkedDoorGO = doorsArray[i].teleportMarker.gameObject;
                            else
                                linkedDoorGO = doorsArray[i].gameObject;
                        }
                    }

                    if (linkedDoorGO != null)
                    {
                        Selection.activeGameObject = linkedDoorGO;
                        SceneView.FrameLastActiveSceneView();
                    }
                    else
                        Debug.LogWarning("The DoorObjectReferenceID: " + linkedDoorID + " does not exists.");
                }
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (serializedObject != null)
            {
                if(serializedObject.targetObject != null)
                    EditorUtility.SetDirty(serializedObject.targetObject);
                serializedObject.ApplyModifiedProperties();
            }
        }



        protected SceneAsset GetSceneObject(string sceneObjectName)
        {
            if (string.IsNullOrEmpty(sceneObjectName))
            {
                return null;
            }

            foreach (var editorScene in EditorBuildSettings.scenes)
            {
                if (editorScene.path.IndexOf(sceneObjectName) != -1)
                {
                    return AssetDatabase.LoadAssetAtPath(editorScene.path, typeof(SceneAsset)) as SceneAsset;
                }
            }
            Debug.LogWarning("Scene [" + sceneObjectName + "] cannot be used. Add this scene to the 'Scenes in the Build' in build settings.");
            return null;
        }
    }
}