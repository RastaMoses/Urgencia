using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using RPGCreationKit.Player;
using System;
using System.Linq;

public class Gamepad_QuestManagerUIMainTabs : MonoBehaviour
{
    [SerializeField] private Toggle R2Tab;
    [SerializeField] private Toggle L2Tab;

    // Update is called once per frame
    void Update()
    {
        if (RckInput.isUsingGamepad && RckPlayer.instance.input.currentActionMap.name == "QuestJournalUI")
        {
            if (RckPlayer.instance.input.currentActionMap.FindAction("RightPage").triggered)
            {
                R2Tab.isOn = true;
            }
            else if (RckPlayer.instance.input.currentActionMap.FindAction("LeftPage").triggered)
            {
                L2Tab.isOn = true;
            }
        }
    }
}
