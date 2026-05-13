using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.CellsSystem;

namespace RPGCreationKit.CellsSystem
{
    public class CellEntryTransform : MonoBehaviour
    {

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Color c = Gizmos.color;

            Gizmos.color = Color.red;
            DrawCube(transform.position, transform.rotation, transform.localScale);
            GizmosExtension.ArrowForGizmo(transform.position + (Vector3.up * .5f), transform.forward, Color.red);

            Gizmos.color = c;
        }
#endif

        public static void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

            Gizmos.matrix *= cubeTransform;

            Gizmos.DrawSphere(Vector3.zero, 1);

            Gizmos.matrix = oldGizmosMatrix;
        }
    }
}