using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;


namespace RPGCreationKit
{

    /// <summary>
    /// Wrap Lists of QuestDealer, QuestUpdater and Consequences
    /// </summary>
    [System.Serializable]
    public class Events
    {
        //[Space(15)]

        //public Condition[] conditions;

        [Space(15)]

        [SerializeField] public List<QuestDealer> questDealers = new List<QuestDealer>();

        [Space(15)]

        [SerializeField] public List<QuestUpdater> questUpdaters = new List<QuestUpdater>();

        [Space(15)]

        [SerializeField] public List<Consequence> consequences = new List<Consequence>();


        public bool EvaluateConditions()
        {
            return true;
            //return RCKFunctions.VerifyConditions(conditions);
        }

    }
}