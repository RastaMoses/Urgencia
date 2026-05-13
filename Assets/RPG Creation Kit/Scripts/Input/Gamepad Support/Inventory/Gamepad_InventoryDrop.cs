using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using RPGCreationKit.Player;
using System;
using System.Linq;
using UnityEngine.EventSystems;

public class Gamepad_InventoryDrop : MonoBehaviour
{
    ItemInInventoryUI _itemInInventoryUI;
    Button _button;

    private void Start()
    {
        _itemInInventoryUI = GetComponent<ItemInInventoryUI>();
        _button = GetComponent<Button>();
    }

    private void Update()
    {
        if (RckInput.isUsingGamepad && RckPlayer.instance.input.currentActionMap.name == "InventoryUI")
        {
            if (EventSystem.current.currentSelectedGameObject == this.gameObject)
            {
                if(RckPlayer.instance.input.currentActionMap.FindAction("DropItem").triggered)
                {
                    _itemInInventoryUI.OnClickForDrop();
                }
            }
        }
    }
}
