using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    [System.Flags]
    public enum TradableItems
    {
        Ammos = (1 << 0),
        Armors = (1 << 1),
        Books = (1 << 2),
        Consumables = (1 << 3),
        Keys = (1 << 4),
        Miscs = (1 << 5),
        Weapons = (1 << 6)
    };

    public class Merchant : MonoBehaviour
    {
        public string ID;
        public string merchantName;

        [Space(5)]

        public TradableItems WantsToBuy;
        public bool buysStolenItems = false;

        public float buyMultiplier = 0.75f;
        public float sellMultiplier = 1.55f;

        [Space(5)]

        public Inventory inventory;
        public Equipment equipment;
    }
}