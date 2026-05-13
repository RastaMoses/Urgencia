using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    public enum CameraAttackAnimation
    {
        None = 0,
        DownToUp = 1,
        UpToDown = 2,
        RightToLeft = 3,
        LeftToRight = 4,
        DownLeftToUpperRight = 5,
        UpperRightToDownLeft = 6,
        UpperLeftToDownRight = 7,
        DownRightToUpperLeft = 8
    };

    public class CameraAnimation : MonoBehaviour
    {
        public Animator m_Animator;
        public PlayerCombat PCombat;

        public Camera FPCharacterCamera;
        public Camera FPArmsCamera;

        public Camera TPSCamera;

        public bool hasToTarget = false;
        Vector3 target;
        float interpolationRatio = 1f;

        public Transform tpsLookAtTarget;

        public void Update()
        {
            if (hasToTarget)
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, interpolationRatio * Time.deltaTime);

        }

        public void LerpCamera(Vector3 _target, float _interpolationRatio)
        {
            target = _target;
            interpolationRatio = _interpolationRatio;
            hasToTarget = true;
        }

        public void MoveCamera(Vector3 _target, bool stopCam = false)
        {
            transform.localPosition = _target;

            if (stopCam)
                StopCam();
        }

        public void StopCam()
        {
            hasToTarget = false;
        }

        public void AttackAnimation(CameraAttackAnimation anim)
        {
            switch (anim)
            {
                case CameraAttackAnimation.None:
                    break;

                case CameraAttackAnimation.LeftToRight:
                    m_Animator.SetTrigger("LeftToRight");
                    break;
            }
        }

        public void SwitchToFPSCamera()
        {
            FPCharacterCamera.enabled = true;
            FPArmsCamera.enabled = true;
            TPSCamera.enabled = false;

            FPCharacterCamera.gameObject.GetComponent<AudioListener>().enabled = true;
            TPSCamera.gameObject.GetComponent<AudioListener>().enabled = false;

        }

        public void SwitchToTPSCamera()
        {
            FPCharacterCamera.enabled = false;
            FPArmsCamera.enabled = false;
            TPSCamera.enabled = true;

            FPCharacterCamera.gameObject.GetComponent<AudioListener>().enabled = false;
            TPSCamera.gameObject.GetComponent<AudioListener>().enabled = true;
        }

        public void DisableAnimator()
        {
            m_Animator.enabled = false;
        }

        public void EnableAnimator()
        {
            m_Animator.enabled = true;
        }

        public void DisableAllCameras()
        {
            FPCharacterCamera.enabled = false;
            FPArmsCamera.enabled = false;
            TPSCamera.enabled = false;
        }

        Vector3 preMountPosFPS;
        Quaternion preMountRotFPS;
        public void FPSMoveToMountingPos(Transform fpsCameraAnimPos)
        {
            preMountPosFPS = transform.localPosition;
            preMountRotFPS = transform.localRotation;

            transform.position = fpsCameraAnimPos.position;
            transform.rotation = fpsCameraAnimPos.rotation;
        }

        Vector3 preMountPosTPS;
        Quaternion preMountRotTPS;
        public void TPSMoveToMountingPos(Transform tpsCameraPos)
        {
            preMountPosTPS = ThirdPersonPlayer.instance.TPSCameraHold.transform.localPosition;
            preMountRotTPS = ThirdPersonPlayer.instance.TPSCameraHold.transform.localRotation;

            ThirdPersonPlayer.instance.TPSCameraHold.transform.position = tpsCameraPos.position;
            ThirdPersonPlayer.instance.TPSCameraHold.transform.rotation = tpsCameraPos.rotation;
        }

        public void TPSRestoreFromMountingPos()
        {
            ThirdPersonPlayer.instance.TPSCameraHold.transform.localPosition = preMountPosTPS;
            ThirdPersonPlayer.instance.TPSCameraHold.transform.localRotation = preMountRotTPS;
        }

        public void FPSRestoreFromMountingPos()
        {
            transform.localPosition = preMountPosFPS;
            transform.localRotation = preMountRotFPS;
        }
    }
}