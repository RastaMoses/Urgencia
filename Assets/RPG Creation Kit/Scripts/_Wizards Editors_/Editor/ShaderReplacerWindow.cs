using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ShaderReplacerWindow: EditorWindow
{
    [MenuItem("RPG Creation Kit/Rendering Pipelines/Shader Replacer")]
    public static void Open()
    {
        GetWindow<ShaderReplacerWindow>("Shader Replacer");
    }

    Shader shader;                // The shader to find
    Shader targetShader;          // The shader to assign
    List<string> materials = new List<string>();
    Vector2 scroll;

    void OnGUI()
    {
        Shader prev = shader;
        shader = EditorGUILayout.ObjectField("Find Shader", shader, typeof(Shader), false) as Shader;
        targetShader = EditorGUILayout.ObjectField("Replace With", targetShader, typeof(Shader), false) as Shader;

        if (shader != prev && shader != null)
        {
            string[] allMaterialGuids = AssetDatabase.FindAssets("t:Material");
            materials.Clear();

            foreach (string guid in allMaterialGuids)
            {
                string materialPath = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (mat != null && mat.shader == shader)
                {
                    materials.Add(materialPath);
                }
            }
        }

        if (materials.Count > 0 && targetShader != null)
        {
            if (GUILayout.Button("Fix All (Replace Shader)"))
            {
                for (int i = materials.Count - 1; i >= 0; i--)
                {
                    string matPath = materials[i];
                    Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (mat != null && mat.shader == shader)
                    {
                        mat.shader = targetShader;
                        EditorUtility.SetDirty(mat);
                        materials.RemoveAt(i); // Remove from list
                    }
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("All materials updated.");
            }
        }

        scroll = GUILayout.BeginScrollView(scroll);
        for (int i = 0; i < materials.Count;)
        {
            string matPath = materials[i];
            GUILayout.BeginHorizontal();
            GUILayout.Label(Path.GetFileNameWithoutExtension(matPath));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Show", GUILayout.Width(50)))
            {
                Object mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                EditorGUIUtility.PingObject(mat);
            }

            GUI.enabled = targetShader != null;
            if (GUILayout.Button("Fix", GUILayout.Width(50)))
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat != null && mat.shader == shader)
                {
                    mat.shader = targetShader;
                    EditorUtility.SetDirty(mat);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"Material '{mat.name}' updated.");
                    materials.RemoveAt(i); // Remove after fixing
                    continue;
                }
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
            i++;
        }
        GUILayout.EndScrollView();
    }
}
