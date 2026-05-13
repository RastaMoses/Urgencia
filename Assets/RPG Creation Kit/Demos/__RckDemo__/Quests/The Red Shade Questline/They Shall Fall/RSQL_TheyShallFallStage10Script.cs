using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Quests;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using System.Collections;

namespace RPGCreationKit.Quests
{
    public class RSQL_TheyShallFallStage10Script : QuestStageScript
    {
        private void Start()
        {
            // Your code here

            // Make smoke and fire particles appear in the castle
            RCKFunctions.MutateMutable("Mutable_TheyShallFall_FireAndSmoke", false);
            RCKFunctions.MutateMutable("Mutable_TheyShallFall_FireAndSmokeInside", false);
            RCKFunctions.MutateMutable("Mutable_TheyShallFall_QuestUpdateGoto", false);

            
            // Send the original guards into the oblivion
            RCKFunctions.SendIntoOblivion("CityGuardOutside001");
            RCKFunctions.SendIntoOblivion("CityGuardOutside002");

            // Kill everyone in the city
            KillEveryoneInTheCity();

            // Spawn the new guys in the city
            RCKFunctions.SpawnAIInCell("TheyShallFall_Ryan", "CityInteriorCell", new Vector3(3.52f, 0.2f, 24.77f), Quaternion.Euler(0, 1.6f, 0));

            RCKFunctions.SpawnAIInCell("TheyShallFall_CityGuardsInside1", "CityInteriorCell", new Vector3(8.79f, 0.2f, 22.14f), Quaternion.Euler(0, 74.63f, 0));
            RCKFunctions.SpawnAIInCell("TheyShallFall_RedShadeFollower1", "CityInteriorCell", new Vector3(12.79f, 0.2f, 23.14f), Quaternion.Euler(0, -109.31f, 0));

            RCKFunctions.SpawnAIInCell("TheyShallFall_CityGuardsInside2", "CityInteriorCell", new Vector3(-5.68f, 0.2f, 22.16f), Quaternion.Euler(0, -292.823f, 0));
            RCKFunctions.SpawnAIInCell("TheyShallFall_RedShadeFollower2", "CityInteriorCell", new Vector3(-1.3f, 0.2f, 24.14f), Quaternion.Euler(0, 226.4f, 0));

            // Spawn the new guards outside
            RCKFunctions.SpawnAIInCell("TheyShallFall_CityGuardsOutside1", "Virrihael(0,0)", new Vector3(-5.031f, 0f, 49.108f), Quaternion.Euler(0, 16.607f, 0));
            RCKFunctions.SpawnAIInCell("TheyShallFall_CityGuardsOutside2", "Virrihael(0,0)", new Vector3(1.311f, 0f, 49.154f), Quaternion.Euler(0, -13.393f, 0));

            // Spawn dead followers in the king's palace
            RCKFunctions.SpawnAIInCell("TheyShallFall_DeadRedShadeFollower1", "CityKingPalace", new Vector3(6.71f, 0f, -24.228f), Quaternion.Euler(0, 63.9f, 0));
            RCKFunctions.SpawnAIInCell("TheyShallFall_DeadRedShadeFollower2", "CityKingPalace", new Vector3(2.951f, 0f, -32.228f), Quaternion.Euler(0, -90f, 0));

            RCKFunctions.KillRckAI("TheyShallFall_DeadRedShadeFollower1");
            RCKFunctions.KillRckAI("TheyShallFall_DeadRedShadeFollower2");

            var aiData = SaveSystem.SaveSystemManager.instance.saveFile.AIData;

            // Set the king to be not essential anymore so he can die
            var kingData = aiData.aiDictionary["TheKing001"];
            kingData.isEssential = false;

            // Send the Count in the Oblivion
            RCKFunctions.SendIntoOblivion("CountTheveninThibault001");

            // Use this line to destroy the script if it is not longer needed.
            Destroy(this);
        }

        void KillEveryoneInTheCity()
        {
            RCKFunctions.KillRckAI("CityGuard001");
            RCKFunctions.KillRckAI("CityGuard002");
            RCKFunctions.KillRckAI("CityGuard003");
            RCKFunctions.KillRckAI("CityGuard004");

            RCKFunctions.KillRckAI("Lanius001");
            RCKFunctions.KillRckAI("MurieldeAyala");

            RCKFunctions.KillRckAI("YvonneGraf001");
            RCKFunctions.KillRckAI("AdelindeVeturia001");

            RCKFunctions.KillRckAI("CityGuard005");
            RCKFunctions.KillRckAI("CityGuard006");
            RCKFunctions.KillRckAI("CityGuard007");

            RCKFunctions.KillRckAI("TaholianVedance001");
            RCKFunctions.KillRckAI("SteveWood001");
            RCKFunctions.KillRckAI("EverettLancaster001");
            RCKFunctions.KillRckAI("FlorenceGaines001");
            RCKFunctions.KillRckAI("Mack001");

            RCKFunctions.KillRckAI("CityKingPalaceGuard001");
            RCKFunctions.KillRckAI("CityKingPalaceGuard002");
            RCKFunctions.KillRckAI("CityKingPalaceGuard003");
            RCKFunctions.KillRckAI("CityKingPalaceGuard004");

            RCKFunctions.KillRckAI("RaymondJenkins001");
            RCKFunctions.KillRckAI("GasparodeAlbarate001");
            RCKFunctions.KillRckAI("Yeneas001");
            RCKFunctions.KillRckAI("Tess001");
            RCKFunctions.KillRckAI("Helmer001");
        }
    }
}