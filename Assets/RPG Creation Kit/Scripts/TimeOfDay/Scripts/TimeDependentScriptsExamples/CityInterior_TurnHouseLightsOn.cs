using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit
{
    public class CityInterior_TurnHouseLightsOn : MonoBehaviour
    {
        public List<Renderer> houseWindowRenderer;
        bool isEmissionActive = false;


        void Awake()
        {
            isEmissionActive = false;
        }

        private void OnEnable()
        {
            TimeOfDayManager.instance.onHourChanges -= HandleOnHourChange;
            TimeOfDayManager.instance.onHourChanges += HandleOnHourChange;

            if (TimeOfDayManager.instance != null)
                HandleOnHourChange(TimeOfDayManager.instance.hours);
        }

        private void OnDisable()
        {
            TimeOfDayManager.instance.onHourChanges -= HandleOnHourChange;
        }

        private void OnDestroy()
        {
            TimeOfDayManager.instance.onHourChanges -= HandleOnHourChange;
        }

        public void HandleOnHourChange(int curHour)
        {
            // It's better to get the renderer's material instance 
            // rather than modifying the public Asset directly.

            if (curHour >= 7 && curHour < 20)
            {
                if (isEmissionActive)
                {
                    foreach (Renderer r in houseWindowRenderer)
                    {
                        r.material.DisableKeyword("_EMISSION");
                        r.material.SetColor("_EmissionColor", Color.black);
                    }
                    isEmissionActive = false;
                }
            }
            else if (!isEmissionActive)
            {
                foreach (Renderer r in houseWindowRenderer)
                {
                    r.material.EnableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", Color.white * 2f); // 2f for HDR intensity
                }
                isEmissionActive = true;
            }
        }
    }
}
