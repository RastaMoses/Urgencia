using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGCreationKit
{
    public class BloodScreenEffect : MonoBehaviour
    {
        #region Singleton
        public static BloodScreenEffect instance;
        private void Awake()
        {
            if(instance == null)
                instance = this;
            else
            {
                Debug.Log("Anomaly detected with the singleton pattern of 'BloodScreenEffect', do you have multiple instances?");
                Destroy(this);
            }
        }
        #endregion

        public bool effectEnabled = true;

        [SerializeField] private Image bloodImg;

        [SerializeField] private float minAlpha;
        [SerializeField] private float maxAlpha;
        [SerializeField] private float fadeInSpeed;
        [SerializeField] private float durationPerHit;
        [SerializeField] private float fadeOutSpeed;

        bool gotHit = false;
        bool endEffect = false;
        float timeFull = 0;
        Color color;

        private void Start()
        {
            color = bloodImg.color;
        }

        private void Update()
        {
            if (!effectEnabled)
                return;
            
            if(gotHit)
            {
                if (!endEffect)
                {
                    color.a += fadeInSpeed * Time.deltaTime;

                    if (color.a >= maxAlpha)
                    {
                        color.a = maxAlpha;

                        if (timeFull > durationPerHit)
                            endEffect = true;

                        timeFull += 1 * Time.deltaTime;
                    }
                }
                else
                {
                    color.a -= fadeOutSpeed * Time.deltaTime;

                    if (color.a <= minAlpha)
                    {
                        color.a = minAlpha;
                        gotHit = false;
                    }
                }
            }

            bloodImg.color = color;
        }

        public void OnHit()
        {
            if (!effectEnabled)
                return;

            gotHit = true;
            endEffect = false;
            timeFull = 0;
        }
    }
}