using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using RPGCreationKit.Player;
using System;
using System.Linq;

public class Gamepad_InventoryMainTabs : MonoBehaviour
{
    [SerializeField] private Toggle R2Tab;
    [SerializeField] private Toggle L2Tab;
    [SerializeField] private Toggle DOUBLER2Tab;

    [SerializeField] private bool useImages = true;
    [SerializeField] private GameObject R2Go;
    [SerializeField] private GameObject L2Go;

    // Update is called once per frame
    void Update()
    {
        if (RckInput.isUsingGamepad && RckPlayer.instance.input.currentActionMap.name == "InventoryUI")
        {

            if (useImages)
            {
                R2Go.SetActive(true);
                L2Go.SetActive(true);
            }

            if (RckPlayer.instance.input.currentActionMap.FindAction("RightPage").triggered)
            {
                if (R2Tab.isOn)
                    DOUBLER2Tab.isOn = true;
                else
                    R2Tab.isOn = true;
            }
            else if (RckPlayer.instance.input.currentActionMap.FindAction("LeftPage").triggered)
            {
                if (DOUBLER2Tab.isOn)
                    R2Tab.isOn = true;
                else
                    L2Tab.isOn = true;
            }
        }
        else if(useImages)
        {
            R2Go.SetActive(false);
            L2Go.SetActive(false);
        }
    }
}
