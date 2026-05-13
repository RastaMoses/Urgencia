using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit;

namespace RPGCreationKit.DialogueSystem
{
    public enum AfterLine
    {
        NPC_DialogueLine = 0,
        PlayerQuestions = 1,
        EndDialogue = 2,
        Continue = 3
    };

    [CreateNodeMenu("Dialogue System/NPC Dialogue Line", order = 1)]
    public class NPCDialogueLineNode : DialogueNode 
	{
        public static string[] DIALOGUE_ANIMATIONS =
       {
            "Listening_Standing",
            "Listening_Crossarms",
            "Talking_Standing001",
            "Talking_Standing002",
            "Talking_Crossarms"
        };


        public int speakerID = 0;

        [TextArea] public string line;
        public bool plainLine;

        public bool useLenghtOfClip;
        public AudioClip audioClip;
        public float lineTime = 5;
        public string dialogueAnimationStr = string.Empty;
        public string dialogueAnimationListeningStr = string.Empty;

        public AfterLine afterLine;

        [Output] public NPCDialogueLineNode nextLine;

        public bool removePreviousQuestions = false;
        public string[] questionsToRemove = new string[0];
        [Output(dynamicPortList = true, backingValue = ShowBackingValue.Never)] public List<PlayerQuestion> playerQuestions;
        public int lookAtEntityID;

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