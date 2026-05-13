using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RPGCreationKit
{
    public class UISliderValueDisplayer : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI _text;


        public void Update()
        {
            _text.text = slider.value.ToString("F2");
        }
    }
}