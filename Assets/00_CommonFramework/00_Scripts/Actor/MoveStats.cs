using System;
using UnityEngine;

namespace O2un.Actors
{
    [Serializable]
    public struct MoveStats
    {
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _rotationSpeed;

        public float MoveSpeed => _moveSpeed;
        public float RotationSpeed => _rotationSpeed;
    }
}
