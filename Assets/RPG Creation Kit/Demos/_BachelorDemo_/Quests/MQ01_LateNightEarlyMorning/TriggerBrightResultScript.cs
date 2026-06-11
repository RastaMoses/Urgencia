using RPGCreationKit;
using RPGCreationKit.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class TriggerBrightResultScript : ResultScript
    {
        void Start()
        {
            // Your code here
            RckPlayer.instance.DisplayHeardLine("How is it so bright already?!", 5f);

            // Destroy the script
            Destroy(this);
        }
    }
}