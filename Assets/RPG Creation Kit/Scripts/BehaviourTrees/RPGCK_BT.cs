using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.BehaviourTree.Data;
using RPGCreationKit.AI;
using RPGCreationKit.DialogueSystem;
using UnityEditor;

namespace RPGCreationKit.BehaviourTree
{
    public enum TreeTickRate
    {
        EveryFrame = 0,
        EveryXFrames = 1,
        EveryXSeconds_Realtime = 2,
        EveryXSeconds_GameTime = 3
    };

    [CreateAssetMenu(fileName = "New BehaviourTree", menuName = "RPG Creation Kit/BehaviourTrees/New BehaviourTree", order = 1)]
    [System.Serializable]
    public class RPGCK_BT : NodeGraph
    {
        public bool _IS_DEMO_FILE = true;

        public bool hasBeenAsyncCopied = false;
        public string ID;
        public bool IsCombatBehaviour = false;

        public bool isSpecificBehaviourTree = false;
        public bool resetVariablesUponStartResume = false;

        //public GameObject owner;
        //public RckAI ai;

        [SerializeField] public List<BTVariable> graphVariables = new List<BTVariable>();

        [Space(10)]

        [TextArea(30, 30)]
        public string Description;

        public IEnumerator RPGCK_BTDeepCopyAsync(GameObject _owner, RckAI _ai, RPGCK_BT graph)
        {
            graph.hasBeenAsyncCopied = false;

            //graph.ai = _ai;
            //graph.owner = _owner;

            // Step 2: Copy nodes
            graph.nodes = new List<Node>(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null)
                {
                    graph.nodes.Add(null);
                    continue;
                }

                Node.graphHotfix = graph;
                Node node = Instantiate(nodes[i]);
                node.graph = graph;
                graph.nodes.Add(node);

                // Yield periodically to prevent frame hang
                if (i % 10 == 0) yield return null;
            }

            // Step 3: Redirect connections
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                if (graph.nodes[i] == null) continue;

                foreach (NodePort port in graph.nodes[i].Ports)
                {
                    port.Redirect(nodes, graph.nodes);
                }

                // Yield periodically to prevent frame hang
                if (i % 10 == 0) yield return null;
            }

            // Step 4: Copy graph variables
            graph.graphVariables = new List<BTVariable>(graphVariables.Count);
            for (int i = 0; i < graphVariables.Count; i++)
            {
                if (graphVariables[i] == null)
                {
                    graph.graphVariables.Add(null);
                    continue;
                }

                BTVariable variable = Instantiate(graphVariables[i] as BTVariable);
                variable.name = graphVariables[i].name; // Fix the name
                graph.graphVariables.Add(variable);

                // Yield periodically to prevent frame hang
                if (i % 10 == 0) yield return null;
            }

