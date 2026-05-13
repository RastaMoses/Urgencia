using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit.CellsSystem
{
    /// <summary>
    /// Represents the point of a door that the AI needs to reach before being able to teleport to the linked cell
    /// </summary>
    public class DoorGroundPivot : MonoBehaviour, ITargetable
    {
        public Door door;

        // Start is called before the first frame update
        void Start()
        {
            if (door == null)
                door = GetComponentInParent<Door>();
        }

        string ITargetable.GetExtraData()
        {
            return door.toCell.ID;
        }

        string ITargetable.GetID()
        {
            return door.objReference;
        }

        ITargetableType ITargetable.GetTargetableType()
        {
            return ITargetableType.Door;
        }



    }
}