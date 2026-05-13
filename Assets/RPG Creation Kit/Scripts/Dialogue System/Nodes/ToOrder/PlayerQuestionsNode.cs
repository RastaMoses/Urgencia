using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit;

namespace RPGCreationKit.DialogueSystem
{
    [CreateNodeMenu("Dialogue System/Player Questions", order = 2)]
    public class PlayerQuestionsNode : DialogueNode
    {
        public string dialogueAnimationListeningStr = string.Empty;

        public bool removePreviousQuestions = false;
        public string[] questionsToRemove = new string[0];
        [Output(dynamicPortList = true)] public List<PlayerQuestion> playerQuestions;

        // Use this for initialization
        protected override void Init()
        {
            base.Init();

        }

        public override void Trigger()
        {

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            return null; // Replace this
        }
    }
}