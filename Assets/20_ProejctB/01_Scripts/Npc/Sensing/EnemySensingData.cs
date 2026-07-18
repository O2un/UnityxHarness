using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/EnemySensingData")]
    public sealed class EnemySensingData : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _sightDistance = 8f;
        [SerializeField, Range(0f, 360f)] private float _sightAngle = 60f;
        [SerializeField, Min(0f)] private float _hearingRadius = 6f;
        [SerializeField, Min(0f)] private float _aggroRange = 10f;
        [SerializeField, Min(0.01f)] private float _sensingInterval = 0.1f;

        [SerializeField, Min(0f)] private float _groundProbeDistance = 1f;
        [SerializeField, Min(0f)] private float _wallProbeDistance = 0.5f;

        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private LayerMask _groundMask;

        public float SightDistance => _sightDistance;
        public float SightAngle => _sightAngle;
        public float HearingRadius => _hearingRadius;
        public float AggroRange => _aggroRange;
        public float SensingInterval => _sensingInterval;

        public float GroundProbeDistance => _groundProbeDistance;
        public float WallProbeDistance => _wallProbeDistance;

        public LayerMask TargetMask => _targetMask;
        public LayerMask ObstacleMask => _obstacleMask;
        public LayerMask GroundMask => _groundMask;
    }
}
