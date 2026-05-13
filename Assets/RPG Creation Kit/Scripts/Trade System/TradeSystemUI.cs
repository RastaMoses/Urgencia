using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RPGCreationKit;
using System.Linq;
using RPGCreationKit.Player;
using UnityEngine.EventSystems;

namespace RPGCreationKit
{
    public enum ConfirmTradeAction
    {
        BuyFromMerchant = 0,
        SellToMerchant = 1,
        TakeFromNPC = 2,
        GiveToNPC = 3
    }

    public enum GoldsSounds { Spend, Gain };

    public class TradeSystemUI : MonoBehaviour
    {
        public static TradeSystemUI instance;
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

        // Is this a merchant or a Companion?
        public bool isMerchant = false;

        public Merchant curMerchant;

        public bool isBuyingOrDrawing = true;

        public ItemsInListPoolManager itemsPoolManager;

        public InventoryTabs selectedTab;

        [Header("UI Elements")]
        public GameObject[] UIToDisable;

        [Space(5)]

        public GameObject UI;
        public Transform scrollView;
        public Toggle defaultToggle;

        public DropItemsPanel takeDepositItemsPanel;
        public ConfirmTradePanel confirmTradePanel;

        public TextMeshProUGUI currentWeightText;
        public TextMeshProUGUI maxWeightText;

        public GameObject DialogueUI;
        [Space(5)]

        public TextMeshProUGUI merchantName;
        public TextMeshProUGUI playerGolds;
        public TextMeshProUGUI merchantGolds;

        public bool isOpen = false;

        [Header("Sounds")]
        public AudioClip goldSpent;
        public AudioClip goldGained;
        [SerializeField] private Transform inventoryItemsContent;

        public void OpenTradePanel(Merchant _merchant, bool _referenceMerchantInventory = true)
        {
            isMerchant = true;
            curMerchant = _merchant;

            if (_referenceMerchantInventory)
                inventoryToDisplay = _merchant.inventory;
            //else
            // get the inventory of the npc we want to exchange stuff 

            itemsPoolManager.inventoryToPool = inventoryToDisplay;

            // Open
            isOpen = true;

            if (RCKSettings.TRADE_PAUSES_GAME)
                Time.timeScale = 0.0f;

            itemsPoolManager.PopulateAll(true);

            RckPlayer.instance.input.SwitchCurrentActionMap("TradeUI");

            OpenTradeUI();
        }

        /*
        public void OpenTradePanel(NPC_Behaviour _npc, bool _referenceNPCInventory = true)
        {
            isMerchant = true; // TO CHANGE: This should be called for friendly npc that doesn't sell but just carry your burdens
            curNPC = _npc;
            curMerchant = curNPC.GetComponent<Merchant>();

            // Open
            isOpen = true;
            itemsPoolManager.PopulateAll();

            if (_referenceNPCInventory)
                inventoryToDisplay = _npc.GetComponent<Inventory>();

            OpenTradeUI();
        }
        */

        private void OpenTradeUI()
        {
            itemsPoolManager.inventoryToPool = inventoryToDisplay;

            UpdateStatsUI();
            ShowTabItems();

            UI.SetActive(true);

            foreach (GameObject go in UIToDisable)
                go.SetActive(false);

            if (RckPlayer.instance.isInConversation)
                DialogueUI.SetActive(false);
        }

