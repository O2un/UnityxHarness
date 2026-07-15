using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/MovementData")]
    public sealed class MovementData : ScriptableObject
    {
        [SerializeField] private float _maxMoveSpeed;
        [SerializeField] private float _jumpVelocity;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _groundCastDistance;
        [SerializeField] private Vector2 _groundCastSize;

        [SerializeField] private float _coyoteTime;
        [SerializeField] private float _jumpBufferTime;
        [SerializeField] private float _acceleration;
        [SerializeField] private float _deceleration;
        [SerializeField] private float _jumpCutFactor;

        public float MaxMoveSpeed => _maxMoveSpeed;
        public float JumpVelocity => _jumpVelocity;
        public LayerMask GroundMask => _groundMask;
        public float GroundCastDistance => _groundCastDistance;
        public Vector2 GroundCastSize => _groundCastSize;

        public float CoyoteTime => _coyoteTime;
        public float JumpBufferTime => _jumpBufferTime;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float JumpCutFactor => _jumpCutFactor;
    }
}
