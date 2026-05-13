using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;


namespace RPGCreationKit
{

    /// <summary>
    /// Class used in the Dialogue System, this class allows the player to ask questions
    /// and receive NPC_DialogueLines as answer. Combine them to branch your quests.
    /// </summary>
    [System.Serializable]
    public class PlayerQuestion 
    {
        [TextArea] public string Question;      // The question
        public string qID;                      // ID of the question, used to reference questions
        public int Position;                    // The position in the Questions List
        public bool DeleteAfterAnswer = false;  // Should we delete this question after we've got the answer to it?
    }
}
