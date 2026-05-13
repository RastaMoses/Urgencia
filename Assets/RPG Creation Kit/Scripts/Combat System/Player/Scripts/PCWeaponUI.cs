using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class PCWeaponUI : MonoBehaviour
    {
        public Image weaponIcon;
        public TextMeshProUGUI ammoAmount;
        public GameObject weaponContainer;
        public GameObject spellContainer;

        public void SetAmmoAmount(bool _enabled, string _amount = "")
        {
            ammoAmount.gameObject.SetActive(_enabled);
            ammoAmount.text = "x"+_amount;
        }

        public void SetAmmoAmountFirearm(bool _enabled, int _clip, int _inventory)
        {
            ammoAmount.gameObject.SetActive(_enabled);
            ammoAmount.text = _clip + " / " + _inventory;
        }
    }
}