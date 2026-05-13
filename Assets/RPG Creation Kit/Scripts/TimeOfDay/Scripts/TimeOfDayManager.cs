using RPGCreationKit.Player;
using RPGCreationKit.SaveSystem;
using UnityEngine;

namespace RPGCreationKit
{
    public class TimeOfDayManager : MonoBehaviour
    {
        #region singleton
        public static TimeOfDayManager instance;
        private void Awake()
        {
            instance = this;
        }
        #endregion

        [Header("Time Configuration")]
        [Range(0, 24)] [SerializeField] private float currentTime;
        public int hours = 0;
        public int minutes = 0;

        [Space(10)]
        public float currentTimeScale = RCKSettings.TOD_DEFAULT_TIMESCALE;
        private float secondsToUpdateUpdateEnvironmentTimer = 0.0f;
        public float secondsToUpdateUpdateEnvironment = 2.0f;

        [Space(10)]

        public Material skyboxMaterial;

        [Space(10)]

        [Header("Sun & Moon")]
        public Light sunLight;
        public Gradient sunColor;
        public AnimationCurve sunIntensity;

        [Header("Ambient Lighting")]
        public Gradient ambientColor;
        public AnimationCurve ambientIntensity;

        [Header("Fog Settings")]
        public Gradient fogColor;
        public AnimationCurve fogDensity;


        // Delegates
        int previousHour;
        int previousMinute;

        public delegate void OnHourChanges(int curHour);
        public delegate void OnMinuteChanges(int curMin);
        public delegate void OnTimeChanges(float curTime);

        public OnHourChanges onHourChanges;
        public OnMinuteChanges onMinuteChanges;
        public OnTimeChanges onTimeChanges;

        private void Start()
        {
            previousHour = hours+1;
            previousMinute = minutes+1;

            currentTimeScale = RCKSettings.TOD_DEFAULT_TIMESCALE;
        }

        public void SetTimeScale(float tsVal)
        {
            currentTimeScale = tsVal;
        }

        public void SetTime(float tVal)
        {
            currentTime = tVal;

            if (currentTime >= 24.0f)
                currentTime = 0;

            onTimeChanges?.Invoke(currentTime);

            hours = Mathf.FloorToInt(currentTime);
            minutes = Mathf.FloorToInt((currentTime - hours) * 60f);

            if (hours != previousHour)
            {
                previousHour = hours;
                onHourChanges?.Invoke(hours);
            }

            if (minutes != previousMinute)
            {
                previousMinute = minutes;
                onMinuteChanges?.Invoke(minutes);
            }
        }

        void Update()
        {
            // Progress time
            SetTime(currentTime + (currentTimeScale * Time.deltaTime));

            if (CanEditLightsAndSkybox())
            {
                // Update
                float blendValue = UpdateSkyboxMaterial();

                UpdateSun(blendValue);
                UpdateSceneLighting(blendValue);
            }

            // Update timer
            secondsToUpdateUpdateEnvironmentTimer += 1 * Time.deltaTime;
        }

        void UpdateSceneLighting(float t)
        {
            if (!CanEditLightsAndSkybox())
                return;
            
            CellsSystem.Cell curCell = WorldManager.instance.currentCenterCell;

            // Environment Lighting
            if (curCell.overrideAmbient)
            {
                // Use the local cell settings
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

                RenderSettings.ambientLight = curCell.localAmbientColor.Evaluate(t);
                RenderSettings.ambientIntensity = curCell.localAmbientIntensity.Evaluate(t);
            }
            else
            {
                // Use the global worldspace settings
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

                RenderSettings.ambientLight = curCell.worldspace.globalAmbientColor.Evaluate(t);
                RenderSettings.ambientIntensity = curCell.worldspace.globalAmbientIntensity.Evaluate(t);
            }

            // Fog
            if (curCell.overrideFog)
            {
                // Use local cell settings
                RenderSettings.fog = curCell.localFogActive;
                RenderSettings.fogMode = curCell.localFogMode;
                RenderSettings.fogColor = curCell.localFogColor.Evaluate(t);
                RenderSettings.fogDensity = curCell.localFogDensity.Evaluate(t);
                RenderSettings.fogStartDistance = curCell.localFogStart;
                RenderSettings.fogEndDistance = curCell.localFogEnd;
            }
            else
            {
                // Use global worldspace settings
                RenderSettings.fog = curCell.worldspace.globalFogActive;
                RenderSettings.fogMode = curCell.worldspace.globalFogMode;
                RenderSettings.fogColor = curCell.worldspace.globalFogColor.Evaluate(t);
                RenderSettings.fogDensity = curCell.worldspace.globalFogDensity.Evaluate(t);
                RenderSettings.fogStartDistance = curCell.worldspace.globalFogStart;
                RenderSettings.fogEndDistance = curCell.worldspace.globalFogEnd;
            }

            RenderSettings.reflectionIntensity = Mathf.Clamp(sunIntensity.Evaluate(t), 0.1f, 1.0f);

            // UpdateEnvironment
            if (secondsToUpdateUpdateEnvironmentTimer >= secondsToUpdateUpdateEnvironment)
            {
                DynamicGI.UpdateEnvironment();
                secondsToUpdateUpdateEnvironmentTimer = 0;
            }
        }

        float UpdateSkyboxMaterial()
        {
            if (!CanEditLightsAndSkybox())
                return -1.0f;

            // Normalize time
            float blendValue = currentTime / 24f;

            // Update Skybox
            skyboxMaterial.SetFloat("_Blend", blendValue);

            return blendValue;
        }

        void UpdateSun(float blendValue)
        {
            if (!CanEditLightsAndSkybox())
                return;

            if (sunLight != null)
            {
                // Sun rotation
                float sunAngle = (blendValue * 360f) - 90f;
                sunLight.transform.rotation = Quaternion.Euler(sunAngle, 258.0f, 0f);

                // Sun Visuals
                sunLight.color = sunColor.Evaluate(blendValue);
                sunLight.intensity = sunIntensity.Evaluate(blendValue);
            }
        }

        public void RefreshLightingImmediate()
        {
            if (!CanEditLightsAndSkybox())
                return;

            float blendValue = currentTime / 24f;
            UpdateSkyboxMaterial();
            UpdateSun(blendValue);
            UpdateSceneLighting(blendValue);

            DynamicGI.UpdateEnvironment();
        }

        public float GetCurrentTime()
        {
            return currentTime;
        }

        public void LoadFromSavefile()
        {
            var todData = SaveSystemManager.instance.saveFile.TimeOfDayData;
            SetTime(todData.curTime);
        }

        bool CanEditLightsAndSkybox()
        {
            if (WorldManager.instance == null)
                return false;

            if (WorldManager.instance.isLoading)
                return false;

            if (!RCKSettings.TOD_ENABLE_SKYBOX_AND_LIGHTING_EDITS)
                return false;

            var cell = WorldManager.instance.currentCenterCell;

            if (cell.local_OverrideToDSetting)
                return cell.local_TOD_ENABLE_SKYBOX_AND_LIGHTING_EDITS;

            return cell.worldspace.global_TOD_ENABLE_SKYBOX_AND_LIGHTING_EDITS;
        }
    }
}
