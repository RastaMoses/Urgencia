using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;

namespace RPGCreationKit.Player
{
    public class PlayerUI : PlayerInteractor
    {
        public GameObject fadeInOutGameObject;
        public Image fadeInOutImage;

        [SerializeField] private AudioClip[] hitSounds;

        public override void Start()
        {
            base.Start();
            uiManager.playerHealthSlider.maxValue = playerAttributes.MaxHealth;
            uiManager.playerStaminaSlider.maxValue = playerAttributes.MaxStamina;

            uiManager.playerHealthSlider.value = playerAttributes.CurHealth;
            uiManager.playerStaminaSlider.value = playerAttributes.CurStamina;
        }

        public void UpdateHealthStaminaGUI()
        {
            uiManager.playerHealthSlider.maxValue = playerAttributes.MaxHealth;
            uiManager.playerStaminaSlider.maxValue = playerAttributes.MaxStamina;
            uiManager.playerManaSlider.maxValue = playerAttributes.MaxMana;

            uiManager.playerHealthSlider.value = playerAttributes.CurHealth;
            uiManager.playerStaminaSlider.value = playerAttributes.CurStamina;
            uiManager.playerManaSlider.value = playerAttributes.CurMana;
        }

        public override void Update()
        {
            base.Update();
        }

        public void LateUpdate()
        {
            uiManager.playerHealthSlider.value = playerAttributes.CurHealth;
            uiManager.playerStaminaSlider.value = playerAttributes.CurStamina;
            uiManager.playerManaSlider.value = playerAttributes.CurMana;
        }

        /// <summary>
        /// Enablest the Hit UI
        /// </summary>
        public void OnPlayerHits()
        {
            uiManager.crosshairHit.SetActive(true);
            Invoke("OnHitEnds", RCKSettings.CROSSHAIR_HIT_TIME);

            // Play hit sound
            int n = Random.Range(1, hitSounds.Length);

            if(hitSounds[n] != null)
                GameAudioManager.instance.PlayOneShot(AudioSources.UISounds, hitSounds[n]);
        }

        void OnHitEnds()
        {
            uiManager.crosshairHit.SetActive(false);
        }

        public void FadeScreen(bool fadeOut, float duration = 1.5f)
        {
            if (fadeOut)
                StartCoroutine(FadeOutTask(duration));
            else
                StartCoroutine(FadeInTask(duration));
        }

        private IEnumerator FadeOutTask(float _duration)
        {
            float currentTime = 0f;
            while (currentTime < _duration)
            {
                float alpha = Mathf.Lerp(1f, 0f, currentTime / _duration);
                fadeInOutImage.color = new Color(fadeInOutImage.color.r, fadeInOutImage.color.g, fadeInOutImage.color.b, alpha);
                currentTime += Time.unscaledDeltaTime;
                yield return null;
            }

            fadeInOutImage.color = new Color(fadeInOutImage.color.r, fadeInOutImage.color.g, fadeInOutImage.color.b, 0f);

            yield break;
        }

        private IEnumerator FadeInTask(float _duration)
        {
            float currentTime = 0f;
            while (currentTime < _duration)
            {
                float alpha = Mathf.Lerp(0f, 1f, currentTime / _duration);
                fadeInOutImage.color = new Color(fadeInOutImage.color.r, fadeInOutImage.color.g, fadeInOutImage.color.b, alpha);
                currentTime += Time.unscaledDeltaTime;
                yield return null;
            }

            fadeInOutImage.color = new Color(fadeInOutImage.color.r, fadeInOutImage.color.g, fadeInOutImage.color.b, 1f);

            yield break;
        }

        public override void PlayerDeath()
        {
            base.PlayerDeath();
        }
    }
}
