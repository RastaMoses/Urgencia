using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using System.Collections;
using System.Dynamic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

public class RckCustomToolbar
{
    [MainToolbarElement("RPG Creation Kit/Load Last Cell", defaultDockPosition = MainToolbarDockPosition.Middle)]
    public static MainToolbarElement LoadLastCellButton()
    {
        var icon = EditorGUIUtility.IconContent("Import").image as Texture2D;
        var content = new MainToolbarContent(icon);
        return new MainToolbarButton(content, () => 
        {
            if (EditorApplication.isPlaying)
                return;

            // Get CellView
            CellView window = EditorWindow.GetWindow<CellView>();

            if(window)
            {
                window.LoadLastLoadedCell();
            }

        });
    }

    [MainToolbarElement("RPG Creation Kit/Load Last Save", defaultDockPosition = MainToolbarDockPosition.Left)]
    public static MainToolbarElement LoadLastSaveButton()
    {
        var icon = EditorGUIUtility.IconContent("PlayButton").image as Texture2D;
        var content = new MainToolbarContent(icon);
        return new MainToolbarButton(content, () =>
        {
            if (EditorApplication.isPlaying)
                return;

            // Get CellView
            CellView window = EditorWindow.GetWindow<CellView>();

            if (window)
            {
                window.LoadMainMenu();

                // SET LOAD SAVE FLAG TRUE
                EditorPrefs.SetBool("EDITOR_RCK_LOAD_LAST_SAVE_ON_PLAY", true);

                EditorApplication.EnterPlaymode();
            }

        });
    }

    const float k_MinTimeScale = 0.0f;
    const float k_MaxTimeScale = 24.0f;
    public static float curSliderValue;

    [MainToolbarElement("RPG Creation Kit/Time of Day", defaultDockPosition = MainToolbarDockPosition.Middle)]
    public static MainToolbarElement TimeSlider()
    {
        var content = new MainToolbarContent("Time of Day", "Time of Day");
        return new MainToolbarSlider(content, 0.0f, k_MinTimeScale, k_MaxTimeScale, OnSliderValueChanged);
    }
    public static void OnSliderValueChanged(float newValue)
    {
        curSliderValue = newValue;

        Material m = RckProjectSettings.instance.timeOfDaySkyMaterial;
        if (m)
        {
            m.SetFloat("_Blend", newValue / 24f);
        }

        CellInformation curCell = Object.FindAnyObjectByType<CellInformation>();

        if (curCell != null)
        {
            float t = newValue / 24f;
            UpdateSceneLighting(curCell, t);
        }
    }

    // Pass curCell as a parameter since the method is static
    static void UpdateSceneLighting(CellInformation curCell, float t)
    {
        // Environment Lighting
        if (curCell.cell.overrideAmbient)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = curCell.cell.localAmbientColor.Evaluate(t);
            RenderSettings.ambientIntensity = curCell.cell.localAmbientIntensity.Evaluate(t);
        }
        else if (curCell.cell.worldspace != null)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = curCell.cell.worldspace.globalAmbientColor.Evaluate(t);
            RenderSettings.ambientIntensity = curCell.cell.worldspace.globalAmbientIntensity.Evaluate(t);
        }

        // Fog Settings
        if (curCell.cell.overrideFog)
        {
            RenderSettings.fog = curCell.cell.localFogActive;
            RenderSettings.fogMode = curCell.cell.localFogMode;
            RenderSettings.fogColor = curCell.cell.localFogColor.Evaluate(t);
            RenderSettings.fogDensity = curCell.cell.localFogDensity.Evaluate(t);
        }
        else if (curCell.cell.worldspace != null)
        {
            RenderSettings.fog = curCell.cell.worldspace.globalFogActive;
            RenderSettings.fogColor = curCell.cell.worldspace.globalFogColor.Evaluate(t);
            RenderSettings.fogDensity = curCell.cell.worldspace.globalFogDensity.Evaluate(t);
        }

        // Refresh Scene View
        DynamicGI.UpdateEnvironment();
        SceneView.RepaintAll();
    }
}