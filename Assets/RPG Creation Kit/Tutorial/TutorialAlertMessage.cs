using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    /// <summary>
    /// This class manages the alert messages shown in the game.
    /// It contains a static instance that must be set on every scene (Awake method) 
    /// </summary>
    public class TutorialAlertMessage : MonoBehaviour
    {
        public bool messageOpened = false;

        #region Singleton
        public static TutorialAlertMessage instance;

        /// <summary>
        /// Set the instance
        /// </summary>
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                // Replace the previous instance
                Destroy(instance.gameObject);
                instance = this;
            }
        }
        #endregion

        public GameObject UI;
        [SerializeField] private TextMeshProUGUI message;


        public void OpenMessage(string _message)
        {
            RPGCreationKit.Player.RckPlayer.instance.EnableDisableControls(false);
            messageOpened = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0.0f;

            UI.SetActive(true);
            message.text = _message;
        }

        public void OpenMessageAfterDialogueEnds(string _message)
        {
            StartCoroutine(IEOpenMessageAfterDialogueEnds(_message));
        }

        public void CloseButton(bool forcedClose = false)
        {

            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1.0f;

            UI.SetActive(false);
            messageOpened = false;
            RPGCreationKit.Player.RckPlayer.instance.EnableDisableControls(true);
        }

        public IEnumerator IEOpenMessageAfterDialogueEnds(string _message)
        {
            while(RckPlayer.instance.isInConversation)
                yield return null;

            OpenMessage(_message);
        }
    }
}