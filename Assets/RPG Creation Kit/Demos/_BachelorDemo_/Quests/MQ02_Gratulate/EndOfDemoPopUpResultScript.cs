using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.Game.ResultScripts
{
    public class EndOfDemoPopUpResultScript : ResultScript
    {
        void Start()
        {
            // Your code here

            TutorialAlertMessage.instance.OpenMessage("This is the end of the demo for now\n\nYou have played for exactly this amount of time \n" + FindAnyObjectByType<GameTimer>().GetFormattedTime());


            // Destroy the script
            Destroy(this);
        }
    }
}