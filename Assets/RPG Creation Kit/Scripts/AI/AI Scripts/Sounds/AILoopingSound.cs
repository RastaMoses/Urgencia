using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace RPGCreationKit.AI
{ 
    // This script is useful for AI that emit sounds constantly, like the rats squaking.
    public class AILoopingSound : MonoBehaviour
    {
        public float initialPauseTime = 0.0f; // useful when having multiple creatures near and to avoid overlap
        public float minWaitTime = 0.0f;
        public float maxWaitTime = 5.0f;

        public AudioSource audioSource;
        public AudioClip clip;

        public RckAI ai;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            ai = GetComponentInParent<RckAI>();
            StartCoroutine(nameof(HandleAudio));
        }

        private void Update()
        {
            if (!ai.isAlive)
            {
                StopAllCoroutines();
                audioSource.Stop();
            }
        }


        IEnumerator HandleAudio()
        {
            yield return new WaitForSeconds(initialPauseTime);

            while(ai.isAlive)
            {
                audioSource.clip = clip;
                audioSource.Play();

                yield return new WaitForSeconds(clip.length);

                float randomWait = Random.Range(minWaitTime, maxWaitTime);

                yield return new WaitForSeconds(randomWait);
            }
        }
    }
}