            graph.hasBeenAsyncCopied = true;
        }

        /*
        public void RPGCK_RuntimeNodeAssign(GameObject _owner, RckAI _ai)
        {
            if (IsCombatBehaviour == false)
            {
                _ai.purposeNodesData.Clear();
                _ai.purposeNodes.Clear();
                _ai.purposeNodesIdx = 0;

                // Assign the nodes
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i] == null) continue;

                    _ai.purposeNodes.Add(new BTNode.RuntimeBTNodeData(IsCombatBehaviour, false, NodeState.Null, false));

                    _ai.purposeNodesData.Add(nodes[i].guidStr, _ai.purposeNodes[_ai.purposeNodesIdx]);
                    _ai.purposeNodesIdx++;
                }

                // Instantiate the variables
                for (int i = 0; i < graphVariables.Count; i++)
                {
                    if (graphVariables[i] == null) continue;
                    BTVariable variable = Instantiate(graphVariables[i] as BTVariable);

                    // Ensure that the name of the copied variables are the same as the original one (remove (Clone))
                    variable.name = graphVariables[i].name;
                    variable.guidStr = graphVariables[i].guidStr;

                    _ai.allPurposeBTVariables.Add(variable);
                    _ai.btPurposeVarData.Add(variable.guidStr, _ai.allPurposeBTVariables[_ai.purposeBtVarIndx]);
                    _ai.purposeBtVarIndx++;
                }
            }
            else
            {
                _ai.combatNodesData.Clear();
                _ai.combatNodes.Clear();
                _ai.combatNodesIdx = 0;

                // Assign the nodes
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i] == null) continue;

                    _ai.combatNodes.Add(new BTNode.RuntimeBTNodeData(IsCombatBehaviour, false, NodeState.Null, false));

                    _ai.combatNodesData.Add(nodes[i].guidStr, _ai.combatNodes[_ai.combatNodesIdx]);
                    _ai.combatNodesIdx++;
                }

                _ai.allCombatBTVariables.Clear();
                _ai.btCombatVarData.Clear();
                _ai.combatBtVarIndx = 0;

                // Instantiate the variables
                for (int i = 0; i < graphVariables.Count; i++)
                {
                    if (graphVariables[i] == null) continue;
                    BTVariable variable = Instantiate(graphVariables[i] as BTVariable);

                    // Ensure that the name of the copied variables are the same as the original one (remove (Clone))
                    variable.name = graphVariables[i].name;
                    variable.guidStr = graphVariables[i].guidStr;

                    _ai.allCombatBTVariables.Add(variable);
                    _ai.btCombatVarData.Add(variable.guidStr, _ai.allCombatBTVariables[_ai.combatBtVarIndx]);
                    _ai.combatBtVarIndx++;
                }
            }
        }
        */

        public void RPGCK_RuntimeNodeAssign(GameObject _owner, RckAI _ai)
        {
            if (IsCombatBehaviour == false)
            {
                // Assign the nodes
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i] == null) continue;

                    if(_ai.purposeNodesData.ContainsKey(nodes[i].guidStr))
                        _ai.purposeNodesData.Remove(nodes[i].guidStr);

                    _ai.purposeNodesData.Add(nodes[i].guidStr, new BTNode.RuntimeBTNodeData(IsCombatBehaviour, false, NodeState.Null, false));
                }

                // Instantiate the variables
                for (int i = 0; i < graphVariables.Count; i++)
                {
                    if (graphVariables[i] == null) continue;

                    if(_ai.btPurposeVarData.ContainsKey(graphVariables[i].guidStr))
                        _ai.btPurposeVarData.Remove(graphVariables[i].guidStr);

                    BTVariable variable = Instantiate(graphVariables[i] as BTVariable);

                    // Ensure that the name of the copied variables are the same as the original one (remove (Clone))
                    variable.name = graphVariables[i].name;
                    variable.guidStr = graphVariables[i].guidStr;

                    _ai.btPurposeVarData.Add(variable.guidStr, variable);
                }
            }
            else
            {
                // Assign the nodes
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i] == null) continue;

                    if (_ai.combatNodesData.ContainsKey(nodes[i].guidStr))
                        _ai.combatNodesData.Remove(nodes[i].guidStr);

                    _ai.combatNodesData.Add(nodes[i].guidStr, new BTNode.RuntimeBTNodeData(IsCombatBehaviour, false, NodeState.Null, false));
                }

                // Instantiate the variables
                for (int i = 0; i < graphVariables.Count; i++)
                {
                    if (graphVariables[i] == null) continue;

                    if (_ai.btCombatVarData.ContainsKey(graphVariables[i].guidStr))
                        _ai.btCombatVarData.Remove(graphVariables[i].guidStr);

                    BTVariable variable = Instantiate(graphVariables[i] as BTVariable);

                    // Ensure that the name of the copied variables are the same as the original one (remove (Clone))
                    variable.name = graphVariables[i].name;
                    variable.guidStr = graphVariables[i].guidStr;

                    _ai.btCombatVarData.Add(variable.guidStr, variable);
                }
            }
        }

        public RPGCK_BT RPGCK_BTCopy(GameObject _owner, RckAI _ai)
        {
            //ai = _ai;
            //owner = _owner;

            // Instantiate a new nodegraph instance
            RPGCK_BT graph = Instantiate(this);


            // Instantiate all nodes inside the graph
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null) continue;
                Node.graphHotfix = graph;
                Node node = Instantiate(nodes[i]) as Node;
                node.graph = graph;
                graph.nodes[i] = node;
            }

            // Redirect all connections
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                if (graph.nodes[i] == null) continue;
                foreach (NodePort port in graph.nodes[i].Ports)
                {
                    port.Redirect(nodes, graph.nodes);
                }
            }

            // Copy graphVariables as well
            for (int i = 0; i < graphVariables.Count; i++)
            {
                if (graphVariables[i] == null) continue;
                BTVariable variable = Instantiate(graphVariables[i] as BTVariable);

                // Ensure that the name of the copied variables are the same as the original one (remove (Clone))
                variable.name = graphVariables[i].name;
                graph.graphVariables[i] = variable;
            }

            return graph;
        }

        [ContextMenu("Reorder")]
        public void ReorderTree()
        {
            for(int i = 0; i < nodes.Count; i++)
            {
                if(nodes[i] != null && nodes[i] is BTNode)
                    ((BTNode)nodes[i]).ReorderChild();
            }
        }

        public void ResetVariables()
        {
            for(int i = 0; i < graphVariables.Count; i++)
            {
                graphVariables[i].SetDefaultValue();
            }
        }

        [ContextMenu("Debug InstanceID")]
        public void DebugInstanceID()
        {
            Debug.Log(this.GetInstanceID());
        }

#if UNITY_EDITOR

        [ContextMenu("Update Behavior")]
        public void UpdateChildNodesRef()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null)
                    continue;

                if (((nodes[i]) is BTNode))
                {
                    ((nodes[i]) as BTNode).guidStr = System.Guid.NewGuid().ToString();
                    ((nodes[i]) as BTNode).isInCombatBehavior = IsCombatBehaviour;
                    EditorUtility.SetDirty(nodes[i]);
                }
            }

            AssetDatabase.SaveAssets();
        }
#endif
    }
}