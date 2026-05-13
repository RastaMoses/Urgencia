using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // Required when using Event data.
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Item in Inventory UI, base class.
    /// </summary>
    public class ItemInInventoryUI : MonoBehaviour
    {
        [Header("UI")]
        public Image Icon;
        public Text Name;
        public Text DamageVal;
        public Text ArmorVal;
        public Text WeightVal;
        public Text GoldsVal;
        public Text AmountVal;

        public GameObject tooltipGameObject;
        public TextMeshProUGUI tooltipText;

        [Space(5)]

        [Header("Item Status")]
        public GameObject equippedIcon;
        public GameObject stolenIcon;

        // Other
        public ItemsInListPoolManager pool;


        public void OnEnable()
        {
            tooltipGameObject.SetActive(false);
        }

        public virtual void OnClick(bool takeAll = false)
        {
           
        }

        public virtual void OnClickForDrop()
        {

        }

        public virtual void DropItem()
        {

        }

        public virtual void ConfirmButtonCumulableItem(int amount)
        {

        }

        public virtual void ConfirmTrade(int _amount, ConfirmTradeAction _action)
        {

        }

        public virtual void UpdateItem()
        {

        }
    }
}