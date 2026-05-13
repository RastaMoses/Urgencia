using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class CompassLocation : MonoBehaviour
    {
        [HideInInspector] public Image im;
        public Sprite icon;

        public bool isInExteriorWorldspace;
        public string WorldspaceID;
        public string CellID;

        public void OnEnable()
        {
            if(RCKSettings.HORIZONTAL_COMPASS_ENABLED)
                HorizontalCompass.instance.AddCompassLocation(this);
        }

        private void OnDisable()
        {
            if (RCKSettings.HORIZONTAL_COMPASS_ENABLED)
                HorizontalCompass.instance.RemoveCompassLocation(this);
        }
    }
}