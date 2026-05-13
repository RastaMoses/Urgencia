using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using TMPro;

namespace RPGCreationKit
{
    /// <summary>
    /// Attach this to a TMPRO text to make it display the game version
    /// </summary>
    public class GameVersionText : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<TextMeshProUGUI>().text = RCKSettings.GAME_VERSION;
        }
    }
}