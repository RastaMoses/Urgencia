using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RPGCreationKit.CellsSystem;
using RPGCreationKit;
using UnityEngine.Rendering;

namespace RPGCreationKit.CellsSystem
{
    public class SceneAttribute : PropertyAttribute { }

    /// <summary>
    /// Represent a cell in a World Space.
    /// </summary>
    [CreateAssetMenu(fileName = "New Cell", menuName = "RPG Creation Kit/Cell System/New Cell", order = 1)]
    public class Cell : ScriptableObject
    {
        public string ID;
        public string cellName;

        [Space(5)]

        public bool keepExteriorLight = false;
        public Worldspace worldspace;
        public GameObject distantCellObject;

        [Space(5)]
        public SceneReference sceneRef;
        [Space(5)]

        public Vector2 cellCoordinates;         // Represent the cell in the notation (0,1) (0,2) etc
        public Vector3 cellInWorldCoordinates;  // The real, world location of this cell (160, 0, 160)

        public Cell[] neighboringCells;

        public bool CanBeCached = true;
        public bool CanBeCachedOnlyInSameWorldspace = false;

        // Time of Day - ambient/fog override
        [Header("Ambient Lighting")]
        public bool overrideAmbient = false;

        public AmbientMode localAmbientMode = AmbientMode.Flat;
        public Gradient localAmbientColor;
        public AnimationCurve localAmbientIntensity;

        [Header("Fog Settings")]
        public bool overrideFog = false;

        public bool localFogActive;
        public FogMode localFogMode = FogMode.Linear;
        public Gradient localFogColor;
        public AnimationCurve localFogDensity;
        public int localFogStart = 0;
        public int localFogEnd = 220;

        // If this is set to true, TimeOfDayManager will edit lighting and skybox while the player is in this cell
        // This overrides the Worldspace's setting of this parameter
        public bool local_OverrideToDSetting = false;
        public bool local_TOD_ENABLE_SKYBOX_AND_LIGHTING_EDITS = true;
    }
}