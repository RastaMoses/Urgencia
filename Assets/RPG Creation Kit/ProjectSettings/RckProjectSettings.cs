#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[FilePath("Project Settings/RckProjectSettings.asset", FilePathAttribute.Location.ProjectFolder)]
public class RckProjectSettings : ScriptableSingleton<RckProjectSettings>
{
    public bool showDemoFiles = true;
    public Material timeOfDaySkyMaterial;

    // Call this after changing values
    public void SaveSettings()
    {
        Save(true);
    }
}
#endif
