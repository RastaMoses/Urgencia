using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RPGCreationKit;


namespace RPGCreationKit
{

    /// <summary>
    /// Simple class that contains a Quest field, used in the Events to add quests to the Quest Manager
    /// </summary>
    [Serializable]
    public class QuestDealer
    {
        public Quest newQuest;
        public int atIndex = 0;
        public bool hideAlert = false;
    }
}