        public void SwitchInventory(bool _isPlayerInventory)
        {
            itemsPoolManager.ResetPools();

            isBuyingOrDrawing = !_isPlayerInventory;

            if (_isPlayerInventory)
            {
                inventoryToDisplay = Inventory.PlayerInventory;
                itemsPoolManager.inventoryToPool = inventoryToDisplay;

                if (isMerchant)
                    OpenTradePanel(curMerchant, false);
                //else
                //    OpenTradePanel(curNPC, false);
            }
            else
            {
                if (isMerchant)
                    inventoryToDisplay = curMerchant.inventory;

                itemsPoolManager.inventoryToPool = inventoryToDisplay;

                if (isMerchant)
                    OpenTradePanel(curMerchant, false);
                //else
                //    OpenTradePanel(curNPC, false);
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
                    if (isBuyingOrDrawing)
                        itemsPoolManager.ShowAll();
                    else
                        itemsPoolManager.MerchantTradeShowAll(curMerchant.WantsToBuy);
                    break;

                case InventoryTabs.Armors:
                    if (isBuyingOrDrawing)
                        itemsPoolManager.ArmorsTab();
                    else
                        itemsPoolManager.MerchantTradeArmorsTab(curMerchant.WantsToBuy);
                    break;

                case InventoryTabs.Keys:
                    if (isBuyingOrDrawing)
                        itemsPoolManager.KeysTab();
                    else
                        itemsPoolManager.MerchantTradeKeysTab(curMerchant.WantsToBuy);
                    break;

                case InventoryTabs.MiscItems:
                    if (isBuyingOrDrawing)
                        itemsPoolManager.MiscTab();
                    else
                        itemsPoolManager.MerchantTradeMiscTab(curMerchant.WantsToBuy);
                    break;

                case InventoryTabs.Weapons:
                    if (isBuyingOrDrawing)
                        itemsPoolManager.WeaponsTab();
                    else
                        itemsPoolManager.MerchantTradeWeaponsTab(curMerchant.WantsToBuy);
                    break;
            }

            SelectFirstButton();
            AdjustPrices();

            if(!isBuyingOrDrawing)
                MerchantHideStolenItems();
        }

        public void UpdateStatsUI()
        {
            currentWeightText.text = ((int)Inventory.PlayerInventory.CurTotalWeight).ToString();

            merchantName.text = curMerchant.merchantName;

            playerGolds.text = RCKFunctions.GetItemCount(Inventory.PlayerInventory, "Gold001").ToString();
            merchantGolds.text = RCKFunctions.GetItemCount(curMerchant.inventory, "Gold001").ToString();
        }

        public void CloseUI()
        {
            itemsPoolManager.ResetPools();
            defaultToggle.isOn = true;

            UI.SetActive(false);

            foreach (GameObject go in UIToDisable)
                go.SetActive(true);

            isOpen = false;

            if(RCKSettings.TRADE_PAUSES_GAME)
                Time.timeScale = 1.0f;

            confirmTradePanel.CancelButton();
            RckPlayer.instance.input.SwitchCurrentActionMap("Player");

            if(RckPlayer.instance.isInConversation)
                StartCoroutine(SelectDialogueOption());

            if (RckPlayer.instance.isInConversation)
                DialogueUI.SetActive(true);

            if (RckInput.isUsingGamepad)
                RckPlayer.instance.uiManager.PreventUnselectedQuestionWithGamepad();
        }

        IEnumerator SelectDialogueOption()
        {
            yield return new WaitForEndOfFrame();
            RckPlayer.instance.input.SwitchCurrentActionMap("DialogueUI");
            RckPlayer.instance.uiManager.playerQuestionsContent.GetComponentInChildren<Button>().Select();
        }

