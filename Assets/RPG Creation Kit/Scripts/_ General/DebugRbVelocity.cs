using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit
{
    /// <summary>
    /// Script that debugs the velocity of a rigidbody, used on runtime testing purposes.
    /// </summary>
    public class DebugRbVelocity : MonoBehaviour
    {
        Rigidbody rb;
        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            Debug.Log("RBV: " + rb.linearVelocity);
        }
    }
}