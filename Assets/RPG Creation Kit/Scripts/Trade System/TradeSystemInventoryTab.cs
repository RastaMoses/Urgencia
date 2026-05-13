using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class TradeSystemInventoryTab : MonoBehaviour
    {
        public InventoryTabs thisTab;

        public void OnValueChanges(Toggle t)
        {
            if (t.isOn)
                TradeSystemUI.instance.ChangeTab(thisTab);
        }
    }
}