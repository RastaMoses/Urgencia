using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;

namespace RPGCreationKit.DialogueSystem
{
    [CreateAssetMenu(fileName = "New DialogueGraph", menuName = "RPG Creation Kit/Dialogues/New DialogueGraph", order = 1)]
    public class DialogueGraph : NodeGraph
    {
        public bool _IS_DEMO_FILE = true;

        public string ID;

        public EntryNode GetEntryNode()
        {
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i] is EntryNode) return nodes[i] as EntryNode;

            //Debug.LogWarning("Tried to get Entry Node of Dialogue " + this.name + " but no Entry Node has been found. Assure that one and only one EntryNode exist.");
            return null;
        }

        public EndNode GetEndNode()
        {
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i] is EndNode) return nodes[i] as EndNode;

            //Debug.LogWarning("Tried to get Entry Node of Dialogue " + this.name + " but no Entry Node has been found. Assure that one and only one EntryNode exist.");
            return null;
        }
    }
}