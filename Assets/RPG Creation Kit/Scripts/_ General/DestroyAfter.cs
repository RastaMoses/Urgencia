using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RPGCreationKit
{
    /// <summary>
    /// Destroys a GameObject after X seconds.
    /// </summary>
    public class DestroyAfter : MonoBehaviour
    {
        [SerializeField] public float time = 0;

        DestroyAfter(float time)
        {
            this.time = time;
        }

        // Update is called once per frame
        void Start()
        {
            Destroy(this.gameObject, time);
        }
    }
}