using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class BodyPart : MonoBehaviour, IHittable, IDraggable
    {
        public enum BodyParts
        {
            Head = 0,
            Upperchest = 1,
            Chest = 2,
            Arm = 3,
            Hand = 4,
            Leg = 5,
            Foot = 6
        };

        public EntityAttributes entity;
        public BodyParts thisBodyPart;
        [SerializeField] Rigidbody rb;

        private void Reset()
        {
            entity = GetComponentInParent<EntityAttributes>();
            rb = GetComponent<Rigidbody>();
            gameObject.tag = "RPG Creation Kit/BodyPart";
            RCKLayers.SetLayer(gameObject, RCKLayers.BodyPart, false);
        }

        public void Hit(Vector3 hitDir, float forceAmount = 25, DamageContext damageContext = null)
        {
            if(rb)
                rb.AddForce(hitDir * forceAmount, ForceMode.Impulse);
        }

        public void GenerateDecal()
        {

        }

        Collider coll;
        private void OnDrawGizmos()
        {
            if (!coll)
                coll = GetComponent<Collider>();

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(coll.bounds.center, .05f);
        }

        void IDraggable.OnBeingDragged()
        {

        }

        void IDraggable.OnDragEnds()
        {

        }

        GameObject IDraggable.GetGameObject()
        {
            return gameObject;
        }
    }
}