using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class LootingInventoryTab : MonoBehaviour
    {
        public InventoryTabs thisTab;

        public void OnValueChanges(Toggle t)
        {
            if (t.isOn)
                LootingInventoryUI.instance.ChangeTab(thisTab);
        }

    }
}