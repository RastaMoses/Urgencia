using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit.Game.ResultScripts
{
    public class DescentMineshaftResultScript : ResultScript
    {
        void Start()
        {
            // Your code here
            RckPlayer.instance.transform.position = new Vector3(-26.0499992f, -11.9300003f, 11.5500002f);

            // Destroy the script
            Destroy(this);
        }
    }
}