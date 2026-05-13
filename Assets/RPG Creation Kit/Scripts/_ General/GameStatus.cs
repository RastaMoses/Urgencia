using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    public class GameStatus : MonoBehaviour
    {
        public static GameStatus instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                DestroyImmediate(this);
                Debug.LogError("Anomaly detected with the Singleton Pattern of 'GameStatus', are you using multple GameStatus?");
            }
        }

        private bool isPaused;
        public bool IsPaused
        {
            get
            {
                return isPaused;
            }

            set
            {
                isPaused = value;
            }
        }

        public bool canSave
        {
            get { return CanSave(); }
        }
        public bool PlayerPlaying()
        {
            return (!InventoryUI.instance.isOpen && !isPaused && !LootingInventoryUI.instance.isOpen && !QuestManagerUI.instance.isUIopened);
        }

        public bool AnyLoading()
        {
            return (WorldManager.instance == null || Player.RckPlayer.instance == null ||
                WorldManager.instance.isLoading || !Player.RckPlayer.instance.IsControlledByPlayer() || !CellsSystem.CellInformation.AllActiveCellsLoaded());
        }

        private bool CanSave()
        {
            return ((!RckPlayer.instance.isInConversation || RckPlayer.instance.isPlainLineDialogue) && 
                !RckPlayer.instance.isInCutsceneMode && !RckPlayer.instance.isInDeathSequence && 
                WorldManager.instance.currentWorldspace.worldSpaceID != "ZombiesWorldspace"); // cannot save in zombies mode
        }
    }
}