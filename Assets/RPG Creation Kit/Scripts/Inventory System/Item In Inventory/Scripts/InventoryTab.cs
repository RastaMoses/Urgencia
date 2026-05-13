using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;

namespace RPGCreationKit
{

    public enum InventoryTabs { All, Weapons, Armors, MiscItems, Keys };
    public enum CharacterTabs { Character, Effects, Attributes, Factions, Stats };

    public class InventoryTab : MonoBehaviour
    {
        public InventoryTabs thisTab;

        public void OnValueChanges(Toggle t)
        {
            if(t.isOn)
                InventoryUI.instance.ChangeTab(thisTab);
        }
        
    }
}