using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using System;
using RPGCreationKit.Player;

namespace RPGCreationKit.AI
{
    /// <summary>
    /// Allows NPCs to look at target with the head and body
    /// </summary>
    public class AILookAt : MonoBehaviour
    {
        public bool isEnabled = true;

        // Force Target is used to force the AI to look at something, like in a conversation we always want the AI to look at the interlocutor
        public bool forceTarget = false;
        [SerializeField] Animator animator;

        [Space(5)]

        // Target determines the target to look at
        public Transform target;

        // Previous Target is used to restore the original view without jittering
        [HideInInspector] public Transform previousTarget;

        [Space(5)]
        public float rotSpeed = 1.0f;
        public bool bodyWeight = true;

        private float tTime = 0f;
        public bool shouldLook = false;

        public bool IsLookingAtSomeone
        {
            get
            {
                return shouldLook;
            }
        }


        private void Update()
        {
            if (!isEnabled)
                return;

            // Updates the tTime to manage the weight of which the limbs are affected in the rotation
            if (shouldLook)
            {
                tTime += rotSpeed * Time.deltaTime;
                tTime = Mathf.Clamp01(tTime);
            }
            else
            {
                tTime -= rotSpeed * Time.deltaTime;
                tTime = Mathf.Clamp01(tTime);
            }

        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!isEnabled)
                return;

            if (target != null)
            {
                if(bodyWeight)
                    animator.SetLookAtWeight(tTime / 2, tTime / 5, tTime, tTime, .7f);
                else
                    animator.SetLookAtWeight(tTime / 2, 0, tTime, tTime, .7f);

                animator.SetLookAtPosition(target.position);
            }
        }

        /// <summary>
        /// Sets a Target, this will not affect anything if the AI is currently looking in forceTarget mode.
        /// </summary>
        /// <param name="_target"></param>
        public void SetTarget(Transform _target)
        {
            if (_target.CompareTag("Player"))
                _target = RckPlayer.instance.entityFocusPart;

            if (forceTarget)
                return;

            if (target != null)
                previousTarget = target;

            target = _target;
            shouldLook = true;
        }

        /// <summary>
        /// Clears the target.
        /// </summary>
        public void ClearTarget()
        {
            shouldLook = false;
        }


        /// <summary>
        /// Make the AI look at the providen target, no matter what.
        /// </summary>
        /// <param name="_target"></param>
        public void ForceToLookAtTarget(Transform _target)
        {
            if (_target.CompareTag("Player"))
                _target = RckPlayer.instance.entityFocusPart;

            if (IsLookingAtSomeone == true)
            {
                forceTarget = true;

                if (target != null &&target.CompareTag("Player") && _target.CompareTag("Player"))
                    return;

                // Not if its the same pp
                if (_target != target)
                {
                    // If should look is true we have to debunk ttime and when it reaches 0 we set the new target and force
                    StartCoroutine(ForceLookTask(_target));
                }
                return;
            }

            target = _target;
            shouldLook = true;
            forceTarget = true;

            if (target.CompareTag("Player"))
                target = RckPlayer.instance.entityFocusPart;
        }


        public IEnumerator ForceLookTask(Transform _target)
        {
            shouldLook = false;

            // wait to debunk
            while(tTime >= 0.1f)
            { yield return null; }

            target = _target;
            shouldLook = true;
            forceTarget = true;
        }


        /// <summary>
        /// Releases the AI, allowing it to look at whatever it wants/prefers.
        /// </summary>
        public void StopForcingLookAtTarget()
        {
            shouldLook = false;
            forceTarget = false;
        }
    }
}