using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RS_SpawnVeraAfterQuest : ResultScript
    {
        void Start()
        {
            // Your code here
            RCKFunctions.SpawnAIInDistantCell("Vera001", "Virrihael(0,2)", new Vector3(14f, .5f, 250f), Quaternion.Euler(0, -154f, 0));

            // Destroy the script
            Destroy(this);
        }
    }
}