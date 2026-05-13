using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    public class HorizontalCompass : MonoBehaviour
    {
        #region Singleton
        public static HorizontalCompass instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Debug.LogError("Anomaly detected with the Singleton Pattern of 'HorizontalCompass', are you using multple HorizontalCompass?");
                Destroy(this);
            }
        }
        #endregion

        [SerializeField] RawImage compassContent;
        [SerializeField] Transform player;

        [SerializeField] private Image curObjMarker;
        private Transform currentObjective;

        private Rect compassRect;
        
        float compassUnit;

        // Locations
        public GameObject compassLocationPrefab;

        public List<CompassLocation> locations = new List<CompassLocation>();

        // Start is called before the first frame update
        private void Start()
        {
            compassUnit = compassContent.rectTransform.rect.width / 360f;

            // At the start we look if the compass pointer (red one) should be active or not
            CheckActiveObjectives();
        }

        // Update is called once per frame
        void Update()
        {
            compassContent.uvRect = new Rect(player.localEulerAngles.y / 360f, 0f, 1f, 1f);

            // Update current objective if active
            if (currentObjective != null && curObjMarker.gameObject.activeSelf)
            {
                curObjMarker.rectTransform.anchoredPosition = TransformToCompass(currentObjective.transform);
            }

            // Update all loaded locations
            for (int i = 0; i < locations.Count; i++)
            {
                if ((locations[i].isInExteriorWorldspace && locations[i].WorldspaceID == WorldManager.instance.currentWorldspace.worldSpaceID) ||
                    (!locations[i].isInExteriorWorldspace && locations[i].CellID == WorldManager.instance.currentCenterCell.ID))
                {
                    locations[i].im.rectTransform.anchoredPosition = TransformToCompass(locations[i].transform);

                    float dst = Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(locations[i].transform.position.x, locations[i].transform.position.z));
                    float scale = 0f;

                    if (dst < RCKSettings.HORCOMPASS_MAX_DISTANCE_LOC_VISIBLE)
                        scale = 1f - (dst / RCKSettings.HORCOMPASS_MAX_DISTANCE_LOC_VISIBLE);

                    locations[i].im.rectTransform.localScale = Vector3.one * scale;
                }
            }
        }

        /// <summary>
        /// Enable/Disable the Red Pointer when it is the case.
        /// </summary>
        public void CheckActiveObjectives()
        {
            // Disable the Red Pointer if no marked quest objective is active
            curObjMarker.gameObject.SetActive(currentObjective != null ? true : false);
        }

        /// <summary>
        /// Called when a Quest or a QuestObjective updates.
        /// </summary>
        public void OnQuestUpdate()
        {
            currentObjective = null;
            CheckActiveObjectives();
        }

        /// <summary>
        /// Called when a new Quest Objective is active
        /// </summary>
        /// <param name="t">The transform to point to</param>
        public void ChangeQuestObjective(Transform t)
        {
            currentObjective = t;
            CheckActiveObjectives();
        }

        Vector2 TransformToCompass(Transform t)
        {
            // Adjust player transform for Mount if needed
            if (RckPlayer.instance.isMounted)
                player = RckPlayer.instance.aiMounting.transform;
            else
                player = RckPlayer.instance.transform;

            Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.z);
            Vector2 playerFwd = new Vector2(player.transform.forward.x, player.transform.forward.z);

            float angle = Vector2.SignedAngle(new Vector2(t.position.x, t.position.z) - playerPos, playerFwd);

            return new Vector2(compassUnit * angle, 0f);
        }

        public void AddCompassLocation(CompassLocation loc)
        {
            GameObject newLoc = Instantiate(compassLocationPrefab, compassContent.transform);
            loc.im = newLoc.GetComponent<Image>();
            loc.im.sprite = loc.icon;

            // Start hidden
            loc.im.rectTransform.localScale = Vector3.one * 0;

            locations.Add(loc);
        }

        public void RemoveCompassLocation(CompassLocation loc)
        {
            if (loc != null)
            {
                HorizontalCompass.instance.locations.Remove(loc);

                if(loc.im != null)
                    Destroy(loc.im.gameObject);
            }
        }
    }
}