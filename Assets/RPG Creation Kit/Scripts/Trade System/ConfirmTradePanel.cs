using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RPGCreationKit;
using UnityEngine.EventSystems;

namespace RPGCreationKit
{
    public class ConfirmTradePanel : MonoBehaviour
    {
        ItemInInventory item;
        ItemInInventoryUI itemInInventoryUI;

        public Button continueButton;
        public Button cancelButton;
        public TextMeshProUGUI confirmationText;

        public GameObject previousSelected;
        public DropItemsPanel dropPanel;

        public void Init(ItemInInventory _item, ItemInInventoryUI _itemInInventoryUI, string _text, bool _openedFromDropPanel = false)
        {
            item = _item;
            itemInInventoryUI = _itemInInventoryUI;

            confirmationText.text = _text;

            if (_openedFromDropPanel)
                previousSelected = dropPanel.previousSelected;
            else
                previousSelected = EventSystem.current.currentSelectedGameObject;

            continueButton.Select();
        }

        public void ConfirmButton()
        {
            itemInInventoryUI.ConfirmTrade(0, 0);
            gameObject.SetActive(false);
        }

        public void CancelButton()
        {
            continueButton.onClick.RemoveAllListeners();
            gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(previousSelected);
        }
    }
}