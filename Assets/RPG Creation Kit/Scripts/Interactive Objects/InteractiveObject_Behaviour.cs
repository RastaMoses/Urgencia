using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;


namespace RPGCreationKit
{
    /// <summary>
    /// The InteractiveObject in the scene
    /// </summary>
    public class InteractiveObject_Behaviour : MonoBehaviour
    {

        // Reference to InteractiveObject base data
        public InteractiveObject interactiveObj;

        // Array of events, used with the InteractiveOptions to have Events to every options
        public Events[] events = new Events[0];

        // Array of result scripts
        public string[] resultScripts = new string[0];

        // Can we interact with the object
        public bool canInteract = true;

    }
}