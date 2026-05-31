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

            TutorialAlertMessage.instance.OpenMessage("This is the end of the demo for now\n\nCheckste");


            // Destroy the script
            Destroy(this);
        }
    }
}