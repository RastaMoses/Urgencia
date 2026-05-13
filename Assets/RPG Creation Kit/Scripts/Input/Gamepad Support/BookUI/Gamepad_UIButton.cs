using RPGCreationKit.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;

public class Gamepad_UIButton : MonoBehaviour
{
    public Button _button;
    public string InputActionName;

    public GameObject graphics;

    private void Update()
    {
        if(RckInput.input.currentActionMap.FindAction(InputActionName) != null)
        {
            if(RckInput.input.currentActionMap.FindAction(InputActionName).triggered)
                _button.onClick.Invoke();
        }
    }
}
