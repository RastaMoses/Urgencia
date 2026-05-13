using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;


namespace RPGCreationKit
{
    /// <summary>
    /// The Navigation Menu used in the QuestManagerUI, allows us to switch tabs
    /// </summary>
    public class NavigationMenu : MonoBehaviour
    {

        // Index for showing/hiding tabs
        [SerializeField] private int index = 0;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        /// <summary>
        /// Called by the toggles to switch tabs
        /// </summary>
        public void TriggeredToggle(Toggle t)
        {
            if (t.isOn)
                t.GetComponent<NavigationElement>().Target.SetActive(true);
            else
                t.GetComponent<NavigationElement>().Target.SetActive(false);
        }

    }
}