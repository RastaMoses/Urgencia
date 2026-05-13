using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using RPGCreationKit.Player;
using System;
using System.Linq;

public class Gamepad_OnScreenCommands : MonoBehaviour
{
    [SerializeField] private GameObject[] commands;

    private void OnEnable()
    {
        if(RckInput.isUsingGamepad)
        {
            for (int i = 0; i < commands.Length; i++)
                commands[i].SetActive(true);
        }
        else
        {
            for (int i = 0; i < commands.Length; i++)
                commands[i].SetActive(false);
        }
    }

}
