using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.Game.ResultScripts
{
    public class TKQL_TheGoodNews_ResetKingsRoomOwnership : ResultScript
    {
        void Start()
        {
            // Your code here

            // At this point we're in the King's room, get all the Items In Wolrd and reset ownership so that the player may take them
            ItemInWorld[] items = (ItemInWorld[])GameObject.FindObjectsOfType(typeof(ItemInWorld));
            foreach(ItemInWorld item in items)
            {
                if (item.isActiveAndEnabled && item.metadata.ownerID == "TheKing001")
                {
                    item.metadata.Clear();
                    item.SetTouched(); // make sure the change is saved on the save file
                }
            }


            // Destroy the script
            Destroy(this);
        }
    }
}