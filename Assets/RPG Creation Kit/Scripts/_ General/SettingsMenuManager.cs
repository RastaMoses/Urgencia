using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace RPGCreationKit
{
    public class SettingsMenuManager : MonoBehaviour
    {
        public bool applyOnStart = false;

        [SerializeField] private GameObject ui;


        [SerializeField] private TMP_Dropdown resDropdown;
        [SerializeField] private TMP_Dropdown qualityDropdown;

        [SerializeField] private Toggle fullScreenToggle;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private Toggle autosaveToggle;

        [SerializeField] private Slider horizontalSpeedSlider;
        [SerializeField] private Slider verticalSpeedSlider;


        Resolution[] allRes;


        // Start is called before the first frame update
        void Start()
        {
            Init();

            LoadFromPlayerPrefs();
        }

        int curResIndex;

        [SerializeField] GameObject firstElementInList;

        void Init()
        {
            // Init Resolutions
            resDropdown.ClearOptions();
            allRes = Screen.resolutions;

            List<string> dropdownOptions = new List<string>();

            curResIndex = 0;

            for (int i = 0; i < allRes.Length; i++)
            {
                string newRes = allRes[i].width + " x " + allRes[i].height;
                dropdownOptions.Add(newRes);

                if (allRes[i].width == Screen.currentResolution.width && allRes[i].height == Screen.currentResolution.height)
                    curResIndex = i;
            }

            resDropdown.AddOptions(dropdownOptions);
            resDropdown.value = curResIndex;
            resDropdown.RefreshShownValue();
        }


        public void LoadFromPlayerPrefs()
        {
            if (PlayerPrefs.HasKey("Resolution"))
                resDropdown.value = PlayerPrefs.GetInt("Resolution");
            else
                PlayerPrefs.SetInt("Resolution", resDropdown.value);

            if(applyOnStart)
                SetResolution(allRes[resDropdown.value]);


            if (PlayerPrefs.HasKey("Fullscreen"))
                fullScreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen") == 1;
            else
            {
                PlayerPrefs.SetInt("Fullscreen", 1);
                fullScreenToggle.isOn = true;
            }

            if (applyOnStart)
                SetFullscreen(fullScreenToggle.isOn);

            if (PlayerPrefs.HasKey("Vsync"))
                vsyncToggle.isOn = PlayerPrefs.GetInt("Vsync") == 1;
            else
            {
                PlayerPrefs.SetInt("Vsync", 1);
                vsyncToggle.isOn = true;
            }

            if (applyOnStart)
                SetVsync(vsyncToggle.isOn ? 1 : 0);

            if (PlayerPrefs.HasKey("QualitySettings"))
                qualityDropdown.value = PlayerPrefs.GetInt("QualitySettings");
            else
            {
                PlayerPrefs.SetInt("QualitySettings", 5);
                qualityDropdown.value = 5;
            }

            if (applyOnStart)
                SetQualitySettings(qualityDropdown.value);


            if (PlayerPrefs.HasKey("HorizontalSpeed"))
                horizontalSpeedSlider.value = PlayerPrefs.GetFloat("HorizontalSpeed");
            else
            {
                PlayerPrefs.SetFloat("HorizontalSpeed", 5);
                horizontalSpeedSlider.value = 5;
            }

            if (applyOnStart)
                SetHorizontalSpeed(horizontalSpeedSlider.value);

            if (PlayerPrefs.HasKey("VerticalSpeed"))
                verticalSpeedSlider.value = PlayerPrefs.GetFloat("VerticalSpeed");
            else
            {
                PlayerPrefs.SetFloat("VerticalSpeed", 5);
                verticalSpeedSlider.value = 5;
            }

            if (applyOnStart)
                SetVerticalSpeed(verticalSpeedSlider.value);

            if (PlayerPrefs.HasKey("Autosave"))
                autosaveToggle.isOn = PlayerPrefs.GetInt("Autosave") == 1;
            else
            {
                PlayerPrefs.SetInt("Autosave", 1);
                autosaveToggle.isOn = true;
            }

            if (applyOnStart)
                SetAutosave(autosaveToggle.isOn ? 1 : 0);
            

            PlayerPrefs.Save();
        }

        public void ApplySettings()
        {
            SetResolution(allRes[resDropdown.value]);
            SetFullscreen(fullScreenToggle.isOn);
            SetVsync(vsyncToggle.isOn ? 1 : 0);
            SetQualitySettings(qualityDropdown.value);
            SetHorizontalSpeed(horizontalSpeedSlider.value);
            SetVerticalSpeed(verticalSpeedSlider.value);
            SetAutosave(autosaveToggle.isOn ? 1 : 0);

            PlayerPrefs.SetInt("Resolution", resDropdown.value);
            PlayerPrefs.SetInt("Fullscreen", fullScreenToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt("Vsync", vsyncToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt("QualitySettings", qualityDropdown.value);
            PlayerPrefs.SetFloat("HorizontalSpeed", horizontalSpeedSlider.value);
            PlayerPrefs.SetFloat("VerticalSpeed", verticalSpeedSlider.value);
            PlayerPrefs.SetInt("Autosave", autosaveToggle.isOn ? 1 : 0);

            PlayerPrefs.Save();
        }

        private void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        private void SetResolution(Resolution res)
        {
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }

        private void SetVsync(int on)
        {
            QualitySettings.vSyncCount = on;
        }

        private void SetQualitySettings(int index)
        {
            QualitySettings.SetQualityLevel(index);
        }

        private void SetHorizontalSpeed(float _speed)
        {
            RCKSettings.MOUSE_HORIZONTAL_SPEED = _speed;
        }

        private void SetVerticalSpeed(float _speed)
        {
            RCKSettings.MOUSE_VERTICAL_SPEED = _speed;
        }

        private void SetAutosave(int on)
        {
            RCKSettings.AUTOSAVE_ENABLED = (on > 0) ? true : false;
        }

        public void OpenSettings()
        {
            ui.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }

        public void CloseSettings()
        {
            ui.SetActive(false);
        }

        public void SelectFirstElementInList()
        {

        }

        IEnumerator SelectFirstElementTask()
        {
            if (firstElementInList != null)
                EventSystem.current.SetSelectedGameObject(firstElementInList);

            yield break;
        }

    }
}