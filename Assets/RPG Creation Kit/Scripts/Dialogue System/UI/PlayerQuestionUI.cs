using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    /// <summary>
    /// The player question in the Canvas, contains data about the Dialogue and the answer
    /// </summary>
    public class PlayerQuestionUI : MonoBehaviour
    {
        [HideInInspector] public PlayerQuestion question;
        [HideInInspector] public IDialoguable dialoguable;
        [HideInInspector] public IDialoguable[] dialoguables;
        [HideInInspector] public XNode.Node response;
        [HideInInspector] public bool DestroyAfter;
        public int position;

        public TextMeshProUGUI text;


        /// <summary>
        /// Init the gameobject by set up data and Listener for the Button
        /// </summary>

        public void Init(PlayerQuestion _question, IDialoguable _dialoguable, RPGCreationKit.DialogueSystem.DialogueNode _response, string _text, bool _destroyAfter, int _position, bool isNpcToNpcDialogue = false, IDialoguable[] _dialoguables = null)
        {
            question = _question;
            dialoguable = _dialoguable;
            dialoguables = _dialoguables;
            response = _response;
            text.text = _text;
            DestroyAfter = _destroyAfter;
            position = _position;

            if(!isNpcToNpcDialogue)
                gameObject.GetComponent<Button>().onClick.AddListener(() => AskQuestion());
            else
                gameObject.GetComponent<Button>().onClick.AddListener(() => AskQuestionNpcToNpcDialogue());

        }

        /// <summary>
        /// Calls the Interactor.AskQuestion() passing the PlayerQuestionUI parameters
        /// </summary>
        public void AskQuestion()
        {
            RckPlayer.instance.AskQuestion(dialoguable, response, DestroyAfter, gameObject);
        }

        public void AskQuestionNpcToNpcDialogue()
        {
            RckPlayer.instance.AskQuestion(dialoguable, response, DestroyAfter, gameObject, true, dialoguables);
        }
    }
}
