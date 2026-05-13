using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEngine.InputSystem;

namespace RPGCreationKit.Player
{
    /// <summary>
    /// Base Class of RckPlayer that contains vital references to other components used troughout all the Player code
    /// </summary>
    public class PlayerBase : EntityFactionable
    {
        // References
        public Camera mainCamera;
        public Camera tpsCamera;
        public Camera weaponCamera;

        [SerializeField] public EntityAttributes playerAttributes;
        [SerializeField] public CameraAnimation cameraAnim;

        [SerializeField] protected Inventory inventory;
        [SerializeField] protected Equipment equipment;

        [SerializeField] public PlayerUIManager uiManager;



        public PlayerInput input;

        public virtual void Start()
        {
            input = GetComponent<PlayerInput>();
            playerAttributes = EntityAttributes.PlayerAttributes;
        }

        public virtual void Update()
        {

        }
    }
}