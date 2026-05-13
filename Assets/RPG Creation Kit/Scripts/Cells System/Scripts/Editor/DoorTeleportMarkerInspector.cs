using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.CellsSystem
{
    [CustomEditor(typeof(DoorTeleportMarker))]
    public class DoorTeleportMarkerInspector : Editor
    {
        [SerializeField] DoorTeleportMarker myDoorT;

        public override void OnInspectorGUI()
        {
            myDoorT = (DoorTeleportMarker)target;

            base.OnInspectorGUI();

            GUI.enabled = myDoorT.owner;
            if(GUILayout.Button("View Door"))
            {
                GameObject[] newSelection = new GameObject[1];
                newSelection[0] = myDoorT.owner.gameObject;
                Selection.objects = newSelection;
            }
        }
    }
}