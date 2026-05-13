using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit.BehaviourTree.Data;
using RPGCreationKit.AI;

namespace RPGCreationKit.BehaviourTree.Data
{
    public static class BTReference
    {
        public static BTVariable SolveReference(RPGCK_BT graph, string varName, RckAI _ai)
        {
            string guidToFind = "";

            for (int i = 0; i < graph.graphVariables.Count; i++)
            {
                if (graph.graphVariables[i].name == varName)
                {
                    guidToFind = graph.graphVariables[i].guidStr;
                    break;
                }
            }

            if (graph.IsCombatBehaviour == false)
            {
                if (_ai.btPurposeVarData.TryGetValue(guidToFind, out var lookingFor))
                {
                    return lookingFor;
                }
            }
            else
            {
                if (_ai.btCombatVarData.TryGetValue(guidToFind, out var lookingFor))
                {
                    return lookingFor;
                }
            }


            /*
            for(int i = 0; i < graph.graphVariables.Count; i++)
            {
                if (graph.graphVariables[i].name == varName)
                    return graph.graphVariables[i];
            }
            */

            return null;
        }
    }
}