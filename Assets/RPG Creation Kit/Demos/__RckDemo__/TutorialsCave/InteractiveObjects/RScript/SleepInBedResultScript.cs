using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit.Game.ResultScripts
{
    public class SleepInBedResultScript : ResultScript
    {
        void Start()
        {
            // Your code here
            WaitUIManager.instance.OpenCloseWaitUIBySleeping();

            // Destroy the script
            Destroy(this);
        }
    }
}