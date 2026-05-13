using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit.BehaviourTree;


namespace RPGCreationKit.DialogueSystem
{
    [CreateNodeMenu("Dialogue System/Comment", order = 3)]
    [NodeTint("#044d01")]
    public class CommentNode : Node
    {
        [TextArea] [SerializeField] string content;
    }
}