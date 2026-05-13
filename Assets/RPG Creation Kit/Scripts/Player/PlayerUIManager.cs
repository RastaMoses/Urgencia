using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RPGCreationKit.Player
{
    /// <summary>
    /// Contains references to UI components and methods to modify/interact with them
    /// </summary>
    public class PlayerUIManager : MonoBehaviour
    {
        [Header("General")]
        public Slider playerHealthSlider;
        public Slider playerStaminaSlider;
        public Slider playerManaSlider;
        public Slider playerXpSlider;

        public TextMeshProUGUI Interact_text;
        public GameObject compassGameObject;
        public GameObject horCompassGameObject;
        public GameObject crosshairGameObject;
        public GameObject crosshairHit;
        public GameObject stealItemIcon;
        public GameObject takeItemIcon;


        [Header("Dialogue System")]
        public GameObject dialogueContainer;
        public GameObject playerQuestionsContainer;
        public TextMeshProUGUI npc_Name;
        public TextMeshProUGUI npc_Line;
        public TextMeshProUGUI heardDialogues;
        public Transform playerQuestionsContent;

        [Header("Combat UI")]
        public GameObject enemyHealthContainer;
        public Slider enemyHealthSlider;
        public TextMeshProUGUI enemyName;

        [Header("Prefabs")]
        public GameObject interactiveObjectUIContainer;
        public Transform interactiveOptionContent;
        public TextMeshProUGUI interactiveObjectName;
        public TextMeshProUGUI interactiveObjectDescription;

        [Header("Prefabs")]
        public GameObject playerQuestionPrefab;
        public GameObject interactiveOptionUIPrefab;


        [SerializeField] public List<PlayerQuestion> allPlayerQuestions = new List<PlayerQuestion>();


        private void Update()
        {
            if(RckPlayer.instance.isInConversation && RckInput.isUsingGamepad)
                PreventUnselectedQuestionWithGamepad();
        }


        public void OnPlayerHits()
        {
            crosshairHit.SetActive(true);
            Invoke("OnHitEnds", RCKSettings.CROSSHAIR_HIT_TIME);
        }

        void OnPlayerHitEnds()
        {
            crosshairHit.SetActive(false);
        }

        public void ReorderQuestions()
        {
            List<PlayerQuestionUI> questions = new List<PlayerQuestionUI>();

            foreach (Transform t in playerQuestionsContent)
                questions.Add(t.GetComponent<PlayerQuestionUI>());


            questions.Sort(SortByPosition);
            for (int i = 0; i < questions.Count; i++)
                questions[i].GetComponent<RectTransform>().SetSiblingIndex(questions[i].position);

            if(questions.Count > 0)
                EventSystem.current.SetSelectedGameObject(questions[0].gameObject);
        }

        public void PreventUnselectedQuestionWithGamepad()
        {
            if (EventSystem.current.currentSelectedGameObject == null && RckInput.isUsingGamepad)
            {
                if (playerQuestionsContent.childCount > 0 && playerQuestionsContent.GetChild(0) != null)
                    EventSystem.current.SetSelectedGameObject(playerQuestionsContent.GetChild(0).gameObject);
            }
        }

        public static int SortByPosition(PlayerQuestionUI p1, PlayerQuestionUI p2)
        {
            return p1.position.CompareTo(p2.position);
        }
    }
}