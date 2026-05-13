using RPGCreationKit;
using RPGCreationKit.SaveSystem;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPGCreationKit
{
    public class MainMenuUI : MonoBehaviour
    {
        public PlayerInput input;
        public LoadPanel loadPanel;

        public GameObject mainMenuButtons;
        public GameObject defaultSelected;
        public GameObject previouslySelected;

        private void Start()
        {
            Time.timeScale = 1.0f;

            RckInput.LoadInputSave();
            RckInput.SetPlayerInputInstance(input);

            OnShowingUp();

#if UNITY_EDITOR
            if (EditorPrefs.GetBool("EDITOR_RCK_LOAD_LAST_SAVE_ON_PLAY", false))
            {
                EditorPrefs.DeleteKey("EDITOR_RCK_LOAD_LAST_SAVE_ON_PLAY");
                MainMenuButton_LoadLastSave();
            }
#endif
        }

        public void OnShowingUp()
        {
            mainMenuButtons.SetActive(true);

            if (RckInput.isUsingGamepad)
            {
                if (previouslySelected != null)
                    EventSystem.current.SetSelectedGameObject(previouslySelected);
                else
                    EventSystem.current.SetSelectedGameObject(defaultSelected);
            }
        }

        private void Update()
        {

        }

        public void MainMenuButton_Load()
        {
            previouslySelected = EventSystem.current.currentSelectedGameObject;

            mainMenuButtons.SetActive(false);
            loadPanel.gameObject.SetActive(true);

            if(RckInput.isUsingGamepad)
                loadPanel.SelectFirstElementInList();
        }

        public void MainMenuButton_LoadLastSave()
        {
            previouslySelected = EventSystem.current.currentSelectedGameObject;

            mainMenuButtons.SetActive(false);
            loadPanel.gameObject.SetActive(true);

            loadPanel.LoadLastSave();
        }

        public void SelectedButton(GameObject btn)
        {
            previouslySelected = btn;
        }

        public void MainMenuButton_NewGame()
        {
            SceneManager.LoadScene("_CharacterCreation_");
        }

        public void MainMenuButton_Exit()
        {
            Application.Quit();
        }

    }
}