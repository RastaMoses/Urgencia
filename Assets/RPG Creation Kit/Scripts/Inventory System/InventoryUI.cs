using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using RPGCreationKit.Player;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

namespace RPGCreationKit
{
    /// <summary>
    /// Class that manages the UI for the Inventory
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {

        #region Singleton
        public static InventoryUI instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Debug.LogError("Anomaly detected with the Singleton Pattern of 'InventoryUI', are you using multple InventoryUIs?");
                Destroy(this);
            }
        }
        #endregion

        public Inventory inventory;
        public Equipment playerEquipment;
        public ItemsInListPoolManager itemsPoolManager;

        public InventoryTabs selectedTab;
        public CharacterTabs selectedCharacterTab;

        [Header("UI Elements")]
        public GameObject UI;
        public Transform scrollView;

        public DropItemsPanel dropItemsPanel;

        public Text currentWeightText;
        public Text maxWeightText;

        public Text ArmorRatingText;
        public Text GoldsText;

        [HideInInspector] public bool isOpen = false;

        [SerializeField] private Transform inventoryItemsContent;

        public bool dropKeyBeingHeld;

        public GameObject alertInCharacterUI;

        public void INPUT_DropKeyKeyboardOnly(InputAction.CallbackContext value)
        {
            if (value.started)
                dropKeyBeingHeld = true;
            else if (value.canceled)
                dropKeyBeingHeld = false;
        }

        // Use this for initialization
        void Start()
        {
            /// Grab the PlayerInventory
            if (Inventory.PlayerInventory)
                inventory = Inventory.PlayerInventory;
            else
                Debug.LogError("Could not retrieve the Player Inventory from the Singleton pattern, are you missing 'Inventory' component on the Player GameObject?");

            
            playerEquipment = Equipment.PlayerEquipment;
        }

        // Update is called once per frame
        void Update()
        {
            if (RckPlayer.instance.input.currentActionMap.name == "Player" || RckPlayer.instance.input.currentActionMap.name == "InventoryUI")
            {
                if (RckPlayer.instance.input.currentActionMap.FindAction("OpenCloseInventory").triggered &&
                    !QuestManagerUI.instance.isUIopened && !RckPlayer.instance.isInConversation && !TradeSystemUI.instance.isOpen &&
                    !RckPlayer.instance.isInteracting && !RckPlayer.instance.isReadingBook && !RckPlayer.instance.isLooting && !TutorialAlertMessage.instance.messageOpened && !RckPlayer.instance.isInDeathSequence)
                    OpenClose();
            }
        }

        public void OpenClose()
        {
            if (RckPlayer.instance.isReadingBook)
                return;

            RckPlayer.instance.EnableDisableControls(UI.activeInHierarchy);

            if (!UI.activeInHierarchy)
            {
                // Open
                isOpen = true;
                InventoryUI.instance.itemsPoolManager.PopulateAll();

                if (RCKSettings.INVENTORY_PAUSES_GAME)
                    Time.timeScale = 0.0f;

                UpdateStatsUI();
                ShowTabItems();
                if (EntityAttributes.PlayerAttributes.curAttributePoints > 0)
                    alertInCharacterUI.SetActive(true);

                PlayerInInventory.instance.ShowHideCharacter(true);

                RckPlayer.instance.input.SwitchCurrentActionMap("InventoryUI");
            }
            else
            {
                // Close          
                dropItemsPanel.CancelButton();
                isOpen = false;
                InventoryUI.instance.itemsPoolManager.ResetPools();

                if (RCKSettings.INVENTORY_PAUSES_GAME)
                    Time.timeScale = 1.0f;

                PlayerInInventory.instance.ShowHideCharacter(false);

                RckPlayer.instance.input.SwitchCurrentActionMap("Player");
            }

            UI.SetActive(!UI.activeInHierarchy);
        }

        public void ChangeTab(InventoryTabs tab)
        {
            selectedTab = tab;

            /// Show Tab
            ShowTabItems();
        }

        public void ChangeCharacterTab(int tab)
        {
            selectedCharacterTab = (CharacterTabs)tab;
        }

        public void ShowTabItems()
        {
            switch(selectedTab)
            {
                case InventoryTabs.All:
                    itemsPoolManager.ShowAll();
                    break;

                case InventoryTabs.Armors:
                    itemsPoolManager.ArmorsTab();
                    break;

                case InventoryTabs.Keys:
                    itemsPoolManager.KeysTab();
                    break;

                case InventoryTabs.MiscItems:
                    itemsPoolManager.MiscTab();
                    break;

                case InventoryTabs.Weapons:
                    itemsPoolManager.WeaponsTab();
                    break;
                
            }

            SelectFirstButton();
        }

        public void SelectFirstButton()
        {
            if (!RckInput.isUsingGamepad)
                return;

            StartCoroutine(SelectFirstButtonC());
        }

        public GameObject grabbed;

        public void MoveSafe(MoveDirection direction)
        {
            AxisEventData data = new AxisEventData(EventSystem.current)
            {
                moveDir = direction,
                selectedObject = EventSystem.current.currentSelectedGameObject
            };
            ExecuteEvents.Execute(data.selectedObject, data, ExecuteEvents.moveHandler);

            grabbed = EventSystem.current.currentSelectedGameObject;

            // Revert
            if (data.moveDir == MoveDirection.Up)
                data.moveDir = MoveDirection.Down;
            else if (data.moveDir == MoveDirection.Down)
                data.moveDir = MoveDirection.Up;

            data.selectedObject = EventSystem.current.currentSelectedGameObject;
            ExecuteEvents.Execute(data.selectedObject, data, ExecuteEvents.moveHandler);

            StartCoroutine(SelectNextButtonC(grabbed));
        }


        public void SelectNextButton(bool calledFromDropPanel = false)
        {
            if (!RckInput.isUsingGamepad)
                return;

            if (UINavigationHelper.CanNavigateDown())
                MoveSafe(MoveDirection.Down);
            else if (UINavigationHelper.CanNavigateUp())
                MoveSafe(MoveDirection.Up);
            else
                StartCoroutine(SelectFirstButtonC());
        }


        private IEnumerator SelectFirstButtonC()
        {
            yield return new WaitForEndOfFrame();
            EventSystem.current.SetSelectedGameObject(null);
            var a = inventoryItemsContent.GetComponentInChildren<Button>();
            if (a != null)
                EventSystem.current.SetSelectedGameObject(a.gameObject, null);
        }

        private IEnumerator SelectNextButtonC(GameObject go)
        {
            yield return new WaitForEndOfFrame();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(go);
        }

        public void UpdateStatsUI()
        {
            ArmorRatingText.text = ((int)playerEquipment.ArmorRate).ToString();
            currentWeightText.text = ((int)inventory.CurTotalWeight).ToString();
            GoldsText.text = (inventory.GetItemCount("Gold001").ToString());
        }

        public void UpdateAllItems()
        {
            for (int i = 0; i < itemsPoolManager.AmmosPool.usedObjects.Count; i++)
                itemsPoolManager.AmmosPool.usedObjects[i].UpdateItem();

            for (int i = 0; i < itemsPoolManager.ArmorsPool.usedObjects.Count; i++)
                itemsPoolManager.ArmorsPool.usedObjects[i].UpdateItem();
            
            
            for (int i = 0; i < itemsPoolManager.WeaponsPool.usedObjects.Count; i++)
                itemsPoolManager.WeaponsPool.usedObjects[i].UpdateItem();
        }


    }
}