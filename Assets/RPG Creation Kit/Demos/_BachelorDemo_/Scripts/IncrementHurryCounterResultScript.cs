using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.Game.ResultScripts
{
    public class IncrementHurryCounterResultScript : ResultScript
    {
        void Start()
        {
            // Your code here
            FindAnyObjectByType<HurryCounter>().IncrementHurryCount();

            // Destroy the script
            Destroy(this);
        }
    }
}