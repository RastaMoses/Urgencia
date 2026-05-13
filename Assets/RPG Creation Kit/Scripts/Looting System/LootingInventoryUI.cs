using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RPGCreationKit;
using RPGCreationKit.Player;
using UnityEngine.EventSystems;
using RPGCreationKit.AI;

namespace RPGCreationKit
{
    public class LootingInventoryUI : MonoBehaviour
    {
        public static LootingInventoryUI instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Debug.Log("Anomaly detected with the singleton pattern of 'LootingInventoryUI', do you have more than one LootingInventoryUI?");
                Destroy(this);
            }
        }

        public Inventory inventoryToDisplay;
        public TMPro.TextMeshProUGUI lootingDisplayName;

        public LootingPoint curLootingPoint;
        public bool isDrawing = true;

        public ItemsInListPoolManager itemsPoolManager;

        public InventoryTabs selectedTab;

        [Header("UI Elements")]
        public GameObject UI;
        public Transform scrollView;
        public Toggle defaultToggle;

        public DropItemsPanel takeDepositItemsPanel;

        public TextMeshProUGUI currentWeightText;
        public TextMeshProUGUI maxWeightText;

        [HideInInspector] public bool isOpen = false;

        [SerializeField] private Transform inventoryItemsContent;
        [SerializeField] private Button takeAllButton;
        private GameObject grabbed;

        // Pickpocket
        public GameObject pickpocketingUI;
        public TextMeshProUGUI pickpocketChanceText;
        public bool isPickPocketing = false;
        public RckAI pickPocketingAI = null;

        public GameObject[] pickPocketUIBlockers;

        public void OpenLootingPanel(LootingPoint _lootingPoint, bool _referenceLootInventory = true, bool _isPickPocket = false, RckAI _ai = null)
        {
            curLootingPoint = _lootingPoint;
            isPickPocketing = _isPickPocket;
            pickPocketingAI = _ai;

            pickpocketingUI.SetActive(isPickPocketing);
            pickpocketChanceText.text = "?";

            if(isPickPocketing)
            {
                foreach (GameObject obj in pickPocketUIBlockers)
                    obj.SetActive(true);
            }
            else
            {
                foreach (GameObject obj in pickPocketUIBlockers)
                    obj.SetActive(false);
            }

            if (_referenceLootInventory)
            {
                isDrawing = true;
                inventoryToDisplay = curLootingPoint.inventory;
            } else
            {
                isDrawing = false;
            }

            if (RCKSettings.LOOTING_PAUSES_GAME && !isPickPocketing)
            {
                Time.timeScale = 0.0f;
            }

            itemsPoolManager.inventoryToPool = inventoryToDisplay;

            if(!isPickPocketing)
                lootingDisplayName.text = curLootingPoint.pointName;
            else
                lootingDisplayName.text = pickPocketingAI.GetEntityName();

            OpenLootUI();
        }

        public void Update()
        {
            // Ensure that if we are pickpocketing, if the AI is too far away the looting ui is closed
            if(isOpen && isPickPocketing && pickPocketingAI != null && Vector3.Distance(pickPocketingAI.transform.position, RckPlayer.instance.transform.position) > RCKSettings.MAX_PICKPOCKET_DISTANCE)
            {
                CloseUI();
            }
        }

        private void OpenLootUI()
        {
            RckPlayer.instance.EnableDisableControls(false);

            // Open
            isOpen = true;
            itemsPoolManager.PopulateAll();

            UpdateStatsUI();
            ShowTabItems();
            
            UI.SetActive(true);

            RckPlayer.instance.input.SwitchCurrentActionMap("LootingUI");

            // Close
            // isOpen = false;
            // itemsPoolManager.ResetPools();
        }

        public void SwitchInventory(bool _isPlayerInventory)
        {
            itemsPoolManager.ResetPools();

            isDrawing = !_isPlayerInventory;

            if (_isPlayerInventory)
            {
                inventoryToDisplay = Inventory.PlayerInventory;
                itemsPoolManager.inventoryToPool = inventoryToDisplay;

                takeAllButton.gameObject.SetActive(false);

                OpenLootingPanel(curLootingPoint, false);
            }
            else
            {
                inventoryToDisplay = curLootingPoint.inventory;
                itemsPoolManager.inventoryToPool = inventoryToDisplay;

                takeAllButton.gameObject.SetActive(true);

                OpenLootingPanel(curLootingPoint, true);
            }
        }

        public void ChangeTab(InventoryTabs tab)
        {
            selectedTab = tab;

            /// Show Tab
            ShowTabItems();
        }

        public void ShowTabItems()
        {
            switch (selectedTab)
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

            // Hide quest items if pickpocketing
            if (isPickPocketing)
                HideQuestItems();
        }

        public void HideQuestItems()
        {
            for (int i = 0; i < itemsPoolManager.usedObjects.Count; i++)
            {
                bool hide = false;

                if (itemsPoolManager.usedObjects[i] is AmmoItemInInventoryUI)
                    hide = ((AmmoItemInInventoryUI)itemsPoolManager.usedObjects[i]).ammoItemInInventory.item.QuestItem;
                else if (itemsPoolManager.usedObjects[i] is ArmorItemInInventoryUI)
                    hide = ((ArmorItemInInventoryUI)itemsPoolManager.usedObjects[i]).armorItemInInventory.item.QuestItem;
                else if (itemsPoolManager.usedObjects[i] is BookItemInInventoryUI)
                    hide = ((BookItemInInventoryUI)itemsPoolManager.usedObjects[i]).bookItemInInventory.item.QuestItem;
                else if (itemsPoolManager.usedObjects[i] is ConsumableItemInInventoryUI)
                    hide = ((ConsumableItemInInventoryUI)itemsPoolManager.usedObjects[i]).consumableItemInInventory.item.QuestItem;
                else if (itemsPoolManager.usedObjects[i] is KeyItemInInventoryUI)
                    hide = ((KeyItemInInventoryUI)itemsPoolManager.usedObjects[i]).keyItemInInventory.item.QuestItem;
                else if (itemsPoolManager.usedObjects[i] is MiscItemInInventoryUI)
                    hide = ((MiscItemInInventoryUI)itemsPoolManager.usedObjects[i]).miscItemInInventory.item.QuestItem;
                else if (itemsPoolManager.usedObjects[i] is WeaponItemInInventoryUI)
                    hide = ((WeaponItemInInventoryUI)itemsPoolManager.usedObjects[i]).weaponItemInInventory.item.QuestItem;

                if (hide)
                    itemsPoolManager.usedObjects[i].gameObject.SetActive(false);
            }
        }

        public void UpdateStatsUI()
        {
            currentWeightText.text = ((int)Inventory.PlayerInventory.CurTotalWeight).ToString();
        }

        public void TakeAll()
        {
            // Click on all
            for(int i = itemsPoolManager.usedObjects.Count-1; i >= 0; i--)
                itemsPoolManager.usedObjects[i].OnClick(true);

            CloseUI();
        }

        public void CloseUI()
        {
            curLootingPoint.SaveOnFile();
            SwitchInventory(false);

            RckPlayer.instance.isLooting = false;
            itemsPoolManager.ResetPools();
            isOpen = false;

            defaultToggle.SetIsOnWithoutNotify(true);

            UI.SetActive(false);

            isPickPocketing = false;
            pickPocketingAI = null;

            RckPlayer.instance.EnableDisableControls(true);

            if (curLootingPoint.closeSound)
                GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, curLootingPoint.closeSound);

            takeDepositItemsPanel.CancelButton();
            RckPlayer.instance.input.SwitchCurrentActionMap("Player");

            if (RCKSettings.LOOTING_PAUSES_GAME && !isPickPocketing)
            {
                Time.timeScale = 1.0f;
            }

            curLootingPoint = null;
        }

        public void SelectFirstButton()
        {
            if (RckPlayer.instance.input.currentControlScheme != "Gamepad")
                return;

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

        public void Move(MoveDirection direction)
        {
            AxisEventData data = new AxisEventData(EventSystem.current);
            data.moveDir = direction;
            data.selectedObject = EventSystem.current.currentSelectedGameObject;
            ExecuteEvents.Execute(data.selectedObject, data, ExecuteEvents.moveHandler);
        }

        public void MoveSafe(MoveDirection direction)
        {
            AxisEventData data = new AxisEventData(EventSystem.current);
            data.moveDir = direction;
            data.selectedObject = EventSystem.current.currentSelectedGameObject;
            ExecuteEvents.Execute(data.selectedObject, data, ExecuteEvents.moveHandler);

            grabbed = EventSystem.current.currentSelectedGameObject;

            // Revert
            if (data.moveDir == MoveDirection.Up)
                data.moveDir = MoveDirection.Down;
            else if (data.moveDir == MoveDirection.Down)
                data.moveDir = MoveDirection.Up;

            data.selectedObject = EventSystem.current.currentSelectedGameObject;
            ExecuteEvents.Execute(data.selectedObject, data, ExecuteEvents.moveHandler);
            Debug.Log(grabbed);

            StartCoroutine(SelectNextButtonC(grabbed));
        }

        public void SelectNextButton(bool calledFromDropPanel = false)
        {
            if (RckPlayer.instance.input.currentControlScheme != "Gamepad")
                return;

            if (UINavigationHelper.CanNavigateDown())
                MoveSafe(MoveDirection.Down);
            else if (UINavigationHelper.CanNavigateUp())
                MoveSafe(MoveDirection.Up);
            else
                StartCoroutine(SelectFirstButtonC());
        }

        private IEnumerator SelectNextButtonC(GameObject go)
        {
            yield return new WaitForEndOfFrame();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(go);
        }
    }
}