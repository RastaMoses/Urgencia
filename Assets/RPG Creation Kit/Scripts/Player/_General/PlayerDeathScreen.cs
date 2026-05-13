using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using UnityEngine.SceneManagement;

namespace RPGCreationKit.Player
{
    public class PlayerDeathScreen : MonoBehaviour
    {
        public static PlayerDeathScreen instance;

        
        public GameObject ui;

        public GameObject buttonsT;
        public Button defaultButton;
        [SerializeField] private LoadPanel loadPanel;

        private void Awake()
        {
            if(instance == null)
                instance = this;
        }

        public void ShowDeathScreen()
        {
            RckInput.input.SwitchCurrentActionMap("DeathUI");
            Time.timeScale = 0.0f;
            ui.SetActive(true);

            OnShowingUp();

            RckPlayer.instance.mouseLook.UnlockCursor();
        }

        public void OnShowingUp()
        {
            buttonsT.SetActive(true);

            if (RckInput.isUsingGamepad)
                defaultButton.Select();
        }

        public void LoadButton()
        {
            loadPanel.gameObject.SetActive(true);

            if (RckInput.isUsingGamepad)
                loadPanel.SelectFirstElementInList();
        }

        public void QuitButton()
        {
            SceneManager.LoadScene("_MainMenu_");
        }
    }
}