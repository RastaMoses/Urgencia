using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RPGCreationKit;

namespace RPGCreationKit.Player
{
    public class MouseLook : MonoBehaviour
    {
        public bool isMountMouseLook = false;

        public bool lookEnabled = true;
        public bool forceLookAtTarget = false;
        Transform target;
        public float rotSpeed = 4.5f;


        public Transform player;

        private float curXSensitivy;
        private float curYSensitivy;

        // Abs values
        public float x,y;
        public float xRotation = 0f;

        // Movements values
        float xMov;
        float yMov;

        public float xLimitUpFPS;
        public float xLimitDownFPS;

        public float xLimitUpTPS;
        public float xLimitDownTPS;

        public float recoil;

        public void INPUT_OnLookChanges(InputAction.CallbackContext value)
        {
            Vector2 dValue = value.ReadValue<Vector2>();
            x = dValue.x;
            y = dValue.y;
        }

        public void INPUT_OnLookChanges2(Vector2 value)
        {
            Vector2 dValue = value;
            x = dValue.x;
            y = dValue.y;
        }

        public void INPUT_OnLookEnd(InputAction.CallbackContext value)
        {
            x = 0;
            y = 0;
        }

        private void Start()
        {

        }

        private void LateUpdate()
        {
            if (!forceLookAtTarget)
            {
                if (RckInput.isUsingGamepad)
                {
                    curXSensitivy = RCKSettings.MOUSE_HORIZONTAL_SPEED * 10;
                    curYSensitivy = RCKSettings.MOUSE_VERTICAL_SPEED * 10;
                }
                else
                {
                    curXSensitivy = RCKSettings.MOUSE_HORIZONTAL_SPEED;
                    curYSensitivy = RCKSettings.MOUSE_VERTICAL_SPEED;
                }

                if (lookEnabled)
                {
                    if (RckPlayer.instance.isDismounting)
                        x = 0;

                    xMov = x * curXSensitivy * Time.deltaTime;
                    yMov = y * curYSensitivy * Time.deltaTime;

                    recoil -= RCKSettings.RECOIL_DECRASE_RATE * Time.deltaTime;
                    recoil = Mathf.Max(recoil, 0f); // Ensure recoil doesn't go below 0

                    yMov += recoil * (RckPlayer.instance.iscrouching ? RCKSettings.RECOIL_CROUCHED_REDUCTION : 1.0f) * Time.deltaTime;
                    xMov += Random.Range(-recoil, +recoil) * (RckPlayer.instance.iscrouching ? RCKSettings.RECOIL_CROUCHED_REDUCTION : 1.0f) * Time.deltaTime;

                    xRotation -= yMov;
                    xRotation = Mathf.Clamp(xRotation, RckPlayer.instance.isInThirdPerson ? xLimitDownTPS : xLimitDownFPS, RckPlayer.instance.isInThirdPerson ? xLimitUpTPS : xLimitUpFPS);

                    transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                    player.Rotate(Vector3.up * xMov);
                }
            }
            else if(target != null)
            {
                // Look at target (Camera):
                var lookAtPos = target.position - transform.position;
                var lookAt = Quaternion.LookRotation(lookAtPos);

                lookAt.y = 0;
                lookAt.z = 0;

                xRotation = lookAt.eulerAngles.x;
                if (xRotation > 180)
                    xRotation -= 360f;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);

                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(xRotation, 0f, 0f), rotSpeed * Time.deltaTime);


                // Rotate towards target (Player):
                lookAtPos = target.position - player.position;
                lookAt = Quaternion.LookRotation(lookAtPos);

                lookAt.x = 0;
                lookAt.z = 0;

                player.rotation = Quaternion.RotateTowards(player.rotation, lookAt, DIALOGUE_CAMERA_ROTATION_SPEED * Time.deltaTime);
            }

            /*
            if(RckPlayer.instance.isInDeathSequence)
            {
                transform.LookAt(RckPlayer.instance.transform);
            }
            */
        }

        public float DIALOGUE_CAMERA_ROTATION_SPEED = 200f;

        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void UnlockCursor()
        {
            if(RckInput.isUsingGamepad)
            {
                Cursor.lockState = CursorLockMode.Locked;
                return;
            }

            Cursor.lockState = CursorLockMode.None;
        }

        public void LoadSavegameRotation()
        {
            xRotation = SaveSystem.SaveSystemManager.instance.saveFile.PlayerData.mouseRotX;
        }

        public void ApplyCurrentRotation()
        {

        }

        private static float UnwrapAngle(float angle)
        {
            if (angle >= 0)
                return angle;

            angle = -angle % 360;

            return 360 - angle;
        }

        public static float Clamp0360(float eulerAngles)
        {
            float result = eulerAngles - Mathf.CeilToInt(eulerAngles / 360f) * 360f;
            if (result < 0)
            {
                result += 360f;
            }
            return result;
        }

        public void SetLookAtTarget(Transform _target)
        {
            target = _target;
            forceLookAtTarget = true;
        }

        public void ClearLookAt()
        {
            forceLookAtTarget = false;
            target = null;
        }

        public void BindToRCKInput()
        {
            RckInput.input.actions["Look"].performed += INPUT_OnLookChanges;
            RckInput.input.actions["Look"].canceled += INPUT_OnLookEnd;

            //Invoke("UnbindToRCKInput", 3);
        }

        public void UnbindToRCKInput()
        {
            RckInput.input.actions["Look"].performed -= INPUT_OnLookChanges;
            RckInput.input.actions["Look"].canceled -= INPUT_OnLookEnd;

            x = 0;
            y = 0;
        }
    }
}