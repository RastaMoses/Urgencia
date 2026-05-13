using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.Game.ResultScripts
{
    public class SQ_EveryDeadMan_SendThiefIntoOblivion : ResultScript
    {
        void Start()
        {
            // Your code here
            RCKFunctions.SendIntoOblivion("ThiefOfTheDead001");

            // Destroy the script
            Destroy(this);
        }
    }
}