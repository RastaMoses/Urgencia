using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using RPGCreationKit.Player;
using System;
using System.Linq;

public class Gamepad_LootingUITabs : MonoBehaviour
{
    [SerializeField] private Toggle[] tabs;

    [SerializeField] private bool useImages = true;
    [SerializeField] private GameObject goRightImage;
    [SerializeField] private GameObject goLeftImage;

    // Update is called once per frame
    void Update()
    {
        if (RckInput.isUsingGamepad && RckPlayer.instance.input.currentActionMap.name == "LootingUI")
        {
            if (useImages)
            {
                goRightImage.SetActive(true);
                goLeftImage.SetActive(true);
            }

            if (RckPlayer.instance.input.currentActionMap.FindAction("RightTab").triggered)
            {
                if ((LootingInventoryUI.instance.selectedTab == Enum.GetValues(typeof(InventoryTabs)).Cast<InventoryTabs>().Last()))
                    tabs[0].isOn = true;
                else
                    tabs[(int)LootingInventoryUI.instance.selectedTab + 1].isOn = true;
            }
            else if (RckPlayer.instance.input.currentActionMap.FindAction("LeftTab").triggered)
            {
                if ((LootingInventoryUI.instance.selectedTab == 0))
                    tabs[(int)Enum.GetValues(typeof(InventoryTabs)).Cast<InventoryTabs>().Last()].isOn = true;
                else
                    tabs[(int)LootingInventoryUI.instance.selectedTab - 1].isOn = true;
            }
        }
        else if (useImages)
        {
            goRightImage.SetActive(false);
            goLeftImage.SetActive(false);
        }
    }
}
