using UnityEngine;
using System.Collections;
using RPGCreationKit;
using UnityEngine.EventSystems;
using RPGCreationKit.Player;
using UnityEngine.InputSystem;

namespace RPGCreationKit
{
    /// <summary>
    /// This script is just used to rotate the Player in the inventory
    /// </summary>
    public class RotateWithMouse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject PlayerInInventory;
        public float rotateAmount;

        private bool isOver = false;


        private float _sensitivity;
        private Vector2 _mouseReference;
        private Vector2 _mouseOffset;
        private Vector3 _rotation;
        private bool _isRotating;

        Vector3 center;

        void Start()
        {
            _sensitivity = 0.4f;
            _rotation = Vector3.zero;
        }

        public void Update()
        {
            RotatePlayer();
        }


        bool isClickingToRotate;
        public void RotatePlayer()
        {
            if (RckInput.isUsingGamepad)
            {
                Vector2 rotValue = RckInput.input.actions["RotatePlayer"].ReadValue<Vector2>();
                _rotation.y = -(rotValue.x) * (_sensitivity * 10);
                PlayerInInventory.transform.Rotate(_rotation);
            }
            else
            {
                isClickingToRotate = RckInput.input.actions["Click"].ReadValue<float>() > 0.1f;
                Vector2 mousePos = RckInput.input.actions["Aim"].ReadValue<Vector2>();

                if (isOver && isClickingToRotate && !_isRotating)
                {
                    // rotating flag
                    _isRotating = true;

                    // store mouse
                    _mouseReference = mousePos;
                }
                if (_isRotating)
                {
                    // offset
                    _mouseOffset = (mousePos - _mouseReference);

                    // apply rotation
                    _rotation.y = -(_mouseOffset.x + _mouseOffset.y) * _sensitivity;

                    // rotate
                    PlayerInInventory.transform.Rotate(_rotation);

                    // store mouse
                    _mouseReference = mousePos;

                    _isRotating = true;
                }

                if (!isClickingToRotate)
                    _isRotating = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isOver = true;

            // store mouse
            if (!_isRotating)
                _mouseReference = PlayerInInventory.transform.localRotation.eulerAngles;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isOver = false;
        }


    }

}