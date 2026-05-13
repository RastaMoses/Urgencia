using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class EffectOnEntityUI : MonoBehaviour
    {
        [SerializeField] Image image;
        EffectOnEntity effect;

        [SerializeField] Slider slider;

        bool isOnDuration = false;

        public void Init(EffectOnEntity _effect, bool _isOnDuration)
        {
            effect = _effect;
            isOnDuration = _isOnDuration;

            if (effect.effectIconID != "NONE")
                effect.effectIcon = IconsDatabase.GetItem(effect.effectIconID);

            if (effect.effectIcon != null)
                image.sprite = effect.effectIcon;

            if (isOnDuration)
                slider.maxValue = effect.duration;
            else
                slider.maxValue = effect.magnitude;

            slider.minValue = 0;
        }

        private void Update()
        {
            if (isOnDuration)
                slider.value = effect.duration - effect.magnitudeAlreadyApplied;
            else
                slider.value = effect.magnitude - effect.magnitudeAlreadyApplied;

            if(slider.value <= 0)
            {
                slider.value = 0;
                gameObject.SetActive(false);
            }
        }
    }
}