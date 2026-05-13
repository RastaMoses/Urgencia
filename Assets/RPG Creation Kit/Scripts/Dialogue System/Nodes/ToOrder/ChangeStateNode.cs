using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit;

namespace RPGCreationKit.DialogueSystem
{
    [CreateNodeMenu("Dialogue System/Change State", order = 4)]
    [NodeTint("#909a15")]
    public class ChangeStateNode : DialogueNode
    {
        public enum DialogueStateChange
        {
            Trade = 0,
            Loot = 1,
            Teleport = 2,
            ChangeCell = 3,
            SpeakToNPC = 4
        };

        public DialogueStateChange stateChange;

        public bool toCurrentNPC = false;
        public string npcRef = "";
        public string lootingPointRef = "";
        public RCKTransform coordinates;

        public CellsSystem.Cell cell;

        public bool interruptsDialogue = false;

        // Use this for initialization
        protected override void Init()
        {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            return null; // Replace this
        }

        public override void Trigger()
        {
            
        }
    }
}