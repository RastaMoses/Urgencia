using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.Game.ResultScripts
{
    public class TutorialSetTimeToZero : ResultScript
    {
        void Start()
        {
            // We start at night
            TimeOfDayManager.instance.SetTime(0.0f);

            RCKFunctions.MutateMutable("Mutable_SetTimeTo0BeforeGoingOut", false);

            // Destroy the script
            Destroy(this);
        }
    }
}
