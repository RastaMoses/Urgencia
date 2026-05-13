using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.Game.ResultScripts
{
    public class MQRS_UnlockCityGate : ResultScript
    {
        void Start()
        {
            // Your code here
            RCKFunctions.UnlockDoor("MainDoorToCityInterior");

            // Destroy the script
            Destroy(this);
        }
    }
}