        public void PlaySound(GoldsSounds _sound)
        {
            switch (_sound)
            {
                case GoldsSounds.Gain:
                    GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, goldGained);
                    break;

                case GoldsSounds.Spend:
                    GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, goldSpent);
                    break;
            }
        }

        // Adjust prices for merchants
        public void AdjustPrices()
        {
            if(curMerchant != null)
            {
                for(int i = 0; i < itemsPoolManager.usedObjects.Count; i++)
                {

                    int adjustedGolds = 0;

                    // Get the base price
                    if (itemsPoolManager.usedObjects[i] is AmmoItemInInventoryUI)
                        adjustedGolds = ((AmmoItemInInventoryUI)itemsPoolManager.usedObjects[i]).ammoItemInInventory.item.Value;
                    else if (itemsPoolManager.usedObjects[i] is ArmorItemInInventoryUI)
                        adjustedGolds = ((ArmorItemInInventoryUI)itemsPoolManager.usedObjects[i]).armorItemInInventory.item.Value;
                    else if (itemsPoolManager.usedObjects[i] is BookItemInInventoryUI)
                        adjustedGolds = ((BookItemInInventoryUI)itemsPoolManager.usedObjects[i]).bookItemInInventory.item.Value;
                    else if (itemsPoolManager.usedObjects[i] is ConsumableItemInInventoryUI)
                        adjustedGolds = ((ConsumableItemInInventoryUI)itemsPoolManager.usedObjects[i]).consumableItemInInventory.item.Value;
                    else if (itemsPoolManager.usedObjects[i] is KeyItemInInventoryUI)
                        adjustedGolds = ((KeyItemInInventoryUI)itemsPoolManager.usedObjects[i]).keyItemInInventory.item.Value;
                    else if (itemsPoolManager.usedObjects[i] is MiscItemInInventoryUI)
                        adjustedGolds = ((MiscItemInInventoryUI)itemsPoolManager.usedObjects[i]).miscItemInInventory.item.Value;
                    else if (itemsPoolManager.usedObjects[i] is WeaponItemInInventoryUI)
                        adjustedGolds = ((WeaponItemInInventoryUI)itemsPoolManager.usedObjects[i]).weaponItemInInventory.item.Value;
                    else
                    {
                        Debug.LogWarning("Cannot adjust price!");
                    }

                    if (isBuyingOrDrawing)
                    {
                        adjustedGolds = Mathf.RoundToInt(adjustedGolds * curMerchant.sellMultiplier);
                        itemsPoolManager.usedObjects[i].GoldsVal.text = adjustedGolds.ToString();
                    }
                    else
                    {
                        adjustedGolds = Mathf.RoundToInt(adjustedGolds * curMerchant.buyMultiplier);
                        itemsPoolManager.usedObjects[i].GoldsVal.text = adjustedGolds.ToString();
                    }
                }
            }
        }

        public void MerchantHideStolenItems()
        {
            if (curMerchant != null && !curMerchant.buysStolenItems)
            {
                for (int i = 0; i < itemsPoolManager.usedObjects.Count; i++)
                {
                    bool hide = false;

                    if (itemsPoolManager.usedObjects[i] is AmmoItemInInventoryUI)
                        hide = ((AmmoItemInInventoryUI)itemsPoolManager.usedObjects[i]).ammoItemInInventory.metadata.isOwned;
                    else if (itemsPoolManager.usedObjects[i] is ArmorItemInInventoryUI)
                        hide = ((ArmorItemInInventoryUI)itemsPoolManager.usedObjects[i]).armorItemInInventory.metadata.isOwned;
                    else if (itemsPoolManager.usedObjects[i] is BookItemInInventoryUI)
                        hide = ((BookItemInInventoryUI)itemsPoolManager.usedObjects[i]).bookItemInInventory.metadata.isOwned;
                    else if (itemsPoolManager.usedObjects[i] is ConsumableItemInInventoryUI)
                        hide = ((ConsumableItemInInventoryUI)itemsPoolManager.usedObjects[i]).consumableItemInInventory.metadata.isOwned;
                    else if (itemsPoolManager.usedObjects[i] is KeyItemInInventoryUI)
                        hide = ((KeyItemInInventoryUI)itemsPoolManager.usedObjects[i]).keyItemInInventory.metadata.isOwned;
                    else if (itemsPoolManager.usedObjects[i] is MiscItemInInventoryUI)
                        hide = ((MiscItemInInventoryUI)itemsPoolManager.usedObjects[i]).miscItemInInventory.metadata.isOwned;
                    else if (itemsPoolManager.usedObjects[i] is WeaponItemInInventoryUI)
                        hide = ((WeaponItemInInventoryUI)itemsPoolManager.usedObjects[i]).weaponItemInInventory.metadata.isOwned;

                    if (hide)
                        itemsPoolManager.usedObjects[i].gameObject.SetActive(false);
                }
            }
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

        public GameObject grabbed;
        /// <summary>
        /// Moves the navigation inventory via code
        /// </summary>
        /// <param name="direction"></param>
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