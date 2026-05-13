using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Player;

namespace RPGCreationKit.Game.ResultScripts
{
    public class RSQL_TheyShallFall_PostgameLoad : ResultScript
    {
        void Start()
        {
            StartCoroutine(ScriptHandler());
        }

        IEnumerator ScriptHandler()
        {
            RckPlayer.instance.EnterInCutsceneMode();

            // Fade in the screen
            yield return new WaitForSeconds(1);

            RckPlayer.instance.FadeScreen(false);

            yield return new WaitForSeconds(1.55f);

            // Post-game world

            // Clear King's Palace
            RCKFunctions.SendIntoOblivion("TheKing001");
            RCKFunctions.SendIntoOblivion("CityKingPalaceGuard001");
            RCKFunctions.SendIntoOblivion("CityKingPalaceGuard002");
            RCKFunctions.SendIntoOblivion("CityKingPalaceGuard003");
            RCKFunctions.SendIntoOblivion("CityKingPalaceGuard004");
            RCKFunctions.SendIntoOblivion("TheyShallFall_DeadRedShadeFollower1");
            RCKFunctions.SendIntoOblivion("TheyShallFall_DeadRedShadeFollower2");
            RCKFunctions.SendIntoOblivion("AdamusLatinius001");
            RCKFunctions.SendIntoOblivion("VirgiliaValera001");

            // Clear instantiated
            RCKFunctions.SendIntoOblivion("TheyShallFall_RedShadeFollower1");
            RCKFunctions.SendIntoOblivion("TheyShallFall_RedShadeFollower2");
            RCKFunctions.SendIntoOblivion("TheyShallFall_Ryan");
            RCKFunctions.SendIntoOblivion("TheyShallFall_CityGuardsInside1");
            RCKFunctions.SendIntoOblivion("TheyShallFall_CityGuardsInside2");
            RCKFunctions.SendIntoOblivion("TheyShallFall_CityGuardsOutside1");
            RCKFunctions.SendIntoOblivion("TheyShallFall_CityGuardsOutside2");


            // Clear City
            RCKFunctions.SendIntoOblivion("CityGuard001");
            RCKFunctions.SendIntoOblivion("CityGuard002");
            RCKFunctions.SendIntoOblivion("CityGuard003");
            RCKFunctions.SendIntoOblivion("CityGuard004");

            RCKFunctions.SendIntoOblivion("Lanius001");
            RCKFunctions.SendIntoOblivion("MurieldeAyala");

            RCKFunctions.SendIntoOblivion("YvonneGraf001");
            RCKFunctions.SendIntoOblivion("AdelindeVeturia001");

            RCKFunctions.SendIntoOblivion("CityGuard005");
            RCKFunctions.SendIntoOblivion("CityGuard006");
            RCKFunctions.SendIntoOblivion("CityGuard007");

            RCKFunctions.SendIntoOblivion("TaholianVedance001");
            RCKFunctions.SendIntoOblivion("SteveWood001");
            RCKFunctions.SendIntoOblivion("EverettLancaster001");
            RCKFunctions.SendIntoOblivion("FlorenceGaines001");

            /*
            RCKFunctions.SendIntoOblivion("JuliusLancaster001");
            RCKFunctions.SendIntoOblivion("IshildeMorrison001");
            RCKFunctions.SendIntoOblivion("CornielesdeArena001");
            */

            // Restore fire
            RCKFunctions.MutateMutable("Mutable_TheyShallFall_FireAndSmokeInside", true);
            RCKFunctions.MutateMutable("Mutable_TheyShallFall_FireAndSmoke", true);

            // Spawn new agents of the red shade

            // King's Palace
            RCKFunctions.SpawnAIInCurrentCell("Postgame_RS_Follower001", new Vector3(8.039f, 0, -11.65f), Quaternion.Euler(0f, 182f, 0));
            RCKFunctions.SpawnAIInCurrentCell("Postgame_RS_Follower002", new Vector3(0.33f, 0, -11.28f), Quaternion.Euler(0f, 182f, 0));
            RCKFunctions.SpawnAIInCurrentCell("Postgame_RS_Follower003", new Vector3(1.15f, 0, -47.376f), Quaternion.Euler(0f, 356f, 0));
            RCKFunctions.SpawnAIInCurrentCell("Postgame_RS_Follower004", new Vector3(5.152f, 0, -47.376f), Quaternion.Euler(0f, 352.444f, 0));

            // City interior
            RCKFunctions.SpawnAIInCell("Postgame_RS_Follower005", "CityInteriorCell", new Vector3(-0.61f, 0, -16.82f), Quaternion.Euler(0f, 0f, 0f));
            RCKFunctions.SpawnAIInCell("Postgame_RS_Follower006", "CityInteriorCell", new Vector3(6.17f, 0, -16.82f), Quaternion.Euler(0f, 0f, 0f));
            RCKFunctions.SpawnAIInCell("Postgame_RS_Follower007", "CityInteriorCell", new Vector3(7.02f, 0, 40.11f), Quaternion.Euler(0f, -174.81f, 0f));
            RCKFunctions.SpawnAIInCell("Postgame_RS_Follower008", "CityInteriorCell", new Vector3(-3.13f, 0, 40.11f), Quaternion.Euler(0f, -191.95f, 0f));


            RCKFunctions.SpawnAIInCell("Postgame_RS_Follower009", "Virrihael(0,0)", new Vector3(-5.03f, 0, 48.92f), Quaternion.Euler(0f, 0f, 0f));
            RCKFunctions.SpawnAIInCell("Postgame_RS_Follower010", "Virrihael(0,0)", new Vector3(1.8f, 0, 48.92f), Quaternion.Euler(0f, 0f, 0f));


            RCKFunctions.SpawnAIInCell("Postgame_RS_Follower011", "CityInteriorCell", new Vector3(19.386f, 0, 33.854f), Quaternion.Euler(0f, -69f, 0f));
            RCKFunctions.SpawnAIInCell("Postgame_RS_Follower012", "CityInteriorCell", new Vector3(-37.941f, 0, -17.345f), Quaternion.Euler(0f, -31.51f, 0f));
            RCKFunctions.SpawnAIInCell("Postgame_RS_Follower013", "CityInteriorCell", new Vector3(-31.5f, 0, 42.11f), Quaternion.Euler(0f, -31.51f, 0f));


            //RCKFunctions.UnlockDoor("CityInteriorToArmorShop");
            //RCKFunctions.UnlockDoor("CityInteriorToGeneralGoodsStore");
            //RCKFunctions.UnlockDoor("CityInteriorToBlacksmithShop");

            RCKFunctions.UnlockDoor("KingsPalaceToCityInterior");

            // Destroy every created item in the King's Palace
            for (int i = 0; i < CellInformation.activeCells["CityKingPalace"].createdItemsT.childCount; i++)
            {
                foreach (Transform t in CellInformation.activeCells["CityKingPalace"].createdItemsT.transform)
                {
                    ItemInWorld item = t.GetComponent<ItemInWorld>();
                    item.DeleteCreatedRecord();
                    Destroy(t.gameObject);
                }
            }

            yield return new WaitForSeconds(3f);

            RckPlayer.instance.FadeScreen(true);

            yield return new WaitForSeconds(2);

            RckPlayer.instance.LeaveCutsceneMode();

            // Change Martin's dialogue and make him talk
            RckAI martin = null;
            CellInformation.TryToGetAI("Martin_RedShadePostGame", out martin);

            martin.ChangeDialogueGraph("RSDIALOGUE_MartinPostgameDialogue");
            martin.SendAIToSpeakToPlayer(false);

            // Unlock main city gate
            RCKFunctions.UnlockDoor("MainDoorToCityExterior");

            Destroy(this);
            yield return null;
        }
    }
}