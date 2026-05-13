using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RPGCreationKit;
using UnityEngine.UI;
public class DisplayTotalEncumbranceText : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        GetComponent<Text>().text = EntityAttributes.PlayerAttributes.derivedAttributes.maxEncumbrance.ToString();
    }
}
