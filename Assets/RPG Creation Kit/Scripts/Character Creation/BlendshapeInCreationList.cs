using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class BlendshapeInCreationList : MonoBehaviour
    {
        SkinnedMeshRenderer skinnedRenderer;
        int index;
        string name;

        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] public Slider slider;

        public bool usesHeadBlendshapes = false;
        HeadBlendshapesManager headBlendshapes;

        private void Start()
        {
            
        }

        public void AssignBlendshape(SkinnedMeshRenderer renderer, int _index, string _name)
        {
            skinnedRenderer = renderer;
            index = _index;
            name = _name;

            text.text = name;
            slider.value = skinnedRenderer.GetBlendShapeWeight(index);

            slider.onValueChanged.AddListener(ChangeShape);

            if (usesHeadBlendshapes)
                headBlendshapes = skinnedRenderer.GetComponent<HeadBlendshapesManager>();
        }

        public void ChangeShape(float arg)
        {
            skinnedRenderer.SetBlendShapeWeight(index, arg);
            headBlendshapes.AdjustChildBlendshapes();
        }
    }
}