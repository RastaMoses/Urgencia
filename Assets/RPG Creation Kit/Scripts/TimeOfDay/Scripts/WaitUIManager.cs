using RPGCreationKit.CellsSystem;
using RPGCreationKit.Player;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGCreationKit
{
    public class WaitUIManager : MonoBehaviour
    {
        #region singleton
        public static WaitUIManager instance;
        private void Awake()
        {
            instance = this;
        }
        #endregion

        // State
        [SerializeField] public bool m_isOpen = false; // is ui open
        [SerializeField] private bool m_isSleeping = false; // WaitUI can be used for waiting or sleeping

        // Functionality
        private bool m_isWaiting = false;
        private bool m_cancelled = false;

        /// UI
        [SerializeField] private GameObject m_WholeUI;
        [SerializeField] private TextMeshProUGUI m_headerText;
        [SerializeField] private Slider m_hoursSlider;
        [SerializeField] private TextMeshProUGUI m_hoursSliderText;
        [SerializeField] private TextMeshProUGUI m_currentTime;
        [SerializeField] private TextMeshProUGUI m_waitSleepButtonText;

        public void OpenCloseWaitUIBySleeping()
        {
            m_isSleeping = true;
            OpenCloseWaitUIByWaiting();
        }

        public void OpenCloseWaitUIByWaiting()
        {
            if(CellInformation.IsAnyAIInCombatWithPlayer())
            {
                AlertMessage.instance.InitAlertMessage("You can't do that while in combat.", 2.5f);
                m_isSleeping = false;
                return;
            }

            if (!m_isOpen && !RckPlayer.instance.isInCutsceneMode)
            {
                RckPlayer.instance.input.SwitchCurrentActionMap("WaitUI");

                RckPlayer.instance.EnableDisableControls(false);

                Time.timeScale = 0.0f;

                m_cancelled = false;
                m_isWaiting = false;

                if (m_isSleeping)
                    m_headerText.text = "How long would you like to sleep?";
                else
                    m_headerText.text = "How long would you like to wait?";

                m_hoursSlider.value = 1;
                m_currentTime.text = $"Current Time: {TimeOfDayManager.instance.hours}:{TimeOfDayManager.instance.minutes:00}";

                if(m_isSleeping)
                    m_waitSleepButtonText.text = "Sleep";
                else
                    m_waitSleepButtonText.text = "Wait";

                m_isOpen = true;

                m_WholeUI.SetActive(true);

            }
            else
            {
                CloseButton(false);
            }
        }

        public void OnSliderChanges()
        {
            if ((int)m_hoursSlider.value == 1)
                m_hoursSliderText.text = "1 Hour";
            else
                m_hoursSliderText.text = m_hoursSlider.value + " Hours";
        }

        public void CloseButton(bool forceit)
        {
            if (!forceit && m_isWaiting)
                return;

            RckPlayer.instance.EnableDisableControls(true);

            m_cancelled = false;
            m_isWaiting = false;
            m_isSleeping = false;

            m_WholeUI.SetActive(false);
            m_isOpen = false;
            Time.timeScale = 1.0f;

            RckPlayer.instance.input.SwitchCurrentActionMap("Player");

        }

        public void WaitCancelButton()
        {
            if(m_isWaiting)
            {
                PerformCancel();
            }
            else
            {
                m_isWaiting = true;
                m_waitSleepButtonText.text = "Cancel";
                StartCoroutine(nameof(PerformWaitCoroutine));
            }
        }

        private IEnumerator PerformWaitCoroutine()
        {
            int hoursToDo = (int)m_hoursSlider.value;
            bool operationCancelled = false;

            while (hoursToDo > 0 && !operationCancelled)
            {
                yield return new WaitForSecondsRealtime(1.0f);

                TimeOfDayManager.instance.SetTime(TimeOfDayManager.instance.GetCurrentTime() + 1.0f);
                hoursToDo--;
                m_hoursSlider.value = hoursToDo;
                m_currentTime.text = $"Current Time: {TimeOfDayManager.instance.hours}:{TimeOfDayManager.instance.minutes:00}";
                if (m_cancelled)
                    operationCancelled = true;
            }

            if (operationCancelled || hoursToDo == 0)
            {
                m_isWaiting = false;
                CloseButton(true);
            }
        }

        public void PerformCancel()
        {
            m_cancelled = true;
        }
    }
}