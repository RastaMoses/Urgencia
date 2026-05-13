using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RPGCreationKit
{
    /// <summary>
    /// Destroys a GameObject after X seconds.
    /// </summary>
    public class SpellExplodeAfter : MonoBehaviour
    {
        public SpellProjectile thisSpell;
        [SerializeField] public float time = 0;

        SpellExplodeAfter(float time)
        {
            this.time = time;
        }

        // Update is called once per frame
        void Start()
        {
            Invoke("Explode", time);
        }

        public void Explode()
        {
            thisSpell.Explode();
        }
    }
}