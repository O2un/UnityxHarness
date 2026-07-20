using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/MovementData")]
    public sealed class MovementData : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _maxMoveSpeed;
        [SerializeField, Min(0f)] private float _jumpVelocity;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField, Min(0f)] private float _groundCastDistance;
        [SerializeField] private Vector2 _groundCastSize;

        [SerializeField, Min(0f)] private float _coyoteTime;
        [SerializeField, Min(0f)] private float _jumpBufferTime;
        [SerializeField, Min(0f)] private float _acceleration;
        [SerializeField, Min(0f)] private float _deceleration;
        [SerializeField, Range(0f, 1f)] private float _jumpCutFactor;

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
