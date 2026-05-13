using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;


namespace RPGCreationKit
{
    /// <summary>
    /// Editor for NPC_PathPoint to dinamically show/hide fields
    /// </summary>
    [CustomEditor(typeof(NPC_PathPoint))]
    public class NPC_PathPointEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var main = target as NPC_PathPoint;


            if (main.Wait)
            {
                main.WaitTime = EditorGUILayout.FloatField("Wait Time", main.WaitTime);
                main.DirectionToFace = (Transform)EditorGUILayout.ObjectField("Direction To Face:", main.DirectionToFace, typeof(Transform), true);
            }

            if (GUI.changed)
                EditorUtility.SetDirty(target);

        }
    }
}
