using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit.BehaviourTree;
using XNode;
using XNodeEditor;
using UnityEditor;
using RPGCreationKit.AI;


namespace RPGCreationKit.BehaviourTree
{
    [CustomNodeGraphEditor(typeof(RPGCK_BT))]
    public class RPGCK_BTEditor : NodeGraphEditor
    {
        public RckAI ai;

        public Rect windowRect = new Rect(100, 100, 200, 200);

        public bool isDebugging = false;
        bool isDebuggingCombatTree = false;

        RPGCK_BT thisbt;
        BTWindowInspector thisInspector;
        PurposeNodesDictionary aiNodes;

        public override void OnOpen()
        {
            base.OnOpen();
            this.window.titleContent = new GUIContent(this.window.graph.name + " (Behaviour Tree)");

            thisbt = this.target as RPGCK_BT;

            thisInspector = EditorWindow.GetWindow<BTWindowInspector>("BT Inspector");
            thisInspector.ShowWindow(thisbt, this);

            // Reset m_NodeDebugState
            foreach (Node node in thisbt.nodes)
            {
                BTNode btNode = node as BTNode;
                if (btNode)
                    btNode.m_NodeDebugState = NodeState.Null;
            }
        }


        public override void OnGUI()
        {
            base.OnGUI();

            if (Application.isPlaying)
            {
                ai = (RckAI)EditorGUILayout.ObjectField("AI Target", ai, typeof(RckAI), true);

                if (GUILayout.Button("Attach to debug"))
                {
                    if (ai != null)
                    {
                        // Check whether the current BTreeID is the same as the AI's
                        if (ai.currentBehaviour.ID == thisbt.ID)
                        {
                            EditorDialog.DisplayAlertDialog("Debug AI Behavior", "Starting Debug!", "Ok", DialogIconType.Info);
                            isDebuggingCombatTree = thisbt.IsCombatBehaviour;

                            if (thisInspector)
                                thisInspector.InitDebug(ai, isDebuggingCombatTree);

                            isDebugging = true;
                            aiNodes = (!isDebuggingCombatTree) ? ai.purposeNodesData : ai.combatNodesData;
                        }
                        else
                        {
                            EditorDialog.DisplayAlertDialog("Debug AI Behavior", "This Behavior is not the one the selected AI is currently running.", "Ok", DialogIconType.Error);
                        }
                    }
                    else
                        EditorDialog.DisplayAlertDialog("Debug AI Behavior", "You need to assign the AI to debug first.", "Ok", DialogIconType.Error);
                }

                if (isDebugging)
                {
                    // Color the nodes
                    foreach (Node node in thisbt.nodes)
                    {
                        BTNode btNode = node as BTNode;

                        if (btNode)
                            btNode.m_NodeDebugState = aiNodes[node.guidStr].m_NodeState;
                    }
                }

                window.Repaint();
            }
            else
            {
                if (isDebugging)
                {
                    // cleanup
                    foreach (Node node in thisbt.nodes)
                    {
                        BTNode btNode = node as BTNode;

                        if (btNode)
                            btNode.m_NodeDebugState = NodeState.Null;
                    }
                }
                isDebugging = false;
                if (thisInspector)
                    thisInspector.StopDebug();
            }
        }
    }
}
