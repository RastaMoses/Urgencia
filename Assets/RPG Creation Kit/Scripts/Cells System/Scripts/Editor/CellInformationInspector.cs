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
    [CustomEditor(typeof(CellInformation))]
    public class CellInformationInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            EditorGUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("Update Cell", "Updates the Cell, fills the CellInformation fields and organizes the Scene in the correct way")))
            {
                bool choice = EditorUtility.DisplayDialog("Warning!",
                            "Updating this Cell will save and close every other opened scene.",
                            "Save",
                            "Cancel");


                if (!choice)
                    return;

                // If we get 
                Debug.Log("Updating");

                // Close every other scene
                EditorSceneManager.SaveOpenScenes();

                CellInformation cellInfo = (CellInformation)target;

                // Unload all other scenes
                int countLoaded = SceneManager.sceneCount;
                Scene[] loadedScenes = new Scene[countLoaded];

                for (int i = 0; i < countLoaded; i++)
                    loadedScenes[i] = EditorSceneManager.GetSceneAt(i);

                for (int i = 0; i < loadedScenes.Length; i++)
                    if (loadedScenes[i].path != cellInfo.cell.sceneRef.ScenePath)
                        EditorSceneManager.CloseScene(loadedScenes[i], true);

                cellInfo.UpdateCellFromInspector();

                EditorUtility.SetDirty(cellInfo);

                EditorSceneManager.MarkAllScenesDirty();
                EditorSceneManager.SaveOpenScenes();
            }

            if (GUILayout.Button(new GUIContent("Regenerate all Items GUID", "Regenerates every GUID of every Item In World present in this scene.")))
            {
                bool choice = EditorUtility.DisplayDialog("Warning!",
                            "Are you sure you want to Regenerate every GUID of every Item In World present in this Cell?.",
                            "Yes",
                            "Cancel");


                if (!choice)
                    return;

                // If we get 
                Debug.Log("Updating");

                // Close every other scene
                EditorSceneManager.SaveOpenScenes();

                CellInformation cellInfo = (CellInformation)target;

                // Unload all other scenes
                int countLoaded = SceneManager.sceneCount;
                Scene[] loadedScenes = new Scene[countLoaded];

                for (int i = 0; i < countLoaded; i++)
                    loadedScenes[i] = EditorSceneManager.GetSceneAt(i);

                for (int i = 0; i < loadedScenes.Length; i++)
                    if (loadedScenes[i].path != cellInfo.cell.sceneRef.ScenePath)
                        EditorSceneManager.CloseScene(loadedScenes[i], true);

                cellInfo.UpdateCellFromInspector();

                ItemInWorld[] itemsInWolrd = FindObjectsOfType<ItemInWorld>();
                foreach (ItemInWorld item in itemsInWolrd)
                    item.RegenerateGUIDStr();

                EditorUtility.SetDirty(cellInfo);

                EditorSceneManager.MarkAllScenesDirty();
                EditorSceneManager.SaveOpenScenes();
            }

            if (GUILayout.Button(new GUIContent("Assign Items Ownership to Cell Owner", "Assigns the ownership for all the items in this Cell (including containers) to the Cell Owner.")))
            {
                CellInformation cellInfo = (CellInformation)target;

                bool choice = EditorUtility.DisplayDialog("Warning!",
                            "Are you sure you want to assign the ownership for all the items (including containers) to the Cell Owner \"" + cellInfo.cellOwner + "\"?",
                            "Yes",
                            "Cancel");


                if (!choice)
                    return;

                // If we get 
                Debug.Log("Updating");

                // Close every other scene
                EditorSceneManager.SaveOpenScenes();


                // Unload all other scenes
                int countLoaded = SceneManager.sceneCount;
                Scene[] loadedScenes = new Scene[countLoaded];

                for (int i = 0; i < countLoaded; i++)
                    loadedScenes[i] = EditorSceneManager.GetSceneAt(i);

                for (int i = 0; i < loadedScenes.Length; i++)
                    if (loadedScenes[i].path != cellInfo.cell.sceneRef.ScenePath)
                        EditorSceneManager.CloseScene(loadedScenes[i], true);

                ItemInWorld[] itemsInWolrd = FindObjectsOfType<ItemInWorld>();
                foreach (ItemInWorld item in itemsInWolrd)
                {
                    item.metadata.isOwned = true;
                    item.metadata.ownerID = cellInfo.cellOwner;
                }

                LootingPoint[] lootingPoints = FindObjectsOfType<LootingPoint>();
                foreach (LootingPoint item in lootingPoints)
                {
                    item.metadata.isOwned = true;
                    item.metadata.ownerID = cellInfo.cellOwner;
                    item.ApplyLootingPointOwnershipToItems();
                }

                EditorUtility.SetDirty(cellInfo);

                EditorSceneManager.MarkAllScenesDirty();
                EditorSceneManager.SaveOpenScenes();
            }
        }
    }
}