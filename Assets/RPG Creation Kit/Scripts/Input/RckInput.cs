using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Contains static references of information to the Input in the RPG Creation Kit
/// </summary>
namespace RPGCreationKit
{
    public static class RckInput
    {
        public static PlayerInput input { get; private set; }
        public static bool isUsingGamepad
        {
            get { return input.currentControlScheme == "Gamepad"; }
        }

        public static string currentControlScheme;

        public static void LoadInputSave()
        {
            currentControlScheme = "Gamepad";
            //currentControlScheme = "Gamepad";
        }

        public static void SetPlayerInputInstance(PlayerInput _input)
        {
            input = _input;
            input.SwitchCurrentControlScheme(currentControlScheme);
        }

        public static void SetGamepadInput()
        {
            currentControlScheme = "Gamepad";
            input.SwitchCurrentControlScheme(currentControlScheme);
        }

        public static void SetKeyboardInput()
        {
            currentControlScheme = "Keyboard&Mouse";
            input.SwitchCurrentControlScheme(currentControlScheme);
        }
    }
}