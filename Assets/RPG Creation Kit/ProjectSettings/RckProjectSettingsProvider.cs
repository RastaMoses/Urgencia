#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

static class RckProjectSettingsProvider
{
    [SettingsProvider]
    public static SettingsProvider CreateProvider()
    {
        return new SettingsProvider("RPG Creation Kit/Settings", SettingsScope.Project)
        {
            label = "RPG Creation Kit",
            guiHandler = _ =>
            {
                var s = RckProjectSettings.instance;

                EditorGUI.BeginChangeCheck();
                s.showDemoFiles = EditorGUILayout.Toggle("Show Demo Files", s.showDemoFiles);

                s.timeOfDaySkyMaterial = (Material)EditorGUILayout.ObjectField("Time Of Day Sky Material", s.timeOfDaySkyMaterial, typeof(Material), false);

                if (EditorGUI.EndChangeCheck())
                    s.SaveSettings();
            }
        };
    }
}
#endif
