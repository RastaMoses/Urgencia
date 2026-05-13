using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;


namespace RPGCreationKit
{
    /// <summary>
    /// A tab for the QuestManagerUI, keeps data of Texts and transforms of the canvas
    /// </summary>
    public class QuestManagerUI_QuestTab : MonoBehaviour
    {

        public Text questNameHeader;
        public Text questObjectivesHeader;
        public Text questDescription;

        [Space(5)]

        public Transform QuestsContent;
        public Transform QuestObjectivesContent;
    }
}
