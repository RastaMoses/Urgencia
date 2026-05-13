using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGCreationKit
{
    public class UICategorySelection : MonoBehaviour
    {
        public void TriggeredToggle(Toggle t)
        {
            if (t.isOn)
                t.GetComponent<UITab>().targetUI.SetActive(true);
            else
                t.GetComponent<UITab>().targetUI.SetActive(false);
        }

    }
}