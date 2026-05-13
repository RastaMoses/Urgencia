using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    /// <summary>
    /// Class used by buttons when the player is interacting with an InteractiveObject
    /// </summary>
    public class InteractiveObjectUI : MonoBehaviour
    {
        // References and ID
        [HideInInspector] public int id = -1;
        [HideInInspector] public InteractiveObject_Behaviour Object_Behaviour;
        [HideInInspector] public InteractiveOptions option;
        public Text text;

        /// <summary>
        /// Called when the Button is instantiated
        /// </summary>
        /// <param name="obj_behaviour">The obj behaviour</param>
        /// <param name="obj">The ScriptableObject reference</param>
        /// <param name="_option">The current option</param>
        public void Init(InteractiveObject_Behaviour obj_behaviour, InteractiveObject obj, InteractiveOptions _option)
        {
            // Set all values
            Object_Behaviour = obj_behaviour;
            option = _option;
            text.text = option.ToString();

            // Add listener to perform actions
            gameObject.GetComponent<Button>().onClick.AddListener(() => DoAction());
        }

        /// <summary>
        /// Called by the button in the same GameObject of this script
        /// </summary>
        public void DoAction()
        {
            RckPlayer.instance.DoInteractiveAction(Object_Behaviour, option);
        }
    }
}