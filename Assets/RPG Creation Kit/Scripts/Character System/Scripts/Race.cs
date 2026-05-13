using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Represent a Race in the RPG Creation Kit
    /// </summary>
    [CreateAssetMenu(fileName = "New Race", menuName = "RPG Creation Kit/Character/New Race", order = 1)]
    [System.Serializable]
    public class Race : ScriptableObject
    {
        //- General data
        [SerializeField] public string ID;
        public string raceName;

        //- Attributes
        public BaseAttributes baseAttributesMale;
        public BaseAttributes baseAttributesFemale;

        public EntityDerivedAttributes baseDerivedAttributesMale;
        public EntityDerivedAttributes baseDerivedAttributesFemale;

        //- Body Data
        public BodyData maleModel;
        public BodyData femaleModel;

        public PCFPSArms maleFPS;
        public PCFPSArms femaleFPS;
         
        public List<Hair> maleHairTypes = new List<Hair>();
        public List<Hair> femaleHairTypes = new List<Hair>();

        public List<Eye> eyeTypes = new List<Eye>();

        public int defaultMaleHair = -1;
        public int defaultFemaleHair = -1;

        public int defaultMaleEye = 0;
        public int defaultFemaleEye = 0;

    }
}