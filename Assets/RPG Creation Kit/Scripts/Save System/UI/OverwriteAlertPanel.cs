using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RPGCreationKit;
using UnityEngine.EventSystems;

namespace RPGCreationKit
{
    public class OverwriteAlertPanel : MonoBehaviour
    {
        public TextMeshProUGUI description;
        public GameObject previouslySelectedObject;

        public void OnConfirm()
        {

            if(RckInput.isUsingGamepad)
            {
                if(previouslySelectedObject != null)
                    EventSystem.current.SetSelectedGameObject(previouslySelectedObject);
            }
        }

        public void OnCancel()
        {

            if (RckInput.isUsingGamepad)
            {
                if (previouslySelectedObject != null)
                    EventSystem.current.SetSelectedGameObject(previouslySelectedObject);
            }
        }
    }
}