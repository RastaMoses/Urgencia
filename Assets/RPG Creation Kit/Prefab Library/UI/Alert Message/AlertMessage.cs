using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Simple pop-up in the game
    /// </summary>
    public class AlertMessage : MonoBehaviour
    {

        public static float DEFAULT_MESSAGE_DURATION_SHORT = 1.5f;
        public static float DEFAULT_MESSAGE_DURATION_MEDIUM = 3f;
        public static float DEFAULT_MESSAGE_DURATION_LONG = 5f;


        #region Singleton
        public static AlertMessage instance;

        [SerializeField] AlertMessageUIElement uiElement;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Debug.LogError("Anomaly detected with the Singleton Pattern of 'AlertMessage', are you using multple AlertMessages?");
                Destroy(this);
            }
        }

        #endregion

        [System.Serializable]
        public struct QueuedAlertMessage
        {
            public string message;
            public float duration;

            public QueuedAlertMessage(string _message, float _duration)
            {
                message = _message;
                duration = _duration;
            }
        }

        public List<QueuedAlertMessage> queuedAlerts = new List<QueuedAlertMessage>();

        /// <summary>
        /// Init the Pop-Up
        /// </summary>
        /// <param name="_text">The message</param>
        /// <param name="duration">The duration (seconds)</param>
        /// <param name="allowDuplication"> If this is enabled you can que multiple message that have the same text</param>
        public void InitAlertMessage(string _text, float duration, bool allowDuplication = false)
        {
            if (!uiElement.gameObject.activeInHierarchy)
            {
                uiElement.gameObject.SetActive(true);
                uiElement.AlertText.text = _text;

                // Use unscaled time
                StopCoroutine(nameof(DisableAlertAfterRealtime));
                StartCoroutine(DisableAlertAfterRealtime(duration));
            }
            else
            {
                if (!allowDuplication && _text == uiElement.AlertText.text)
                    return;

                queuedAlerts.Add(new QueuedAlertMessage(_text, duration));
                StartCoroutine(nameof(WaitForEndOfAlert));
            }
        }

        private IEnumerator DisableAlertAfterRealtime(float duration)
        {
            yield return new WaitForSecondsRealtime(duration); // unaffected by Time.timeScale
            DisableAlert();
        }

        IEnumerator WaitForEndOfAlert()
        {
            yield return new WaitForEndOfFrame();

            // If there is no queued alerts, break!
            if (queuedAlerts.Count == 0) yield break;

            // If there is still some other alerts
            while (uiElement.gameObject.activeInHierarchy)
            {
                // just wait a bit more
                yield return null;
            }
            // When there are no other alerts, init the first alert added in the list
            InitAlertMessage(queuedAlerts[0].message, queuedAlerts[0].duration);

            // and then remove it from the list
            queuedAlerts.RemoveAt(0);
        }

        private void DisableAlert()
        {
            uiElement.AlertText.text = "";
            uiElement.gameObject.SetActive(false);
        }
    }
}
