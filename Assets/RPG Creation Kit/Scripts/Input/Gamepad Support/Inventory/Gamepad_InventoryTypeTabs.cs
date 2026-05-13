using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using RPGCreationKit.Player;
using System;
using System.Linq;

public class Gamepad_InventoryTypeTabs : MonoBehaviour
{
    [SerializeField] private Toggle[] tabs;

    [SerializeField] private bool useImages = true;
    [SerializeField] private GameObject goRightImage;
    [SerializeField] private GameObject goLeftImage;

    // Update is called once per frame
    void Update()
    {
        if (RckInput.isUsingGamepad)
        {
            if (useImages)
            {
                goRightImage.SetActive(true);
                goLeftImage.SetActive(true);
            }

            if (RckPlayer.instance.input.currentActionMap.FindAction("RightInventoryTab").triggered)
            {
                if ((InventoryUI.instance.selectedCharacterTab == Enum.GetValues(typeof(CharacterTabs)).Cast<CharacterTabs>().Last()))
                    tabs[0].isOn = true;
                else
                    tabs[(int)InventoryUI.instance.selectedCharacterTab + 1].isOn = true;
            }
            else if (RckPlayer.instance.input.currentActionMap.FindAction("LeftInventoryTab").triggered)
            {
                if ((InventoryUI.instance.selectedCharacterTab == 0))
                    tabs[(int)Enum.GetValues(typeof(CharacterTabs)).Cast<CharacterTabs>().Last()].isOn = true;
                else
                    tabs[(int)InventoryUI.instance.selectedCharacterTab - 1].isOn = true;
            }
        }
        else if (useImages)
        {
            goRightImage.SetActive(false);
            goLeftImage.SetActive(false);
        }
    }
}
