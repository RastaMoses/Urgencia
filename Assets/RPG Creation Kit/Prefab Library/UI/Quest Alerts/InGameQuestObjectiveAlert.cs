using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class InGameQuestObjectiveAlert : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image[] imagedToFadeInOut;

        [SerializeField] private float totalTime = 8f;

        [SerializeField] private float fadeInTime = 1.0f;
        [SerializeField] private float fadeOutTime = 1.0f;

        float timePassedSinceStart;

        [ContextMenu("Get References")]
        private void GetReferences()
        {
            text = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Initialize(string _quest)
        {
            text.text = _quest;
            StartCoroutine(ManageAlertTask());
        }

        private IEnumerator ManageAlertTask()
        {
            timePassedSinceStart = 0.0f;
            float alpha = 0.0f;
            bool fadingOut = false;

            while (true)
            {
                // Wait if the game is paused
                while(!GameStatus.instance.PlayerPlaying())
                    yield return null;

                if (!fadingOut)
                {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, (alpha));

                    for(int i = 0; i < imagedToFadeInOut.Length; i++)
                        imagedToFadeInOut[i].color = new Color(imagedToFadeInOut[i].color.r, imagedToFadeInOut[i].color.g, imagedToFadeInOut[i].color.b, (alpha));

                    alpha += (fadeInTime * Time.deltaTime);
                    alpha = Mathf.Clamp01(alpha);
                }
                else
                {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, (alpha));

                    for (int i = 0; i < imagedToFadeInOut.Length; i++)
                        imagedToFadeInOut[i].color = new Color(imagedToFadeInOut[i].color.r, imagedToFadeInOut[i].color.g, imagedToFadeInOut[i].color.b, (alpha));

                    alpha -= (fadeOutTime * Time.deltaTime);
                    alpha = Mathf.Clamp01(alpha);
                }

                timePassedSinceStart += 1 * Time.deltaTime;

                // Check for FadeOut

                if (timePassedSinceStart >= totalTime)
                    Destroy(this.gameObject);

                fadingOut = (timePassedSinceStart + fadeOutTime >= totalTime) ? true : false;

                yield return null;
            }
        }
    }
}