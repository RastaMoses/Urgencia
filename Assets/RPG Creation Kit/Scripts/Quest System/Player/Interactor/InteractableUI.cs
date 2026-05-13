using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGCreationKit;


namespace RPGCreationKit
{

    /// <summary>
    /// This class keeps the references of the OnScreen UI
    /// </summary>
    public class InteractableUI : MonoBehaviour
    {
        [Header("General")]
        public TextMeshProUGUI Interact_text;
        public GameObject compassGameObject;
        public GameObject crosshairGameObject;
        public GameObject crosshairHit;

        [Space(5)]

        [Header("Dialogue System")]
        public GameObject DialogueContainer;
        public GameObject PlayerQuestionsContainer;
        public TextMeshProUGUI NPC_Name;
        public TextMeshProUGUI NPC_Line;
        public TextMeshProUGUI heardDialogues;

        [Space(3)]

        public Transform PlayerQuestionsContent;

        [Space(8)]
        public GameObject InteractiveObjectUIContainer;
        public Transform InteractiveOptionContent;
        public Text InteractiveObjectName;
        public Text InteractiveObjectDescription;


        [Space(8)]
        [Header("Prefabs")]
        public GameObject PlayerQuestionPrefab;
        public GameObject InteractiveOptionUIPrefab;

        public void OnHit()
        {
            crosshairHit.SetActive(true);
            Invoke("OnHitEnds", RCKSettings.CROSSHAIR_HIT_TIME);
        }

        void OnHitEnds()
        {
            crosshairHit.SetActive(false);
        }
    }
}