using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGCreationKit;
using UnityEngine.EventSystems;

namespace RPGCreationKit
{
    public class EffectInCharacterTab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Image image;
        [SerializeField] TextMeshProUGUI effectName;
        [SerializeField] TextMeshProUGUI effectMagnitude;

        [SerializeField] GameObject tooltipGameObject;
        [SerializeField] TextMeshProUGUI tooltipText;


        EffectOnEntity effect;

        bool updateText;

        public void Init(EffectOnEntity _effect, bool isOnDuration)
        {
            effect = _effect;

            image.sprite = effect.effectIcon;
            effectName.text = effect.effectType.ToString();
            effectMagnitude.text = ((int)effect.magnitude).ToString();

            // Tooltip?
        }

        private void Update()
        {
            if (effect.isFinished)
                gameObject.SetActive(false);

            if(updateText)
                tooltipText.text = (effect.isOnDuration) ?
               "Time left: " + ((int)(effect.duration - effect.magnitudeAlreadyApplied)).ToString() :
               "Applied: " + ((int)effect.magnitudeAlreadyApplied + "/" + (int)effect.magnitude);
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            tooltipGameObject.SetActive(true);
            updateText = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tooltipGameObject.SetActive(false);
            updateText = false;
        }
    }
}