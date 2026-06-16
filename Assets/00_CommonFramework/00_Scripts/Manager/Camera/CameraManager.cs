using System;
using R3;
using Unity.Cinemachine;
using UnityEngine;

namespace O2un.Camera 
{
    public sealed class CameraManager
    {
        private readonly CinemachineCamera _gamePlayerCamera;
        private readonly CinemachineCamera _cinematicCamera;

        public CameraManager(CinemachineCamera gamePlay, CinemachineCamera cinematic)
        {
            _gamePlayerCamera = gamePlay;
            _cinematicCamera = cinematic;
            
            _gamePlayerCamera.Priority = 10;
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
