using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.SaveSystem;

namespace RPGCreationKit
{
    public class Ragdoll : MonoBehaviour
    {
        public Animator animator;
        [SerializeField] Rigidbody[] rigidbodies;
        [SerializeField] public Collider[] colliders;
        [SerializeField] Quaternion[] rotations;

        bool isInRagdoll = false;

        private void OnEnable()
        {
            if (animator == null)
                animator = GetComponentInParent<Animator>();

            for(int i = 0; i < rotations.Length; i++)
            {
                rigidbodies[i].linearVelocity = rigidbodies[i].angularVelocity = Vector3.zero;
            }
        }


        // Start is called before the first frame update
        void Start()
        {
            if (animator == null)
                animator = GetComponentInParent<Animator>();

            foreach (Rigidbody ragdollBone in rigidbodies)
            {
                ragdollBone.collisionDetectionMode = CollisionDetectionMode.Discrete;
                ragdollBone.isKinematic = true;
            }
        }

        [ContextMenu("Force Ragdoll")]
        public void ForceRagdoll()
        {
            animator.enabled = false;

            foreach (Rigidbody ragdollBone in rigidbodies)
            {
                ragdollBone.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                ragdollBone.isKinematic = false;
                RCKLayers.SetLayer(ragdollBone.gameObject, RCKLayers.IgnorePlayer);
            }
        }

        public void ToggleRagdoll(bool bisAnimating)
        {
            isInRagdoll = !bisAnimating;

            foreach (Rigidbody ragdollBone in rigidbodies)
            {
                if (bisAnimating)
                {
                    ragdollBone.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                }
                else
                {
                    ragdollBone.collisionDetectionMode = CollisionDetectionMode.Discrete;
                }

                ragdollBone.isKinematic = bisAnimating;

                if (isInRagdoll)
                    RCKLayers.SetLayer(ragdollBone.gameObject, RCKLayers.IgnorePlayer);
            }

            animator.enabled = bisAnimating;
        }

        [ContextMenu("Find Rigidbodies")]
        private void FindRigidbodies()
        {
            rigidbodies = GetComponentsInChildren<Rigidbody>();
        }

        public void LoadFromSaveData(RagdollSaveData data)
        {
            for (int i = 0; i < data.ragdollElements.Count; i++)
            {
                rigidbodies[i].gameObject.transform.position = data.ragdollElements[i].pos;
                rigidbodies[i].gameObject.transform.rotation = data.ragdollElements[i].rot;
            }
        }

        public RagdollSaveData ToSaveData()
        {
            RagdollSaveData data = new RagdollSaveData();

            for (int i = 0; i < rigidbodies.Length; i++)
                data.ragdollElements.Add(new RagdollSaveData.RagdollElement(rigidbodies[i].gameObject.transform.position, rigidbodies[i].gameObject.transform.rotation));

            return data;
        }
    }
}