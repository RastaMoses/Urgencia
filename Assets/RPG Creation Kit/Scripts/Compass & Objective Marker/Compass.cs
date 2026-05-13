using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;

namespace RPGCreationKit
{
    /// <summary>
    /// Manages the whole logic behind the Compass
    /// </summary>
    public class Compass : MonoBehaviour
    {

        #region Singleton
        public static Compass instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Debug.LogError("Anomaly detected with the Singleton Pattern of 'Compass', are you using multple Compass?");
                Destroy(this);
            }
        }
        #endregion

        // Transform of the camera
        [SerializeField] private Transform cameraT;

        // References to the compass UI
        [SerializeField] private RectTransform northPointer;
        [SerializeField] private RectTransform curObjPointer;

        // The current objective to point
        private Transform currentObjective;
        private Vector3 lastKnownPos;

        // The north direction
        private Vector3 northDir;

        // currentObjective
        private Vector3 objDir;
        private float angle;

        private void Start()
        {
            // At the start we look if the compass pointer (red one) should be active or not
            CheckActiveObjectives();
        }

        // Update is called once per frame
        private void Update()
        {
            // In each frame we calcualte and update the North and the Direction of the CurrentObjective
            CalculateNorth();

            if (currentObjective != null)
                CalculateObjDir();
            else
                CalculateObjDirLastPos();
        }

        /// <summary>
        /// Calculate and rotate the Compass to north
        /// </summary>
        private void CalculateNorth()
        {
            northDir.z = cameraT.eulerAngles.y;
            transform.localEulerAngles = northDir;
            northPointer.eulerAngles = northDir;
        }

        /// <summary>
        /// Calculate the direction of the object (from the UI pointer to the world) and rotate the pointer toward it
        /// </summary>
        private void CalculateObjDir()
        {
            // Calculate the direction
            objDir = cameraT.InverseTransformDirection(currentObjective.position - cameraT.position);
            // Calculate the angle
            angle = Mathf.Atan2(-objDir.x, objDir.z) * Mathf.Rad2Deg;
            // Rotate the pointer
            curObjPointer.eulerAngles = new Vector3(0, 0, angle);
            lastKnownPos = currentObjective.position;
        }

        /// <summary>
        /// Calculate the direction of the object (from the UI pointer to the world) and rotate the pointer toward it
        /// </summary>
        private void CalculateObjDirLastPos()
        {
            // Calculate the direction
            objDir = cameraT.InverseTransformDirection(lastKnownPos - cameraT.position);
            // Calculate the angle
            angle = Mathf.Atan2(-objDir.x, objDir.z) * Mathf.Rad2Deg;
            // Rotate the pointer
            curObjPointer.eulerAngles = new Vector3(0, 0, angle);
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

        /// <summary>
        /// Enable/Disable the Red Pointer when it is the case.
        /// </summary>
        public void CheckActiveObjectives()
        {
            // Disable the Red Pointer if no marked quest objective is active
            curObjPointer.gameObject.SetActive(currentObjective != null ? true : false);
        }
    }
}