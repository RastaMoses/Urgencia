using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using TMPro;

namespace RPGCreationKit
{
    public class DisplayCharacterName : MonoBehaviour
    {
        // Start is called before the first frame update
        void OnEnable()
        {
            GetComponent<TextMeshProUGUI>().text = "<"+SaveSystem.SaveSystemManager.instance.saveFile.PlayerData.playerName+">";
        }
    }
}