using System.Collections;
using UnityEngine;

namespace RPGCreationKit
{
    public class InGameHelpUI : MonoBehaviour
    {
        public static InGameHelpUI instance;
        private void Awake()
        {
            if (!instance)
                instance = this;
        }


        public GameObject dismountHelpUI;
        public GameObject walkToggleUI;

        // Abs tutorial panels
        public GameObject movementsPanel;
        public GameObject attacksPanel;
        public GameObject sneakAttackPanel;
        public GameObject diaryHelpPanel;

        public void SetDismountHelpUI(bool doEnable)
        {
            if (dismountHelpUI)
                dismountHelpUI.SetActive(doEnable);
        }

        public void SetWalkToggleUI(bool doEnable)
        {
            if(walkToggleUI)
                walkToggleUI.SetActive(doEnable);
        }

        public void StartTutorial()
        {
            StartCoroutine(nameof(StartTutorialCoroutine));
        }

        private IEnumerator StartTutorialCoroutine()
        {
            movementsPanel.SetActive(true);

            yield return new WaitForSeconds(13.5f);

            movementsPanel.SetActive(false);

            yield return new WaitForSeconds(1.0f);

            attacksPanel.SetActive(true);

            yield return new WaitForSeconds(15.0f);

            attacksPanel.SetActive(false);

            yield break;
        }

        public void TriggerSneakAttackHelp()
        {
            StopAllCoroutines();
            StartCoroutine(nameof(TriggerSneakAttackHelpCoroutine));
        }

        private IEnumerator TriggerSneakAttackHelpCoroutine()
        {
            // Prevent overlap if player speedruns
            movementsPanel.SetActive(false);
            attacksPanel.SetActive(false);

            sneakAttackPanel.SetActive(true);

            yield return new WaitForSeconds(15.0f);

            sneakAttackPanel.SetActive(false);
        }

        public void TriggerDiaryHelp()
        {
            StopAllCoroutines();
            StartCoroutine(nameof(TriggerDiaryHelpCoroutine));
        }

        private IEnumerator TriggerDiaryHelpCoroutine()
        {
            // Prevent overlap if player speedruns
            movementsPanel.SetActive(false);
            attacksPanel.SetActive(false);
            sneakAttackPanel.SetActive(false);

            diaryHelpPanel.SetActive(true);

            yield return new WaitForSeconds(15.0f);

            diaryHelpPanel.SetActive(false);
        }
    }
}