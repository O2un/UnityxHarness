using System;
using R3;
using Unity.Cinemachine;
using UnityEngine;

namespace O2un.Camera 
{
    public sealed class CameraManager : ICameraBasisProvider
    {
        private const float PLANAR_EPSILON = 0.0001f;

        private readonly CinemachineCamera _gamePlayerCamera;
        private readonly CinemachineCamera _cinematicCamera;

        public CameraManager(CinemachineCamera gamePlay, CinemachineCamera cinematic)
        {
            _gamePlayerCamera = gamePlay;
            _cinematicCamera = cinematic;

            _gamePlayerCamera.Priority = 10;
        }

        public Vector3 PlanarForward
        {
            get
            {
                Transform cameraTransform = _gamePlayerCamera.transform;
                Vector3 planar = ProjectToPlane(cameraTransform.forward);
                if (planar.sqrMagnitude < PLANAR_EPSILON)
                {
                    planar = ProjectToPlane(cameraTransform.up);
                }

                return planar.normalized;
            }
        }

        public Vector3 PlanarRight
        {
            get
            {
                Transform cameraTransform = _gamePlayerCamera.transform;
                Vector3 planar = ProjectToPlane(cameraTransform.right);
                if (planar.sqrMagnitude < PLANAR_EPSILON)
                {
                    planar = ProjectToPlane(cameraTransform.up);
                }

                return planar.normalized;
            }
        }

        private static Vector3 ProjectToPlane(Vector3 vector)
        {
            return new Vector3(vector.x, 0f, vector.z);
        }

        public void SetFollowTarget(Transform target)
        {
            _gamePlayerCamera.Follow = target;
            _gamePlayerCamera.LookAt = target;
        }

        public void SwitchToGamePlay() => _cinematicCamera.Priority = 0;
        public void SwitchToCinematic() => _cinematicCamera.Priority = 20;
    }
}
