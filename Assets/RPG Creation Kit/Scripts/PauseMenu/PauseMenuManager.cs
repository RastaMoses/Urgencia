using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace RPGCreationKit
{
    public class PauseMenuManager : MonoBehaviour
    {
        public bool menuOpened = false;

        [SerializeField] private GameObject defaultButton;

        [SerializeField] private GameObject pauseUI;
        [SerializeField] private LoadPanel loadPanel;
        [SerializeField] private SavePanel savePanel;
        [SerializeField] private SettingsMenuManager settingsPanel;

        public Button saveButton;

        public GameObject menuButtons;
        public GameObject previouslySelected;

        bool mountAudioWasPlaying = false;

        public void INPUT_PauseGame(InputAction.CallbackContext value)
        {
            if (value.started)
                ToggleMenu();
        }

        public void ToggleMenu()
        {
            if (TutorialAlertMessage.instance.messageOpened)
                return;

            menuOpened = !menuOpened;

            // Open
            if(menuOpened && GameStatus.instance.PlayerPlaying())
            {
                // Quick audio fix for mount
                if(RckPlayer.instance.aiMounting && RckPlayer.instance.aiMounting.audioSource && RckPlayer.instance.aiMounting.audioSource.isPlaying)
                {
                    RckPlayer.instance.aiMounting.audioSource.Stop();
                    mountAudioWasPlaying = true;
                }

                RckInput.input.SwitchCurrentActionMap("PauseMenu");
                RckPlayer.instance.FreezeAndDisableControl();
                RckPlayer.instance.mouseLook.UnlockCursor();
                Time.timeScale = 0.0f;
                pauseUI.SetActive(true);

                saveButton.enabled = GameStatus.instance.canSave;

                OnShowingUp();

                if (RckInput.isUsingGamepad && !previouslySelected)
                    EventSystem.current.SetSelectedGameObject(defaultButton);
            }
            // Close
            else
            {
                CloseAndResume();

                if (mountAudioWasPlaying && RckPlayer.instance.aiMounting != null)
                    RckPlayer.instance.aiMounting.playingSound = false;
            }
        }

        public void OnShowingUp()
        {
            menuButtons.SetActive(true);

            if (RckInput.isUsingGamepad)
            {
                if (previouslySelected != null)
                    EventSystem.current.SetSelectedGameObject(previouslySelected);
                else
                    EventSystem.current.SetSelectedGameObject(defaultButton);
            }
        }

        public void LoadButton()
        {
            menuButtons.SetActive(false);

            previouslySelected = EventSystem.current.currentSelectedGameObject;
            loadPanel.gameObject.SetActive(true);

            if (RckInput.isUsingGamepad)
                loadPanel.SelectFirstElementInList();
        }

        public void SettingsButton()
        {
            menuButtons.SetActive(false);

            previouslySelected = EventSystem.current.currentSelectedGameObject;
            settingsPanel.OpenSettings();

            if (RckInput.isUsingGamepad)
                settingsPanel.SelectFirstElementInList();
        }

        public void SaveButton()
        {
            menuButtons.SetActive(false);

            previouslySelected = EventSystem.current.currentSelectedGameObject;
            savePanel.gameObject.SetActive(true);

            if (RckInput.isUsingGamepad)
                savePanel.SelectFirstElementInList();
        }

        public void ResumeButton()
        {
            CloseAndResume();
        }

        public void CloseAndResume()
        {
            RckInput.input.SwitchCurrentActionMap("Player");
            previouslySelected = null;
            RckPlayer.instance.mouseLook.LockCursor();
            Time.timeScale = 1.0f;
            pauseUI.SetActive(false);

            RckPlayer.instance.UnfreezeAndEnableControls();

            if(RckPlayer.instance.isInCutsceneMode)
                RckPlayer.instance.EnterInCutsceneMode();

            if(RckPlayer.instance.isInDeathSequence)
                RckPlayer.instance.FreezeAndDisableControl();

            savePanel.ClosePanel();
            loadPanel.ClosePanel();
        }

        public void QuitToMainMenu()
        {
            SceneManager.LoadScene("_MainMenu_");
        }
    }
}