using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using RPGCreationKit;
using UnityEngine.EventSystems;

namespace RPGCreationKit
{
    /// <summary>
    /// UI panel for dropping cumulable items.
    /// </summary>
    public class DropItemsPanel : MonoBehaviour
    {
        ItemInInventory item;
        ItemInInventoryUI itemInInventoryUI;

        public Slider slider;
        public Text Amount;

        public GameObject previousSelected;

        public void Init(ItemInInventory _item, ItemInInventoryUI _itemInInventoryUI)
        {
            item = _item;
            itemInInventoryUI = _itemInInventoryUI;

            slider.minValue = 1;
            slider.maxValue = item.Amount;

            slider.value = slider.maxValue;

            previousSelected = EventSystem.current.currentSelectedGameObject;
            slider.Select();
        }

        
        public void UpdateAmountUI()
        {
            Amount.text = slider.value.ToString();
        }

        public bool reselectPreviousOnConfirm = true;
        public void ConfirmButton()
        {
            gameObject.SetActive(false);

            itemInInventoryUI.ConfirmButtonCumulableItem((int)slider.value);

            if (reselectPreviousOnConfirm && slider.value < slider.maxValue)
                EventSystem.current.SetSelectedGameObject(previousSelected);
        }

        public void CancelButton()
        {
            gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(previousSelected);
        }

        public void SetPreviousSelected()
        {
            EventSystem.current.SetSelectedGameObject(previousSelected);
        }

        IEnumerator SetPreviousSelectedC()
        {
            yield return new WaitForEndOfFrame();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(previousSelected);
        }
    }
}
