using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;





namespace RPGCreationKit
{
    // What the player can do with objects
    public enum InteractiveOptions { Use, Sleep, Exit, Take };

    /// <summary>
    /// ScriptableObject that allows us to create new Interactive Objects
    /// </summary>
    [CreateAssetMenu(fileName = "New Interactive Object", menuName = "RPG Creation Kit/Interactive Objects/New Interactive Object", order = 1)]
    public class InteractiveObject : ScriptableObject
    {
        // Base data for the Interactive Objects
        public string Name;
        public string Action; //Enter, take, read, etc
        [TextArea] public string Description;

        // Options for this object
        public InteractiveOptions[] InteractiveOptions;

        public bool openUIEvenWithOneOption = false;
    }

}