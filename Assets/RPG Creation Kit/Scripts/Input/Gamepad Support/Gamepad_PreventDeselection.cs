using RPGCreationKit.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RPGCreationKit;

public class Gamepad_PreventDeselection : MonoBehaviour
{
    EventSystem evt;
    GameObject sel;

    private void Start()
    {
        evt = EventSystem.current;
    }


    private void Update()
    {
        if (RckInput.isUsingGamepad)
        {
            if (evt.currentSelectedGameObject != null && evt.currentSelectedGameObject != sel)
                sel = evt.currentSelectedGameObject;
            else if (sel != null && evt.currentSelectedGameObject == null)
                evt.SetSelectedGameObject(sel);
        }
    }
}