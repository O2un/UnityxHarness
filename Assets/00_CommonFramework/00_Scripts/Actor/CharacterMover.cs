using System;
using R3;
using UnityEngine;

namespace O2un.Actors
{
    public sealed class CharacterMover : IDisposable
    {
        private const float DIRECTION_EPSILON = 0.0001f;

        private readonly float _moveSpeed;
        private readonly float _rotationSpeed;

        public CharacterMover(MoveStats stats)
        {
            _moveSpeed = stats.MoveSpeed;
            _rotationSpeed = stats.RotationSpeed;
            TargetRotation = Quaternion.identity;
        }

        private readonly ReactiveProperty<Vector3> _velocity = new();

        public ReadOnlyReactiveProperty<Vector3> Velocity => _velocity;
        public Quaternion TargetRotation { get; private set; }
        public float RotationSpeed => _rotationSpeed;

        public void SetDirection(Vector3 worldDir)
        {
            if (worldDir.sqrMagnitude < DIRECTION_EPSILON)
            {
                _velocity.Value = Vector3.zero;
                return;
            }

            _velocity.Value = worldDir * _moveSpeed;
            TargetRotation = Quaternion.LookRotation(worldDir, Vector3.up);
        }

        public void Dispose()
        {
            _velocity.Dispose();
        }
    }
}
