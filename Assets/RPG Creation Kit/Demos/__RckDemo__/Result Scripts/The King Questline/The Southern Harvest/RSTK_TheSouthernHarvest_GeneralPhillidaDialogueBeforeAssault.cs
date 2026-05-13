using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSTK_TheSouthernHarvest_GeneralPhillidaDialogueBeforeAssault : ResultScript
    {

        void Start()
        {
            StartCoroutine(ScriptExecution());
        }

        public IEnumerator ScriptExecution()
        {
            // Spawn Red Shade wave behind the mountain, they'll reach the outpos

            RCKFunctions.SetQuestStage("TKQL_TheSouthernHarvest", 50);
            RCKFunctions.CompleteQuestStage("TKQL_TheSouthernHarvest", 40);

            RckAI vera = RCKFunctions.SpawnAIInCell("TKQL_Vera", "Virrihael(-1,2)", new Vector3(-138.49f, 0f, 266.2f), Quaternion.identity);
            RckAI martin = RCKFunctions.SpawnAIInCell("TKQL_Martin", "Virrihael(-1,2)", new Vector3(-138.49f, 0f, 269f), Quaternion.identity);

            RckAI foll1 = RCKFunctions.SpawnAIInCell("RedShadeFollower001", "Virrihael(-1,2)", new Vector3(-136f, 0f, 266f), Quaternion.identity);
            RckAI foll2 = RCKFunctions.SpawnAIInCell("RedShadeFollower002", "Virrihael(-1,2)", new Vector3(-136f, 0f, 269f), Quaternion.identity);

            vera.SetNewBehaviourTree(false, "BTree_PReachGuardsOutpost001");
            martin.SetNewBehaviourTree(false, "BTree_PReachGuardsOutpost001");
            foll1.SetNewBehaviourTree(false, "BTree_PReachGuardsOutpost001");
            foll2.SetNewBehaviourTree(false, "BTree_PReachGuardsOutpost001");

            vera.SwitchBehaviourTree(false);
            martin.SwitchBehaviourTree(false);
            foll1.SwitchBehaviourTree(false);
            foll2.SwitchBehaviourTree(false);

            // Adjust health for foll1 and foll2
            //foll1.attributes.MaxHealth = 250;
            //foll1.attributes.CurHealth = 250;

            //foll2.attributes.MaxHealth = 250;
            //foll2.attributes.CurHealth = 250;

            // Destroy the script
            Destroy(this);
            yield return null;
        }
    }
}